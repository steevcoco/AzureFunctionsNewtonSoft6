using System;
using System.Security.Cryptography;
using Microsoft.Azure.KeyVault.WebKey;


namespace Sc.Azure.Helpers.KeyVault
{
	public static class HashAlgorithmNameHelper
	{
		/// <summary>
		/// Converts a <see cref="HashAlgorithmName"/> to a <see cref="JsonWebKeySignatureAlgorithm"/>
		/// constant. Will return <see cref="JsonWebKeySignatureAlgorithm.RS512"/>,
		/// <see cref="JsonWebKeySignatureAlgorithm.RS384"/>, or
		/// <see cref="JsonWebKeySignatureAlgorithm.RS256"/>; and otherwise
		/// <see cref="JsonWebKeySignatureAlgorithm.RSNULL"/>.
		/// </summary>
		/// <param name="hashAlgorithmName">This algorithm.</param>
		/// <returns>Not null.</returns>
		public static string ToJson(this HashAlgorithmName hashAlgorithmName)
		{
			if (string.Equals(
					HashAlgorithmName.SHA512.Name,
					hashAlgorithmName.Name,
					StringComparison.InvariantCultureIgnoreCase))
				return JsonWebKeySignatureAlgorithm.RS512;
			if (string.Equals(
					HashAlgorithmName.SHA384.Name,
					hashAlgorithmName.Name,
					StringComparison.InvariantCultureIgnoreCase))
				return JsonWebKeySignatureAlgorithm.RS384;
			if (string.Equals(
					HashAlgorithmName.SHA256.Name,
					hashAlgorithmName.Name,
					StringComparison.InvariantCultureIgnoreCase))
				return JsonWebKeySignatureAlgorithm.RS256;
			return JsonWebKeySignatureAlgorithm.RSNULL;
		}
	}
}
