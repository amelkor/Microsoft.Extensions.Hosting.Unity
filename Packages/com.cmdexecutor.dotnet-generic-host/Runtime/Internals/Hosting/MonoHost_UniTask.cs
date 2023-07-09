#if UNITASK_ENABLED
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Unity;
using UnityEngine;

namespace Microsoft.Extensions.Hosting
{
    internal sealed partial class MonoHost : IUnityHost
    {
        async UniTask IUnityHost.StartAsync(CancellationToken cancellationToken)
        {
            _logger.Starting();

            using var combinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _applicationLifetime.ApplicationStopping);
            CancellationToken combinedCancellationToken = combinedCancellationTokenSource.Token;

            await _hostLifetime.WaitForStartAsync(combinedCancellationToken).AsUniTask();

            combinedCancellationToken.ThrowIfCancellationRequested();
            _hostedServices = Services.GetRequiredService<IEnumerable<IHostedService>>();

            foreach (IHostedService hostedService in _hostedServices)
            {
                // Fire IHostedService.Start
                await hostedService.StartAsync(combinedCancellationToken).AsUniTask();

                if (hostedService is BackgroundService backgroundService)
                {
                    throw new NotSupportedException("Running " + nameof(BackgroundService) + " is not supported in Unity");
                }
            }

            // Fire IHostApplicationLifetime.Started
            _applicationLifetime.NotifyStarted();

            _logger.Started();
        }

        async UniTask IUnityHost.StopAsync(CancellationToken cancellationToken)
        {
            _stopCalled = true;
            _logger.Stopping();

            using (var cts = new CancellationTokenSource(_options.ShutdownTimeout))
            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken))
            {
                CancellationToken token = linkedCts.Token;
                // Trigger IHostApplicationLifetime.ApplicationStopping
                _applicationLifetime.StopApplication();

                IList<Exception> exceptions = new List<Exception>();
                if (_hostedServices != null) // Started?
                {
                    foreach (IHostedService hostedService in _hostedServices.Reverse())
                    {
                        try
                        {
                            var task = hostedService.StopAsync(token);
                            if (task != Task.CompletedTask)
                                await task.AsUniTask();
                        }
                        catch (Exception ex)
                        {
                            exceptions.Add(ex);
                        }
                    }
                }

                // Fire IHostApplicationLifetime.Stopped
                _applicationLifetime.NotifyStopped();

                try
                {
                    await _hostLifetime.StopAsync(token).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }

                if (exceptions.Count > 0)
                {
                    var ex = new AggregateException("One or more hosted services failed to stop.", exceptions);
                    _logger.StoppedWithException(ex);
                    throw ex;
                }
            }

            _logger.Stopped();
        }

        public void Dispose() => DisposeAsync().AsTask().GetAwaiter().GetResult();

        public async ValueTask DisposeAsync()
        {
            // The user didn't change the ContentRootFileProvider instance, we can dispose it
            if (ReferenceEquals(_hostEnvironment.ContentRootFileProvider, _defaultProvider))
            {
                // Dispose the content provider
                await DisposeAsync(_hostEnvironment.ContentRootFileProvider);
            }
            else
            {
                // In the rare case that the user replaced the ContentRootFileProvider, dispose it and the one
                // we originally created
                await DisposeAsync(_hostEnvironment.ContentRootFileProvider);
                await DisposeAsync(_defaultProvider);
            }

            // Dispose the service provider
            await DisposeAsync(Services);

            static async ValueTask DisposeAsync(object o)
            {
                switch (o)
                {
                    case IAsyncDisposable asyncDisposable:
                        await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                        break;
                    case IDisposable disposable:
                        disposable.Dispose();
                        break;
                }
            }
        }

        // public Task StartAsync(CancellationToken cancellationToken = new CancellationToken()) => throw new System.NotSupportedException("Use UniTask StartAsync");
        // public Task StopAsync(CancellationToken cancellationToken = new CancellationToken()) => throw new System.NotSupportedException("Use UniTask StopAsync");
        
        public Task StartAsync(CancellationToken cancellationToken = new CancellationToken()) => ((IUnityHost)this).StartAsync(cancellationToken).AsTask();
        public Task StopAsync(CancellationToken cancellationToken = new CancellationToken()) => ((IUnityHost)this).StopAsync(cancellationToken).AsTask();
    }
}
#endif