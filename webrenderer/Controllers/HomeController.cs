using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.Diagnostics;
using webrenderer.Models;

namespace webrenderer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory httpClientFactory;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<IActionResult> Index([FromQuery] string link)
        {
            if (string.IsNullOrWhiteSpace(link))
            {
                return View();
            }
            using var client = httpClientFactory.CreateClient();
            var result = await client.GetAsync(link);
            if (!result.IsSuccessStatusCode)
            {
                return View();
            }
            var bytes = await result.Content.ReadAsByteArrayAsync();
            return new FileContentResult(bytes, " text/html");
        }
    }
}