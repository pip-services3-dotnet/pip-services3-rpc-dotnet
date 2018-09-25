using System;
using System.IO;
using PipServices.Commons.Commands;
using PipServices.Commons.Run;

namespace PipServices.Rpc.Services
{
    public class CommandableHttpService : RestService
    {
        public CommandableHttpService(string baseRoute)
        {
            _baseRoute = baseRoute;
            _dependencyResolver.Put("controller", "none");
        }

        public override void Register()
        {
            var controller = _dependencyResolver.GetOneRequired<ICommandable>("controller");
            var commands = controller.GetCommandSet().Commands;

            foreach (var command in commands)
            {
                RegisterRoute("post", command.Name, async (request, response, routeData) =>
                {
                    try
                    {
                        var body = string.Empty;

                        using (var streamReader = new StreamReader(request.Body))
                        {
                            body = streamReader.ReadToEnd();
                        }

                        var parameters = string.IsNullOrEmpty(body) ? new Parameters() : Parameters.FromJson(body);
                        var correlationId = request.Query.ContainsKey("correlation_id")
                           ? request.Query["correlation_id"][0]
                           : parameters.GetAsStringWithDefault("correlation_id", string.Empty);

                        using (var timing = Instrument(correlationId, _baseRoute + '.' + command.Name))
                        {
                            var result = await command.ExecuteAsync(correlationId, parameters);
                            await SendResultAsync(response, result);
                        }
                    }
                    catch (Exception ex)
                    {
                        await SendErrorAsync(response, ex);
                    }
                });
            }
        }

    }
}