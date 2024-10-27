using System.Net;
using Solrm.Payment.Api.Options;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using Solrm.Payment.Api.Dtos;

namespace Solrm.Payment.Api;

public class MomoDelegatingHandler(IOptions<MomoApiOptions> momoApiOptions) : DelegatingHandler
{
    private readonly MomoApiOptions _momoApiOptions = momoApiOptions.Value;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var result = await base.SendAsync(request, cancellationToken);
        if (result is not { IsSuccessStatusCode: false, StatusCode: HttpStatusCode.Unauthorized}) return result;
        
        var getNewAccessTokenUrl = $"{_momoApiOptions.BaseUrl}/collection/token/";
        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(getNewAccessTokenUrl);

        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_momoApiOptions.ApiUser}:{_momoApiOptions.ApiKey}")));

        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _momoApiOptions.SubscriptionKey);

        var getAccessTokenResult = await httpClient.PostAsync(getNewAccessTokenUrl, null, cancellationToken);
        if (!getAccessTokenResult.IsSuccessStatusCode)
        {
            return result;
        }

        var getAccessTokenResultContent = await getAccessTokenResult.Content.ReadAsStringAsync(cancellationToken);
        var newAccessTokenDto = JsonSerializer.Deserialize<NewTokenDto>(getAccessTokenResultContent);
        MomoApiTokens.AccessToken = newAccessTokenDto!.AccessToken;

        request.Headers.Remove("Authorization");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", MomoApiTokens.AccessToken);

        return await base.SendAsync(request, cancellationToken);
    }
}