using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Aeronautica.Models;

namespace Aeronautica.Services
{
    public class ApiService
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly JsonSerializerOptions _jsonSerializerOptionsDeserialize;

        public ApiService()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri("http://ApiAeronautica.somee.com/api/")
            };

            _jsonSerializerOptions = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,
                PropertyNameCaseInsensitive = true
            };

            _jsonSerializerOptionsDeserialize = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            _jsonSerializerOptionsDeserialize.Converters.Add(new JsonStringEnumConverter());
        }

        public async Task<Response> GetAsync<T>(string controller)
        {
            try
            {
                var response = await _client.GetAsync(controller);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = content
                    };
                }

                var result = JsonSerializer.Deserialize<T>(content, _jsonSerializerOptionsDeserialize);

                return new Response
                {
                    IsSuccess = true,
                    Result = result
                };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<Response> PostAsync<T>(string controller, T data)
        {
            try
            {
                var response = await _client.PostAsJsonAsync(controller, data, _jsonSerializerOptions);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = content
                    };
                }

                // If successful and there's content, try to deserialize it
                // T here would be Passageiro when creating a passenger
                if (!string.IsNullOrWhiteSpace(content))
                {
                    try
                    {
                        // Check if the content is an object that can be deserialized to T
                        // The API's CreatedAtRoute returns the object T
                        var result = JsonSerializer.Deserialize<T>(content, _jsonSerializerOptionsDeserialize);
                        return new Response
                        {
                            IsSuccess = true,
                            Result = result // <<< IMPORTANT: Store the deserialized created object
                        };
                    }
                    catch (JsonException jsonEx)
                    {
                        // Log or handle the case where deserialization fails but POST was successful
                        System.Diagnostics.Debug.WriteLine($"ApiService.PostAsync deserialization error: {jsonEx.Message}. Content: {content}");
                        return new Response
                        {
                            IsSuccess = true, // POST was ok
                            Message = "Operação bem-sucedida, mas resposta do servidor não pôde ser totalmente processada."
                            // Result will be null or default(T)
                        };
                    }
                }

                // If no content but successful (e.g., 204 No Content, or simple success message not deserializable to T)
                return new Response
                {
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<Response> PutAsync<T>(string controller, T data)
        {
            try
            {
                var response = await _client.PutAsJsonAsync(controller, data, _jsonSerializerOptions);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = content
                    };
                }

                if (!string.IsNullOrWhiteSpace(content)) 
                {
                    var result = JsonSerializer.Deserialize<T>(content, _jsonSerializerOptionsDeserialize);
                    return new Response
                    {
                        IsSuccess = true,
                        Result = result
                    };
                }

                // Se não houver conteúdo, retorna sucesso simples
                return new Response
                {
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<Response> DeleteAsync(string controller)
        {
            try
            {
                var response = await _client.DeleteAsync(controller);

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return new Response
                    {
                        IsSuccess = false,
                        Message = content
                    };
                }

                return new Response
                {
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
    }
}