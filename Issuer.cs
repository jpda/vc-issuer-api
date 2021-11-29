using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Serialization;

namespace IssuerApi
{
    public class Issuer
    {
        private readonly ILogger<Issuer> _logger;
        private readonly IConfidentialClientApplication _msal;
        private readonly VerifiableCredentialServiceConfiguration _vcServiceConfig;
        private readonly HttpClient _httpClient;

        public Issuer(ILogger<Issuer> logger, MsalTokenProvider app, IOptions<VerifiableCredentialServiceConfiguration> vcServiceConfig, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _msal = app._client;
            _vcServiceConfig = vcServiceConfig.Value;
            _httpClient = clientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(_vcServiceConfig.Endpoint);
        }

        [FunctionName("issuer")]
        public async Task<IActionResult> IssuerRequest([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "issuer/request/{credType}")] HttpRequest req, string credType)
        {
            _logger.LogInformation($"Issuer Request for credtype {credType}");

            //"VC:Endpoint": "https://beta.did.msidentity.com/v1.0/{0}/verifiablecredentials/request",
            _logger.LogInformation("Starting new issue request");
            var t = await _msal.AcquireTokenForClient(new[] { _vcServiceConfig.ServiceScope }).ExecuteAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", t.AccessToken);

            if (credType == null || string.IsNullOrEmpty(credType) || !_vcServiceConfig.ValidCredentialTypes.Contains(credType))
            {
                credType = "testcred"; // default, could be setting
            }

            var requestId = Guid.NewGuid().ToString();

            _logger.LogInformation($"Generating request ID {requestId}");
            var request = GenerateVcRequest(requestId, credType, _vcServiceConfig);

            var reqPayload = System.Text.Json.JsonSerializer.Serialize(request, new System.Text.Json.JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            _logger.LogInformation($"Sending {reqPayload} to VC service {_vcServiceConfig.Endpoint}"); 

            //var serviceRequest = await _httpClient.PostAsJsonAsync<VcRequest>("verifiablecredentials/request", request);
            var a = new StringContent(reqPayload, System.Text.Encoding.UTF8, "application/json");

            var serviceRequest = await _httpClient.PostAsync("verifiableCredentials/request", a);

            //var response = await serviceRequest.Content.ReadAsStringAsync();
            var response = await serviceRequest.Content.ReadAsStringAsync();
            var res = System.Text.Json.JsonSerializer.Deserialize<VcResponse>(response);

            return new OkObjectResult(res);
        }

        private static VcRequest GenerateVcRequest(string reqId, string credType, VerifiableCredentialServiceConfiguration config)
        {
            var a = new VcRequest()
            {
                Id = reqId,
                Callback = new Callback()
                {
                    Url = config.CallbackUrl,
                    State = reqId,
                },
                Authority = config.IssuerAuthority,
                IncludeQRCode = false,
                Registration = new Registration()
                {
                    ClientName = config.ClientName
                },
                Issuance = new Issuance()
                {
                    Type = $"{config.CredentialTypeBase}/{credType}",
                    Manifest = config.CredentialManifest,
                    // Pin = new Pin()
                    // {
                    //     Value = "123456",
                    //     Length = 4
                    // }
                }
            };
            return a;
        }
    }
}
