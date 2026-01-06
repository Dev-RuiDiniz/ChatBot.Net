namespace OmniChat.Domain.Flows;

public class FlowNode
{
    public string Id { get; set; } // "node_1"
    public string Content { get; set; } // "Olá! Escolha: 1. Financeiro, 2. Suporte"
    
    // Tipo de ação: Message, Input, Handoff (Passar para Humano), AI_Fallback
    public FlowNodeType Type { get; set; } 
    
    // Opções de resposta que levam a outros nós
    public List<FlowOption> Options { get; set; } = new();
}