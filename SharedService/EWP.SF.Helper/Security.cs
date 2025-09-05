using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace EWP.SF.Helper;

#pragma warning disable
public static class Security
{
	public static string GetSHA1(this string text)
	{
		using SHA1 sha1 = SHA1.Create();
		byte[] originalText = Encoding.Default.GetBytes(text);
		byte[] hash = sha1.ComputeHash(originalText);
		StringBuilder strBld = new();
		foreach (byte i in hash)
		{
			strBld.AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", i);
		}
		return strBld.ToString();
	}
	
	public static bool validateIdHash(int id, string hash, string salt = "|")
	{
		using MD5 md5Hash = MD5.Create();
		return hash.Equals(md5Hash.FromString(salt + id.ToString(CultureInfo.InvariantCulture)), StringComparison.InvariantCultureIgnoreCase);
	}

	public static string getHash(int id, string salt = "|")
	{
		using MD5 md5Hash = MD5.Create();
		return md5Hash.FromString(salt + id.ToString(CultureInfo.InvariantCulture)).ToUpperInvariant();
	}

	public static string getHash(string textId)
	{
		using MD5 md5Hash = MD5.Create();
		return md5Hash.FromString(textId).ToUpperInvariant();
	}

	public static async Task<string> sslEncryptAsync(this string plainText, string passphrase)
	{
		// generate salt
		byte[] salt = new byte[8];
		using RNGCryptoServiceProvider rng = new();
		rng.GetNonZeroBytes(salt);
		DeriveKeyAndIV(passphrase, salt, out byte[] key, out byte[] iv);

		// encrypt bytes
		byte[] encryptedBytes = await EncryptStringToBytesAes(plainText, key, iv).ConfigureAwait(false);

		// add salt as first 8 bytes
		byte[] encryptedBytesWithSalt = new byte[salt.Length + encryptedBytes.Length + 8];
		Buffer.BlockCopy(Encoding.ASCII.GetBytes("Salted__"), 0, encryptedBytesWithSalt, 0, 8);
		Buffer.BlockCopy(salt, 0, encryptedBytesWithSalt, 8, salt.Length);
		Buffer.BlockCopy(encryptedBytes, 0, encryptedBytesWithSalt, salt.Length + 8, encryptedBytes.Length);

		// base64 encode
		return Convert.ToBase64String(encryptedBytesWithSalt);
	}

	public static async Task<string> sslDecryptAsync(this string encrypted, string passphrase)
	{
		// base 64 decode
		byte[] encryptedBytesWithSalt = Convert.FromBase64String(encrypted);

		// extract salt (first 8 bytes of encrypted)
		byte[] salt = new byte[8];
		byte[] encryptedBytes = new byte[encryptedBytesWithSalt.Length - salt.Length - 8];
		Buffer.BlockCopy(encryptedBytesWithSalt, 8, salt, 0, salt.Length);
		Buffer.BlockCopy(encryptedBytesWithSalt, salt.Length + 8, encryptedBytes, 0, encryptedBytes.Length);

		// get key and iv
		DeriveKeyAndIV(passphrase, salt, out byte[] key, out byte[] iv);
		return await DecryptStringFromBytesAes(encryptedBytes, key, iv).ConfigureAwait(false);
	}

	private static void DeriveKeyAndIV(string passphrase, byte[] salt, out byte[] key, out byte[] iv)
	{
		// generate key and iv
		List<byte> concatenatedHashes = new(48);

		byte[] password = Encoding.UTF8.GetBytes(passphrase);
		byte[] currentHash = [];
		using MD5 md5 = MD5.Create();
		bool enoughBytesForKey = false;

		// See http://www.openssl.org/docs/crypto/EVP_BytesToKey.html#KEY_DERIVATION_ALGORITHM
		while (!enoughBytesForKey)
		{
			int preHashLength = currentHash.Length + password.Length + salt.Length;
			byte[] preHash = new byte[preHashLength];

			Buffer.BlockCopy(currentHash, 0, preHash, 0, currentHash.Length);
			Buffer.BlockCopy(password, 0, preHash, currentHash.Length, password.Length);
			Buffer.BlockCopy(salt, 0, preHash, currentHash.Length + password.Length, salt.Length);

			currentHash = md5.ComputeHash(preHash);
			concatenatedHashes.AddRange(currentHash);

			if (concatenatedHashes.Count >= 48)
				enoughBytesForKey = true;
		}

		key = new byte[32];
		iv = new byte[16];
		concatenatedHashes.CopyTo(0, key, 0, 32);
		concatenatedHashes.CopyTo(32, iv, 0, 16);

		md5.Clear();
	}

	private static async Task<byte[]> EncryptStringToBytesAes(string plainText, byte[] key, byte[] iv)
	{
		// Check arguments.
		if (plainText is null || plainText.Length <= 0)
			throw new ArgumentNullException(nameof(plainText));
		if (key is null || key.Length <= 0)
			throw new ArgumentNullException(nameof(key));
		if (iv is null || iv.Length <= 0)
			throw new ArgumentNullException(nameof(iv));

		try
		{
			// Create a RijndaelManaged object with the specified key and IV.
			using RijndaelManaged aesAlg = new() { Mode = CipherMode.CBC, KeySize = 256, BlockSize = 128, Key = key, IV = iv };

			// Create an encryptor to perform the stream transform.
			using ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

			// Create the streams used for encryption.
			using MemoryStream msEncrypt = new();
			using CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write);
			using StreamWriter swEncrypt = new(csEncrypt);

			//Write all data to the stream.
			await swEncrypt.WriteAsync(plainText).ConfigureAwait(false);
			await swEncrypt.FlushAsync().ConfigureAwait(false);
			swEncrypt.Close();

			// Return the encrypted bytes from the memory stream.
			return msEncrypt.ToArray();
		}
		catch { }

		return Array.Empty<byte>();
	}

	private static async Task<string> DecryptStringFromBytesAes(byte[] cipherText, byte[] key, byte[] iv)
	{
		// Check arguments.
		if (cipherText is null || cipherText.Length <= 0)
			throw new ArgumentNullException(nameof(cipherText));
		if (key is null || key.Length <= 0)
			throw new ArgumentNullException(nameof(key));
		if (iv is null || iv.Length <= 0)
			throw new ArgumentNullException(nameof(iv));

		try
		{
			// Create a RijndaelManaged object with the specified key and IV.
			using RijndaelManaged aesAlg = new() { Mode = CipherMode.CBC, KeySize = 256, BlockSize = 128, Key = key, IV = iv };

			// Create a decrytor to perform the stream transform.
			using ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

			// Create the streams used for decryption.
			using MemoryStream msDecrypt = new(cipherText);
			using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
			using StreamReader srDecrypt = new(csDecrypt);

			// Declare the string used to hold the decrypted text.
			// Read the decrypted bytes from the decrypting stream and place them in a string.
			string plaintext = await srDecrypt.ReadToEndAsync().ConfigureAwait(false);
			srDecrypt.Close();

			return plaintext;
		}
		catch { }

		return string.Empty;
	}
}
