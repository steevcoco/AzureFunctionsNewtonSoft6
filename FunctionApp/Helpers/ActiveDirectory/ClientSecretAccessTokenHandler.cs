using System;
using System.Security;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Sc.Azure.Helpers.MissingImplementations;


namespace Sc.Azure.Helpers.ActiveDirectory
{
	/// <summary>
	/// Used as a disposable callback to obtain an AccessToken
	/// from a <see cref="ClientCredential"/> --- an Active Directory client secret. This object
	/// is disposable; and also disposes itself when the callback runs.
	/// </summary>
	public struct ClientSecretAccessTokenHandler
			: IDisposable
	{
		private string clientId;
		private SecureString clientSecret;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="clientId">Required.</param>
		/// <param name="clientSecret">Required.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public ClientSecretAccessTokenHandler(string clientId, SecureString clientSecret)
		{
			this.clientId
					= string.IsNullOrWhiteSpace(clientId)
							? throw new ArgumentNullException(nameof(clientId))
							: clientId;
			this.clientSecret = clientSecret ?? throw new ArgumentNullException(nameof(clientSecret));
		}


		/// <summary>
		/// Used as a callback when obtaining an AccessToken.
		/// </summary>
		/// <param name="authority">Handler argument.</param>
		/// <param name="resource">>Handler argument.</param>
		/// <param name="scope">Handler argument</param>
		/// <returns>The AccessToken.</returns>
		public async Task<string> GetToken(string authority, string resource, string scope)
		{
			AuthenticationContext authenticationContext = new AuthenticationContext(authority);
			ClientCredential clientCredential
					= new ClientCredential(
							clientId,
							new SecureClientSecretImpl(clientSecret));
			AuthenticationResult authenticationResult
					= await authenticationContext.AcquireTokenAsync(
							resource,
							clientCredential);
			Dispose();
			if (authenticationResult == null)
				throw new InvalidOperationException("Failed to obtain the AccessToken");
			return authenticationResult.AccessToken;
		}


		public void Dispose()
		{
			clientId = null;
			clientSecret?.Dispose();
			clientSecret = null;
		}
	}
}
