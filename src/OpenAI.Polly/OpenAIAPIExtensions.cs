using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Polly;

// ReSharper disable once CheckNamespace
namespace OpenAI_API;

/// <summary>
/// A static class providing extension methods for handling retries when consuming the OpenAI API.
/// </summary>
public static class OpenAIAPIExtensions
{
    private const int DefaultTimeOutInSeconds = 1;
    private const int MaxRetries = 5;
    private static readonly string RateLimitReachedMessage = "Rate limit reached".ToUpperInvariant();
    private static readonly Regex Regex = new(@"Please try again in (\d+)s\.", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static bool TryExtractWaitSecondsFromExceptionMessage(string exceptionMessage, out int waitSeconds)
    {
        var match = Regex.Match(exceptionMessage);
        if (match.Success && int.TryParse(match.Groups[1].Value, out var parsedValue))
        {
            waitSeconds = parsedValue;
            return true;
        }

        waitSeconds = default;
        return false;
    }

    private static readonly AsyncPolicy AsyncRetryPolicy = Policy
        .Handle<HttpRequestException>(ex => ex.Message.ToUpperInvariant().Contains(RateLimitReachedMessage))
        .WaitAndRetryAsync(MaxRetries, SleepDurationProvider, OnRetryAsync);

    private static readonly Policy RetryPolicy = Policy
        .Handle<HttpRequestException>(ex => ex.Message.ToUpperInvariant().Contains(RateLimitReachedMessage))
        .WaitAndRetry(MaxRetries, SleepDurationProvider, OnRetry);

    private static TimeSpan SleepDurationProvider(int retryAttempt, Exception exception, Context context)
    {
        return TimeSpan.FromSeconds(TryExtractWaitSecondsFromExceptionMessage(exception.Message, out var waitSeconds) ? waitSeconds : DefaultTimeOutInSeconds);
    }

    private static Task OnRetryAsync(Exception exception, TimeSpan timeSpan, int retryCount, Context context)
    {
        Debug.WriteLine($"Request failed. Waiting {timeSpan} before next retry. Retry attempt {retryCount}/{MaxRetries}.");
        return Task.CompletedTask;
    }

    private static void OnRetry(Exception exception, TimeSpan timeSpan, int retryCount, Context context)
    {
        Debug.WriteLine($"Request failed. Waiting {timeSpan} before next retry. Retry attempt {retryCount}/{MaxRetries}.");
    }

    /// <summary>
    /// Executes a given asynchronous function with retry logic, handling rate limits for the OpenAI API.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by the function.</typeparam>
    /// <typeparam name="TInstance">The type of the instance on which the function is executed.</typeparam>
    /// <param name="instance">The instance on which the function is executed.</param>
    /// <param name="func">The asynchronous function to be executed with retry logic.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static Task<TResult> WithRetry<TResult, TInstance>(this TInstance instance, Func<TInstance, Task<TResult>> func) where TInstance : class
    {
        return AsyncRetryPolicy.ExecuteAsync(() => func(instance));
    }

    /// <summary>
    /// Executes a given synchronous function with retry logic, handling rate limits for the OpenAI API.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by the function.</typeparam>
    /// <typeparam name="TInstance">The type of the instance on which the function is executed.</typeparam>
    /// <param name="instance">The instance on which the function is executed.</param>
    /// <param name="func">The synchronous function to be executed with retry logic.</param>
    /// <returns>The result of the function execution.</returns>
    public static TResult WithRetry<TResult, TInstance>(this TInstance instance, Func<TInstance, TResult> func) where TInstance : class
    {
        return RetryPolicy.Execute(() => func(instance));
    }

    /// <summary>
    /// Executes a given synchronous action with retry logic, handling rate limits for the OpenAI API.
    /// </summary>
    /// <typeparam name="TInstance">The type of the instance on which the action is executed.</typeparam>
    /// <param name="instance">The instance on which the action is executed.</param>
    /// <param name="func">The synchronous action to be executed with retry logic.</param>
    public static void WithRetry<TInstance>(this TInstance instance, Action<TInstance> func) where TInstance : class
    {
        RetryPolicy.Execute(() => func(instance));
    }
}