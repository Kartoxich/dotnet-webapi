namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;

public sealed class EmailAlreadyTakenException(string email) : Exception($"Email already taken: {email}");
