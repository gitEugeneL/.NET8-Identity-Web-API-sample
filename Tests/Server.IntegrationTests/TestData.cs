using System.Collections;
using Server.Contracts;
using Server.Helpers;

namespace Server.IntegrationTests;

public class TestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return [new RegistrationRequest("mail@test.com", "strongPwd!1@", 
                "strongPwd!1@", "testUser", Paths.Registration)];
        
        yield return [new RegistrationRequest("email@test.test", "myPassword12@", 
            "myPassword12@", "user123", Paths.Registration)];

        yield return [new RegistrationRequest("test@gmail.com", "zaqWSX123!@#", 
            "zaqWSX123!@#", "new-user999", Paths.Registration)];
        
        yield return [new RegistrationRequest("user1@example.com", "Password!23", 
            "Password!23", "user_one", Paths.Registration)];

        yield return [new RegistrationRequest("hello@world.com", "SecurePwd#123", 
            "SecurePwd#123", "hello_world", Paths.Registration)];

        yield return [new RegistrationRequest("demo@test.org", "Test@1234", 
            "Test@1234", "demo_user", Paths.Registration)];

        yield return [new RegistrationRequest("sample@domain.com", "SamplePass$1", 
            "SamplePass$1", "sample_user", Paths.Registration)];

        yield return [new RegistrationRequest("user999@site.net", "SitePass!678", 
            "SitePass!678", "user_999", Paths.Registration)];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}