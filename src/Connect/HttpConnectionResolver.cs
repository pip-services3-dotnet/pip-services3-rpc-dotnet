using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipServices.Commons.Config;
using PipServices.Commons.Errors;
using PipServices.Commons.Refer;
using PipServices.Components.Connect;

namespace PipServices.Rpc.Connect
{
    public class HttpConnectionResolver : IReferenceable, IConfigurable {
        protected ConnectionResolver _connectionResolver = new ConnectionResolver();

        public void SetReferences(IReferences references)
        {
            _connectionResolver.SetReferences(references);
        }

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

        public async Task<ConnectionParams> ResolveAsync(string correlationId)
        {
            var connection = await _connectionResolver.ResolveAsync(correlationId);
            ValidateConnection(correlationId, connection);
            UpdateConnection(connection);
            return connection;
        }

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

        public async Task RegisterAsync(string correlationId)
        {
            var connection = await _connectionResolver.ResolveAsync(correlationId);
            ValidateConnection(correlationId, connection);

            await _connectionResolver.RegisterAsync(correlationId, connection);
        }

    }
}
