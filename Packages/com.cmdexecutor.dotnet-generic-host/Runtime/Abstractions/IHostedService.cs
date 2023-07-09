using System.Threading;
using Cysharp.Threading.Tasks;

namespace Microsoft.Extensions.Hosting.Unity
{
    /// <summary>
    /// Defines methods for objects that are managed by the Unity host.
    /// </summary>
    public interface IUnityHostedService
    {
        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        UniTask StartAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        UniTask StopAsync(CancellationToken cancellationToken);
    }
}