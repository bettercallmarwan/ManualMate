using ManualMate.API.Controllers.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ManualMate.API.Controllers
{
    public static class ControllerBaseExtension
    {
        public static IActionResult GetResponse<T>(this ControllerBase controllerBase, Result<T> result)
        {
            if (result.Success)
            {
                return controllerBase.StatusCode(
                    (int)(result.StatusCode ?? HttpStatusCode.OK),
                    new { data = result.Value }
                );
            }

            return controllerBase.StatusCode(
                (int)(result.StatusCode ?? HttpStatusCode.BadRequest),
                new { error = result.Error }
            );
        }

    }
}