using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using StarFederation.Datastar.DependencyInjection;
using AzureFunctionsHelloWorld;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddHttpContextAccessor();
builder.Services.AddDatastar();

// In the Azure Functions isolated worker model, this middleware is required for the dependency injection used by the Datastar SDK.
builder.UseMiddleware<HttpContextAccessorMiddleware>();

builder.Build().Run();
