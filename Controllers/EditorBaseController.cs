using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Portfolio.Controllers;

/// <summary>
/// EVERY editor controller inherits from this class, and [Authorize] is inherited with it.
/// A signed-out visitor is redirected to /Account/Login on every editor page and action.
/// </summary>
[Authorize]
public abstract class EditorBaseController : Controller
{
}
