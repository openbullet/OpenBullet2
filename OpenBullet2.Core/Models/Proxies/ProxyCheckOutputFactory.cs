using RuriLib.Models.Proxies;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace OpenBullet2.Core.Models.Proxies;

/// <summary>
/// Factory that creates a <see cref="IProxyCheckOutput"/> from the <see cref="ProxyCheckOutputOptions"/>.
/// </summary>
public class ProxyCheckOutputFactory
{
    private readonly IServiceScopeFactory _scopeFactory;
    
    /// <summary></summary>
    public ProxyCheckOutputFactory(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    /// <summary>
    /// Creates a <see cref="IProxyCheckOutput"/> from the <see cref="ProxyCheckOutputOptions"/>.
    /// </summary>
    public IProxyCheckOutput FromOptions(ProxyCheckOutputOptions options)
    {
        IProxyCheckOutput output = options switch
        {
            DatabaseProxyCheckOutputOptions _ => new DatabaseProxyCheckOutput(_scopeFactory),
            _ => throw new NotImplementedException()
        };

        return output;
    }
}
