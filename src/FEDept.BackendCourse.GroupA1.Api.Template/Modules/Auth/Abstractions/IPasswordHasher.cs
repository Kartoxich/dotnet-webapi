namespace FEDept.BackendCourse.GroupA1.Api.Template.Modules.Auth.Abstractions;

public interface IPasswordHasher
{
    string Hash(string password);
}
