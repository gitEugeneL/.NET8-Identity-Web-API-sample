using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Server.Contracts;
using Server.Helpers;
using Xunit;

namespace Server.IntegrationTests.Features.Auth;

public class RefreshTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = TestCase.CreateTestHttpClient(factory, "RefreshEndpoint");

    [Theory]
    [ClassData(typeof(TestData))]
    public async Task Refresh_withValidRefreshToken_ReturnsOkResult(RegistrationRequest testData)
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
        // confirm email
        await _client.GetAsync($"{Paths.EmailConfirmation}?confirmationToken=moqToken&email={model.Email}");
        // first login
        var loginModel = new LoginRequest(model.Email, model.Password);
        var loginData = await TestCase
            .DeserializeResponse<LoginResponse>(await _client.PostAsJsonAsync(Paths.Login, loginModel));
    
        var refreshModel = new RefreshRequest(loginData.RefreshToken);
        
        /*** act ***/
        var response = await _client.PostAsJsonAsync(Paths.Refresh, refreshModel);
        var responseData = await TestCase.DeserializeResponse<RefreshResponse>(response);
        
        /*** assert ***/
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseData.AccessToken.Should().NotBeNullOrEmpty();
        responseData.RefreshToken.Should().NotBeNullOrEmpty();
        responseData.RefreshTokenExpires.Should().BeAfter(DateTime.UtcNow);
    }

    [Theory]
    [ClassData(typeof(TestData))]
    public async Task Refresh_withInvalidRefreshToken_ReturnsBadRequest(RegistrationRequest testData)
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
        // confirm email
        await _client.GetAsync($"{Paths.EmailConfirmation}?confirmationToken=moqToken&email={model.Email}");
        // first login
        await _client.PostAsJsonAsync(Paths.Login, new LoginRequest(model.Email, model.Password));
        
        /*** act ***/
        var response = await _client.PostAsJsonAsync(Paths.Refresh, new RefreshRequest("invalid refreshToken"));
        
        /*** assert ***/
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [ClassData(typeof(TestData))]
    public async Task Refresh_withSignOutUser_ReturnsBadRequest(RegistrationRequest testData)
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
        // confirm email
        await _client.GetAsync($"{Paths.EmailConfirmation}?confirmationToken=moqToken&email={model.Email}");
        // first login
        var loginRequest = await TestCase.DeserializeResponse<LoginResponse>(await _client.
            PostAsJsonAsync(Paths.Login, new LoginRequest(model.Email, model.Password)));
        // sign out
        TestCase.IncludeTokenInRequest(_client, loginRequest.AccessToken);
        var r= await _client.PostAsJsonAsync(Paths.Logout, new RefreshRequest(loginRequest.RefreshToken));
        
        /*** act ***/
        var response = await _client.PostAsJsonAsync(Paths.Refresh, new RefreshRequest(loginRequest.RefreshToken));
        
        /*** assert ***/
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [ClassData(typeof(TestData))]
    public async Task Refresh_withLockedPassword_ReturnsBadRequest(RegistrationRequest testData)
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
        // confirm email
        await _client.GetAsync($"{Paths.EmailConfirmation}?confirmationToken=moqToken&email={model.Email}");
        // login
        var loginModel = new LoginRequest(model.Email, model.Password);
        var loginData = await TestCase
            .DeserializeResponse<LoginResponse>(await _client.PostAsJsonAsync(Paths.Login, loginModel));
        
        var validRefreshToken = loginData.RefreshToken;
        
        // lockPassword
        for (var i = 0; i < 10; i++)
            await _client.PostAsJsonAsync(Paths.Login, loginModel with { Password = "invalidPassword" });
        
        /*** act ***/
        var response = await _client.PostAsJsonAsync(Paths.Refresh, new RefreshRequest(validRefreshToken)); 
        
        /*** assert ***/
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}