using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Server.Contracts;
using Server.Helpers;
using Xunit;

namespace Server.IntegrationTests.Features.Auth;

public class ResetPasswordTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = TestCase.CreateTestHttpClient(factory, "ResetPasswordEndpoint");

    [Theory]
    [ClassData(typeof(TestData))]
    public async Task ResetPassword_withValidUser_ReturnsOkResult(RegistrationRequest testData)
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
        // get reset token
        await _client
            .PostAsJsonAsync(Paths.ForgotPassword, new ForgotPasswordRequest(model.Email, Paths.ForgotPassword));

        var resetModel = new ResetPasswordRequest(
            "newPassword!@#123", 
            "newPassword!@#123", 
            model.Email, 
            "moqResetToken");
        
        /*** act ***/
        var resetResponse = await _client.PostAsJsonAsync(Paths.ResetPassword, resetModel);
        var loginResponse =
            await _client.PostAsJsonAsync(Paths.Login, new LoginRequest(model.Email, resetModel.NewPassword));
        
        /*** assert ***/
        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ResetPassword_withVInvalidUser_ReturnsBadRequestResult()
    {
        /*** arrange ***/
        var resetModel = new ResetPasswordRequest(
            "newPassword!@#123", 
            "newPassword!@#123", 
            "invalidEmail@test.com", 
            "moqResetToken");
      
        /*** act ***/
        var response = await _client.PostAsJsonAsync(Paths.ResetPassword, resetModel);
        
        /*** assert ***/
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}