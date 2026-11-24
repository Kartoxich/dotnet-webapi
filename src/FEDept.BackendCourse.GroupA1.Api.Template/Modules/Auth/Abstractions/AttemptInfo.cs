namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;

public sealed record AttemptInfo(Guid Id, byte[] SecretHash, int Attempts);
