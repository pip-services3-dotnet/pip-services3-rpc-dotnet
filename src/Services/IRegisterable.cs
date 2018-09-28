using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace PipServices.Rpc.Services
{
    /// <summary>
    /// Interface to perform on-demand registrations.
    /// </summary>
    public interface IRegisterable
    {
        /// <summary>
        /// Perform required registration steps.
        /// </summary>
        void Register();
    }
}
