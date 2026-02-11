namespace CareLog.Mobile.Services;

public sealed class ApiClient(HttpClient client)
{
    public Task<List<Models.VisitItem>?> GetTodayVisitsAsync() => client.GetFromJsonAsync<List<Models.VisitItem>>("/api/visits/week?start=" + DateTimeOffset.UtcNow.Date.ToString("O"));
    public Task<HttpResponseMessage> SyncAsync(object payload) => client.PostAsJsonAsync("/api/sync", payload);
}
