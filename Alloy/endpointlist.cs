using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

[Route("/endpoints/")]
public class EndpointListController : Controller
{
    private readonly IEnumerable<EndpointDataSource> _sources;

    public EndpointListController(IEnumerable<EndpointDataSource> sources)
    {
        _sources = sources;
    }

    [HttpGet("list")]
    public async Task<IActionResult> ListEndpoints()
    {
        var endpoints = _sources.SelectMany(es => es.Endpoints).OfType<RouteEndpoint>();
        var output = endpoints.Select(ep =>
        {
            var controller = ep.Metadata
                .OfType<ControllerActionDescriptor>()
                .FirstOrDefault();
            var action = controller != null
                ? $"{controller.ControllerName}.{controller.ActionName}"
                : null;
            var controllerMethod = (controller != null) ? $"{controller.ControllerTypeInfo.FullName}:{controller.MethodInfo.Name}"
                : null;
            return new
            {
                Method = ep.Metadata.OfType<HttpMethodMetadata>().FirstOrDefault()?.HttpMethods?[0],
                Route = $"/{ep.RoutePattern.RawText.TrimStart('/')}",
                Action = action,
                ControllerMethod = controllerMethod
            };
        }
        );
        return Json(output, new JsonSerializerOptions { WriteIndented = true });
    }
}