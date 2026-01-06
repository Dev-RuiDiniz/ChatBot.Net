using MongoDB.Bson.Serialization.Attributes;

namespace OmniChat.Domain.Entities;

public class OrganizationSettings
{
    [BsonId]
    public Guid OrganizationId { get; set; }

    // Atalhos / Respostas Rápidas
    public List<CannedResponse> CannedResponses { get; set; } = new();
    
    // Departamentos (Filas de atendimento)
    public List<Department> Departments { get; set; } = new();
}

public class CannedResponse
{
    public string Shortcut { get; set; } // "/preço"
    public string FullText { get; set; } // "Nossos planos começam em..."
}

public class Department
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } // "Financeiro", "Suporte"
    public List<Guid> AgentIds { get; set; } // Usuários alocados aqui
}