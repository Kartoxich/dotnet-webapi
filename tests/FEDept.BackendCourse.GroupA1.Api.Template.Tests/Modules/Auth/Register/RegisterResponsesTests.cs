using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Xunit;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Tests.Modules.Auth.Register;

public class RegisterResponsesTests
{
    private static readonly Guid AttemptId = Guid.NewGuid();
    private static readonly DateTimeOffset ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10);

    private static readonly ConfirmationTicket Ticket =
        new(ChallengeMethod.Otp, AttemptId, ExpiresAt,
            TimeSpan.FromMinutes(10), TimeSpan.FromSeconds(60));

    [Fact]
    public void From_PendingResult_Is202WithTheContractFields()
    {
        var result = RegisterResponses.From(RegisterResult.Pending(Guid.NewGuid(), "a@b.c", Ticket));

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
    public void From_CreatedResult_Is201WithIdEmailAndTheResultStatus()
    {
        var id = Guid.NewGuid();

        var result = RegisterResponses.From(RegisterResult.Created(id, "a@b.c"));

        var created = Assert.IsType<Created<RegisterResponse>>(result);
        Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
        Assert.Equal($"/auth/users/{id}", created.Location);

        RegisterResponse body = created.Value!;
        Assert.Equal(id, body.Id);
        Assert.Equal("a@b.c", body.Email);
        Assert.Equal("active", body.Status);
    }

    [Fact]
    public void From_InvalidRequest_IsAValidationProblem()
    {
        var result = RegisterResponses.From(RegisterResult.InvalidRequest(
            new Dictionary<string, string[]> { ["email"] = ["Email is required"] }));

        var problem = Assert.IsType<ValidationProblem>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
        Assert.Contains("email", problem.ProblemDetails.Errors.Keys);
    }
}
