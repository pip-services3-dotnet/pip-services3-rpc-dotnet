using System;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using PipServices3.Commons.Config;
using PipServices3.Commons.Data;
using PipServices3.Commons.Errors;
using PipServices3.Commons.Refer;
using PipServices3.Components.Count;
using PipServices3.Components.Log;

namespace PipServices3.Rpc.Services
{
    public class RestOperations : IConfigurable, IReferenceable
    {
        /// <summary>
        /// The logger.
        /// </summary>
        protected CompositeLogger _logger = new CompositeLogger();

        /// <summary>
        /// The performance counters.
        /// </summary>
        protected CompositeCounters _counters = new CompositeCounters();

        /// <summary>
        /// The dependency resolver.
        /// </summary>
        protected DependencyResolver _dependencyResolver = new DependencyResolver();

        public virtual void Configure(ConfigParams config)
        {
            _dependencyResolver.Configure(config);
        }

        public virtual void SetReferences(IReferences references)
        {
            _logger.SetReferences(references);
            _counters.SetReferences(references);
            _dependencyResolver.SetReferences(references);
        }

        protected string GetCorrelationId(HttpRequest request)
        {
            return HttpRequestHelper.GetCorrelationId(request);
        }

        public static string GetId(HttpRequest request)
        {
            return HttpRequestHelper.ExtractFromQuery("id", request);
        }

        protected FilterParams GetFilterParams(HttpRequest request)
        {
            return HttpRequestHelper.GetFilterParams(request);
        }

        protected PagingParams GetPagingParams(HttpRequest request)
        {
            return HttpRequestHelper.GetPagingParams(request);
        }

        protected static ProjectionParams GetProjectionParams(HttpRequest request)
        {
            return ProjectionParams.FromValues(HttpRequestHelper.ExtractFromQuery("projection", request));
        }

        protected RestOperationParameters GetParameters(HttpRequest request)
        {
            return HttpRequestHelper.GetParameters(request);
        }

        protected SortParams GetSortParams(HttpRequest request)
        {
            return HttpRequestHelper.GetSortParams(request);
        }

        public static T GetContextItem<T>(HttpRequest request, string name)
            where T : class
        {
            return HttpRequestHelper.GetContextItem<T>(request, name);
        }

        protected async Task SendResultAsync(HttpResponse response, object result)
        {
            await HttpResponseSender.SendResultAsync(response, result);
        }

        protected async Task SendEmptyResultAsync(HttpResponse response)
        {
            await HttpResponseSender.SendEmptyResultAsync(response);
        }

        protected async Task SendCreatedResultAsync(HttpResponse response, object result)
        {
            await HttpResponseSender.SendCreatedResultAsync(response, result);
        }

        protected async Task SendDeletedResultAsync(HttpResponse response, object result)
        {
            await HttpResponseSender.SendDeletedResultAsync(response, result);
        }

        protected async Task SendErrorAsync(HttpResponse response, Exception error)
        {
            await HttpResponseSender.SendErrorAsync(response, error);
        }

        protected async Task SendBadRequestAsync(HttpRequest request, HttpResponse response, string message)
        {
            var correlationId = GetCorrelationId(request);
            var error = new BadRequestException(correlationId, "BAD_REQUEST", message)
            {
                Status = StatusCodes.Status400BadRequest
            };
            await SendErrorAsync(response, error);
        }

        protected async Task SendUnauthorizedAsync(HttpRequest request, HttpResponse response, string message)
        {
            var correlationId = GetCorrelationId(request);
            var error = new BadRequestException(correlationId, "UNAUTHORIZED", message)
            {
                Status = StatusCodes.Status401Unauthorized
            };
            await SendErrorAsync(response, error);
        }

        protected async Task SendNotFoundAsync(HttpRequest request, HttpResponse response, string message)
        {
            var correlationId = GetCorrelationId(request);
            var error = new BadRequestException(correlationId, "NOT_FOUND", message)
            {
                Status = StatusCodes.Status404NotFound
            };
            await SendErrorAsync(response, error);
        }

        protected async Task SendConflictAsync(HttpRequest request, HttpResponse response, string message)
        {
            var correlationId = GetCorrelationId(request);
            var error = new BadRequestException(correlationId, "CONFLICT", message)
            {
                Status = StatusCodes.Status409Conflict
            };
            await SendErrorAsync(response, error);
        }

        protected async Task SendSessionExpiredASync(HttpRequest request, HttpResponse response, string message)
        {
            var correlationId = GetCorrelationId(request);
            var error = new BadRequestException(correlationId, "SESSION_EXPIRED", message) {Status = 440};
            await SendErrorAsync(response, error);
        }

        protected async Task SendInternalErrorAsync(HttpRequest request, HttpResponse response, string message)
        {
            var correlationId = GetCorrelationId(request);
            var error = new BadRequestException(correlationId, "INTERNAL", message)
            {
                Status = StatusCodes.Status500InternalServerError
            };
            await SendErrorAsync(response, error);
        }

        protected async Task SendServerUnavailableAsync(HttpRequest request, HttpResponse response, string message)
        {
            var correlationId = GetCorrelationId(request);
            var error = new BadRequestException(correlationId, "SERVER_UNAVAILABLE", message)
            {
                Status = StatusCodes.Status503ServiceUnavailable
            };
            await SendErrorAsync(response, error);
        }

        public async Task InvokeAsync(string operation, object[] parameters)
        {
            Type t = GetType();
            MethodInfo method = t.GetMethod(operation);

            if (method != null) await Task.FromResult(method.Invoke(this, parameters));
        }

        public async Task<dynamic> InvokeWithResponseAsync(string operation, object[] parameters)
        {
            Type t = GetType();
            MethodInfo method = t.GetMethod(operation);

            if (method != null) return await Task.FromResult(method.Invoke(this, parameters));
            else return null;
        }

        protected virtual void HandleError(string correlationId, string methodName, Exception ex)
        {
            _logger.Error(correlationId, ex, $"Failed to execute {methodName}");
        }

        protected virtual CounterTiming Instrument(string correlationId, string methodName, string message = "")
        {
            _logger.Trace(correlationId, $"Executed {methodName} {message}");
            return _counters.BeginTiming(methodName + ".exec_time");
        }

        protected virtual async Task SafeInvokeAsync(string methodName, HttpRequest request, HttpResponse response, Func<string, Task> invokeFunc)
        {
            var correlationId = GetCorrelationId(request);
            using (var timing = Instrument(correlationId, methodName))
            {
                try
                {
                    await invokeFunc(correlationId);
                }
                catch (BadRequestException e)
                {
                    HandleError(correlationId, methodName, e);

                    await SendBadRequestAsync(request, response, $"Incorrect body: {e.Message}");
                }
                catch (Exception ex)
                {
                    HandleError(correlationId, methodName, ex);

                    await SendErrorAsync(response, ex);
                }
            }
        }
    }
}