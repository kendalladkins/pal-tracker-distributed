using System;
using Xunit;

namespace ClaimTest
{
    public class ClaimControllerTest
    {
        private readonly Mock<IClaimDataGateway> _gateway;
        private readonly Mock<IProjectClient> _client;
        private readonly ClaimController _controller;

        public ClaimControllerTest()
        {
            _gateway = new Mock<IClaimDataGateway>();
            _client = new Mock<IProjectClient>();
            _controller = new ClaimController(_gateway.Object, _client.Object);
        }

        [Fact]
        public void TestPost()
        {
            Assert(True).Equal(False);
        }
    }
}
