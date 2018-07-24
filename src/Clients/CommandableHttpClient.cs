using System.Net.Http;
using System.Threading.Tasks;

namespace PipServices.Rpc.Clients
{
    /// <summary>
    /// Commandable HTTP Client
    /// </summary>
    /// <seealso cref="PipServices.Rpc.Rest.RestClient" />
    public class CommandableHttpClient : RestClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandableHttpClient"/> class.
        /// </summary>
        /// <param name="baseRoute">The base route.</param>
        public CommandableHttpClient(string baseRoute)
        {
            _baseRoute = baseRoute;
        }

        /// <summary>
        /// Calls the command.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="route">The command route.</param>
        /// <param name="correlationId">The correlation identifier.</param>
        /// <param name="requestEntity">The request entity.</param>
        public Task<T> CallCommandAsync<T>(string route, string correlationId, object requestEntity)
            where T : class
        {
            using (var timing = Instrument(correlationId, _baseRoute + "." + route))
            {
                return ExecuteAsync<T>(correlationId, HttpMethod.Post, route, requestEntity);
            }
        }
    }
}