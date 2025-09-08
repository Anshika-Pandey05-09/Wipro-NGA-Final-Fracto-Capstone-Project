using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

public class AuthControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    public AuthControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Register_Then_Login_Succeeds()
    {
        var uniq = Guid.NewGuid().ToString("N").Substring(0, 8);
        var username = $"test_{uniq}";
        var email = $"t{uniq}@example.com";
        var password = "Test@1234";

        // --- 1) Register ---
        var register = new { username, email, password };
        var regRes = await _client.PostAsJsonAsync("/api/auth/register", register);

        if (!regRes.IsSuccessStatusCode)
        {
            var body = await regRes.Content.ReadAsStringAsync();

            // Accept 400/409 if it looks like a duplicate and continue to login
            if (!(regRes.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Conflict) ||
                (body?.IndexOf("exist", StringComparison.OrdinalIgnoreCase) ?? -1) < 0)
            {
                throw new Exception($"Register failed: {(int)regRes.StatusCode} {regRes.StatusCode}\n{body}");
            }
        }

        // --- 2) Login with USERNAME (your API requires 'username', not 'usernameOrEmail') ---
        var login = new { username, password };
        var loginRes = await _client.PostAsJsonAsync("/api/auth/login", login);
        var loginBody = await loginRes.Content.ReadAsStringAsync();

        if (!loginRes.IsSuccessStatusCode)
            throw new Exception($"Login failed: {(int)loginRes.StatusCode} {loginRes.StatusCode}\n{loginBody}");

        // Basic response shape check: must have token
        using var doc = JsonDocument.Parse(loginBody);
        Assert.True(doc.RootElement.TryGetProperty("token", out _), "Expected 'token' in login response.");
    }
}