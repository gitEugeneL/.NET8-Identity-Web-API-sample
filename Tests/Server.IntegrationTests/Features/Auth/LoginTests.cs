using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Server.Contracts;
using Server.Helpers;
using Xunit;

namespace Server.IntegrationTests.Features.Auth;

public class LoginTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = TestCase.CreateTestHttpClient(factory, "LoginEndpoint");
    
    [Theory]
    [ClassData(typeof(TestData))]
    public async Task Login_withValidUser_ReturnsOkResult(RegistrationRequest testData)
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
        
        /*** act ***/
        var response = await _client.PostAsJsonAsync(Paths.Login, new LoginRequest(model.Email, model.Password));
        var responseData = await TestCase.DeserializeResponse<RefreshResponse>(response);

        /*** assert ***/
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseData.AccessToken.Should().NotBeNullOrEmpty();
        responseData.RefreshToken.Should().NotBeNullOrEmpty();
        responseData.RefreshTokenExpires.Should().BeAfter(DateTime.UtcNow);
    }
    
    [Theory]
    [ClassData(typeof(TestData))]
    public async Task Login_withInvalidUser_ReturnsBadRequestResult(RegistrationRequest testData)
    {
        /*** arrange ***/
        var model = new LoginRequest(testData.Email, testData.Password);
        
        /*** act ***/
        var response = await _client.PostAsJsonAsync(Paths.Login, model);
    
        /*** assert ***/
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Theory]
    [ClassData(typeof(TestData))]
    public async Task Login_withUnconfirmedEmail_ReturnsBadRequestResult(RegistrationRequest testData)
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
        
        /*** act ***/
        var response = await _client.PostAsJsonAsync(Paths.Login, new LoginRequest(model.Email, model.Password));
        
        /*** assert ***/
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Theory]
    [ClassData(typeof(TestData))]
    public async Task Login_withInvalidPassword_ReturnsBadRequestResult(RegistrationRequest testData)
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
        
        /*** act ***/
        var response = await _client.PostAsJsonAsync(Paths.Login, new LoginRequest(model.Email, "InvalidPwd"));
        
        /*** assert ***/
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Theory]
    [ClassData(typeof(TestData))]
    public async Task Login_withLockedPassword_ReturnsBadRequestResult(RegistrationRequest testData)
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
    
        var loginModel = new LoginRequest(model.Email, "InvalidPassword");
        
        /*** act ***/
        for (var i = 0; i < 10; i++)
            await _client.PostAsJsonAsync(Paths.Login, loginModel);
        
        var response = await _client.PostAsJsonAsync(Paths.Login, loginModel with { Password = model.Password });
    
        /*** assert ***/
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}