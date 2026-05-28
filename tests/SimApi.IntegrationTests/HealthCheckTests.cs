using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace SimApi.IntegrationTests;

public class HealthCheckTests : IntegrationTestBase
{
    public HealthCheckTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task Health_Endpoint_Returns_Ok()
    {
        var response = await Client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(content);
        Assert.Contains("status", content!.Keys);
        Assert.Contains("healthy", content["status"]!.ToString()!);
    }
}
