using System.Text.Json;

namespace MiniShop.IntegrationTests;

public class TestJsonHelper
{
    public static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<JsonElement>(
            content,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
    }
    
    public static async Task<int> ReadIdAsync(HttpResponseMessage response)
    {
        var json = await ReadJsonAsync(response);

        if (json.TryGetProperty("id", out var idProperty))
        {
            return idProperty.GetInt32();
        }
        
        throw new InvalidOperationException(
                $"Response does not contain an id property. Body: {json}"
            );
    }
}