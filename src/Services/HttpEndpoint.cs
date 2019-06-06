using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using PipServices3.Commons.Config;
using PipServices3.Commons.Errors;
using PipServices3.Commons.Refer;
using PipServices3.Commons.Run;
using PipServices3.Components.Count;
using PipServices3.Components.Log;
using PipServices3.Rpc.Connect;

namespace PipServices3.Rpc.Services
{
    /// <summary>
    /// Used for creating HTTP endpoints. An endpoint is a URL, at which a given service can be accessed by a client. 
    /// 
    /// ### Configuration parameters ###
    /// 
    /// Parameters to pass to the <c>Configure()</c> method for component configuration:
    /// 
    /// connection(s) - the connection resolver's connections;
    /// - "connection.discovery_key" - the key to use for connection resolving in a discovery service;
    /// - "connection.protocol" - the connection's protocol;
    /// - "connection.host" - the target host;
    /// - "connection.port" - the target port;
    /// - "connection.uri" - the target URI.
    ///
    /// credential - the HTTPS credentials:
    /// - "credential.ssl_key_file" - the SSL private key in PEM
    /// - "credential.ssl_crt_file" - the SSL certificate in PEM
    /// - "credential.ssl_ca_file" - the certificate authorities (root cerfiticates) in PEM
    /// 
    /// ### References ###
    /// 
    /// A logger, counters, and a connection resolver can be referenced by passing the
    /// following references to the object's setReferences() method:
    /// 
    /// - logger: <code>"*:logger:*:*:1.0"</code>;
    /// - counters: <code>"*:counters:*:*:1.0"</code>;
    /// - discovery: <code>"*:discovery:*:*:1.0"</code> (for the connection resolver).
    /// </summary>
    /// <example>
    /// <code>
    /// public MyMethod(string correlationId, ConfigParams _config, IReferences _references) 
    /// {
    ///     var endpoint = new HttpEndpoint();
    ///     if (this._config)
    ///         endpoint.Configure(this._config);
    ///     if (this._references)
    ///         endpoint.SetReferences(this._references);
    ///     ...
    ///     this._endpoint.Open(correlationId);
    ///     ...
    /// }
    /// </code>
    /// </example>
    public class HttpEndpoint : IOpenable, IConfigurable, IReferenceable
    {
        private static readonly ConfigParams _defaultConfig = ConfigParams.FromTuples(
            "connection.protocol", "http",
            "connection.host", "0.0.0.0",
            "connection.port", 3000,
            "credential.ssl_key_file", null,
            "credential.ssl_crt_file", null,
            "credential.ssl_ca_file", null,
            "options.maintenance_enabled", false,
            "options.request_max_size", 1024 * 1024,
            "options.file_max_size", 200 * 1024 * 1024,
            "options.connect_timeout", 60000,
            "options.debug", true
        );

        protected HttpConnectionResolver _connectionResolver = new HttpConnectionResolver();
        protected CompositeLogger _logger = new CompositeLogger();
        protected CompositeCounters _counters = new CompositeCounters();
        protected DependencyResolver _dependencyResolver = new DependencyResolver(_defaultConfig);

        private bool _maintenanceEnabled;
        private long _fileMaxSize = 200 * 1024 * 1024;

        protected IWebHost _server;
        protected RouteBuilder _routeBuilder;
        protected string _address;

        private IList<IRegisterable> _registrations = new List<IRegisterable>();
        private List<Interceptor> _interceptors = new List<Interceptor>();

        /// <summary>
        /// Sets references to this endpoint's logger, counters, and connection resolver.
        /// 
        /// __References:__ - logger: <code>"\*:logger:\*:\*:1.0"</code> - counters:
        /// <code>"\*:counters:\*:\*:1.0"</code> - discovery:
        /// <code>"\*:discovery:\*:\*:1.0"</code> (for the connection resolver)
        /// </summary>
        /// <param name="references">an IReferences object, containing references to a logger, 
        /// counters, and a connection resolver.</param>
        public virtual void SetReferences(IReferences references)
        {
            _logger.SetReferences(references);
            _counters.SetReferences(references);
            _dependencyResolver.SetReferences(references);
            _connectionResolver.SetReferences(references);
        }

        /// <summary>
        /// Configures this HttpEndpoint using the given configuration parameters.
        /// 
        /// __Configuration parameters:__ - __connection(s)__ - the connection resolver's
        /// connections; - "connection.discovery_key" - the key to use for connection
        /// resolving in a discovery service; - "connection.protocol" - the connection's
        /// protocol; - "connection.host" - the target host; - "connection.port" - the
        /// target port; - "connection.uri" - the target URI.
        /// </summary>
        /// <param name="config">configuration parameters, containing a "connection(s)" section.</param>
        /// See <see cref="ConfigParams"/>
        public virtual void Configure(ConfigParams config)
        {
            config = config.SetDefaults(_defaultConfig);
            _dependencyResolver.Configure(config);
            _connectionResolver.Configure(config);

            _maintenanceEnabled = config.GetAsBooleanWithDefault("options.maintenance_enabled", _maintenanceEnabled);
            _fileMaxSize = config.GetAsLongWithDefault("options.file_max_size", _fileMaxSize);
        }

        /// <summary>
        /// Adds instrumentation to log calls and measure call time. It returns a Timing 
        /// object that is used to end the time measurement.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="name">a method name.</param>
        /// <returns>Timing object to end the time measurement.</returns>
        protected Timing Instrument(string correlationId, string name)
        {
            _logger.Trace(correlationId, "Executing {0} method", name);
            return _counters.BeginTiming(name + ".exec_time");
        }

        /// <summary>
        /// Checks if the component is opened.
        /// </summary>
        /// <returns>whether or not this endpoint is open with an actively listening REST server.</returns>
        public virtual bool IsOpen()
        {
            return _server != null;
        }

        /// <summary>
        /// Opens a connection using the parameters resolved by the referenced connection
        /// resolver and creates a REST server(service) using the set options and parameters.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        public virtual async Task OpenAsync(string correlationId)
        {
            if (IsOpen()) return;

            var connection = await _connectionResolver.ResolveAsync(correlationId);
            var credential = connection.GetSection("credential");

            var protocol = connection.Protocol;
            var host = connection.Host;
            var port = connection.Port;
            _address = protocol + "://" + host + ":" + port;

            try
            {
                var builder = new WebHostBuilder()
                    .UseKestrel(options =>
                    {
                        // Convert localhost to IP Address
                        if (host == "localhost")
                        {
                            host = IPAddress.Loopback.ToString();
                        }
                        
                        if (protocol == "https")
                        {
                            var sslPfxFile = credential.GetAsNullableString("ssl_pfx_file");
                            var sslPassword = credential.GetAsNullableString("ssl_pfx_file");
                            
                            options.Listen(IPAddress.Parse(host), port, listenOptions =>
                            {
                                listenOptions.UseHttps(sslPfxFile, sslPassword);
                            });
                        }
                        else
                        {
                            options.Listen(IPAddress.Parse(host), port);
                        }
                    })
                    .ConfigureServices(ConfigureServices)
                    .Configure(ConfigureApplication)
                    .UseContentRoot(Directory.GetCurrentDirectory());

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

        /// <summary>
        /// Closes this endpoint and the REST server (service) that was opened earlier.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
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
            services.AddCors(cors => cors.AddPolicy("CorsPolicy", builder =>
            {
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

        /// <summary>
        /// Registers a registerable object for dynamic endpoint discovery.
        /// </summary>
        /// <param name="registration">the registration to add.</param>
        public void Register(IRegisterable registration)
        {
            _registrations.Add(registration);
        }

        /// <summary>
        /// Unregisters a registerable object, so that it is no longer used in dynamic endpoint discovery.
        /// </summary>
        /// <param name="registration">the registration to remove.</param>
        public void Unregister(IRegisterable registration)
        {
            _registrations.Remove(registration);
        }

        /// <summary>
        /// Registers an action in this objects REST server (service) by the given method and route.
        /// </summary>
        /// <param name="method">the HTTP method of the route.</param>
        /// <param name="route">the route to register in this object's REST server (service).</param>
        /// <param name="action">the action to perform at the given route.</param>
        public void RegisterRoute(string method, string route,
            Func<HttpRequest, HttpResponse, RouteData, Task> action)
        {
            route = FixRoute(route);

            if (_routeBuilder != null)
            {
                method = method.ToUpperInvariant();
                _routeBuilder.MapVerb(method, route, context =>
                {
                    AppendAdditionalParametersFromQuery(route, context.Request);
                    
                    var interceptor = _interceptors.Find(i => route.StartsWith(i.Route));
                    if (interceptor != null)
                    {
                        var nextAction = new Func<HttpRequest, HttpResponse, ClaimsPrincipal, RouteData, Task>(
                            async (request, response, user, routeData) =>
                            {
                                await action(request, response, routeData);
                            });

                        return interceptor.Action.Invoke(context.Request, context.Response, null,
                            context.GetRouteData(), nextAction);
                    }

                    return action.Invoke(context.Request, context.Response, context.GetRouteData());
                });
            }
        }

        private void AppendAdditionalParametersFromQuery(string route, HttpRequest request)
        {
            if (route.Contains("{") && route.Contains("}"))
            {
                var splitRoute = route.Split('/');
                var splitPath = request.Path.Value.Split('/').Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToArray();

                for (var i = 0; i < splitRoute.Length; i++)
                {
                    if (splitPath.Length - 1 < i) break;
                    
                    var r = splitRoute[i];
                    var p = splitPath[i];
                    if (r.StartsWith("{") && r.EndsWith("}"))
                    {
                        var key = r.Substring(1).Substring(0, r.Length - 2);
                        var value = p;
                        
                        request.Headers.Add(key, value);
                    }
                }
            }
        }

        private string FixRoute(string route)
        {
            // Routes cannot start with '/'
            if (!string.IsNullOrEmpty(route) && route[0] == '/')
                route = route.Substring(1);

            return route;
        }

        public void RegisterRouteWithAuth(string method, string route,
            Func<HttpRequest, HttpResponse, ClaimsPrincipal, RouteData,
                Func<Task>, Task> authorize,
            Func<HttpRequest, HttpResponse, ClaimsPrincipal, RouteData, Task> action)
        {
            route = FixRoute(route);
            
            if (authorize != null)
            {
                var nextAction = action;

                action = (request, response, user, routeData) =>
                {
                    return authorize(request, response, user, routeData,
                        async () => await nextAction(request, response, user, routeData));
                };
            }

            if (_routeBuilder != null)
            {
                method = method.ToUpperInvariant();
                _routeBuilder.MapVerb(method, route, context =>
                {
                    AppendAdditionalParametersFromQuery(route, context.Request);
                    
                    var interceptor = _interceptors.Find(i => route.StartsWith(i.Route));
                    if (interceptor != null)
                    {
                        return interceptor.Action.Invoke(context.Request, context.Response, context.User,
                            context.GetRouteData(),
                            action);
                    }

                    return action.Invoke(context.Request, context.Response, context.User, context.GetRouteData());
                });
            }
        }

        public void RegisterInterceptor(string route,
            Func<HttpRequest, HttpResponse, ClaimsPrincipal, RouteData,
                Func<HttpRequest, HttpResponse, ClaimsPrincipal, RouteData, Task>, Task> action)
        {
            route = FixRoute(route);
            _interceptors.Add(new Interceptor() {Action = action, Route = route});
        }
    }
}