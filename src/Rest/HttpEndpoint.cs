using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.DependencyInjection;
using PipServices.Commons.Config;
using PipServices.Commons.Errors;
using PipServices.Commons.Refer;
using PipServices.Commons.Run;
using PipServices.Components.Count;
using PipServices.Components.Log;
using PipServices.Rpc.Connect;

namespace PipServices.Rpc.Services
{
    public class HttpEndpoint : IOpenable, IConfigurable, IReferenceable
    {
        private static readonly ConfigParams _defaultConfig = ConfigParams.FromTuples(
            "connection.protocol", "http",
            "connection.host", "0.0.0.0",
            "connection.port", 3000,

            "options.request_max_size", 1024*1024,
            "options.connect_timeout", 60000,
            "options.debug", true
        );

        protected HttpConnectionResolver _connectionResolver = new HttpConnectionResolver();
        protected CompositeLogger _logger = new CompositeLogger();
        protected CompositeCounters _counters = new CompositeCounters();
        protected DependencyResolver _dependencyResolver = new DependencyResolver(_defaultConfig);

        protected IWebHost _server;
        protected RouteBuilder _routeBuilder;
        protected string _address;

        private IList<IRegisterable> _registrations = new List<IRegisterable>();

        public virtual void SetReferences(IReferences references)
        {
            _logger.SetReferences(references);
            _counters.SetReferences(references);
            _dependencyResolver.SetReferences(references);
            _connectionResolver.SetReferences(references);
        }

        public virtual void Configure(ConfigParams config)
        {
            config = config.SetDefaults(_defaultConfig);
            _dependencyResolver.Configure(config);
            _connectionResolver.Configure(config);
        }

        protected Timing Instrument(string correlationId, string name)
        {
            _logger.Trace(correlationId, "Executing {0} method", name);
            return _counters.BeginTiming(name + ".exec_time");
        }

        public virtual bool IsOpen()
        {
            return _server != null;
        }

        public async virtual Task OpenAsync(string correlationId)
        {
            if (IsOpen()) return;

            var connection = await _connectionResolver.ResolveAsync(correlationId);

            var protocol = connection.Protocol;
            var host = connection.Host;
            var port = connection.Port;
            _address = protocol + "://" + host + ":" + port;

            try
            {
                var builder = new WebHostBuilder()
                    .UseKestrel()
                    .ConfigureServices(ConfigureServices)
                    .Configure(ConfigureApplication)
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseUrls(_address);

                _server = builder.Build();

                _logger.Info(correlationId, "Opened REST service at {0}", _address);

                await _server.StartAsync();
            }
            catch (Exception ex)
            {
                if (_server != null)
                {
                    _server.Dispose();
                    _server = null;
                }

                throw new ConnectionException(correlationId, "CANNOT_CONNECT", "Opening REST service failed")
                    .WithCause(ex).WithDetails("url", _address);
            }
        }

        public virtual Task CloseAsync(string correlationId)
        {
            if (_server != null)
            {
                // Eat exceptions
                try
                {
                    _server.Dispose();
                    _logger.Info(correlationId, "Closed REST service at {0}", _address);
                }
                catch (Exception ex)
                {
                    _logger.Warn(correlationId, "Failed while closing REST service: {0}", ex);
                }

                _server = null;
                _address = null;
            }

            return Task.Delay(0);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
            services.AddCors(cors => cors.AddPolicy("CorsPolicy", builder => {
                builder.AllowAnyHeader()
                       .AllowAnyMethod()
                       .AllowAnyOrigin();
            }));
        }

        private void ConfigureApplication(IApplicationBuilder applicationBuilder)
        {
            _routeBuilder = new RouteBuilder(applicationBuilder);

            // Delegate registering routes
            foreach (var registration in _registrations)
            {
                registration.Register();
            }

            var routes = _routeBuilder.Build();
            applicationBuilder
                .UseCors("CorsPolicy")
                .UseRouter(routes);
            _routeBuilder = null;
        }

        public void Register(IRegisterable registration)
        {
            _registrations.Add(registration);
        }

        public void Unregister(IRegisterable registration)
        {
            _registrations.Remove(registration);
        }

        public void RegisterRoute(string method, string route,
             Func<HttpRequest, HttpResponse, RouteData, Task> action)
        {
            // Routes cannot start with '/'
            if (route[0] == '/')
                route = route.Substring(1);

            if (_routeBuilder != null)
            {
                method = method.ToUpperInvariant();
                _routeBuilder.MapVerb(method, route, action);
            }
        }

    }
}
