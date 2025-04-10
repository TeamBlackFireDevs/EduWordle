using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

public class HuggingFaceAPI
{
    private const string ApiUrl = "https://router.huggingface.co/novita/v3/openai/chat/completions";
    private const string ApiKey = "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";

    private readonly HttpClient _client;

    public HuggingFaceAPI()
    {
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ApiKey);
    }

    public async Task<string> Query(string inputText, int maxTokens)
    {
        var payload = new
        {
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = inputText
                }
            },
            max_tokens = maxTokens,
            model = "deepseek/deepseek-v3-0324"
        };

        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync(ApiUrl, content);

        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<CompletionResponse>(responseBody);
            return result.choices[0].message.content;
        }
        else
        {
            throw new Exception($"Error: {response.StatusCode}");
        }
    }

    private class CompletionResponse
    {
        public Choice[] choices { get; set; }
    }

    private class Choice
    {
        public Message message { get; set; }
    }

    private class Message
    {
        public string content { get; set; }
    }
}