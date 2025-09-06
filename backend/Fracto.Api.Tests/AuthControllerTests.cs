using Xunit;
using System.Threading.Tasks;
using Fracto.Api;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    public AuthControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_Login_Flow()
    {
        var register = new { username = "testuser", email = "test@example.com", password = "Test@1234" };
        var content = new StringContent(JsonConvert.SerializeObject(register), Encoding.UTF8, "application/json");
        var res = await _client.PostAsync("/api/auth/register", content);
        res.EnsureSuccessStatusCode();
    }
}
