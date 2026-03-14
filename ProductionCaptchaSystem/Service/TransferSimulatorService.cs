using Newtonsoft.Json;
using System.Net.Http;

namespace ProductionCaptchaSystem.Services;

public class TransferSimulatorService
{
    private readonly HttpClient _httpClient;

    // Порт 4444, путь точно как в браузере
    private const string Url = "http://127.0.0.1:4444/TransferSimulator/fullName";

    public TransferSimulatorService()
    {
        _httpClient = new HttpClient();
    }

    private class FullNameResponse
    {
        [JsonProperty("value")]
        public string? Value { get; set; }
    }

    public async Task<string> GetRandomFullNameAsync()
    {
        try
        {
            var response = await _httpClient.GetStringAsync(Url);

            // Сервер возвращает: {"value" : "Кузнецов Кузьма Кузьмич"}
            var fullNameResponse = JsonConvert.DeserializeObject<FullNameResponse>(response);

            return fullNameResponse?.Value ?? "Ошибка получения данных";
        }
        catch (HttpRequestException ex)
        {
            return $"Ошибка сети: {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"Ошибка: {ex.Message}";
        }
    }
}
