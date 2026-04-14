namespace ExamSystem.Web.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class RequireRoleAttribute : Attribute
{
    public RequireRoleAttribute(string role)
    {
        Role = role;
    }

    public string Role { get; }
}
