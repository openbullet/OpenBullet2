using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace OpenBullet2.Native.Services;

public interface IUiFactory
{
    T Create<T>(params object[] args) where T : class;
}

public class UiFactory(IServiceProvider serviceProvider, ILogger<UiFactory> logger) : IUiFactory
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<UiFactory> _logger = logger;

    public T Create<T>(params object[] args) where T : class
    {
        _logger.LogDebug("Resolving UI type {UiType} with {ArgumentCount} runtime argument(s)",
            typeof(T).FullName, args.Length);
        return ActivatorUtilities.CreateInstance<T>(_serviceProvider, args);
    }
}
