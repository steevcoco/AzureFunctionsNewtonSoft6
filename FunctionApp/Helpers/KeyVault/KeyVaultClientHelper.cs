using System;
using Microsoft.Azure.KeyVault;


namespace Sc.Azure.Helpers.KeyVault
{
	/// <summary>
	/// Implements hash and sign for a <see cref="KeyVaultClient"/>. Implements <see cref="IDisposable"/>
	/// and a finalizer.
	/// </summary>
	public sealed class KeyVaultClientHelper
			: ResourceLock<KeyVaultClient>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="keyVaultClient">Not null.</param>
		public KeyVaultClientHelper(KeyVaultClient keyVaultClient)
				: base(keyVaultClient) { }
	}
}
