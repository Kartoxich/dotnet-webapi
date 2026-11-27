using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Resend;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Xunit;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Tests.Modules.Auth.Resend;

public class ResendResponsesTests
{
    private static readonly Guid AttemptId = Guid.NewGuid();

    private static readonly ConfirmationTicket Ticket =
        new(ChallengeMethod.Otp, AttemptId, DateTimeOffset.UtcNow.AddMinutes(10),
            TimeSpan.FromMinutes(10), TimeSpan.FromSeconds(60));

    [Fact]
    public void From_Issued_Is202WithTheTicketFields()
    {
        var result = ResendResponses.From(ResendResult.Issued(Ticket));

        var accepted = Assert.IsType<Accepted<ConfirmationTicketResponse>>(result);
        Assert.Equal(StatusCodes.Status202Accepted, accepted.StatusCode);
        Assert.Null(accepted.Location);

        ConfirmationTicketResponse body = accepted.Value!;
        Assert.True(body.ConfirmationRequired);
        Assert.Equal("otp", body.Method);
        Assert.Equal(AttemptId, body.AttemptId);
        Assert.Equal(600, body.ExpiresInSeconds);
        Assert.Equal(60, body.ResendAfterSeconds);
    }

    [Fact]
    public void From_Neutral_Is202WithTheSameShapeButNothingToConfirm()
    {
        var result = ResendResponses.From(ResendResult.Accepted);

        var accepted = Assert.IsType<Accepted<ConfirmationTicketResponse>>(result);
        Assert.Equal(StatusCodes.Status202Accepted, accepted.StatusCode);
        Assert.False(accepted.Value!.ConfirmationRequired);
        Assert.Equal(Guid.Empty, accepted.Value.AttemptId);
    }

    [Fact]
    public void From_InvalidRequest_IsAValidationProblem()
    {
        var result = ResendResponses.From(ResendResult.InvalidRequest(
            new Dictionary<string, string[]> { ["email"] = ["A value is required."] }));

        var problem = Assert.IsType<ValidationProblem>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
        Assert.Contains("email", problem.ProblemDetails.Errors.Keys);
    }
}
