using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Server.Contracts;
using Server.Helpers;
using Xunit;

namespace Server.IntegrationTests.Features.Auth;

public class ConfirmationTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = TestCase.CreateTestHttpClient(factory, "ConfirmationEndpoint");

    [Theory]
    [ClassData(typeof(TestData))]
    public async Task Confirmation_withValidUserAndValidConfirmationToken_ReturnsOkResult(RegistrationRequest testData)
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
            .GetAsync($"{Paths.EmailConfirmation}?confirmationToken=moqToken&email={model.Email}");
        
        /*** assert ***/
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Confirmation_withInvalidUser_ReturnsBadRequestResult()
    {
        /*** arrange ***/

        /*** act ***/
        var response = await _client
            .GetAsync($"{Paths.EmailConfirmation}?confirmationToken=moqToken&email=invalidEmail@email.com");
        
        /*** assert ***/
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Theory]
    [ClassData(typeof(TestData))]
    public async Task Confirmation_withInvalidConfirmationToken_ReturnsBadRequestResult(RegistrationRequest testData)
    {
        /*** arrange ***/
        var model = new RegistrationRequest(
            Email: testData.Email,
            Password: testData.Password,
            ConfirmPassword: testData.ConfirmPassword,
            Username: testData.Username,
            ClientUri: testData.ClientUri);

        /*** act ***/
        var response = await _client
            .GetAsync($"{Paths.EmailConfirmation}?{model.Email}");

        /*** assert ***/
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}