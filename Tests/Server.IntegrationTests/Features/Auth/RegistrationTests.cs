using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Server.Contracts;
using Server.Helpers;
using Xunit;

namespace Server.IntegrationTests.Features.Auth;


public class RegistrationTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = TestCase.CreateTestHttpClient(factory, "RegistrationEndpoint");
    
    [Theory]
    [ClassData(typeof(RegistrationTestData))]
    public async Task Registration_withValidBody_ReturnsCreatedResult(RegistrationRequest testData)
    {
        // arrange
        var model  = TestCase.BaseUserModel with
        {
            Email = testData.Email, 
            Password = testData.Password, 
            ConfirmPassword = testData.ConfirmPassword,
            Username = testData.Username
        };
        
        // act
        var response = await _client.PostAsJsonAsync(Paths.Registration, model);
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
    
    [Theory]
    [ClassData(typeof(RegistrationTestData))]
    public async Task Registration_withExistingEmail_ReturnsBadRequestResult(RegistrationRequest testData)
    {
        // act
        var response = new HttpResponseMessage();
        for (var i = 0; i < 2; i++)
        {
            var model  = TestCase.BaseUserModel with
            {
                Email = testData.Email, 
                Password = testData.Password, 
                ConfirmPassword = testData.ConfirmPassword,
                Username = testData.Username
            };
            response = await _client.PostAsJsonAsync(Paths.Registration, model);
        }
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Theory]
    [ClassData(typeof(RegistrationTestData))]
    public async Task Registration_withExistingUsername_ReturnsBadRequestResult(RegistrationRequest testData)
    {
        // act
        var response = new HttpResponseMessage();
        for (var i = 0; i < 2; i++)
        {
            var model = TestCase.BaseUserModel with
            {
                Email = testData.Email, 
                Password = testData.Password, 
                ConfirmPassword = testData.ConfirmPassword,
                Username = testData.Username
            };
            response = await _client.PostAsJsonAsync(Paths.Registration, model);
        }
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Registration_withInvalidPassword_ReturnsUnprocessableEntityResult()
    {
        // arrange
        var model  = TestCase.BaseUserModel with
        {
            Password = "1234", 
            ConfirmPassword = "1234"
        };

        // act
        var response = await _client.PostAsJsonAsync(Paths.Registration, model);
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Registration_withInvalidConfirmPassword_ReturnsUnprocessableEntityResult()
    {
        // arrange
        var model = TestCase.BaseUserModel with
        {
            ConfirmPassword = "asdQ@E!23ASd"
        };

        // act
        var response = await _client.PostAsJsonAsync(Paths.Registration, model);
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}