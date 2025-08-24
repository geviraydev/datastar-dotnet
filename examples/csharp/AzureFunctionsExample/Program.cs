using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using StarFederation.Datastar.DependencyInjection;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();
builder.Services.AddDatastar();

// Add your custom middleware to populate HttpContextAccessor
builder.UseMiddleware<HttpContextMiddleware>();

builder.Build().Run();

// Custom middleware to populate HttpContextAccessor
public class HttpContextMiddleware : IFunctionsWorkerMiddleware
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextMiddleware(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        // Ensure HttpContext is available on the accessor if it's an HTTP trigger
        if (context.GetHttpContext() != null && _httpContextAccessor.HttpContext == null)
        {
            _httpContextAccessor.HttpContext = context.GetHttpContext();
        }

        await next(context);
    }
}