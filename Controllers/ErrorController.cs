using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Portfolio.Controllers;

/// <summary>Friendly pages. /Error = unexpected exception. /Error/404 = id not found. Reached through the middleware in Program.cs.</summary>
[AllowAnonymous]
[IgnoreAntiforgeryToken]
public class ErrorController : Controller
{
    private readonly ILogger<ErrorController> _log;
    public ErrorController(ILogger<ErrorController> log) => _log = log;

    [Route("Error")]
    public IActionResult Index()
    {
        var ex = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;
        if (ex != null) _log.LogError(ex, "Unhandled exception");     // details go to the log, not to the visitor
        return View("Index");
    }

    [Route("Error/{code:int}")]
    public IActionResult Status(int code) => code == 404 ? View("NotFound") : View("Index");
}
