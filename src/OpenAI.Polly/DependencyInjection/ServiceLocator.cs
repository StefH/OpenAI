using System;
using Microsoft.Extensions.DependencyInjection;
using Stef.Validation;

namespace OpenAI_API.Polly.DependencyInjection;

internal static class ServiceLocator
{
    private static IServiceProvider? _serviceProvider;

    internal static IServiceProvider InitializeServiceLocator(this IServiceProvider serviceProvider)
    {
        _serviceProvider = Guard.NotNull(serviceProvider);
        return _serviceProvider;
    }

    internal static Lazy<T?> GetService<T>() where T : class
    {
        return new(() => _serviceProvider?.GetService<T>());
    }
}