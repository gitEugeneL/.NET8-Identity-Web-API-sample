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
    [ClassData(typeof(TestData))]
    public async Task Registration_withValidBody_ReturnsCreatedResult(RegistrationRequest testData)
    {
        /*** arrange ***/
        var model = new RegistrationRequest(
            Email: testData.Email,
            Password: testData.Password,
            ConfirmPassword: testData.ConfirmPassword,
            Username: testData.Username,
            ClientUri: testData.ClientUri);
        
        /*** act ***/
        var response = await _client.PostAsJsonAsync(Paths.Registration, model);
        
        /*** assert ***/
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
    
    [Theory]
    [ClassData(typeof(TestData))]
    public async Task Registration_withExistingEmail_ReturnsBadRequestResult(RegistrationRequest testData)
    {
        /*** act ***/
        var response = new HttpResponseMessage();
        for (var i = 0; i < 2; i++)
        {
            var model = new RegistrationRequest(
                Email: testData.Email, 
                Password: testData.Password, 
                ConfirmPassword: testData.ConfirmPassword, 
                Username: testData.Username, 
                ClientUri: testData.ClientUri); 
            
            response = await _client.PostAsJsonAsync(Paths.Registration, model);
        }
        
        /*** assert ***/
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Theory]
    [ClassData(typeof(TestData))]
    public async Task Registration_withExistingUsername_ReturnsBadRequestResult(RegistrationRequest testData)
    {
        /*** act ***/
        var response = new HttpResponseMessage();
        for (var i = 0; i < 2; i++)
        {
            var model = new RegistrationRequest(
                Email: testData.Email, 
                Password: testData.Password, 
                ConfirmPassword: testData.ConfirmPassword, 
                Username: testData.Username, 
                ClientUri: testData.ClientUri);
            
            response = await _client.PostAsJsonAsync(Paths.Registration, model);
        }
        
        /*** assert ***/
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [ClassData(typeof(TestData))]
    public async Task Registration_withInvalidPassword_ReturnsUnprocessableEntityResult(RegistrationRequest testData)
    {
        /*** arrange ***/
        var model = new RegistrationRequest(
            Email: testData.Email, 
            Password: "invalidPassword", 
            ConfirmPassword: "invalidPassword", 
            Username: testData.Username, 
            ClientUri: testData.ClientUri);

        /*** act ***/
        var response = await _client.PostAsJsonAsync(Paths.Registration, model);
        
        /*** assert ***/
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Theory]
    [ClassData(typeof(TestData))]
    public async Task Registration_withInvalidConfirmPassword_ReturnsUnprocessableEntityResult(RegistrationRequest testData)
    {
        //*** arrange ***/
        var model = new RegistrationRequest(
            Email: testData.Email, 
            Password: testData.Password, 
            ConfirmPassword: "invalidConfirmationPassword", 
            Username: testData.Username, 
            ClientUri: testData.ClientUri); 

        /*** act ***/
        var response = await _client.PostAsJsonAsync(Paths.Registration, model);
        
        /*** assert ***/
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}