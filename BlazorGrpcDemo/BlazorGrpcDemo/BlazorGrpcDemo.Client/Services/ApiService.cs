using System.Net.Http;
using System.Text.Json;

namespace BlazorGrpcDemo.Client.Services;

public class ApiService
{
    HttpClient http;

    public ApiService(HttpClient _http)
    {
        http = _http;
    }

    public async Task<List<Person>> GetAll()
    {
        try
        {
            var result = await http.GetAsync("persons");
            result.EnsureSuccessStatusCode();
            string responseBody = await result.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Person>>(responseBody);
        }
        catch (Exception ex)
        {
            var msg = ex.Message;
            return null;
        }
    }

    public async IAsyncEnumerable<Person> GetAllStream()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "persons/getstream");
        var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        if (response.IsSuccessStatusCode)
        {
            var stream = await response.Content.ReadAsStreamAsync();
            var people = JsonSerializer.DeserializeAsyncEnumerable<Person>(stream, new JsonSerializerOptions(JsonSerializerDefaults.Web));

            await foreach (var person in people)
            {
                yield return person;
            }
        }
        else
        {
            // Handle error or throw an exception
            throw new HttpRequestException($"Failed to fetch data: {response.StatusCode}");
        }
    }

    public async Task<Person> GetPersonById(int Id)
    {
        try
        {
            var result = await http.GetAsync($"persons/{Id}/getbyid");
            result.EnsureSuccessStatusCode();
            string responseBody = await result.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Person>(responseBody);
        }
        catch (Exception ex)
        {
            return null;
        }
    }
}