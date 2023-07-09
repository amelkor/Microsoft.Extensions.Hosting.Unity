using System;
using System.Collections.Generic;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Hosting
{
    internal sealed partial class MonoHost : IAsyncDisposable
    {
        private readonly ILogger<MonoHost> _logger;
        private readonly IHostLifetime _hostLifetime;
        private readonly ApplicationLifetime _applicationLifetime;
        private readonly HostOptions _options;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly PhysicalFileProvider _defaultProvider;
        private IEnumerable<IHostedService>? _hostedServices;
        private volatile bool _stopCalled;

        public MonoHost(IServiceProvider services,
            IHostEnvironment hostEnvironment,
            PhysicalFileProvider defaultProvider,
            IHostApplicationLifetime applicationLifetime,
            ILogger<MonoHost> logger,
            IHostLifetime hostLifetime,
            IOptions<HostOptions> options)
        {
            if (applicationLifetime == null) throw new ArgumentNullException(nameof(applicationLifetime));

            Services = services ?? throw new ArgumentNullException(nameof(services));
            _applicationLifetime = (applicationLifetime as ApplicationLifetime)!;
            _hostEnvironment = hostEnvironment;
            _defaultProvider = defaultProvider;

            if (_applicationLifetime is null)
            {
                throw new ArgumentException("Replacing IHostApplicationLifetime is not supported.", nameof(applicationLifetime));
            }

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hostLifetime = hostLifetime ?? throw new ArgumentNullException(nameof(hostLifetime));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public IServiceProvider Services { get; }
    }
}