using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Resend;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Xunit;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Tests.Modules.Auth;

public class MailFailureTests
{
    [Fact]
    public void RegisterResponses_MailFailed_Is503WithTheMailNotSentType()
    {
        IResult result = RegisterResponses.From(RegisterResult.MailFailed(Guid.NewGuid(), "a@b.c"));

        var problem = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, problem.StatusCode);
        Assert.Equal(AuthErrorTypes.MailNotSent, problem.ProblemDetails.Type);
    }

    [Fact]
    public void ResendResponses_MailFailed_Is503WithTheMailNotSentType()
    {
        IResult result = ResendResponses.From(ResendResult.MailFailed);

        var problem = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, problem.StatusCode);
        Assert.Equal(AuthErrorTypes.MailNotSent, problem.ProblemDetails.Type);
    }
}
