using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NexosTestFront.Models;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace NexosTestFront.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public HomeController(ILogger<HomeController> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<IActionResult> IndexAsync()
        {
            HttpResponseMessage response = await _httpClient.GetAsync("https://localhost:7248/api/Books");
            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                List<BookModel>? books = JsonSerializer.Deserialize<List<BookModel>>(responseData, options);

                return View(books);
            }
            return RedirectToAction("Error");//redirijo a pagina de error por default en caso de error de solicitud
        }

        public async Task<IActionResult> CreateBookAsync()
        {
            //Consulta los autores para cargar el input de autores en la vista

            //OJO:Mover esto a un servicio si alcanza el tiempo
            HttpResponseMessage response = await _httpClient.GetAsync("https://localhost:7248/api/Authors");
            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();

                List<AuthorModel>? authors = JsonSerializer.Deserialize<List<AuthorModel>>(responseData, options);

                ViewBag.Authors = new SelectList(authors, "AuthorId", "FullName");

                return View("CreateBook");
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateBook(CreateBookModel model)
        {
            string json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync("https://localhost:7248/api/Books", content);

            if (response.IsSuccessStatusCode)
            {
                // Aquí podrías manejar una respuesta exitosa si es necesario
                return RedirectToAction("Index");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
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

                return View("CreateBook");
            }
            else if(response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
            {
                //regla de negocio exedio maximo de libros
                ViewData["Max"] = "No es posible registrar el libro, se alcanzó el máximo permitido.";
                return View("CreateBook");
            }
            else
            {
                // Otros casos de manejo de errores
                return View("Error");
            }


        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}