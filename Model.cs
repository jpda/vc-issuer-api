using System.Text.Json.Serialization;

namespace IssuerApi
{

    //     {
    //     "requestId": "e45077ff-5997-4c54-b2aa-358cf08c5799",
    //     "url": "openid://vc/?request_uri=https://beta.did.msidentity.com/v1.0/f2c6553e-aeab-450e-b4d2-b06c2143985d/verifiablecredentials/request/e45077ff-5997-4c54-b2aa-358cf08c5799",
    //     "expiry": 1636131655
    // }

    public class VcResponse
    {
        [JsonPropertyName("requestId")]
        public string RequestId { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("expiry")]
        public long Expiry { get; set; }
    }

    // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
    public class Headers
    {
        [JsonPropertyName("api-key")]
        public string ApiKey { get; set; }
    }

    public class Callback
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("headers")]
        public Headers Headers { get; set; }
    }

    public class Registration
    {
        [JsonPropertyName("clientName")]
        public string ClientName { get; set; }
    }

    public class Pin
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("length")]
        public int Length { get; set; }
    }

    public class Claims
    {
        [JsonPropertyName("given_name")]
        public string GivenName { get; set; }

        [JsonPropertyName("family_name")]
        public string FamilyName { get; set; }
    }

    public class Issuance
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("manifest")]
        public string Manifest { get; set; }

        [JsonPropertyName("pin")]
        public Pin Pin { get; set; }

        [JsonPropertyName("claims")]
        public Claims Claims { get; set; }
    }

    public class VcRequest
    {
        [JsonPropertyName("includeQRCode")]
        public bool IncludeQRCode { get; set; }

        [JsonPropertyName("callback")]
        public Callback Callback { get; set; }

        [JsonPropertyName("authority")]
        public string Authority { get; set; }

        [JsonPropertyName("registration")]
        public Registration Registration { get; set; }

        [JsonPropertyName("issuance")]
        public Issuance Issuance { get; set; }

        public string Id { get; set; }
    }
}