using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Server.Helpers;
using Xunit;

namespace Server.IntegrationTests.Features.Auth;

public class RegistrationTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = TestCase.CreateTestHttpClient(factory, "RegistrationEndpoint");
    
    public static IEnumerable<object[]> TestData()
    {
        yield return ["mail@test.com", "testUser", "strongPwd!1@", "strongPwd!1@"];
        yield return ["email@test.test", "user123", "myPassword12@", "myPassword12@"];
    }
    
    [Theory]
    [MemberData(nameof(TestData))]
    public async Task Registration_withValidBody_ReturnsCreatedResult(
        string email, 
        string username, 
        string password, 
        string confirmPassword)
    {
        // arrange
        TestCase.UserModel = TestCase.UserModel with
        {
            Email = email, 
            Password = password, 
            ConfirmPassword = confirmPassword,
            Username = username
        };
        
        // act
        var response = await _client.PostAsJsonAsync(Paths.Registration, TestCase.UserModel);
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
    
    [Theory]
    [MemberData(nameof(TestData))]
    public async Task Registration_withExistingEmail_ReturnsBadRequestResult(
            string email, 
            string username, 
            string password, 
            string confirmPassword)
    {
        // act
        var response = new HttpResponseMessage();
        for (var i = 0; i < 2; i++)
        {
            TestCase.UserModel = TestCase.UserModel with
            {
                Email = email, 
                Password = password, 
                ConfirmPassword = confirmPassword,
                Username = username
            };
            response = await _client.PostAsJsonAsync(Paths.Registration, TestCase.UserModel);
        }
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Theory]
    [MemberData(nameof(TestData))]
    public async Task Registration_withExistingUsername_ReturnsBadRequestResult(
        string email, 
        string username, 
        string password, 
        string confirmPassword)
    {
        // act
        var response = new HttpResponseMessage();
        for (var i = 0; i < 2; i++)
        {
            TestCase.UserModel = TestCase.UserModel with
            {
                Email = email, 
                Password = password, 
                ConfirmPassword = confirmPassword,
                Username = username
            };
            response = await _client.PostAsJsonAsync(Paths.Registration, TestCase.UserModel);
        }
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Registration_withInvalidPassword_ReturnsUnprocessableEntityResult()
    {
        // arrange
        TestCase.UserModel = TestCase.UserModel with
        {
            Password = "1234", 
            ConfirmPassword = "1234"
        };

        // act
        var response = await _client.PostAsJsonAsync(Paths.Registration, TestCase.UserModel);
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Registration_withInvalidConfirmPassword_ReturnsUnprocessableEntityResult()
    {
        // arrange
        TestCase.UserModel = TestCase.UserModel with
        {
            ConfirmPassword = "asdQ@E!23ASd"
        };

        // act
        var response = await _client.PostAsJsonAsync(Paths.Registration, TestCase.UserModel);
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}