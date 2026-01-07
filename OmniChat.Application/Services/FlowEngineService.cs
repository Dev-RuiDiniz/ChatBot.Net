using MongoDB.Driver;
using OmniChat.Domain.Flows;
using OmniChat.Domain.MCP;
using OmniChat.Infrastructure.Persistence;

namespace OmniChat.Application.Services;

public class FlowEngineService
{
    private readonly MongoDbContext _db;
    private readonly McpService _mcpService;

    public FlowEngineService(MongoDbContext db, McpService mcpService)
    {
        _db = db;
        _mcpService = mcpService;
    }

    public async Task<FlowProcessingResult> ProcessStepAsync(McpContextSession session, string userMessage)
    {
        // 1. Carregar o Fluxo e o Nó Atual
        var flow = await _db.Flows.Find(f => f.Id == Guid.Parse(session.CurrentFlowId)).FirstOrDefaultAsync();
        var currentNode = flow.Nodes.FirstOrDefault(n => n.Id == session.CurrentNodeId);

        if (currentNode == null) return FlowProcessingResult.EndFlow();

        // 2. Lógica de Transição baseada no Tipo de Nó Atual
        FlowNode nextNode = null;

        if (currentNode.Type == FlowNodeType.Menu)
        {
            // Tenta dar match na opção (Ex: Usuário digitou "1" ou "Financeiro")
            var selectedOption = currentNode.Options.FirstOrDefault(o => 
                o.Label.Equals(userMessage, StringComparison.OrdinalIgnoreCase) || 
                userMessage == GetOptionIndex(currentNode, o)); // Lógica para detectar "1", "2"...

            if (selectedOption != null)
            {
                nextNode = flow.Nodes.FirstOrDefault(n => n.Id == selectedOption.TargetNodeId);
            }
            else
            {
                // Input inválido: Repete a mensagem atual
                return new FlowProcessingResult { HasResponse = true, Message = "Opção inválida. " + currentNode.Content };
            }
        }
        else if (currentNode.Type == FlowNodeType.Input)
        {
            // Captura o input do usuário e salva no MCP (Context Variable)
            // Ex: Nó pede "Digite seu CPF"
            session.StateVariables[currentNode.Id] = userMessage; 
            
            // Avança para o próximo nó (assumindo que Input tem apenas 1 saída padrão)
            var nextId = currentNode.Options.FirstOrDefault()?.TargetNodeId;
            nextNode = flow.Nodes.FirstOrDefault(n => n.Id == nextId);
        }

        // 3. Executar o Próximo Nó
        if (nextNode != null)
        {
            // Atualiza o estado do MCP para apontar para o novo nó
            session.CurrentNodeId = nextNode.Id;
            await _mcpService.CommitStateAsync(session);

            // Verifica ações especiais no novo nó
            if (nextNode.Type == FlowNodeType.HandoverToHuman)
            {
                session.IsHandedOverToHuman = true;
                session.IsInFlow = false;
                await _mcpService.CommitStateAsync(session);
                return new FlowProcessingResult { HasResponse = true, Message = nextNode.Content }; // "Aguarde um atendente..."
            }

            return new FlowProcessingResult { HasResponse = true, Message = nextNode.Content };
        }

        // Fim do fluxo
        session.ExitFlow();
        await _mcpService.CommitStateAsync(session);
        return FlowProcessingResult.EndFlow();
    }

    private string GetOptionIndex(FlowNode node, FlowOption option) 
    {
        // Retorna "1" se for a primeira opção, "2" se for a segunda...
        return (node.Options.IndexOf(option) + 1).ToString();
    }
}

public class FlowProcessingResult
{
    public bool HasResponse { get; set; }
    public string Message { get; set; }
    public static FlowProcessingResult EndFlow() => new() { HasResponse = false };
}