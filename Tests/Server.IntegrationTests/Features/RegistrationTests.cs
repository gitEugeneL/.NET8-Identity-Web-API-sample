using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Server.Contracts;
using Xunit;

namespace Server.IntegrationTests.Features;

public class RegistrationTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = TestCase.CreateTestHttpClient(factory, "RegistrationEndpoint");

    private readonly string _endpoint = "/api/auth/registration"; 
    
    private RegistrationRequest _testModel = new RegistrationRequest(
        Email: "test@user.com", 
        Password: "strongPwd!1@", 
        ConfirmPassword: "strongPwd!1@",
        Username: "testUser",
        ClientUri: "https://test.com/api/auth/registration"
    );
    
    [Theory]
    [InlineData("mail@test.com", "testUser", "strongPwd!1@", "strongPwd!1@")]
    [InlineData("email@test.test", "user123", "myPassword12@", "myPassword12@")]
    public async Task Registration_withValidBody_ReturnsCreatedResult(
        string email, 
        string username, 
        string password, 
        string confirmPassword)
    {
        // arrange
        _testModel = _testModel with
        {
            Email = email, 
            Password = password, 
            ConfirmPassword = confirmPassword,
            Username = username
        };
        
        // act
        var response = await _client.PostAsJsonAsync(_endpoint, _testModel);
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Registration_withExistingEmail_ReturnsBadRequestResult()
    {
        // act
        var response = new HttpResponseMessage();
        for (var i = 0; i < 2; i++)
        {
            _testModel = _testModel with { Username = _testModel.Username + i };
            response = await _client.PostAsJsonAsync(_endpoint, _testModel);
        }
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Registration_withExistingUsername_ReturnsBadRequestResult()
    {
        // act
        var response = new HttpResponseMessage();
        for (var i = 0; i < 2; i++)
        {
            _testModel = _testModel with { Email = _testModel.Email + i };
            response = await _client.PostAsJsonAsync(_endpoint, _testModel);
        }
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Registration_withInvalidPassword_ReturnsUnprocessableEntityResult()
    {
        // arrange
        _testModel = _testModel with
        {
            Password = "1234", 
            ConfirmPassword = "1234"
        };

        // act
        var response = await _client.PostAsJsonAsync(_endpoint, _testModel);
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Registration_withInvalidConfirmPassword_ReturnsUnprocessableEntityResult()
    {
        // arrange
        _testModel = _testModel with
        {
            ConfirmPassword = "asdQ@E!23ASd"
        };

        // act
        var response = await _client.PostAsJsonAsync(_endpoint, _testModel);
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}