using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Server.Contracts;
using Server.Helpers;
using Xunit;

namespace Server.IntegrationTests.Features.Auth;

public class LogoutTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = TestCase.CreateTestHttpClient(factory, "LogoutEndpoint");

    [Theory]
    [ClassData(typeof(TestData))]
    public async Task Logout_withValidRefreshToken_ReturnsNoContentResult(RegistrationRequest testData)
    {
        /*** arrange ***/
        var model = new RegistrationRequest(
            Email: testData.Email, 
            Password: testData.Password, 
            ConfirmPassword: testData.ConfirmPassword, 
            Username: testData.Username, 
            ClientUri: testData.ClientUri); 
        
        // registration
        await _client.PostAsJsonAsync(Paths.Registration, model);
        // email confirmation
        await _client.GetAsync($"{Paths.EmailConfirmation}?confirmationToken=moqToken&email={model.Email}");
        // login
        var loginResponse = await _client.PostAsJsonAsync(Paths.Login, new LoginRequest(model.Email, model.Password));
        var loginResponseData = await TestCase.DeserializeResponse<LoginResponse>(loginResponse);
        
        /*** act ***/
        TestCase.IncludeTokenInRequest(_client, loginResponseData.AccessToken);
        var response = await _client.PostAsJsonAsync(Paths.Logout, new RefreshRequest(loginResponseData.RefreshToken));
        
        /*** assert ***/
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Theory]
    [ClassData(typeof(TestData))]
    public async Task Logout_withInvalidRefreshToken_ReturnsBadRequestResult(RegistrationRequest testData)
    {
        /*** arrange ***/
        var model = new RegistrationRequest(
            Email: testData.Email, 
            Password: testData.Password, 
            ConfirmPassword: testData.ConfirmPassword, 
            Username: testData.Username, 
            ClientUri: testData.ClientUri); 
        
        // registration
        await _client.PostAsJsonAsync(Paths.Registration, model);
        // email confirmation
        await _client.GetAsync($"{Paths.EmailConfirmation}?confirmationToken=moqToken&email={model.Email}");
        // login
        var loginResponse = await _client.PostAsJsonAsync(Paths.Login, new LoginRequest(model.Email, model.Password));
        var loginResponseData = await TestCase.DeserializeResponse<LoginResponse>(loginResponse);
        
        /*** act ***/
        TestCase.IncludeTokenInRequest(_client, loginResponseData.AccessToken);
        var response = await _client.PostAsJsonAsync(Paths.Logout, new RefreshRequest("invalid-refresh-token"));
        
        /*** assert ***/
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}