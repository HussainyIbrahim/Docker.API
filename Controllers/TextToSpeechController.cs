using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class VoiceController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IWebHostEnvironment _env;

    public VoiceController(IHttpClientFactory httpClientFactory, IWebHostEnvironment env)
    {
        _httpClientFactory = httpClientFactory;
        _env = env;
    }

    [HttpGet("convert")]
    public async Task<IActionResult> Convert([FromQuery] string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return BadRequest("Text parameter is required.");
        }

        try
        {
            using var httpClient = _httpClientFactory.CreateClient();

            // Set up request headers
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "6a4afd5209434b91a39d1cd0d40ed4a1");
            httpClient.DefaultRequestHeaders.Add("Accept", "audio/mpeg");
            httpClient.DefaultRequestHeaders.Add("X-USER-ID", "rokUicb4czXRTlpLHDdyRlgbmj82");

            // Prepare request body
            var requestBody = new
            {
                voice = "s3://voice-cloning-zero-shot/0fdb2207-18eb-404e-a165-8377b78e0b17/hussainy/manifest.json",
                output_format = "mp3",
                voice_engine = "Play3.0-mini",
                text = text,
                language = "arabic"
            };

            var body = new StringContent(JsonSerializer.Serialize(requestBody));
            body.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // Send POST request
            var response = await httpClient.PostAsync("https://api.play.ht/api/v2/tts/stream", body);
            if (!response.IsSuccessStatusCode)
            {
                return BadRequest("Failed to generate speech.");
            }

            // Generate a unique filename
            string fileName = $"{Guid.NewGuid()}.mp3";
            string savePath = Path.Combine(_env.WebRootPath, "voice", fileName);

            // Ensure the voice directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);

            // Save response stream to file
            await using (var responseStream = await response.Content.ReadAsStreamAsync())
            await using (var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await responseStream.CopyToAsync(fileStream);
            }

            // Generate public URL
            string fileUrl = $"{Request.Scheme}://{Request.Host}/voice/{fileName}";

            return Ok(new { url = fileUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

}
