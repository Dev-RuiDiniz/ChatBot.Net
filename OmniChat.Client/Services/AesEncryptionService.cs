using System.Security.Cryptography;
using System.Text;
using OmniChat.Domain.Interfaces;

public class AesEncryptionService : IEncryptionService
{
    // Em produção, esta chave deve ser negociada via Diffie-Hellman ou derivada do login
    // Jamais hardcoded em JS/WASM público. Aqui, simulamos uma chave de sessão.
    private readonly byte[] _key; 
    private readonly byte[] _iv;

    public AesEncryptionService()
    {
        // Exemplo: Chave derivada de variavel de ambiente ou config injetada no build
        _key = Encoding.UTF8.GetBytes("UmaChaveDe32BytesSuperSegura1234"); 
        _iv = Encoding.UTF8.GetBytes("UmVetorDe16Bytes"); 
    }

    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }
        return Convert.ToBase64String(ms.ToArray());
    }

    public string Decrypt(string cipherText)
    {
        
    }

    // Decrypt segue a lógica inversa...
}