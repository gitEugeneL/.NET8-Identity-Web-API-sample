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
    [ClassData(typeof(RegistrationTestData))]
    public async Task Login_withValidUser_ReturnsOkResult(RegistrationRequest testData)
    {
        // arrange
        var model = TestCase.BaseUserModel with
        {
            Email = testData.Email, 
            Password = testData.Password, 
            ConfirmPassword = testData.ConfirmPassword,
            Username = testData.Username
        };
       
        await _client.PostAsJsonAsync(Paths.Registration, model);  // create user
        
        await _client.GetAsync($"{Paths.EmailConfirmation}{TestCase.TokenFromEmail}"); // confirm email
        
        var loginModel = new LoginRequest(model.Email, model.Password);
        
        // act
        var response = await _client.PostAsJsonAsync(Paths.Login, loginModel);
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Theory]
    [ClassData(typeof(RegistrationTestData))]
    public async Task Login_withInvalidUser_ReturnsBadRequestResult(RegistrationRequest testData)
    {
        // arrange
        var model = TestCase.BaseUserModel with
        {
            Email = testData.Email, 
            Password = testData.Password
        };
        
        var loginModel = new LoginRequest(model.Email, model.Password);

        // act
        var response = await _client.PostAsJsonAsync(Paths.Login, loginModel);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [ClassData(typeof(RegistrationTestData))]
    public async Task Login_withUnconfirmedEmail_ReturnsBadRequestResult(RegistrationRequest testData)
    {
        // arrange
        var model = TestCase.BaseUserModel with
        {
            Email = testData.Email,
            Password = testData.Password,
            ConfirmPassword = testData.ConfirmPassword,
            Username = testData.Username
        };
        await _client.PostAsJsonAsync(Paths.Registration, model);  // create user
        
        var loginModel = new LoginRequest(model.Email, model.Password);
        
        // act
        var response = await _client.PostAsJsonAsync(Paths.Login, loginModel);
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [ClassData(typeof(RegistrationTestData))]
    public async Task Login_withInvalidPassword_ReturnsBadRequestResult(RegistrationRequest testData)
    {
        // arrange
        var model = TestCase.BaseUserModel with
        {
            Email = testData.Email,
            Password = testData.Password,
            ConfirmPassword = testData.ConfirmPassword,
            Username = testData.Username
        };
        
        await _client.PostAsJsonAsync(Paths.Registration, model);  // create user
        
        await _client.GetAsync($"{Paths.EmailConfirmation}{TestCase.TokenFromEmail}"); // confirm email

        var loginModel = new LoginRequest(model.Email, "InvalidPassword");
      
        // act
        var response = await _client.PostAsJsonAsync(Paths.Login, loginModel);
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Theory]
    [ClassData(typeof(RegistrationTestData))]
    public async Task Login_withLockedPassword_ReturnsBadRequestResult(RegistrationRequest testData)
    {
        // arrange
        var model = TestCase.BaseUserModel with
        {
            Email = testData.Email,
            Password = testData.Password,
            ConfirmPassword = testData.ConfirmPassword,
            Username = testData.Username
        };
        
        await _client.PostAsJsonAsync(Paths.Registration, model);  // create user
        
        await _client.GetAsync($"{Paths.EmailConfirmation}{TestCase.TokenFromEmail}"); // confirm email

        var loginModel = new LoginRequest(model.Email, "InvalidPassword");

        
        // act
        for (var i = 0; i < 10; i++)
            await _client.PostAsJsonAsync(Paths.Login, loginModel);
        
        var response = await _client.PostAsJsonAsync(Paths.Login, loginModel with { Password = model.Password });

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}