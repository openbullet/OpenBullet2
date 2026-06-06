using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core;
using OpenBullet2.Core.Repositories;
using System.Reflection;

namespace OpenBullet2.Native.Tests;

[Collection("WPF")]
public sealed class UiFactoryArchitectureTests
{
    [Fact]
    public void NativeViewsAndViewModels_ShouldNotInjectDbContextOrRepositories()
    {
        var uiAssembly = typeof(OpenBullet2.Native.App).Assembly;

        var violations = uiAssembly
            .GetTypes()
            .Where(IsUiType)
            .SelectMany(type => type
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .SelectMany(ctor => ctor
                    .GetParameters()
                    .Where(parameter => IsForbidden(parameter.ParameterType))
                    .Select(parameter =>
                        $"{type.FullName} -> {ctor.DeclaringType?.Name}({string.Join(", ", ctor.GetParameters().Select(p => p.ParameterType.Name))}) injects {parameter.ParameterType.FullName}")))
            .OrderBy(message => message)
            .ToList();

        Assert.True(violations.Count == 0,
            "UI types created from the root provider must not inject DbContext or EF-backed repositories."
            + $"{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
    }

    private static bool IsUiType(Type type)
        => type is { IsClass: true, IsAbstract: false }
           && type.Namespace is not null
           && (type.Namespace.StartsWith("OpenBullet2.Native.Views", StringComparison.Ordinal)
               || type.Namespace.StartsWith("OpenBullet2.Native.ViewModels", StringComparison.Ordinal));

    private static bool IsForbidden(Type type)
        => typeof(DbContext).IsAssignableFrom(type)
           || ImplementsOpenGeneric(type, typeof(OpenBullet2.Core.Repositories.IRepository<>))
           || type == typeof(IWordlistRepository);

    private static bool ImplementsOpenGeneric(Type candidateType, Type openGenericType)
        => candidateType.GetInterfaces()
            .Concat([candidateType])
            .Any(type => type.IsGenericType
                         && type.GetGenericTypeDefinition() == openGenericType);
}
