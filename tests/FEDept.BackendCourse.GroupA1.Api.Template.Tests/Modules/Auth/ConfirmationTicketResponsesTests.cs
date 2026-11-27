using System.Text.Json;
using System.Text.Json.Serialization;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Confirm;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Register;
using FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Resend;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Xunit;

namespace FEDept.BackendCourse.GroupA1.Api.Template.Tests.Modules.Auth;

public class ConfirmationTicketResponsesTests
{
    private static readonly Guid AttemptId = Guid.NewGuid();

    private static readonly ConfirmationTicket Ticket =
        new(ChallengeMethod.Otp, AttemptId, DateTimeOffset.UtcNow.AddMinutes(10),
            TimeSpan.FromMinutes(10), TimeSpan.FromSeconds(60));

    private static readonly JsonSerializerOptions ApiJson = BuildApiJson();

    private static JsonSerializerOptions BuildApiJson()
    {
        JsonSerializerOptions options = new(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return options;
    }

    [Fact]
    public void From_Ticket_CarriesEveryContractField()
    {
        ConfirmationTicketResponse body = ConfirmationTicketResponses.From(Ticket);

        Assert.True(body.ConfirmationRequired);
        Assert.Equal("otp", body.Method);
        Assert.Equal(AttemptId, body.AttemptId);
        Assert.Equal(600, body.ExpiresInSeconds);
        Assert.Equal(60, body.ResendAfterSeconds);
    }

    [Fact]
    public void Neutral_SaysNothingToConfirm()
    {
        ConfirmationTicketResponse body = ConfirmationTicketResponses.Neutral();

        Assert.False(body.ConfirmationRequired);
        Assert.Equal(string.Empty, body.Method);
        Assert.Equal(Guid.Empty, body.AttemptId);
        Assert.Equal(0, body.ExpiresInSeconds);
        Assert.Equal(0, body.ResendAfterSeconds);
    }

    [Fact]
    public void TicketBody_SerialisesWithExactlyTheContractFieldNames()
    {
        using JsonDocument json = JsonDocument.Parse(
            JsonSerializer.Serialize(ConfirmationTicketResponses.From(Ticket), ApiJson));
        JsonElement root = json.RootElement;

        Assert.True(root.GetProperty("confirmationRequired").GetBoolean());
        Assert.Equal("otp", root.GetProperty("method").GetString());
        Assert.Equal(AttemptId, root.GetProperty("attemptId").GetGuid());
        Assert.Equal(600, root.GetProperty("expiresInSeconds").GetInt32());
        Assert.Equal(60, root.GetProperty("resendAfterSeconds").GetInt32());
        Assert.Equal(5, root.EnumerateObject().Count());
    }

    [Fact]
    public void NeutralBody_HasTheSameShapeAsTheTicketBody()
    {
        using JsonDocument json = JsonDocument.Parse(
            JsonSerializer.Serialize(ConfirmationTicketResponses.Neutral(), ApiJson));
        JsonElement root = json.RootElement;

        Assert.Equal(5, root.EnumerateObject().Count());
        Assert.False(root.GetProperty("confirmationRequired").GetBoolean());
    }

    [Fact]
    public void RegisterAndResend_SendTheSameBodyType()
    {
        IResult fromRegister = RegisterResponses.From(
            RegisterResult.Pending(Guid.NewGuid(), "a@b.c", Ticket));
        IResult fromResend = ResendResponses.From(ResendResult.Issued(Ticket));

        var registerBody = Assert.IsType<Accepted<ConfirmationTicketResponse>>(fromRegister);
        var resendBody = Assert.IsType<Accepted<ConfirmationTicketResponse>>(fromResend);

        Assert.Equal(
            JsonSerializer.Serialize(registerBody.Value, ApiJson),
            JsonSerializer.Serialize(resendBody.Value, ApiJson));
    }

    [Fact]
    public void CreatedBody_SerialisesWithExactlyThreeFields()
    {
        var id = Guid.NewGuid();
        var result = RegisterResponses.From(RegisterResult.Created(id, "a@b.c"));
        var created = Assert.IsType<Created<RegisterResponse>>(result);

        using JsonDocument json = JsonDocument.Parse(
            JsonSerializer.Serialize(created.Value, ApiJson));
        JsonElement root = json.RootElement;

        Assert.Equal(id, root.GetProperty("id").GetGuid());
        Assert.Equal("a@b.c", root.GetProperty("email").GetString());
        Assert.Equal("active", root.GetProperty("status").GetString());
        Assert.Equal(3, root.EnumerateObject().Count());
    }
}
