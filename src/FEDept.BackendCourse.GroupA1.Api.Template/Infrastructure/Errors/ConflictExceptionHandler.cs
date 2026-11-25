using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Infrastructure.Errors;

public sealed class ConflictExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception exception, CancellationToken ct)
    {
        if (exception is not EmailAlreadyTakenException) return false;

        http.Response.StatusCode = StatusCodes.Status409Conflict;
        await http.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Title = "Email is already registered",
            Type = AuthErrorTypes.EmailTaken,
            Status = StatusCodes.Status409Conflict,
        }, ct);
        return true;
    }
}
