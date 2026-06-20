using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Security.Cryptography;
using System.Text;

namespace foll_backend.Care.Infrastructure.Persistence.EFC.Converters;

public class EncryptedDictionaryConverter : ValueConverter<Dictionary<string, string>, string>
{
    private static readonly string EncryptionKey = GetEncryptionKey();

    public EncryptedDictionaryConverter()
        : base(
            v => EncryptAndSerialize(v),
            v => DeserializeAndDecrypt(v))
    {
    }
    
    private static string GetEncryptionKey()
    {
        // Construimos la configuración para leer el appsettings.json o variables de entorno
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables();

        var configuration = builder.Build();
        var key = configuration["Encryption:Key"];

        if (string.IsNullOrEmpty(key))
            throw new InvalidOperationException("La llave de cifrado no se encontró en appsettings.json o en las variables de entorno.");

        return key;
    }

    private static string EncryptAndSerialize(Dictionary<string, string> dictionary)
    {
        var json = JsonSerializer.Serialize(dictionary, (JsonSerializerOptions?)null);
        return EncryptString(json);
    }

    private static Dictionary<string, string> DeserializeAndDecrypt(string encryptedString)
    {
        if (string.IsNullOrWhiteSpace(encryptedString)) return new Dictionary<string, string>();
        var json = DecryptString(encryptedString);
        return JsonSerializer.Deserialize<Dictionary<string, string>>(json, (JsonSerializerOptions?)null) 
               ?? new Dictionary<string, string>();
    }

    // Lógica simétrica ultra rápida usando AES para  disponibilidad
    private static string EncryptString(string text)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(EncryptionKey.PadRight(32).Substring(0, 32));
        aes.IV = new byte[16]; // Vector de inicialización estático para simplificar (mejorar en prod)
        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        var bytes = Encoding.UTF8.GetBytes(text);
        var encrypted = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
        
        return Convert.ToBase64String(encrypted);
    }

    private static string DecryptString(string encryptedText)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(EncryptionKey.PadRight(32).Substring(0, 32));
        aes.IV = new byte[16];
        var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        var bytes = Convert.FromBase64String(encryptedText);
        var decrypted = decryptor.TransformFinalBlock(bytes, 0, bytes.Length);
        return Encoding.UTF8.GetString(decrypted);
    }
}