using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipServices.Commons.Config;
using PipServices.Commons.Errors;
using PipServices.Commons.Refer;
using PipServices.Components.Connect;

namespace PipServices.Rpc.Connect
{
    /// <summary>
    /// Helper class to retrieve connections for HTTP-based services abd clients.
    /// 
    /// In addition to regular functions of ConnectionResolver is able to parse http:// URIs
    /// and validate connection parameters before returning them.
    /// 
    /// ### Configuration parameters ###
    /// 
    /// connection:
    /// - discovery_key:               (optional) a key to retrieve the connection from <a href="https://rawgit.com/pip-services-dotnet/pip-services-components-dotnet/master/doc/api/interface_pip_services_1_1_components_1_1_connect_1_1_i_discovery.html">IDiscovery</a>
    /// - ...                          other connection parameters
    /// 
    /// connections:                   alternative to connection
    /// - [connection params 1]:       first connection parameters
    /// - ...
    /// - [connection params N]:       Nth connection parameters
    /// - ...
    /// 
    /// ### References ###
    /// 
    /// - *:discovery:*:*:1.0            (optional) <a href="https://rawgit.com/pip-services-dotnet/pip-services-components-dotnet/master/doc/api/interface_pip_services_1_1_components_1_1_connect_1_1_i_discovery.html">IDiscovery</a> services
    /// </summary>
    /// <example>
    /// <code>
    /// var config = ConfigParams.fromTuples(
    /// "connection.host", "10.1.1.100",
    /// "connection.port", 8080 );
    /// 
    /// var connectionResolver = new HttpConnectionResolver();
    /// connectionResolver.Configure(config);
    /// connectionResolver.SetReferences(references);
    /// 
    /// var params = connectionResolver.ResolveAsync("123");
    /// </code>
    /// </example>
    public class HttpConnectionResolver : IReferenceable, IConfigurable {
        /// <summary>
        /// The base connection resolver.
        /// </summary>
        protected ConnectionResolver _connectionResolver = new ConnectionResolver();

        /// <summary>
        /// Sets references to dependent components.
        /// </summary>
        /// <param name="references">references to locate the component dependencies.</param>
        public void SetReferences(IReferences references)
        {
            _connectionResolver.SetReferences(references);
        }

        /// <summary>
        /// Configures component by passing configuration parameters.
        /// </summary>
        /// <param name="config">configuration parameters to be set.</param>
        public void Configure(ConfigParams config)
        {
            _connectionResolver.Configure(config);
        }

        private void ValidateConnection(string correlationId, ConnectionParams connection)
        {
            if (connection == null)
                throw new ConfigException(correlationId, "NO_CONNECTION", "HTTP connection is not set");

            var uri = connection.Uri;
            if (!string.IsNullOrEmpty(uri))
                return;

            var protocol = connection.GetProtocol("http");
            if ("http" != protocol)
            {
                throw new ConfigException(
                    correlationId, "WRONG_PROTOCOL", "Protocol is not supported by REST connection")
                    .WithDetails("protocol", protocol);
            }

            var host = connection.Host;
            if (host == null)
                throw new ConfigException(correlationId, "NO_HOST", "Connection host is not set");

            var port = connection.Port;
            if (port == 0)
                throw new ConfigException(correlationId, "NO_PORT", "Connection port is not set");
        }

        private void UpdateConnection(ConnectionParams connection)
        {
            if (string.IsNullOrEmpty(connection.Uri))
            {
                var uri = connection.Protocol + "://" + connection.Host;
                if (connection.Port != 0)
                    uri += ":" + connection.Port;
                connection.Uri = uri;
            }
            else
            {
                var uri = new Uri(connection.Uri);
                connection.Protocol = uri.Scheme;
                connection.Host = uri.Host;
                connection.Port = uri.Port;
            }
        }

        /// <summary>
        /// Resolves a single component connection. If connections are configured to be
        /// retrieved from Discovery service it finds a IDiscovery and resolves the connection there.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <returns>resolved connection.</returns>
        public async Task<ConnectionParams> ResolveAsync(string correlationId)
        {
            var connection = await _connectionResolver.ResolveAsync(correlationId);
            ValidateConnection(correlationId, connection);
            UpdateConnection(connection);
            return connection;
        }

        /// <summary>
        /// Resolves all component connection. If connections are configured to be
        /// retrieved from Discovery service it finds a IDiscovery and resolves the connection there.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <returns>resolved connections.</returns>
        public async Task<List<ConnectionParams>> ResolveAllAsync(string correlationId)
        {
            var connections = await _connectionResolver.ResolveAllAsync(correlationId);
            foreach (var connection in connections)
            {
                ValidateConnection(correlationId, connection);
                UpdateConnection(connection);
            }
            return connections;
        }

        /// <summary>
        /// Registers the given connection in all referenced discovery services. This
        /// method can be used for dynamic service discovery.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        public async Task RegisterAsync(string correlationId)
        {
            var connection = await _connectionResolver.ResolveAsync(correlationId);
            ValidateConnection(correlationId, connection);

            await _connectionResolver.RegisterAsync(correlationId, connection);
        }

    }
}
