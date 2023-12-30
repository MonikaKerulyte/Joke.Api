using Joke.Api.Model.Dto;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace Joke.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class JokeController : ControllerBase
    {
        // API link string for JokeApi, which is an external service providing jokes.x
        const string JOKE_API_BASE_URL = "https://v2.jokeapi.dev/joke/";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<JokeController> _logger;

        public JokeController(
            ILogger<JokeController> logger,
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET api/v1/[controller]/jokes[?nrOfJokes=3]
        [HttpGet]
        [Route("jokes")]
        [ProducesResponseType(typeof(IEnumerable<JokeDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetJokesAsync([FromQuery] int nrOfJokes = 6)
        {
            List<JokeDto> jokes = new();

            // HTTP request.
            var httpRequestMessage = new HttpRequestMessage(
                HttpMethod.Get,
                JOKE_API_BASE_URL + $"Any?type=twopart&amount={nrOfJokes}");

            // Send request to JokeApi.
            var httpClient = _httpClientFactory.CreateClient();
            var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

            // If success, deserialize response message content and send response.
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                // Deserialize response.
                using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
                var response = await JsonSerializer.DeserializeAsync<JokeGetResponse>(contentStream);
                if (response is null)
                {
                    _logger.LogWarning("Response from JokeApi is null.");
                    return BadRequest();
                }

                // Convert JokeApi response to JokeDto.
                response.jokes.ToList().ForEach(x => jokes.Add(new()
                {
                    Category = x.category,
                    Setup = x.setup,
                    Delivery = x.delivery,
                }));

                // Check if there are any jokes in a list.
                if (!jokes.Any())
                {
                    _logger.LogWarning("A list of jokes is empty.");
                    return BadRequest();
                }

                return Ok(response);
            }

            _logger.LogCritical("A response status code from JokeApi was not success.");
            return BadRequest();
        }
    }

    public class JokeGetResponse
    {
        public bool error { get; set; }
        public int amount { get; set; }
        public IEnumerable<JokeFromResponse> jokes { get; set; } = Enumerable.Empty<JokeFromResponse>();
    }

    public class JokeFromResponse
    {
        public int id { get; set; }
        public required string setup { get; set; }
        public required string delivery { get; set; }
        public required string category { get; set; }
        public required string type { get; set; }
        public required bool safe { get; set; }
        public required string lang { get; set; }
        public required object flags { get; set; }
    }
}
