using System.Security.Cryptography;

namespace Services.CryptoService.Interface;

public interface ICryptoService
{
    ECDsa LoadEcdsaKey(string filePath);
}