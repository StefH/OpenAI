# OpenAI.Polly
Can be used to handle exceptions like:

``` plaintext
Unhandled exception. System.Net.Http.HttpRequestException: Error at embeddings (https://api.openai.com/v1/embeddings) with HTTP status code: TooManyRequests. Content: {
    "error": {
        "message": "Rate limit reached for default-global-with-image-limits in organization org-*** on requests per min. Limit: 60 / min. Please try again in 1s. Contact support@openai.com if you continue to have issues. Please add a payment method to your account to increase your rate limit. Visit https://platform.openai.com/account/billing to add a payment method.",
        "type": "requests",
        "param": null,
        "code": null
    }
}
```

## Polly
[Polly](https://github.com/App-vNext/Polly) is used to handle the TooManyRequests exceptions.

## Usage
```csharp
IOpenAIAPI openAiAPI = new OpenAIAPI("Your OpenAI API-Key", "Your Organization ID");
float[] embeddings = await openAiAPI.Embeddings.WithRetry(embeddings => embeddings.GetEmbeddingsAsync("What is a cat?"));
```