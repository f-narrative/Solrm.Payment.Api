using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Solrm.Payment.Api;
using Solrm.Payment.Api.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<MomoApiOptions>().BindConfiguration(nameof(MomoApiOptions));
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<MomoDelegatingHandler>();

builder.Services.AddHttpClient("momo", (serviceProvider, client) =>
{
    var momoApiOptions = serviceProvider
        .GetRequiredService<IOptions<MomoApiOptions>>().Value;

    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", MomoApiTokens.AccessToken);
    client.DefaultRequestHeaders.Add("X-Target-Environment", "sandbox");
    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", momoApiOptions.SubscriptionKey);
    client.BaseAddress = momoApiOptions.BaseUrl;
}).AddHttpMessageHandler<MomoDelegatingHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
