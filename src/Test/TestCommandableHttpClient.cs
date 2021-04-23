using System;
using System.Threading.Tasks;
using PipServices3.Rpc.Clients;

namespace PipServices3.Rpc.Test
{
    public class TestCommandableHttpClient : CommandableHttpClient
    {
        public TestCommandableHttpClient(string baseRoute)
            : base(baseRoute)
        { }

        /// <summary>
        /// Calls a remote method via HTTP commadable protocol. The call is made via POST
        /// operation and all parameters are sent in body object. The complete route to
        /// remote method is defined as baseRoute + "/" + name.
        /// </summary>
        /// <typeparam name="T">the class type</typeparam>
        /// <param name="route">a name of the command to call.</param>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="requestEntity">body object.</param>
        /// <returns>result of the command.</returns>
        public async new Task<T> CallCommandAsync<T>(string route, string correlationId, object requestEntity)
            where T : class
        {
            return await base.CallCommandAsync<T>(route, correlationId, requestEntity);
        }
    }
}