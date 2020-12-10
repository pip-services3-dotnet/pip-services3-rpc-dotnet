using Microsoft.AspNetCore.Builder;
using System.Collections.Generic;

namespace PipServices3.Rpc.Services
{
    /// <summary>
    /// Interface to perform Swagger registrations.
    /// </summary>
    public interface ISwaggerService
    {
        /// <summary>
        /// Perform required Swagger registration steps.
        /// </summary>
        void RegisterOpenApiSpec(string baseRoute, string content);

        /// <summary>
        /// Configure application to use Swagger UI
        /// </summary>
        /// <param name="applicationBuilder">application builder</param>
        /// <param name="routes">list of routes to swagger documents</param>
        void ConfigureApplication(IApplicationBuilder applicationBuilder, List<string> routes);
    }
}