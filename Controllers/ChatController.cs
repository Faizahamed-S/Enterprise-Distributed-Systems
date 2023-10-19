using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

[Route("api/[controller]")]
[ApiController]
public class ChatController : ControllerBase
{
    // Retrieve the API key securely, e.g., from an environment variable
    private readonly string OpenAiApiKey = Environment.GetEnvironmentVariable("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");

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] UserMessage message)
    {
        if (String.Equals("hi", message.Text, StringComparison.OrdinalIgnoreCase))
        {
            return Ok(new BotResponse { Text = "Hello! How can I assist you with tax-related questions?" });
        }
//         if (string.IsNullOrEmpty(OpenAiApiKey))
// {
//     Console.WriteLine("OpenAI API key not set.");
//     return BadRequest("OpenAI API key not set.");
// }


        // If the message isn't "hi", we'll forward it to OpenAI's API
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer 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");

            var payload = new
            {
                model="gpt-3.5-turbo-0301",
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful assistant." },
                    new { role = "user", content = message.Text }
                },
                // prompt = $"Q: {message.Text}\nA:",  // Modified prompt for clarity
                max_tokens = 150
            };

            var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", 
                new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"OpenAI API Error: {errorContent}"); // Log the error for debugging
                    return BadRequest($"Error querying OpenAI API: {errorContent}");
                }


            if (!response.IsSuccessStatusCode)
            {
                return BadRequest("Error querying OpenAI API.");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            // Console.WriteLine(responseBody);
            var responseObject = JsonConvert.DeserializeObject<OpenAiResponse>(responseBody);

            return Ok(new BotResponse { Text = responseObject.choices[0].message.content.Trim() });
        }
    }

    // ... (rest of the classes remain unchanged)
}

    public class UserMessage
    {
        public string Text { get; set; }
    }

    public class BotResponse
    {
        public string Text { get; set; }
    }

    public class OpenAiResponse
    {
        public List<Choice> choices { get; set; }
    }

    public class Choice
    {
        public Message message { get; set; }
    }

    public class Message
    {
        public string role { get; set; }
        public string content { get; set; }
    }

