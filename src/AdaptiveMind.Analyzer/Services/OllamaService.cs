namespace AdaptiveMind.Analyzer.Services;

using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class OllamaService
{
    private readonly HttpClient _httpClient;
    private readonly string _ollamaUrl;
    
    public OllamaService(string ollamaUrl)
    {
        _ollamaUrl = ollamaUrl.TrimEnd('/');
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(60);
    }
    
    public async Task<string> GenerateResponse(string prompt, float temperature = 0.7f, int maxTokens = 500)
    {
        var request = new
        {
            model = "llama3.2:3b",
            prompt = prompt,
            stream = false,
            options = new 
            { 
                temperature = temperature,
                num_predict = maxTokens
            }
        };
        
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        try
        {
            var response = await _httpClient.PostAsync($"{_ollamaUrl}/api/generate", content);
            var responseJson = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<OllamaResponse>(responseJson);
                return result?.response ?? "No response generated";
            }
            
            return $"Error: {response.StatusCode} - {responseJson}";
        }
        catch (Exception ex)
        {
            return $"Exception: {ex.Message}";
        }
    }
}

public class OllamaResponse
{
    public string response { get; set; }
    public bool done { get; set; }
}