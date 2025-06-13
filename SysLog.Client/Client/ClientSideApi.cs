using System.Text;
using System.Text.Json;
using SysLog.Shared;

namespace SysLog.Client.Client;

public class ClientSideApi
{
    public HttpClient Client { get; }
    public ClientSideApi(HttpClient client)
    {
        Client = client;
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
    
     
    public async Task<TaskResult<TResponse>?> CallApiAsync<TResponse>(
        string url,
        string httpMethod,
        object? body = null,
        Dictionary<string, string>? headers = null)
    {

        try
        {
            // Creamos peticion al servidor
            var request = new HttpRequestMessage(new HttpMethod(httpMethod), Client.BaseAddress + url);

            if (body is not null)
            {
                string json = JsonSerializer.Serialize(body);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            if (headers is not null)
            {
                foreach (var header in headers)
                    request.Headers.Add(header.Key, header.Value);
            }

            var response = await Client.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            if(string.IsNullOrEmpty(responseContent)) 
                return TaskResult<TResponse>.FromFailure("No data available", 404); 
            
            TResponse? responseObject = JsonSerializer.Deserialize<TResponse>(responseContent);
            return TaskResult<TResponse>.FromData(responseObject);
        }
        catch (JsonException jsonException)
        {
            return TaskResult<TResponse>.FromFailure("Algo fallo en el servidor, reintente de nuevo", 500,
                jsonException.Message);
        }
        catch (HttpRequestException httpException)
        {
            return TaskResult<TResponse>.FromFailure("Algo fallo en el servidor, reintente de nuevo", 500,
                httpException.Message);
        }
        catch (Exception ex)
        {
            return TaskResult<TResponse>.FromFailure("Ocurrio un error, intente de nuevo", 500,
                ex.Message);
        }
        

        
    }
}