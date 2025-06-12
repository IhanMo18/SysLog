using System.Text;
using System.Text.Json;

namespace LogUdp.Api;

public class SysApiDynamic
{
    private readonly IHttpClientFactory _httpClientFactory;

    public SysApiDynamic(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Realiza una petición HTTP dinámica (GET, POST, PUT, DELETE) a una URL interna o externa.
    /// </summary>
    /// <param name="url">URL completa o relativa</param>
    /// <param name="httpMethod">"GET", "POST", "PUT", "DELETE"</param>
    /// <param name="body">Objeto serializable (opcional)</param>
    /// <param name="headers">Headers adicionales (opcional)</param>
    /// <typeparam name="TResponse">Tipo esperado en la respuesta</typeparam>
    /// <returns>Respuesta deserializada</returns>
    
     
    public async Task<TResponse?> CallApiAsync<TResponse>(
        string url,
        string httpMethod,
        object? body = null,
        Dictionary<string, string>? headers = null)
    {
        var client = _httpClientFactory.CreateClient();

        // Configura el request
        var request = new HttpRequestMessage(new HttpMethod(httpMethod), url);

        if (body != null)
        {
            string json = JsonSerializer.Serialize(body);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        if (headers != null)
        {
            foreach (var header in headers)
                request.Headers.Add(header.Key, header.Value);
        }

        var response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TResponse>(responseContent);
    }
}