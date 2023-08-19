using Microsoft.AspNetCore.Mvc;
using NexosTestFront.Models;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace NexosTestFront.Controllers
{
    public class AuthorController : Controller
    {
        private readonly HttpClient _httpClient;
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public AuthorController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> IndexAsync()
        {
            //Consulta los autores para cargar el input de autores en la vista

            //OJO:Mover esto a un servicio si alcanza el tiempo
            HttpResponseMessage response = await _httpClient.GetAsync("https://localhost:7248/api/Authors");
            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();

                List<AuthorModel>? authors = JsonSerializer.Deserialize<List<AuthorModel>>(responseData, options);

                return View(authors);
            }
            else
            {
                return RedirectToAction("Error");
            }
            return View();
        }

        public IActionResult CreateAuthor()
        {
            return View("CreateAuthor");
        }

        [HttpPost]
        public async Task<IActionResult> CreateAuthor(AuthorModel author)
        {
            //consumir servicio para crear
            string json = JsonSerializer.Serialize(author);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync("https://localhost:7248/api/Authors", content);

            if (response.IsSuccessStatusCode)//redirijir a index si exito
            {
                return RedirectToAction("Index");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)//cargar errores del api
            {
                string errorResponseData = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonSerializer.Deserialize<ErrorResponseModel>(errorResponseData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                ModelState.AddModelError("", "Error en el formulario:");
                foreach (var error in errorResponse.Errors)
                {
                    ModelState.AddModelError(error.Key, error.Value[0]);
                }

                return View("CreateAuthor");
            }
            else
            {
                return View("Error");
            }
        }
    }
}
