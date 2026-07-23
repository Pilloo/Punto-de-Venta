using System.Security.Cryptography;
using Services.CryptoService.Interface;

namespace Services.CryptoService.Implementation;

public class CryptoService : ICryptoService
{
    /// <summary>
    /// Loads an ECDsa key from the specified PEM file path.
    /// </summary>
    /// <param name="keyPath">The file path to the PEM file containing the ECDsa key.</param>
    /// <returns>Returns the loaded ECDsa key as an instance of <see cref="ECDsa"/>.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist at the provided path.</exception>
    public ECDsa LoadEcdsaKey(string keyPath)
    {
        ECDsa ecdsaKey = ECDsa.Create();

        if (!File.Exists(keyPath))
        {
            throw new FileNotFoundException("File not found", keyPath);
        }

        string pemContent = File.ReadAllText(keyPath);
        ecdsaKey.ImportFromPem(pemContent);

        return ecdsaKey;
    }
}
