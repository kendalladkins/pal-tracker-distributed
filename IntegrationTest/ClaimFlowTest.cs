using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using static IntegrationTest.AppServerBuilder;

namespace IntegrationTest
{
    public class ClaimFlowTest : IDisposable
    {
        private readonly HttpClient _httpClient = new HttpClient();

        private readonly AppServer _claimServer;
        private readonly AppServer _registrationServer;

        public ClaimFlowTest()
        {
             _registrationServer = TestAppServerBuilder()
                .AppName("RegistrationServer")
                .Port(8883)
                .Database("tracker_registration_dotnet_test")
                .SetEnvironmentVariable("EUREKA__CLIENT__SHOULDREGISTERWITHEUREKA", "false")
                .SetEnvironmentVariable("DISABLE_AUTH", "true")
                .SetEnvironmentVariable("SPRING__CLOUD__CONFIG__ENABLED", "false")
                .SetEnvironmentVariable("SPRING__CLOUD__CONFIG__FAILFAST", "false")                
                .Build();

            _claimServer = TestAppServerBuilder()
                .AppName("ClaimServer")
                .Port(8885)
                .Database("tracker_claims_dotnet_test")
                .SetEnvironmentVariable("REGISTRATION_SERVER_ENDPOINT", _registrationServer.Url())
                .SetEnvironmentVariable("EUREKA__CLIENT__SHOULDFETCHREGISTRY", "false")
                .SetEnvironmentVariable("DISABLE_AUTH", "true")
                .SetEnvironmentVariable("SPRING__CLOUD__CONFIG__ENABLED", "false")
                .SetEnvironmentVariable("SPRING__CLOUD__CONFIG__FAILFAST", "false")                
                .Build();
        }

        [Fact]
        public void TestClaimFlow() {
            _registrationServer.Start();
            _claimServer.Start();
            

            HttpResponseMessage response;

            response = _httpClient.Get(_registrationServer.Url());
            Assert.Equal("Noop!", response.Content.ReadAsStringAsync().Result);


             var createdUserId = _httpClient.Post(_registrationServer.Url("/registration"), new Dictionary<string, object>
            {
                {"name", "aUser"}
            }).Content.FindId();
            AssertGreaterThan(createdUserId, 0);

            var createdAccountId = _httpClient.Get(_registrationServer.Url($"/accounts?ownerId={createdUserId}"))
                .Content.FindId();
            AssertGreaterThan(createdAccountId, 0);

            var createdProjectId = _httpClient.Post(_registrationServer.Url("/projects"), new Dictionary<string, object>
            {
                {"accountId", createdAccountId},
                {"name", "aProject"}
            }).Content.FindId();
            AssertGreaterThan(createdProjectId, 0);

            response = _httpClient.Get(_registrationServer.Url($"/projects?accountId={createdAccountId}"));
            AssertNotNullOrEmpty(response.Content.ReadAsStringAsync().Result);
            Assert.True(response.IsSuccessStatusCode);

            response = _httpClient.Get(_claimServer.Url());
            Assert.Equal("Noop!", response.Content.ReadAsStringAsync().Result);

            string claimType = "TODO";
            decimal claimTotal = 323;

            var createdClaimId = _httpClient.Post(_claimServer.Url($"/claims"), new Dictionary<string, object>
            {
                {"projectId", createdProjectId},
                {"userId", createdUserId},
                {"expenseType", claimType},
                {"expenseTotal", claimTotal},
                {"expenseDate", "2019-02-02"}
            }).Content.FindId();
            AssertGreaterThan(createdClaimId, 0);
        }
        
        public void Dispose()
        {
            _claimServer.Stop();
        }

        private static void AssertNotNullOrEmpty(string str)
        {
            Assert.NotEqual("", str);
            Assert.NotNull(str);
        }

        private static void AssertGreaterThan(long actual, long bound)
        {
            Assert.InRange(actual, bound + 1, long.MaxValue);
        }
    }
}