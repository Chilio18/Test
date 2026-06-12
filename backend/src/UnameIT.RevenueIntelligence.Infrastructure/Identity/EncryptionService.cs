using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using UnameIT.RevenueIntelligence.Application.Common.Interfaces;

namespace UnameIT.RevenueIntelligence.Infrastructure.Identity;

public class EncryptionOptions
{
    public string Key { get; set; } = string.Empty;
}

public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    public EncryptionService(IOptions<EncryptionOptions> options)
    {
        var keyBytes = Convert.FromBase64String(options.Value.Key);
        _key = keyBytes.Length == 32 ? keyBytes : SHA256.HashData(keyBytes);
    }

    public string Encrypt(string plaintext)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();
        using var encryptor = aes.CreateEncryptor();
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var cipherBytes = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);
        var result = new byte[aes.IV.Length + cipherBytes.Length];
        aes.IV.CopyTo(result, 0);
        cipherBytes.CopyTo(result, aes.IV.Length);
        return Convert.ToBase64String(result);
    }

    public string Decrypt(string ciphertext)
    {
        var data = Convert.FromBase64String(ciphertext);
        using var aes = Aes.Create();
        aes.Key = _key;
        var iv = data[..16];
        var cipher = data[16..];
        aes.IV = iv;
        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }
}
