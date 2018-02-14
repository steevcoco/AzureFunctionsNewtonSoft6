using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Sc.Azure.Helpers.ActiveDirectory;


namespace Sc.Azure.Helpers.KeyVault
{
	/// <summary>
	/// Helpers for woring with KeyStores.
	/// </summary>
	public static class KeyVaultHelper
	{
		/// <summary>
		/// Fetches a certificate from a KeyVault, using an Actve Directory client and secret.
		/// </summary>
		/// <param name="clientAssertionCertificate">Required Ad certificate.</param>
		/// <param name="vaultBaseUri">Required KeyStore Uri.</param>
		/// <param name="secretName">Required certificate name.</param>
		/// <param name="cancellationToken">Optional</param>
		/// <returns>Not null.</returns>
		public static async Task<SecretBundle> GetSecret(
				ClientAssertionCertificate clientAssertionCertificate,
				string vaultBaseUri,
				string secretName,
				CancellationToken cancellationToken = default)
		{
			using (ClientAssertionCertificateAccessTokenHandler tokenHandler
					= new ClientAssertionCertificateAccessTokenHandler(clientAssertionCertificate)) {
				return await new KeyVaultClient(tokenHandler.GetToken).GetSecretAsync(
						vaultBaseUri,
						secretName,
						cancellationToken);
			}
		}

		/// <summary>
		/// Fetches a certificate from a KeyVault, using an Actve Directory client and secret.
		/// </summary>
		/// <param name="clientAssertionCertificate">Required Ad certificate.</param>
		/// <param name="keyVaultBaseUri">Required KeyStore Uri.</param>
		/// <param name="certifcateName">Required certificate name.</param>
		/// <param name="cancellationToken">Optional</param>
		/// <returns>Not null.</returns>
		public static async Task<CertificateBundle> GetCertificate(
				ClientAssertionCertificate clientAssertionCertificate,
				string keyVaultBaseUri,
				string certifcateName,
				CancellationToken cancellationToken = default)
		{
			using (ClientAssertionCertificateAccessTokenHandler tokenHandler
					= new ClientAssertionCertificateAccessTokenHandler(clientAssertionCertificate)) {
				return await new KeyVaultClient(tokenHandler.GetToken).GetCertificateAsync(
						keyVaultBaseUri,
						certifcateName,
						cancellationToken);
			}
		}


		/// <summary>
		/// Fetches a certificate from a KeyVault, using an Actve Directory client and secret.
		/// </summary>
		/// <param name="accessTokenClientId">Required Ad client.</param>
		/// <param name="accessTokenClientSecret">Required Ad secret.</param>
		/// <param name="vaultBaseUri">Required KeyStore Uri.</param>
		/// <param name="secretName">Required certificate name.</param>
		/// <param name="cancellationToken">Optional</param>
		/// <returns>Not null.</returns>
		public static async Task<SecretBundle> GetSecret(
				string accessTokenClientId,
				SecureString accessTokenClientSecret,
				string vaultBaseUri,
				string secretName,
				CancellationToken cancellationToken = default)
		{
			using (ClientSecretAccessTokenHandler tokenHandler
					= new ClientSecretAccessTokenHandler(
							accessTokenClientId,
							accessTokenClientSecret)) {
				return await new KeyVaultClient(tokenHandler.GetToken).GetSecretAsync(
						vaultBaseUri,
						secretName,
						cancellationToken);
			}
		}

		/// <summary>
		/// Fetches a certificate from a KeyVault, using an Actve Directory client and secret.
		/// </summary>
		/// <param name="accessTokenClientId">Required Ad client.</param>
		/// <param name="accessTokenClientSecret">Required Ad secret.</param>
		/// <param name="keyVaultBaseUri">Required KeyStore Uri.</param>
		/// <param name="certifcateName">Required certificate name.</param>
		/// <param name="cancellationToken">Optional</param>
		/// <returns>Not null.</returns>
		public static async Task<CertificateBundle> GetCertificate(
				string accessTokenClientId,
				SecureString accessTokenClientSecret,
				string keyVaultBaseUri,
				string certifcateName,
				CancellationToken cancellationToken = default)
		{
			using (ClientSecretAccessTokenHandler tokenHandler
					= new ClientSecretAccessTokenHandler(
							accessTokenClientId,
							accessTokenClientSecret)) {
				return await new KeyVaultClient(tokenHandler.GetToken).GetCertificateAsync(
						keyVaultBaseUri,
						certifcateName,
						cancellationToken);
			}
		}


		/// <summary>
		/// Fetches a certificate from a KeyVault, using an Actve Directory client and secret.
		/// </summary>
		/// <param name="accessTokenClientId">Required Ad client.</param>
		/// <param name="accessTokenClientSecret">Required Ad secret.</param>
		/// <returns>Not null.</returns>
		public static KeyVaultClient GetKeyVaultClient(
				string accessTokenClientId,
				SecureString accessTokenClientSecret)
		{
			ClientSecretAccessTokenHandler tokenHandler
					= new ClientSecretAccessTokenHandler(
							accessTokenClientId,
							accessTokenClientSecret);
			return new KeyVaultClient(tokenHandler.GetToken);
		}
	}
}
