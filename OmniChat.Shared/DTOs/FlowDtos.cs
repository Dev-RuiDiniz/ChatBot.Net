namespace OmniChat.Shared.DTOs;

public class FlowNodeDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Content { get; set; }
    public string Type { get; set; } // "Message", "Menu", "Input"
    public List<FlowOptionDto> Options { get; set; } = new();

    // Propriedades para renderização da árvore/grafo (se usar biblioteca de diagrama)
    public double X { get; set; }
    public double Y { get; set; }
}