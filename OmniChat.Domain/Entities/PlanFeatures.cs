namespace OmniChat.Domain.Entities;

public class PlanFeatures
{
    public bool HasInternalChat { get; set; } = true;
    public bool HasFlowChatbot { get; set; } = true;
    public bool HasAiChatbot { get; set; } = true; // "Chatbot com IA (grátis!)"
    
    // O "**" na imagem em Campanhas sugere restrições ou disponibilidade
    public bool CanSendCampaigns { get; set; } 
    public int MonthlyCampaignMessagesLimit { get; set; } // Limite oculto sugerido pelos asteriscos
    
    public bool CanImportChats { get; set; }
    public bool HasDashboards { get; set; }
    public bool HasMessageShortcuts { get; set; } // "Atalhos de mensagens"
    public bool HasDepartments { get; set; }
    
    // Diferenciais notáveis (geralmente Enterprise/Custom)
    public bool HasApiAccess { get; set; } // "API aberta"
    public bool HasTrelloIntegration { get; set; }
    public bool HasPrioritySupport { get; set; } // Instalação/Suporte
}