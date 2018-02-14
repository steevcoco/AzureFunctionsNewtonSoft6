using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.IdentityModel.Clients.ActiveDirectory;


namespace Sc.Azure.Helpers.MissingImplementations
{
	/// <summary>
	/// Implementation of <see cref="ISecureClientSecret"/>.
	/// </summary>
	public sealed class SecureClientSecretImpl
			: ISecureClientSecret
	{
		internal static string ClientSecret
			=> "client_secret";


		private SecureString secret;


		/// <summary>
		/// Required Constructor
		/// </summary>
		/// <param name="secret">SecureString secret. Required and cannot be null.</param>
		public SecureClientSecretImpl(SecureString secret)
			=> this.secret = secret ?? throw new ArgumentNullException(nameof(secret));


		public void ApplyTo(IDictionary<string, string> parameters)
		{
			char[] output = new char[secret.Length];
			IntPtr secureStringPtr = Marshal.SecureStringToCoTaskMemUnicode(secret);
			for (int i = 0; i < secret.Length; i++) {
				output[i] = (char)Marshal.ReadInt16(secureStringPtr, i * 2);
			}
			Marshal.ZeroFreeCoTaskMemUnicode(secureStringPtr);
			parameters[SecureClientSecretImpl.ClientSecret] = new string(output);
			if ((secret != null)
					&& !secret.IsReadOnly()) {
				secret.Clear();
			}
			secret = null;
		}
	}
}
