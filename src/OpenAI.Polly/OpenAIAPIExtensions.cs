using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;

// ReSharper disable once CheckNamespace
namespace OpenAI_API;

/// <summary>
/// A static class providing extension methods for handling retries when consuming the OpenAI API.
/// </summary>
public static partial class OpenAIAPIExtensions
{
    private const int DefaultTimeOutInSeconds = 20;
    private const int MaxRetries = 10;
    private static readonly Regex RateLimitReachedRegex = new("Rate limit reached", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex PleaseTryAgainRegex = new(@"Please try again in (\d+)s\.", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static bool TryExtractWaitSecondsFromExceptionMessage(string exceptionMessage, out int waitSeconds)
    {
        var match = PleaseTryAgainRegex.Match(exceptionMessage);
        if (match.Success && int.TryParse(match.Groups[1].Value, out var parsedValue))
        {
            waitSeconds = parsedValue;
            return true;
        }

        waitSeconds = default;
        return false;
    }

    private static readonly AsyncRetryPolicy AsyncRetryPolicy = Policy
        .Handle<HttpRequestException>(IsRateLimitReachedException)
        .WaitAndRetryAsync(MaxRetries, SleepDurationProvider, OnRetryAsync);

    private static readonly Policy RetryPolicy = Policy
        .Handle<HttpRequestException>(IsRateLimitReachedException)
        .WaitAndRetry(MaxRetries, SleepDurationProvider, OnRetry);

    private static bool IsRateLimitReachedException(HttpRequestException httpException)
    {
        return RateLimitReachedRegex.IsMatch(httpException.Message);
    }

    private static TimeSpan SleepDurationProvider(int retryAttempt, Exception exception, Context context)
    {
        var seconds = TryExtractWaitSecondsFromExceptionMessage(exception.Message, out var waitSeconds)
            ? waitSeconds
            : DefaultTimeOutInSeconds;

        if (seconds == 1)
        {
            seconds = 2;
        }

        return retryAttempt == 1 ? TimeSpan.FromSeconds(seconds) : TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
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

    ///// <summary>
    ///// Executes a given asynchronous function with retry logic, handling rate limits for the OpenAI API.
    ///// </summary>
    ///// <typeparam name="TInstance">The type of the instance on which the function is executed.</typeparam>
    ///// <typeparam name="TResult">The type of the result returned by the function.</typeparam>
    ///// <param name="instance">The instance on which the function is executed.</param>
    ///// <param name="func">The asynchronous function to be executed with retry logic.</param>
    ///// <returns>A task that represents the asynchronous operation.</returns>
    //public static Task<TResult> WithRetry<TResult, TInstance>(this TInstance instance, Func<TInstance, Task<TResult>> func) where TInstance : class
    //{
    //    return AsyncRetryPolicy.ExecuteAsync(() => func(instance));
    //}

    ///// <summary>
    ///// Executes a given asynchronous function with retry logic, handling rate limits for the OpenAI API.
    ///// </summary>
    ///// <typeparam name="TInstance">The type of the instance on which the function is executed.</typeparam>
    ///// <typeparam name="TResult">The type of the result returned by the function.</typeparam>
    ///// <param name="instance">The instance on which the function is executed.</param>
    ///// <param name="func">The asynchronous function to be executed with retry logic.</param>
    ///// <param name="cancellationToken">The cancellation token.</param>
    ///// <returns>A task that represents the asynchronous operation.</returns>
    //public static Task<TResult> WithRetry<TResult, TInstance>(this TInstance instance, Func<TInstance, CancellationToken, Task<TResult>> func, CancellationToken cancellationToken) where TInstance : class
    //{
    //    return AsyncRetryPolicy.ExecuteAsync(ct => func(instance, ct), cancellationToken);
    //}

    ///// <summary>
    ///// Executes a given synchronous function with retry logic, handling rate limits for the OpenAI API.
    ///// </summary>
    ///// <typeparam name="TInstance">The type of the instance on which the function is executed.</typeparam>
    ///// <param name="instance">The instance on which the function is executed.</param>
    ///// <param name="func">The synchronous function to be executed with retry logic.</param>
    ///// <returns>A task that represents the asynchronous operation.</returns>
    //public static Task WithRetry<TInstance>(this TInstance instance, Func<TInstance, Task> func) where TInstance : class
    //{
    //    return RetryPolicy.Execute(() => func(instance));
    //}

    ///// <summary>
    ///// Executes a given synchronous function with retry logic, handling rate limits for the OpenAI API.
    ///// </summary>
    ///// <typeparam name="TInstance">The type of the instance on which the function is executed.</typeparam>
    ///// <param name="instance">The instance on which the function is executed.</param>
    ///// <param name="func">The synchronous function to be executed with retry logic.</param>
    ///// <param name="cancellationToken">The cancellation token.</param>
    ///// <returns>A task that represents the asynchronous operation.</returns>
    //public static Task WithRetry<TInstance>(this TInstance instance, Func<TInstance, CancellationToken, Task> func, CancellationToken cancellationToken) where TInstance : class
    //{
    //    return AsyncRetryPolicy.ExecuteAsync(ct => func(instance, ct), cancellationToken);
    //}

    ///// <summary>
    ///// Executes a given synchronous action with retry logic, handling rate limits for the OpenAI API.
    ///// </summary>
    ///// <typeparam name="TInstance">The type of the instance on which the action is executed.</typeparam>
    ///// <typeparam name="TResult">The type of the result returned by the function.</typeparam>
    ///// <param name="instance">The instance on which the action is executed.</param>
    ///// <param name="func">The synchronous action to be executed with retry logic.</param>
    ///// <returns>The result.</returns>
    //public static TResult WithRetry<TInstance, TResult>(this TInstance instance, Func<TInstance, TResult> func) where TInstance : class
    //{
    //    return RetryPolicy.Execute(() => func(instance));
    //}

    ///// <summary>
    ///// Executes a given synchronous action with retry logic, handling rate limits for the OpenAI API.
    ///// </summary>
    ///// <typeparam name="TInstance">The type of the instance on which the action is executed.</typeparam>
    ///// <param name="instance">The instance on which the action is executed.</param>
    ///// <param name="func">The synchronous action to be executed with retry logic.</param>
    //public static void WithRetry<TInstance>(this TInstance instance, Action<TInstance> func) where TInstance : class
    //{
    //    RetryPolicy.Execute(() => func(instance));
    //}
}