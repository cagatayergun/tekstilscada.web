// Dosya: TekstilScada.WebApp/Services/ScadaDataService.cs (TAM SÜRÜM)

using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Concurrent;
using System.Net.Http.Json;
using TekstilScada.Models;
using TekstilScada.Repositories;

namespace TekstilScada.WebApp.Services
{
    public class ScadaDataService
    {
        private HubConnection? _hubConnection;
        private readonly HttpClient _httpClient;

        public ConcurrentDictionary<int, FullMachineStatus> MachineData { get; private set; } = new();
        public event Action? OnDataUpdated;

        public ScadaDataService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task InitializeAsync()
        {
            var hubUrl = new Uri(_httpClient.BaseAddress!, "/scadaHub");
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .WithAutomaticReconnect()
                .Build();
            _hubConnection.On<FullMachineStatus>("ReceiveMachineUpdate", (status) =>
            {
                MachineData[status.MachineId] = status;
                OnDataUpdated?.Invoke();
            });
            try
            {
                await _hubConnection.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalR bağlantı hatası: {ex.Message}");
            }
        }

        public async Task<List<Machine>?> GetMachinesAsync()
        {
            try { return await _httpClient.GetFromJsonAsync<List<Machine>>("api/machines"); }
            catch (Exception ex) { Console.WriteLine($"Makine listesi alınamadı: {ex.Message}"); return null; }
        }

        public async Task<Machine?> AddMachineAsync(Machine machine)
        {
            var response = await _httpClient.PostAsJsonAsync("api/machines", machine);
            return await response.Content.ReadFromJsonAsync<Machine>();
        }

        public async Task<bool> UpdateMachineAsync(Machine machine)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/machines/{machine.Id}", machine);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteMachineAsync(int machineId)
        {
            var response = await _httpClient.DeleteAsync($"api/machines/{machineId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<User>?> GetUsersAsync()
        {
            try { return await _httpClient.GetFromJsonAsync<List<User>>("api/users"); }
            catch (Exception ex) { Console.WriteLine($"Kullanıcılar alınamadı: {ex.Message}"); return null; }
        }

        public async Task<List<ScadaRecipe>?> GetRecipesAsync()
        {
            try { return await _httpClient.GetFromJsonAsync<List<ScadaRecipe>>("api/recipes"); }
            catch (Exception ex) { Console.WriteLine($"Reçete listesi alınamadı: {ex.Message}"); return null; }
        }

        public async Task<ScadaRecipe?> GetRecipeDetailsAsync(int recipeId)
        {
            try { return await _httpClient.GetFromJsonAsync<ScadaRecipe>($"api/recipes/{recipeId}"); }
            catch (Exception ex) { Console.WriteLine($"Reçete detayı alınamadı: {ex.Message}"); return null; }
        }

        public async Task<ScadaRecipe?> SaveRecipeAsync(ScadaRecipe recipe)
        {
            var response = await _httpClient.PostAsJsonAsync("api/recipes", recipe);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<ScadaRecipe>() : null;
        }

        public async Task<bool> DeleteRecipeAsync(int recipeId)
        {
            var response = await _httpClient.DeleteAsync($"api/recipes/{recipeId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SendRecipeToPlcAsync(int recipeId, int machineId)
        {
            var response = await _httpClient.PostAsync($"api/recipes/{recipeId}/send-to-plc/{machineId}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<ScadaRecipe?> ReadRecipeFromPlcAsync(int machineId)
        {
            try { return await _httpClient.GetFromJsonAsync<ScadaRecipe>($"api/recipes/read-from-plc/{machineId}"); }
            catch { return null; }
        }

        public async Task<List<ProductionReportItem>?> GetProductionReportAsync(ReportFilters filters)
        {
            var response = await _httpClient.PostAsJsonAsync("api/reports/production", filters);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<List<ProductionReportItem>>() : new List<ProductionReportItem>();
        }

        // === EKSİK OLAN METOT BURADA ===
        public async Task<List<string>?> GetHmiRecipesAsync(int machineId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<string>>($"api/ftp/list/{machineId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HMI reçeteleri alınamadı: {ex.Message}");
                return new List<string> { $"Hata: {ex.Message}" };
            }
        }
    }
}