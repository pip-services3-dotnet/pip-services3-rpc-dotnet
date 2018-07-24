using System;
using PipServices.Commons.Config;
using Xunit;

namespace PipServices.Rpc.Connect
{
    public class HttpConnectionResolverTest
    {
        public HttpConnectionResolverTest()
        {
        }

        [Fact]
        public void TestConnectionParams()
        {
            var connectionResolver = new HttpConnectionResolver();
            connectionResolver.Configure(ConfigParams.FromTuples(
                "connection.host", "somewhere.com",
                "connection.port", 123
            ));

            var connection = connectionResolver.ResolveAsync(null).Result;

            Assert.Equal("http", connection.Protocol);
            Assert.Equal("somewhere.com", connection.Host);
            Assert.Equal(123, connection.Port);
            Assert.Equal("http://somewhere.com:123", connection.Uri);
        }

        [Fact]
        public void TestConnectionUri()
        {
            var connectionResolver = new HttpConnectionResolver();
            connectionResolver.Configure(ConfigParams.FromTuples(
                "connection.uri", "https://somewhere.com:123"
            ));

            var connection = connectionResolver.ResolveAsync(null).Result;

            Assert.Equal("https", connection.Protocol);
            Assert.Equal("somewhere.com", connection.Host);
            Assert.Equal(123, connection.Port);
            Assert.Equal("https://somewhere.com:123", connection.Uri);
        }
    
    }
}
