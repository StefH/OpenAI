using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace OpenAI_API.DependencyInjection;

public static class Class1
{
    public static IServiceCollection AddOpenAIWithPolly(this IServiceCollection services)
    {


        return services;
    }
}