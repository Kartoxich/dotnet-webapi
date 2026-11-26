using System.Text.Json;
using System.Text.Json.Serialization;
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
        new(ChallengeMethod.Otp, AttemptId, ExpiresAt, TimeSpan.FromMinutes(10));

    [Fact]
    public void From_PendingResult_Is202WithTheContractFields()
    {
        var result = RegisterResponses.From(RegisterResult.Pending(Guid.NewGuid(), "a@b.c", Ticket));

        var accepted = Assert.IsType<Accepted<RegisterPendingResponse>>(result);
        Assert.Equal(StatusCodes.Status202Accepted, accepted.StatusCode);
        Assert.Null(accepted.Location);

        RegisterPendingResponse body = accepted.Value!;
        Assert.True(body.ConfirmationRequired);
        Assert.Equal("otp", body.Method);
        Assert.Equal(AttemptId, body.AttemptId);
        Assert.Equal(600, body.ExpiresInSeconds);
        Assert.Equal(600, body.ResendAfterSeconds);
    }

    [Fact]
    public void PendingBody_SerialisesWithTheContractFieldNames_AndNoSecret()
    {
        var result = RegisterResponses.From(RegisterResult.Pending(Guid.NewGuid(), "a@b.c", Ticket));
        var accepted = Assert.IsType<Accepted<RegisterPendingResponse>>(result);

        JsonSerializerOptions options = new(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

        using JsonDocument json = JsonDocument.Parse(
            JsonSerializer.Serialize(accepted.Value, options));
        JsonElement root = json.RootElement;

        Assert.True(root.GetProperty("confirmationRequired").GetBoolean());
        Assert.Equal("otp", root.GetProperty("method").GetString());
        Assert.Equal(AttemptId, root.GetProperty("attemptId").GetGuid());
        Assert.Equal(600, root.GetProperty("expiresInSeconds").GetInt32());
        Assert.Equal(600, root.GetProperty("resendAfterSeconds").GetInt32());
        Assert.Equal(5, root.EnumerateObject().Count());
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
