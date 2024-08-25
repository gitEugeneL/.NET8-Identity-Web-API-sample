using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Server.Contracts;
using Server.Helpers;
using Xunit;

namespace Server.IntegrationTests.Features.Auth;

public class ForgotPasswordTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = TestCase.CreateTestHttpClient(factory, "ForgotPasswordEndpoint");

    [Theory]
    [ClassData(typeof(TestData))]
    public async Task ForgotPassword_withValidUser_ReturnsOkResult(RegistrationRequest testData)
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
        var response = await _client
            .PostAsJsonAsync(Paths.ForgotPassword, new ForgotPasswordRequest(model.Email, Paths.ForgotPassword));
        
        /*** assert ***/
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ForgotPassword_withInvalidUser_ReturnsBadRequestResult()
    {
        /*** arrange ***/
        var model = new ForgotPasswordRequest("invalid@email.com", Paths.ForgotPassword);
        
        /*** act ***/
        var response = await _client.PostAsJsonAsync(Paths.ForgotPassword, model);
        
        /*** assert ***/
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [ClassData(typeof(TestData))]
    public async Task ForgotPasswordWithUnconfirmedEmail_ReturnsBadRequestResult(RegistrationRequest testData)
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
        var response = await _client
            .PostAsJsonAsync(Paths.ForgotPassword, new ForgotPasswordRequest(model.Email, Paths.ForgotPassword));
        
        /*** assert ***/
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
