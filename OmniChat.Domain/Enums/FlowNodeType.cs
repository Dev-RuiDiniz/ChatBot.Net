namespace OmniChat.Domain.Flows;

public enum FlowNodeType
{ 
    Message, 
    Menu, 
    Input, // << Certifique-se que isto existe
    HandoverToHuman, 
    HandoverToAI 
}