using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Core.Interfaces;

namespace Infrastructure.Services
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
            ecdsaKey.ImportFromPem(keyPath);

            return ecdsaKey;
        }
    }
}
