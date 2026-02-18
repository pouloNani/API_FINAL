
// Api/Controllers/BaseApiController.cs
using Core.POCO;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
public class BaseApiController : ControllerBase
{
    protected  ActionResult NotFound(string message = "Resource introuvable.") =>
        base.StatusCode(404, new AppError(404, message));

    protected  ActionResult BadRequest(string message) =>
        base.StatusCode(400, new AppError(400, message));

    protected  ActionResult Unauthorized(string message = "Non autorisé.") =>
        base.StatusCode(401, new AppError(401, message));

    protected ActionResult Forbidden(string message = "Accès refusé.") =>
        base.StatusCode(403, new AppError(403, message));
}