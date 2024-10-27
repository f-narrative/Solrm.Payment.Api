using Microsoft.AspNetCore.Mvc;
using Solrm.Payment.Api.Dtos;
using System.Text.Json;

namespace Solrm.Payment.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController(IHttpClientFactory factory) : ControllerBase
{
    [HttpPost("sendRequest")]
    public async Task<IActionResult> SendPayment([FromBody] RequestToPayDto dto)
    {
        using var client = factory.CreateClient("momo");

        client.DefaultRequestHeaders.Add("X-Reference-Id", dto.ReferenceId.ToString());

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        var requestToPayJson = JsonSerializer.Serialize(dto, options);
        var requestToPayResult = await client.PostAsync("/collection/v1_0/requesttopay",
            new StringContent(requestToPayJson));

        var contentResult = await requestToPayResult.Content.ReadAsStringAsync();
        if (requestToPayResult.IsSuccessStatusCode)
        {
            return StatusCode((int)requestToPayResult.StatusCode, new { dto.ReferenceId });
        }

        return StatusCode((int)requestToPayResult.StatusCode, contentResult);
    }

    [HttpPost("getRequestStatus/{referenceId}")]
    public async Task<IActionResult> GetRequestToPayTransactionStatus([FromRoute] Guid referenceId)
    {
        using var client = factory.CreateClient("momo");

        var requestToPayResult = await client.GetFromJsonAsync<object>($"/collection/v1_0/requesttopay/{referenceId}");

        return Ok(requestToPayResult);
    }
}