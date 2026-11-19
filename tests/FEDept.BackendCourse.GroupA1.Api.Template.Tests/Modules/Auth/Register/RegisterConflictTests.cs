using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Xunit;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Tests.Modules.Auth.Register;

public class RegisterConflictTests(WebApplicationFactory<Program> factory)
: IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory
        .WithWebHostBuilder(builder => builder.ConfigureLogging(logging => logging.ClearProviders()))
        .CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

    [Fact]
    public async Task Register_SameEmailTwice_Returns409WithUrnType()
    {
        var email = $"conflict-{Guid.NewGuid():N}@example.com";
        var body = new StringContent(
            JsonSerializer.Serialize(new { email, password = "secret12" }),
            Encoding.UTF8, "application/json"
        );

        var first = await _client.PostAsync("/auth/register", body);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await _client.PostAsync("/auth/register", body);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);

        var problem = await second.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("urn:fedept:register:email-taken", problem!.Type);
    }
}
