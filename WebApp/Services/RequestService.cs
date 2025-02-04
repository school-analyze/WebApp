using WebApp.Models;

namespace WebApp.Services;

public static class RequestService
{
    public static async Task<IEnumerable<GradeModel>?> GetGrades(int userId, IHttpClientFactory httpClientFactory)
    {
        var client = httpClientFactory.CreateClient("WebApp.ServerAPI");
        var response = await client.GetAsync($"/users/{userId}/grades");
        if (response.IsSuccessStatusCode)
        {
            return await client.GetFromJsonAsync<IEnumerable<GradeModel>>($"/users/{userId}/grades");
        }
        return Array.Empty<GradeModel>();
    }
    public static async Task<bool> RemoveGradeAsync(int id, IHttpClientFactory httpClientFactory)
    {
        var client = httpClientFactory.CreateClient("WebApp.ServerAPI");
        var response = await client.DeleteAsync($"/grades/delete/{id}");
        return response.IsSuccessStatusCode;
    }
}