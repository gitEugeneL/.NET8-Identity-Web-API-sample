using System.Collections;
using Server.Contracts;
using Server.Helpers;

namespace Server.IntegrationTests;

public class RegistrationTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return [new RegistrationRequest(
                "mail@test.com", "strongPwd!1@", 
                "strongPwd!1@", "testUser", Paths.Registration)];
        
        yield return [new RegistrationRequest("email@test.test", "myPassword12@", 
            "myPassword12@", "user123", Paths.Registration)];

        yield return [new RegistrationRequest("test@gmail.com", "zaqWSX123!@#", 
            "zaqWSX123!@#", "new-user999", Paths.Registration)];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}