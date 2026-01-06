using System.Security.Cryptography;

namespace OmniChat.Domain.ValueObjects;

public class EncryptedText
{
    // Armazena APENAS o dado criptografado (Base64)
    public string CipherText { get; private set; }
    public string IV { get; private set; } // Vetor de Inicialização único por mensagem

    private EncryptedText(string cipherText, string iv) 
    {
        CipherText = cipherText;
        IV = iv;
    }

    // Factory para criar a partir de texto plano (Criptografa na hora)
    public static EncryptedText FromPlainText(string plainText, string key)
    {
        using var aes = Aes.Create();
        aes.Key = Convert.FromBase64String(key); // Chave mestra ou por usuário
        aes.GenerateIV();
        
        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }
        
        return new EncryptedText(Convert.ToBase64String(ms.ToArray()), Convert.ToBase64String(aes.IV));
    }

    // Método para descriptografar (Somente usado em memória volátil)
    public string ToPlainText(string key)
    {
        using var aes = Aes.Create();
        aes.Key = Convert.FromBase64String(key);
        aes.IV = Convert.FromBase64String(IV);

        var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(Convert.FromBase64String(CipherText));
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        
        return sr.ReadToEnd();
    }
    
    // Construtor para ORM (EF Core) carregar do banco
    public static EncryptedText Restore(string cipherText, string iv) => new EncryptedText(cipherText, iv);
}