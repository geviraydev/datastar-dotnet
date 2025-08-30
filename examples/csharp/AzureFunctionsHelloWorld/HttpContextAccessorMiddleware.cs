
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace AzureFunctionsHelloWorld;

public class HttpContextAccessorMiddleware(IHttpContextAccessor httpContextAccessor) : IFunctionsWorkerMiddleware
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var httpContext = context.GetHttpContext();
        if (httpContext != null)
        {
            _httpContextAccessor.HttpContext = httpContext;
        }

        try
        {
            await next(context);
        }
        finally
        {
            if (httpContext != null)
            {
                _httpContextAccessor.HttpContext = null;
            }
        }
    }
}
