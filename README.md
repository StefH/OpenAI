# OpenAI
OpenAI related projects...

## OpenAI.Polly
[Polly](https://github.com/App-vNext/Polly) is used to handle exceptions like:

```
Unhandled exception. System.Net.Http.HttpRequestException: Error at embeddings (https://api.openai.com/v1/embeddings) with HTTP status code: TooManyRequests. Content: {
    "error": {
        "message": "Rate limit reached for default-global-with-image-limits in organization org-*** on requests per min. Limit: 60 / min. Please try again in 1s. Contact support@openai.com if you continue to have issues. Please add a payment method to your account to increase your rate limit. Visit https://platform.openai.com/account/billing to add a payment method.",
        "type": "requests",
        "param": null,
        "code": null
    }
}
```

### Usage
m