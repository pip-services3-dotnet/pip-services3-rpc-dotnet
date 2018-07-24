using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using PipServices.Commons.Config;
using PipServices.Commons.Convert;
using PipServices.Commons.Data;
using PipServices.Commons.Errors;
using PipServices.Commons.Refer;
using PipServices.Commons.Run;
using PipServices.Components.Count;
using PipServices.Components.Log;
using PipServices.Rpc.Connect;

namespace PipServices.Rpc.Clients
{
    public class RestClient : IOpenable, IConfigurable, IReferenceable
    {
        private static readonly ConfigParams _defaultConfig = ConfigParams.FromTuples(
            "connection.protocol", "http",
            //"connection.host", "localhost",
            //"connection.port", 3000,

            "options.request_max_size", 1024*1024,
            "options.connect_timeout", 60000,
            "options.retries", 1,
            "options.debug", true
        );

        protected HttpConnectionResolver _connectionResolver = new HttpConnectionResolver();
        protected CompositeLogger _logger = new CompositeLogger();
        protected CompositeCounters _counters = new CompositeCounters();
        protected ConfigParams _options = new ConfigParams();
        protected string _baseRoute;
        protected int _retries = 1;

        protected HttpClient _client;
        protected string _address;

        public virtual void Configure(ConfigParams config)
        {
            config = config.SetDefaults(_defaultConfig);
            _connectionResolver.Configure(config);
            _options = _options.Override(config.GetSection("options"));

            _retries = config.GetAsIntegerWithDefault("options.retries", _retries);

            _baseRoute = config.GetAsStringWithDefault("base_route", _baseRoute);
        }

        public virtual void SetReferences(IReferences references)
        {
            _connectionResolver.SetReferences(references);
            _logger.SetReferences(references);
            _counters.SetReferences(references);
        }

        protected Timing Instrument(string correlationId, [CallerMemberName]string methodName = null)
        {
            var typeName = GetType().Name;
            _logger.Trace(correlationId, "Calling {0} method of {1}", methodName, typeName);
            return _counters.BeginTiming(typeName + "." + methodName + ".call_time");
        }

        public virtual bool IsOpened()
        {
            return _client != null;
        }

        public async virtual Task OpenAsync(string correlationId)
        {
            var connection = await _connectionResolver.ResolveAsync(correlationId);

            var protocol = connection.Protocol;
            var host = connection.Host;
            var port = connection.Port;

            _address = protocol + "://" + host + ":" + port;

            _client?.Dispose();

            _client = new HttpClient(new HttpClientHandler
            {
                CookieContainer = new CookieContainer(),
                AllowAutoRedirect = true,
                UseCookies = true
            });

            _client.DefaultRequestHeaders.ConnectionClose = true;

            _logger.Debug(correlationId, "Connected via REST to {0}", _address);
        }

        public virtual Task CloseAsync(string correlationId)
        {
            _client?.Dispose();
            _client = null;

            _address = null;

            _logger.Debug(correlationId, "Disconnected from {0}", _address);

            return Task.CompletedTask;
        }

        private static HttpContent CreateEntityContent(object value)
        {
            if (value == null) return null;

            var content = JsonConverter.ToJson(value);
            var result = new StringContent(content, Encoding.UTF8, "application/json");
            return result;
        }

        private Uri CreateRequestUri(string route)
        {
            var builder = new StringBuilder(_address);

            if (!string.IsNullOrEmpty(_baseRoute))
            {
                if (_baseRoute[0] != '/')
                    builder.Append('/');
                builder.Append(_baseRoute);
            }

            if (route[0] != '/')
                builder.Append('/');
            builder.Append(route);

            var uri = builder.ToString();

            var result = new Uri(uri, UriKind.Absolute);

            return result;
        }

        private static string ConstructQueryString(NameValueCollection parameters)
        {
            StringBuilder builder = new StringBuilder();

            foreach (string name in parameters)
            {
                if (builder.Length > 0)
                    builder.Append('&');
                builder.Append(name);
                builder.Append('=');
                builder.Append(System.Web.HttpUtility.UrlEncode(parameters[name]));
            }

            return builder.ToString();
        }

        protected string AddCorrelationId(string route, string correlationId)
        {
            var pos = route.IndexOf('?');
            var path = pos >= 0 ? route.Substring(0, pos) : route;
            var query = pos >= 0 ? route.Substring(pos) : "";

            var parameters = HttpUtility.ParseQueryString(query);
            parameters["correlation_id"] = correlationId;
            query = ConstructQueryString(parameters);
            return path + "?" + query;
        }

        protected string AddFilterParams(string route, FilterParams filter)
        {
            var pos = route.IndexOf('?');
            var path = pos >= 0 ? route.Substring(0, pos) : route;
            var query = pos >= 0 ? route.Substring(pos) : "";

            var parameters = HttpUtility.ParseQueryString(query);

            foreach (var key in filter.Keys)
            {
                parameters[key] = filter[key];
            }

            query = ConstructQueryString(parameters);
            return path + "?" + query;
        }

        protected string AddPagingParams(string route, PagingParams paging)
        {
            var pos = route.IndexOf('?');
            var path = pos >= 0 ? route.Substring(0, pos) : route;
            var query = pos >= 0 ? route.Substring(pos) : "";

            var parameters = HttpUtility.ParseQueryString(query);

            if (paging.Skip.HasValue)
                parameters["skip"] = paging.Skip.Value.ToString();
            if (paging.Take.HasValue)
                parameters["take"] = paging.Take.Value.ToString();
            if (paging.Total)
                parameters["total"] = StringConverter.ToString(paging.Take);
 
            query = ConstructQueryString(parameters);
            return path + "?" + query;
        }

        private async Task<HttpResponseMessage> ExecuteRequestAsync(
            string correlationId, HttpMethod method, Uri uri, HttpContent content = null)
        {
            if (_client == null)
                throw new InvalidOperationException("REST client is not configured");

            HttpResponseMessage result = null;

            var retries = Math.Min(1, Math.Max(5, _retries));
            while (retries > 0)
            {
                try
                {
                    if (method == HttpMethod.Get)
                        result = await _client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
                    else if (method == HttpMethod.Post)
                        result = await _client.PostAsync(uri, content);
                    else if (method == HttpMethod.Put)
                        result = await _client.PutAsync(uri, content);
                    else if (method == HttpMethod.Delete)
                        result = await _client.DeleteAsync(uri);
                    else
                        throw new InvalidOperationException("Invalid request type");

                    retries = 0;
                }
                catch (HttpRequestException ex)
                {
                    retries--;
                    if (retries > 0)
                    {
                        throw new ConnectionException(correlationId, null, "Unknown communication problem on REST client", ex);
                    }
                    else
                    {
                        _logger.Trace(correlationId, $"Connection failed to uri '{uri}'. Retrying...");
                    }
                }
            }

            if (result == null)
            {
                throw ApplicationExceptionFactory.Create(ErrorDescriptionFactory.Create(
                    new UnknownException(correlationId, $"Unable to get a result from uri '{uri}' with method '{method}'")));
            }

            if ((int)result.StatusCode >= 400)
            {
                var responseContent = await result.Content.ReadAsStringAsync();

                ErrorDescription errorObject = null;
                try
                {
                    errorObject = JsonConverter.FromJson<ErrorDescription>(responseContent);
                }
                finally
                {
                    if (errorObject == null)
                    {
                        errorObject = ErrorDescriptionFactory.Create(new UnknownException(correlationId, $"UNKNOWN_ERROR with result status: '{result.StatusCode}'", responseContent));
                    }
                }

                throw ApplicationExceptionFactory.Create(errorObject);
            }

            return result;
        }

        protected async Task ExecuteAsync(string correlationId, HttpMethod method, string route)
        {
            route = AddCorrelationId(route, correlationId);
            var uri = CreateRequestUri(route);

            await ExecuteRequestAsync(correlationId, method, uri);
        }

        protected async Task ExecuteAsync(string correlationId, HttpMethod method, string route, object requestEntity)
        {
            route = AddCorrelationId(route, correlationId);
            var uri = CreateRequestUri(route);

            using (var requestContent = CreateEntityContent(requestEntity))
            {
                await ExecuteRequestAsync(correlationId, method, uri, requestContent);
            }
        }

        private static async Task<T> ExtractContentEntityAsync<T>(string correlationId, HttpContent content)
        {
            var value = await content.ReadAsStringAsync();

            try
            {
                return JsonConverter.FromJson<T>(value);
            }
            catch (Exception ex)
            {
                throw new BadRequestException(correlationId, null, "Unexpected protocol format", ex);
            }
        }

        protected async Task<T> ExecuteAsync<T>(string correlationId, HttpMethod method, string route)
            where T : class
        {
            route = AddCorrelationId(route, correlationId);
            var uri = CreateRequestUri(route);

            using (var response = await ExecuteRequestAsync(correlationId, method, uri))
            {
                return await ExtractContentEntityAsync<T>(correlationId, response.Content);
            }
        }

        private static async Task<string> ExtractContentEntityAsync(string correlationId, HttpContent content)
        {
            var value = await content.ReadAsStringAsync();

            try
            {
                return value;
            }
            catch (Exception ex)
            {
                throw new BadRequestException(correlationId, null, "Unexpected protocol format", ex);
            }
        }

        protected async Task<string> ExecuteStringAsync(
            string correlationId, HttpMethod method, string route)
        {
            route = AddCorrelationId(route, correlationId);
            var uri = CreateRequestUri(route);

            using (var response = await ExecuteRequestAsync(correlationId, method, uri))
            {
                return await ExtractContentEntityAsync(correlationId, response.Content);
            }
        }

        protected async Task<T> ExecuteAsync<T>(
            string correlationId, HttpMethod method, string route, object requestEntity)
            where T : class
        {
            route = AddCorrelationId(route, correlationId);
            var uri = CreateRequestUri(route);

            using (var requestContent = CreateEntityContent(requestEntity))
            {
                using (var response = await ExecuteRequestAsync(correlationId, method, uri, requestContent))
                {
                    return await ExtractContentEntityAsync<T>(correlationId, response.Content);
                }
            }
        }
    }
}

