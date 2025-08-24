using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using StarFederation.Datastar.DependencyInjection;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;
using StarFederation.Datastar.FSharp;

namespace AzureFunctionsExample;

public class DatastarExample
{
    private readonly ILogger<DatastarExample> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DatastarExample(ILogger<DatastarExample> logger, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public record Signals
    {
        [JsonPropertyName("delay")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public float? Delay { get; set; } = null;
    }

    [Function("StreamElementPatches")]
    public async Task<HttpResponseData> StreamElementPatches(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "stream-element-patches")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request for element patches.");

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/event-stream");
        response.Headers.Add("Cache-Control", "no-cache");
        response.Headers.Add("Connection", "keep-alive");

        // Manually create IDatastarService using the injected IHttpContextAccessor
        IDatastarService datastarService = new DatastarService(new ServerSentEventGenerator(_httpContextAccessor));

        const string message = "Hello, Elements!";
        Signals? mySignals = await datastarService.ReadSignalsAsync<Signals>();

        await datastarService.PatchSignalsAsync(new { show_patch_element_message = true });

        for (var index = 1; index < message.Length; ++index)
        {
            await datastarService.PatchElementsAsync($"""<div id="message">{message[..index]}</div>""");
            await Task.Delay(TimeSpan.FromMilliseconds(mySignals?.Delay.GetValueOrDefault(0) ?? 0));
        }

        await datastarService.PatchElementsAsync($"""<div id="message">{message}</div>""");
        return response;
    }

    [Function("StreamSignalPatches")]
    public async Task<HttpResponseData> StreamSignalPatches(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "stream-signal-patches")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request for signal patches.");

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/event-stream");
        response.Headers.Add("Cache-Control", "no-cache");
        response.Headers.Add("Connection", "keep-alive");

        // Manually create IDatastarService using the injected IHttpContextAccessor
        IDatastarService datastarService = new DatastarService(new ServerSentEventGenerator(_httpContextAccessor));

        const string message = "Hello, Signals!";
        Signals? mySignals = await datastarService.ReadSignalsAsync<Signals>();

        await datastarService.PatchSignalsAsync(new { show_patch_element_message = false });

        for (var index = 1; index < message.Length; ++index)
        {
            await datastarService.PatchSignalsAsync(new { signals_message = message[..index] });
            await Task.Delay(TimeSpan.FromMilliseconds(mySignals?.Delay.GetValueOrDefault(0) ?? 0));
        }

        await datastarService.PatchSignalsAsync(new { signals_message = message });
        return response;
    }

    [Function("ExecuteScript")]
    public async Task<HttpResponseData> ExecuteScript(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "execute-script")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request for script execution.");

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/event-stream");
        response.Headers.Add("Cache-Control", "no-cache");
        response.Headers.Add("Connection", "keep-alive");

        // Manually create IDatastarService using the injected IHttpContextAccessor
        IDatastarService datastarService = new DatastarService(new ServerSentEventGenerator(_httpContextAccessor));

        await datastarService.ExecuteScriptAsync("alert('Hello! from the server 🚀')");
        return response;
    }

    [Function("ServeHtmlFunction")]
    public async Task<HttpResponseData> ServeHtmlFunction(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "index.html")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request to serve HTML.");

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/html; charset=utf-8");

        var htmlFilePath = Path.Combine(AppContext.BaseDirectory, "hello-world.html");
        if (!File.Exists(htmlFilePath))
        {
            _logger.LogError($"HTML file not found at: {htmlFilePath}");
            response.StatusCode = HttpStatusCode.NotFound;
            await response.WriteStringAsync("HTML file not found.");
            return response;
        }

        var htmlContent = await File.ReadAllTextAsync(htmlFilePath);
        await response.WriteStringAsync(htmlContent);

        return response;
    }
}