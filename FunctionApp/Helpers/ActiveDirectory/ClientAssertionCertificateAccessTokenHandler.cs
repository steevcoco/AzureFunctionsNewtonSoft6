using System;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;


namespace Sc.Azure.Helpers.ActiveDirectory
{
	/// <summary>
	/// Used as a disposable callback to obtain an AccessToken
	/// from a <see cref="ClientAssertionCertificate"/>. This object
	/// is disposable; and also disposes itself when the callback runs.
	/// </summary>
	public struct ClientAssertionCertificateAccessTokenHandler
			: IDisposable
	{
		private ClientAssertionCertificate clientAssertionCertificate;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="clientAssertionCertificate">Required.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public ClientAssertionCertificateAccessTokenHandler(ClientAssertionCertificate clientAssertionCertificate)
			=> this.clientAssertionCertificate
					= clientAssertionCertificate
					?? throw new ArgumentNullException(nameof(clientAssertionCertificate));


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
			AuthenticationResult authenticationResult
					= await authenticationContext.AcquireTokenAsync(
							resource,
							clientAssertionCertificate);
			Dispose();
			if (authenticationResult == null)
				throw new InvalidOperationException("Failed to obtain the AccessToken");
			return authenticationResult.AccessToken;
		}


		public void Dispose()
			=> clientAssertionCertificate = null;
	}
}
