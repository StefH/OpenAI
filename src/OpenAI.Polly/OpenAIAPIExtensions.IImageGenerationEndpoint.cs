using System;
using System.Threading;
using System.Threading.Tasks;
using OpenAI_API.Images;

namespace OpenAI_API;

public static partial class OpenAIAPIExtensions
{
    /// <summary>
    /// Executes a given asynchronous function with retry logic, handling rate limits for the OpenAI API.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by the function.</typeparam>
    /// <param name="endpoint">The endpoint on which the function is executed.</param>
    /// <param name="func">The asynchronous function to be executed with retry logic.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static Task<TResult> WithRetry<TResult>(this IImageGenerationEndpoint endpoint, Func<IImageGenerationEndpoint, Task<TResult>> func)
    {
        return AsyncRetryPolicy.ExecuteAsync(() => func(endpoint));
    }

    /// <summary>
    /// Executes a given asynchronous function with retry logic, handling rate limits for the OpenAI API.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by the function.</typeparam>
    /// <param name="endpoint">The endpoint on which the function is executed.</param>
    /// <param name="func">The asynchronous function to be executed with retry logic.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static Task<TResult> WithRetry<TResult>(this IImageGenerationEndpoint endpoint, Func<IImageGenerationEndpoint, CancellationToken, Task<TResult>> func, CancellationToken cancellationToken)
    {
        return AsyncRetryPolicy.ExecuteAsync(ct => func(endpoint, ct), cancellationToken);
    }

    /// <summary>
    /// Executes a given synchronous function with retry logic, handling rate limits for the OpenAI API.
    /// </summary>
    /// <param name="endpoint">The endpoint on which the function is executed.</param>
    /// <param name="func">The synchronous function to be executed with retry logic.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static Task WithRetry(this IImageGenerationEndpoint endpoint, Func<IImageGenerationEndpoint, Task> func)
    {
        return RetryPolicy.Execute(() => func(endpoint));
    }

    /// <summary>
    /// Executes a given synchronous function with retry logic, handling rate limits for the OpenAI API.
    /// </summary>
    /// <param name="endpoint">The endpoint on which the function is executed.</param>
    /// <param name="func">The synchronous function to be executed with retry logic.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static Task WithRetry(this IImageGenerationEndpoint endpoint, Func<IImageGenerationEndpoint, CancellationToken, Task> func, CancellationToken cancellationToken)
    {
        return AsyncRetryPolicy.ExecuteAsync(ct => func(endpoint, ct), cancellationToken);
    }

    /// <summary>
    /// Executes a given synchronous action with retry logic, handling rate limits for the OpenAI API.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by the function.</typeparam>
    /// <param name="endpoint">The endpoint on which the action is executed.</param>
    /// <param name="func">The synchronous action to be executed with retry logic.</param>
    /// <returns>The result.</returns>
    public static TResult WithRetry<TResult>(this IImageGenerationEndpoint endpoint, Func<IImageGenerationEndpoint, TResult> func)
    {
        return RetryPolicy.Execute(() => func(endpoint));
    }

    /// <summary>
    /// Executes a given synchronous action with retry logic, handling rate limits for the OpenAI API.
    /// </summary>
    /// <param name="endpoint">The endpoint on which the action is executed.</param>
    /// <param name="func">The synchronous action to be executed with retry logic.</param>
    public static void WithRetry(this IImageGenerationEndpoint endpoint, Action<IImageGenerationEndpoint> func)
    {
        RetryPolicy.Execute(() => func(endpoint));
    }
}