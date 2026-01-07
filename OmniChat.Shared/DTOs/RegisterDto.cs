namespace OmniChat.Shared.DTOs;

public class RegisterDto
{
    public string CompanyName { get; set; }
    public string PlanName { get; set; } // "Básico", "Profissional"
    
    // Dados do Admin
    public string AdminPhone { get; set; }
    public string AdminEmail { get; set; }
    public string Password { get; set; }
}