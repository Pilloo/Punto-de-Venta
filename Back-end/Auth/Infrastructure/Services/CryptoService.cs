using System.Security.Cryptography;
using AuthModule.Core.Interfaces;

namespace AuthModule.Infrastructure.Services
{
    public class CryptoService : ICryptoService
    {
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
}
