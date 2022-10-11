using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace PipServices3.Rpc.Services
{
    public interface IInitializable
    {
        void ConfigureServices(IServiceCollection services);
        void ConfigureApplication(IApplicationBuilder applicationBuilder);
    }
}
