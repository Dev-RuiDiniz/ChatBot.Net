namespace OmniChat.Shared.DTOs;

public class LoginResultDto
{
    public string Token { get; set; }
    public string Role { get; set; } // "SuperAdmin", "OrganizationAdmin", "Agent"
    public DateTime Expiration { get; set; }
}