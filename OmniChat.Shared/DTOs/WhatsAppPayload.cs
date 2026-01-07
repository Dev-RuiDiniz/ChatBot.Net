namespace OmniChat.Shared.DTOs;

public class WhatsAppPayload
{
    public List<WhatsAppEntry> Entry { get; set; }
}

public class WhatsAppEntry
{
    public List<WhatsAppChange> Changes { get; set; }
}

public class WhatsAppChange
{
    public WhatsAppValue Value { get; set; }
}

public class WhatsAppValue
{
    public List<WhatsAppMessage> Messages { get; set; }
    public List<WhatsAppContact> Contacts { get; set; }
}

public class WhatsAppMessage
{
    public string From { get; set; }
    public string Id { get; set; }
    public string Timestamp { get; set; }
    public string Type { get; set; }
    public WhatsAppText Text { get; set; }
}

public class WhatsAppText
{
    public string Body { get; set; }
}

public class WhatsAppContact
{
    public string Wa_Id { get; set; }
    public WhatsAppProfile Profile { get; set; }
}

public class WhatsAppProfile
{
    public string Name { get; set; }
}