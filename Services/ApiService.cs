using System.Net.Http.Json;

namespace Kanban.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;

        public ApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<LoginResponse?> LoginAsync(string nome, string senha)
        {
            var response = await _http.PostAsJsonAsync("api/auth/login", new
            {
                nome,
                senha
            });

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<LoginResponse>();
        }

        public async Task<bool> RegisterAsync(string nome, string senha)
        {
            var response = await _http.PostAsJsonAsync("api/clientes", new
            {
                nome,
                senha // 🔹 envia exatamente o que foi digitado
            });

            return response.IsSuccessStatusCode;
        }

    }

    public class LoginResponse
    {
        public int Id { get; set; }
        public string? Nome { get; set; }
        public int? GrupoId { get; set; }
        public string? GrupoNome { get; set; }
    }
}
