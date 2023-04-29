using System;

namespace OpenAI_API.DependencyInjection;

/// <summary>
/// 
/// </summary>
public static class ServiceProviderExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static IServiceProvider UseOpenAIWithPolly(this IServiceProvider serviceProvider)
    {
        return serviceProvider.InitializeServiceLocator();
    }
}