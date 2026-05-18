using System.Security.Cryptography;

namespace AuthModule.Core.Interfaces;

public interface ICryptoService
{
    ECDsa LoadEcdsaKey(string filePath);
}