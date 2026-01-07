namespace OmniChat.Domain.Enums;

public enum UserRole
{
    SuperAdmin,         // Dono do SaaS
    OrganizationAdmin,  // Dono da Empresa Cliente
    Agent,              // Atendente Humano
    User                // Usuário final (Cliente do WhatsApp)
}