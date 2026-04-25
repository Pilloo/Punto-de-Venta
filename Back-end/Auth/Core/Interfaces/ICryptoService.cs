using System.Security.Cryptography;

namespace Core.Interfaces;

public interface ICryptoService
{
    ECDsa LoadEcdsaKey(string filePath);
}