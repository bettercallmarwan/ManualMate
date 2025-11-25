using ManualMate.API.Controllers.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ManualMate.API.Controllers
{
    public static class ControllerBaseExtension
    {
        public static IActionResult GetResponse<T>(this ControllerBase controllerBase, Result<T> result)
        {
            switch (result.StatusCode)
            {
                case HttpStatusCode.OK:
                    return controllerBase.Ok(result.Value);
                case HttpStatusCode.Unauthorized:
                    return controllerBase.Unauthorized(new { error = result.Error });
                case HttpStatusCode.BadRequest:
                    return controllerBase.BadRequest(new { error = result.Error });
                case HttpStatusCode.NotFound:
                    return controllerBase.NotFound(new { error = result.Error });
                case HttpStatusCode.Forbidden:
                    return controllerBase.Forbid();
                default:
                    return controllerBase.StatusCode(500); ;
            }
        }
    }
}
