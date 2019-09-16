using System;
using PipServices3.Commons.Config;
using PipServices3.Commons.Errors;
using Xunit;

namespace PipServices3.Rpc.Connect
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
        public void TestHttpsWithCredentialsConnectionParams()
        {
            var connectionResolver = new HttpConnectionResolver();
            connectionResolver.Configure(ConfigParams.FromTuples(
                "connection.host", "somewhere.com",
                "connection.port", 123,
                "connection.protocol", "https",
                "credential.ssl_password", "ssl_password",
                "credential.ssl_pfx_file", "ssl_pfx_file"
            ));

            var connection = connectionResolver.ResolveAsync(null).Result;

            Assert.Equal("https", connection.Protocol);
            Assert.Equal("somewhere.com", connection.Host);
            Assert.Equal(123, connection.Port);
            Assert.Equal("https://somewhere.com:123", connection.Uri);
            Assert.Equal("ssl_password", connection.Get("credential.ssl_password"));
            Assert.Equal("ssl_pfx_file", connection.Get("credential.ssl_pfx_file"));
        }


        /// <summary>
        /// In this test the credential object should not be added to the connection object
        /// So the property 'credential.internal_network' on the connection object should be null
        /// </summary>
        [Fact]
        public void TestHttpsWithNoCredentialsConnectionParams()
        {
            var connectionResolver = new HttpConnectionResolver();
            connectionResolver.Configure(ConfigParams.FromTuples(
                "connection.host", "somewhere.com",
                "connection.port", 123,
                "connection.protocol", "https",
                "credential.internal_network", "internal_network"
            ));

            var connection = connectionResolver.ResolveAsync(null).Result;

            Assert.Equal("https", connection.Protocol);
            Assert.Equal("somewhere.com", connection.Host);
            Assert.Equal(123, connection.Port);
            Assert.Equal("https://somewhere.com:123", connection.Uri);
            Assert.Null(connection.Get("credential.internal_network"));
        }

        [Fact]
        public async void TestHttpsWithMissingCredentialsConnectionParams()
        {
            var connectionResolver = new HttpConnectionResolver();
            connectionResolver.Configure(ConfigParams.FromTuples(
                "connection.host", "somewhere.com",
                "connection.port", 123,
                "connection.protocol", "https"
            ));

            var exception = await Assert.ThrowsAsync<ConfigException>(() => connectionResolver.ResolveAsync(null));
            Assert.Equal("SSL password is not configured in credentials", exception.Message);

            connectionResolver = new HttpConnectionResolver();
            connectionResolver.Configure(ConfigParams.FromTuples(
                "connection.host", "somewhere.com",
                "connection.port", 123,
                "connection.protocol", "https",
                "credential.ssl_password", "ssl_password"
            ));

            exception = await Assert.ThrowsAsync<ConfigException>(() => connectionResolver.ResolveAsync(null));
            Assert.Equal("SSL pfx file is not configured in credentials", exception.Message);

            connectionResolver = new HttpConnectionResolver();
            connectionResolver.Configure(ConfigParams.FromTuples(
                "connection.host", "somewhere.com",
                "connection.port", 123,
                "connection.protocol", "https",
                "credential.ssl_password", "ssl_password",
                "credential.ssl_pfx_file", "ssl_pfx_file"
            ));

            var connection = connectionResolver.ResolveAsync(null).Result;

            Assert.Equal("https", connection.Protocol);
            Assert.Equal("somewhere.com", connection.Host);
            Assert.Equal(123, connection.Port);
            Assert.Equal("https://somewhere.com:123", connection.Uri);
            Assert.Equal("ssl_password", connection.Get("credential.ssl_password"));
            Assert.Equal("ssl_pfx_file", connection.Get("credential.ssl_pfx_file"));
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
