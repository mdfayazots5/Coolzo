using Coolzo.Api.Extensions;
using Coolzo.Contracts.Common;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult<ApiResponse<T>> Success<T>(T data, string message = "Request completed successfully.")
    {
        return Ok(ApiResponseFactory.Success(data, HttpContext.TraceIdentifier, message));
    }
}
