using System;
using System.Net.Http;
using System.Threading.Tasks;
using PipServices3.Rpc.Clients;

namespace PipServices3.Rpc.Test
{
    public class TestRestClient: RestClient
    {
        public TestRestClient(string baseRoute)
        {
            this._baseRoute = baseRoute;
        }

        /// <summary>
        /// Calls a remote method via HTTP/REST protocol.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="method">HTTP method: "get", "head", "post", "put", "delete"</param>
        /// <param name="route">a command route. Base route will be added to this route</param>
        public async new Task CallAsync(string correlationId, HttpMethod method, string route)
        {
            await base.CallAsync(correlationId, method, route);
        }

        /// <summary>
        /// Calls a remote method via HTTP/REST protocol.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="method">HTTP method: "get", "head", "post", "put", "delete"</param>
        /// <param name="route">a command route. Base route will be added to this route</param>
        /// <param name="requestEntity">request body object.</param>
        public async new Task CallAsync(string correlationId, HttpMethod method, string route, object requestEntity)
        {
            await base.CallAsync(correlationId, method, route, requestEntity);
        }

        /// <summary>
        /// Calls a remote method via HTTP/REST protocol.
        /// </summary>
        /// <typeparam name="T">the class type</typeparam>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="method">HTTP method: "get", "head", "post", "put", "delete"</param>
        /// <param name="route">a command route. Base route will be added to this route</param>
        /// <returns>result object.</returns>
        public async new Task<T> CallAsync<T>(string correlationId, HttpMethod method, string route)
            where T : class
        {
            return await base.CallAsync<T>(correlationId, method, route);
        }

        public async new Task<string> CallStringAsync(string correlationId, HttpMethod method, string route)
        {
            return await base.CallStringAsync(correlationId, method, route);
        }

        public async new Task<string> CallStringAsync(string correlationId, HttpMethod method, string route, object requestEntity)
        {
            return await base.CallStringAsync(correlationId, method, route);
        }

        /// <summary>
        /// Calls a remote method via HTTP/REST protocol.
        /// </summary>
        /// <typeparam name="T">the class type</typeparam>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="method">HTTP method: "get", "head", "post", "put", "delete"</param>
        /// <param name="route">a command route. Base route will be added to this route</param>
        /// <param name="requestEntity">request body object.</param>
        /// <returns>result object.</returns>
        public async new Task<T> CallAsync<T>(string correlationId, HttpMethod method, string route, object requestEntity)
            where T : class
        {
            return await base.CallAsync<T>(correlationId, method, route, requestEntity);
        }

        /// <summary>
        /// Safely calls a remote method via HTTP/REST protocol and logs execution time.
        /// </summary>
        /// <typeparam name="T">the class type</typeparam>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="method">HTTP method: "post", "put", "patch"</param>
        /// <param name="route">a command route. Base route will be added to this route</param>
        /// <param name="requestEntity">request body object.</param>
        /// <returns>result object.</returns>
        public async new Task<T> SafeCallAsync<T>(string correlationId, HttpMethod method, string route, object requestEntity)
            where T : class
        {
            return await base.SafeCallAsync<T>(correlationId, method, route, requestEntity);
        }

        /// <summary>
        /// Safely calls a remote method via HTTP/REST protocol and logs execution time.
        /// </summary>
        /// <typeparam name="T">the class type</typeparam>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="method">HTTP method: "get", "delete"</param>
        /// <param name="route">a command route. Base route will be added to this route</param>
        /// <returns>result object.</returns>
        public async new Task<T> SafeCallAsync<T>(string correlationId, HttpMethod method, string route)
            where T : class
        {
            return await base.SafeCallAsync<T>(correlationId, method, route);
        }
    }
}
