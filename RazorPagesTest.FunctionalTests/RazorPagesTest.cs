using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RazorPagesTest.FunctionalTests
{
    public class RazorPagesTest : IClassFixture<MvcTestFixture<Startup>>
    {
        private static readonly Assembly _resourcesAssembly = typeof(RazorPagesTest).GetTypeInfo().Assembly;

        public RazorPagesTest(MvcTestFixture<Startup> fixture)
        {
            Client = fixture.Client;
        }

        public HttpClient Client { get; }

        [Fact]
        public async void action_should_return_razor_page()
        {
            var response = await Client.GetAsync("http://localhost/");
            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
