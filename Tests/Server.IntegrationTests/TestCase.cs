using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Quartz;
using Server.Contracts;
using Server.Data;
using Server.Helpers;
using Server.IntegrationTests.FakeServices;
using Server.Services.Interfaces;

namespace Server.IntegrationTests;

public static class TestCase
{
    public static HttpClient CreateTestHttpClient(WebApplicationFactory<Program> factory, string dbname)
    {
        return factory.WithWebHostBuilder(builder => 
            {
                builder.ConfigureServices(services =>
                {
                    // remove DB service
                    services.Remove(services.SingleOrDefault(service =>
                        service.ServiceType == typeof(DbContextOptions<AppDbContext>))!);
                    
                    // add inMemory db
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseInMemoryDatabase(dbname));
                    
                    // remove Quartz service
                    services.Remove(services.SingleOrDefault(service =>
                        service.ServiceType == typeof(IHostedService) && 
                        service.ImplementationType == typeof(QuartzHostedService))!);
                    
                    // add fake mail service
                    services.AddTransient<IMailService, FakeMailService>();
                    
                    // add fake emailConfirmation service
                    services.AddTransient<IConfirmationService, FakeConfirmationService>();
                });
            })
            .CreateClient();
    }
    
    public static async Task<LoginResponse> Login(HttpClient client, string email, string password)
    {
        var model = new LoginRequest(email, password);
        var response = await client.PostAsJsonAsync(Paths.Login, model);
    
        var loginResponse = await DeserializeResponse<LoginResponse>(response);
        
        IncludeTokenInRequest(client, loginResponse.AccessToken);
        
        return loginResponse;
    }
    
    public static void IncludeTokenInRequest(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
    
    public static async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
    {
        var jsonResponse = await response.Content.ReadAsStringAsync(); 
        return JsonConvert.DeserializeObject<T>(jsonResponse)!;
    }
}