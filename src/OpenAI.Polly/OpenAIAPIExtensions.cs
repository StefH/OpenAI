﻿using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenAI_API.Polly.DependencyInjection;
using OpenAI_API.Polly.Logging;
using Polly;
using Polly.Retry;

namespace OpenAI_API.Polly;

/// <summary>
/// A static class providing extension methods for handling retries when consuming the OpenAI API.
/// </summary>
public static partial class OpenAIAPIExtensions
{
    private const int DefaultTimeOutInSeconds = 20;
    private const int MaxRetries = 10;
    private static readonly Regex RateLimitReachedRegex = new("Rate limit reached", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex PleaseRetryYourRequestRegex = new("Please retry your request", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex TooManyRequestsRegex = new("TooManyRequests", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex PleaseTryAgainRegex = new(@"Please try again in (\d+)s\.", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Lazy<ILogger> Logger = new(() => ServiceLocator.GetService<ILoggerFactory>().Value?.CreateLogger(nameof(OpenAIAPIExtensions)) ?? new DebugLogger(nameof(OpenAIAPIExtensions)));

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
        .Handle<HttpRequestException>(IsExceptionRetryable)
        .WaitAndRetryAsync(MaxRetries, SleepDurationProvider, OnRetryAsync);

    private static readonly Policy RetryPolicy = Policy
        .Handle<HttpRequestException>(IsExceptionRetryable)
        .WaitAndRetry(MaxRetries, SleepDurationProvider, OnRetry);

    private static bool IsExceptionRetryable(HttpRequestException httpException)
    {
        return RateLimitReachedRegex.IsMatch(httpException.Message) || 
               PleaseRetryYourRequestRegex.IsMatch(httpException.Message) ||
               TooManyRequestsRegex.IsMatch(httpException.Message);
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
        OnRetry(exception, timeSpan, retryCount, context);
        return Task.CompletedTask;
    }

    private static void OnRetry(Exception exception, TimeSpan timeSpan, int retryCount, Context context)
    {
        Logger.Value.LogDebug(exception, "Request failed. Waiting {timeSpan} before next retry. Retry attempt {retryCount}/{maxRetries}.", timeSpan, retryCount, MaxRetries);
    }
}