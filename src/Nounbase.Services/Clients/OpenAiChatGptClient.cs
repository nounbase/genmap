using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using Nounbase.Core.Interfaces.Clients;
using Nounbase.Core.Interfaces.Configuration;
using Nounbase.Services.Constants;
using System.Text.Json;
using static Nounbase.Core.Utilities.Environment;
using static Nounbase.Core.Utilities.Resiliency;
using static System.Environment;

namespace Nounbase.Services.Clients
{
    public class OpenAiChatGptClient : IChatGptClient
    {
        private static readonly float temperature;

        private static readonly OpenAIClient aiClient;
        private static readonly SemaphoreSlim throttle;

        private readonly ILogger log;
        private readonly IModelConfiguration modelConfig;

        static OpenAiChatGptClient()
        {
            var envTemp = GetEnvironmentVariable(NounbaseEnv.Temperature);

            if (!string.IsNullOrWhiteSpace(envTemp))
            {
                if (!float.TryParse(envTemp, out temperature) || (temperature <= 0))
                {
                    throw new InvalidOperationException($"[{NounbaseEnv.Temperature}] must be a valid float > 0.");
                }
            }
            else
            {
                temperature = .35f;
            }

            aiClient = new OpenAIClient(GetRequiredEnvironmentVariable(NounbaseEnv.OpenAiApiKey));
            throttle = CreateThrottle();
        }

        static SemaphoreSlim CreateThrottle()
        {
            var throttle = 20;
            var envThrottle = GetEnvironmentVariable(NounbaseEnv.OpenAiThrottle);

            if (!string.IsNullOrWhiteSpace(envThrottle))
            {
                if (!int.TryParse(envThrottle, out throttle) || (throttle <= 0))
                {
                    throw new InvalidOperationException($"[{NounbaseEnv.OpenAiThrottle}] must be a valid integer > 0.");
                }
            }

            return new SemaphoreSlim(throttle);
        }

        public OpenAiChatGptClient(
            ILogger<OpenAiChatGptClient> log,
            IModelConfiguration modelConfig)
        {
            ArgumentNullException.ThrowIfNull(log, nameof(log));
            ArgumentNullException.ThrowIfNull(modelConfig, nameof(modelConfig));

            this.log = log;
            this.modelConfig = modelConfig;
        }

        public async Task<string?> Complete(string prompt, string? modelName = null)
        {
            ArgumentNullException.ThrowIfNull(prompt, nameof(prompt));

            modelName ??= modelConfig.DefaultModelName;

            try
            {
                await throttle.WaitAsync();

                log.LogDebug(
                    $"Getting model [{modelName}] prompt [length: {prompt.Length} character(s)] " +
                     "completion from OpenAI chat completions API...");

                var options = new ChatCompletionsOptions(
                    modelName, new[] { new ChatRequestUserMessage(prompt) })
                    { ChoiceCount = 1, Temperature = temperature };

                var retryPipeline = CreateRetryPipeline<Response<ChatCompletions>>();

                var pollyResponse = await retryPipeline
                    .ExecuteAsync(async token =>
                    {
                        var response = await aiClient.GetChatCompletionsAsync(options);
                        var statusCode = response.GetRawResponse().Status;

                        if (statusCode != 200)
                        {
                            throw new HttpRequestException(
                                $"Unable to complete prompt: [{nameof(aiClient)}] client returned status code [{statusCode}].");
                        }

                        return response;
                    });

                var completion = pollyResponse.Value.Choices[0].Message.Content;

                if (string.IsNullOrEmpty(completion))
                {
                    log.LogWarning($"No completion returned from model [{modelName}]. Prompt may need to be revised.");
                }
                else
                {
                    log.LogDebug(
                        $"Got model [{modelName}] completion [length: {completion.Length} character(s)] " +
                         "from OpenAI chat completions API.");
                }

                return completion;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"An error occurred while trying to get a [{modelName}] from the OpenAI Completions API." +
                     "See inner exception for more details.", ex);
            }
            finally
            {
                throttle.Release();
            }
        }

        public async Task<T?> Complete<T>(string prompt, string? modelName = null)
        {
            ArgumentNullException.ThrowIfNull(prompt, nameof(prompt));

            modelName ??= modelConfig.DefaultModelName;

            try
            {
                await throttle.WaitAsync();

                log.LogDebug(
                    $"Getting model [{modelName}] prompt [length: {prompt.Length} character(s)] " +
                     "completion from OpenAI chat completions API...");

                var options = new ChatCompletionsOptions(
                    modelName, new[] { new ChatRequestUserMessage(prompt) })
                    { ChoiceCount = 1, Temperature = temperature };

                var retryPipeline = CreateRetryPipeline<Response<ChatCompletions>>();

                var pollyResponse = await retryPipeline
                    .ExecuteAsync(async token =>
                    {
                        var response = await aiClient.GetChatCompletionsAsync(options);
                        var statusCode = response.GetRawResponse().Status;

                        if (statusCode != 200)
                        {
                            throw new HttpRequestException(
                                $"Unable to complete prompt: [{nameof(aiClient)}] client returned status code [{statusCode}].");
                        }

                        return response;
                    });

                var completion = pollyResponse.Value.Choices[0].Message.Content;

                if (string.IsNullOrEmpty(completion))
                {
                    log.LogWarning($"No completion returned from model [{modelName}]. Prompt may need to be revised.");
                }
                else
                {
                    log.LogDebug(
                        $"Got model [{modelName}] completion [length: {completion.Length} character(s)] " +
                         "from OpenAI chat completions API.");
                }

                return JsonSerializer.Deserialize<T?>(completion);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"An error occurred while trying to get a [{modelName}] from the OpenAI Completions API." +
                     "See inner exception for more details.", ex);
            }
            finally
            {
                throttle.Release();
            }
        }
    }
}
