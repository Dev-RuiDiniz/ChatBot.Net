namespace OmniChat.Domain.Flows;

public class FlowOption
{
    public string Label { get; set; } // "1" ou "Financeiro"
    public string TargetNodeId { get; set; } // "node_financeiro"
}