using System.Text.Json.Serialization;

namespace Solrm.Payment.Api.Dtos;

public class NewTokenDto
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } 
}