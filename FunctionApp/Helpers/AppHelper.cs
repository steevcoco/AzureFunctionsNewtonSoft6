using System;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;


namespace Sc.Azure.Helpers
{
	/// <summary>
	/// Function App helpers.
	/// </summary>
	public static class AppHelper
	{
		/// <summary>
		/// Fetches an application setting --- is an Environment Variable in the Process.
		/// </summary>
		/// <param name="name">Setting name.</param>
		/// <returns>The value.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static string GetApplicationSetting(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException(nameof(name));
			return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
		}

		/// <summary>
		/// Fetches a connection string --- is a <see cref="ConfigurationManager"/> ConnectionString.
		/// </summary>
		/// <param name="name">Connection strings name.</param>
		/// <returns>The value.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ConfigurationErrorsException"></exception>
		public static string GetConnectionString(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException(nameof(name));
			return ConfigurationManager.ConnectionStrings[name]
					.ConnectionString;
		}

		/// <summary>
		/// Finds a CurrentUser certificate.
		/// </summary>
		/// <param name="findValue">Required.</param>
		/// <param name="x509FindType">Defaults to <see cref="X509FindType.FindByThumbprint"/>.</param>
		/// <param name="validOnly">If true, only valid certs are returned: set this false for
		/// self-signed certificates.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static X509Certificate2 FindCertificateByThumbprint(
				string findValue,
				X509FindType x509FindType = X509FindType.FindByThumbprint,
				bool validOnly = true)
		{
			if (string.IsNullOrWhiteSpace(findValue))
				throw new ArgumentNullException(nameof(findValue));
			X509Store x509Store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
			try {
				x509Store.Open(OpenFlags.ReadOnly);
				X509Certificate2Collection collection
						= x509Store.Certificates.Find(
								x509FindType,
								findValue,
								validOnly);
				return collection.Count == 0
						? null
						: collection[0];
			} finally {
				x509Store.Close();
			}
		}
	}
}
