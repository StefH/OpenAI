using System;
using System.Threading;
using System.Threading.Tasks;
using OpenAI_API.Chat;

namespace OpenAI_API;

public static partial class OpenAIAPIExtensions
{
    /// <summary>
    /// Executes a given asynchronous function with retry logic, handling rate limits for the OpenAI API.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by the function.</typeparam>
    /// <param name="conversation">The conversation on which the function is executed.</param>
    /// <param name="func">The asynchronous function to be executed with retry logic.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static Task<TResult> WithRetry<TResult>(this Conversation conversation, Func<Conversation, Task<TResult>> func)
    {
        return AsyncRetryPolicy.ExecuteAsync(() => func(conversation));
    }
    
    /// <summary>
    /// Executes a given synchronous function with retry logic, handling rate limits for the OpenAI API.
    /// </summary>
    /// <param name="conversation">The conversation on which the function is executed.</param>
    /// <param name="func">The synchronous function to be executed with retry logic.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static Task WithRetry(this Conversation conversation, Func<Conversation, Task> func)
    {
        return RetryPolicy.Execute(() => func(conversation));
    }

    /// <summary>
    /// Executes a given synchronous action with retry logic, handling rate limits for the OpenAI API.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by the function.</typeparam>
    /// <param name="conversation">The conversation on which the action is executed.</param>
    /// <param name="func">The synchronous action to be executed with retry logic.</param>
    /// <returns>The result.</returns>
    public static TResult WithRetry<TResult>(this Conversation conversation, Func<Conversation, TResult> func)
    {
        return RetryPolicy.Execute(() => func(conversation));
    }
}