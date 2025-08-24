# Azure Functions Integration

`DatastarService` was made `public` to allow direct instantiation within Azure Functions, as the isolated worker model does not support automatic dependency injection for all types. Additionally, a custom `HttpContextMiddleware` was implemented to ensure `HttpContext` is available for `DatastarService`.

To run the Azure Functions host, navigate to this directory and execute: `func start`