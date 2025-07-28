using System.Security.Cryptography;
using System.Text;

namespace CarRental.Helpers;

public class AesEncryptionHelper: IAesEncryptionHelper
{
    private readonly IConfiguration _configuration;

    public AesEncryptionHelper(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public string Encrypt(string plainText)
    {
        var key = _configuration["Encrypt:Key"];
        var IV = _configuration["Encrypt:IV"];
        
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.IV = Encoding.UTF8.GetBytes(IV);

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    public string Decrypt(string cipherText)
    {
        var key = _configuration["Encrypt:Key"];
        var IV = _configuration["Encrypt:IV"];
        
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.IV = Encoding.UTF8.GetBytes(IV);

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        var buffer = Convert.FromBase64String(cipherText);

        using var ms = new MemoryStream(buffer);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);

        return sr.ReadToEnd();
    }
}