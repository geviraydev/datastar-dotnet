using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using StarFederation.Datastar.DependencyInjection;
using System.Text.Json.Serialization;

namespace AzureFunctionsExample;

public class DatastarExample(IDatastarService datastarService)
{
    private readonly IDatastarService _datastarService = datastarService;

    [Function("ServeHtmlFunction")]
    public static async Task<HttpResponseData> ServeHtmlFunction(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "index.html")] HttpRequestData req)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/html; charset=utf-8");

        var htmlFilePath = Path.Combine(AppContext.BaseDirectory, "hello-world.html");
        if (!File.Exists(htmlFilePath))
        {
            response.StatusCode = HttpStatusCode.NotFound;
            await response.WriteStringAsync("HTML file not found.");
            return response;
        }

        var htmlContent = await File.ReadAllTextAsync(htmlFilePath);
        await response.WriteStringAsync(htmlContent);

        return response;
    }

    [Function("StreamElementPatches")]
    public async Task StreamElementPatches(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "stream-element-patches")] HttpRequestData req)
    {
        const string message = "Hello, Elements!";
        Signals? mySignals = await _datastarService.ReadSignalsAsync<Signals>();

        await _datastarService.PatchSignalsAsync(new { show_patch_element_message = true });

        for (var index = 1; index < message.Length; ++index)
        {
            await _datastarService.PatchElementsAsync($"""<div id="message">{message[..index]}</div>""");
            await Task.Delay(TimeSpan.FromMilliseconds(mySignals?.Delay.GetValueOrDefault(0) ?? 0));
        }

        await _datastarService.PatchElementsAsync($"""<div id="message">{message}</div>""");
    }

    [Function("StreamSignalPatches")]
    public async Task StreamSignalPatches(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "stream-signal-patches")] HttpRequestData req)
    {
        const string message = "Hello, Signals!";
        Signals? mySignals = await _datastarService.ReadSignalsAsync<Signals>();

        await _datastarService.PatchSignalsAsync(new { show_patch_element_message = false });

        for (var index = 1; index < message.Length; ++index)
        {
            await _datastarService.PatchSignalsAsync(new { signals_message = message[..index] });
            await Task.Delay(TimeSpan.FromMilliseconds(mySignals?.Delay.GetValueOrDefault(0) ?? 0));
        }

        await _datastarService.PatchSignalsAsync(new { signals_message = message });
    }

    [Function("ExecuteScript")]
    public async Task ExecuteScript(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "execute-script")] HttpRequestData req)
    {
        await _datastarService.ExecuteScriptAsync("alert('Hello! from the server 🚀')");
    }
    
    public record Signals
    {
        [JsonPropertyName("delay")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public float? Delay { get; set; } = null;
    }
}