using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;

namespace OAuth
{
	public class OAuthBase
	{

		/// <summary>
		/// Provides a predefined set of algorithms that are supported officially by the protocol
		/// </summary>
		public enum SignatureTypes
		{
			HMACSHA1,
			PLAINTEXT,
			RSASHA1
		}

		/// <summary>
		/// Provides an internal structure to sort the query parameter
		/// </summary>
		protected class QueryParameter
		{
			private string name = null;
			private string value = null;

			public QueryParameter(string name, string value)
			{
				this.name = name;
				this.value = value;
			}

			public string Name
			{
				get { return name; }
			}

			public string Value
			{
				get { return value; }
			}
		}

		/// <summary>
		/// Comparer class used to perform the sorting of the query parameters
		/// </summary>
		protected class QueryParameterComparer : IComparer<QueryParameter>
		{

			#region IComparer<QueryParameter> Members

			public int Compare(QueryParameter x, QueryParameter y)
			{
				if (x.Name == y.Name)
				{
					return string.Compare(x.Value, y.Value);
				}
				else
				{
					return string.Compare(x.Name, y.Name);
				}
			}

			#endregion
		}

		protected const string OAuthVersion = "1.0";
		protected const string OAuthParameterPrefix = "oauth_";

		//
		// List of know and used oauth parameters' names
		//        
		protected const string OAuthConsumerKeyKey = "oauth_consumer_key";
		protected const string OAuthCallbackKey = "oauth_callback";
		protected const string OAuthVersionKey = "oauth_version";
		protected const string OAuthSignatureMethodKey = "oauth_signature_method";
		protected const string OAuthSignatureKey = "oauth_signature";
		protected const string OAuthTimestampKey = "oauth_timestamp";
		protected const string OAuthNonceKey = "oauth_nonce";
		protected const string OAuthTokenKey = "oauth_token";
		protected const string OAuthTokenSecretKey = "oauth_token_secret";

		protected const string HMACSHA1SignatureType = "HMAC-SHA1";
		protected const string PlainTextSignatureType = "PLAINTEXT";
		protected const string RSASHA1SignatureType = "RSA-SHA1";

		protected Random random = new Random();

		protected string unreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";

		private string ComputeHash(HashAlgorithmProvider hashAlgorithm, string data)
		{
			if (hashAlgorithm == null)
			{
				throw new ArgumentNullException("hashAlgorithm");
			}

			if (string.IsNullOrEmpty(data))
			{
				throw new ArgumentNullException("data");
			}

			return CryptographicBuffer.EncodeToBase64String(CryptographicBuffer.ConvertStringToBinary(data, BinaryStringEncoding.Utf8));
		}

		private List<QueryParameter> GetQueryParameters(string parameters)
		{
			if (parameters.StartsWith("?"))
			{
				parameters = parameters.Remove(0, 1);
			}

			List<QueryParameter> result = new List<QueryParameter>();

			if (!string.IsNullOrEmpty(parameters))
			{
				string[] p = parameters.Split('&');
				foreach (string s in p)
				{
					if (!string.IsNullOrEmpty(s) && !s.StartsWith(OAuthParameterPrefix))
					{
						if (s.IndexOf('=') > -1)
						{
							string[] temp = s.Split('=');
							result.Add(new QueryParameter(temp[0], temp[1]));
						}
						else
						{
							result.Add(new QueryParameter(s, string.Empty));
						}
					}
				}
			}

			return result;
		}

		protected string UrlEncode(string value)
		{
			StringBuilder result = new StringBuilder();

			foreach (char symbol in value)
			{
				if (unreservedChars.IndexOf(symbol) != -1)
				{
					result.Append(symbol);
				}
				else
				{
					result.Append('%' + String.Format("{0:X2}", (int)symbol));
				}
			}

			return result.ToString();
		}

		protected string NormalizeRequestParameters(IList<QueryParameter> parameters)
		{
			StringBuilder sb = new StringBuilder();
			QueryParameter p = null;
			for (int i = 0; i < parameters.Count; i++)
			{
				p = parameters[i];
				sb.AppendFormat("{0}={1}", p.Name, p.Value);

				if (i < parameters.Count - 1)
				{
					sb.Append("&");
				}
			}

			return sb.ToString();
		}

		public string GenerateSignatureBase(Uri url, string consumerKey, string token, string tokenSecret, string httpMethod, string timeStamp, string nonce, string signatureType, out string normalizedUrl, out string normalizedRequestParameters)
		{
			if (token == null)
			{
				token = string.Empty;
			}

			if (tokenSecret == null)
			{
				tokenSecret = string.Empty;
			}

			if (string.IsNullOrEmpty(consumerKey))
			{
				throw new ArgumentNullException("consumerKey");
			}

			if (string.IsNullOrEmpty(httpMethod))
			{
				throw new ArgumentNullException("httpMethod");
			}

			if (string.IsNullOrEmpty(signatureType))
			{
				throw new ArgumentNullException("signatureType");
			}

			normalizedUrl = null;
			normalizedRequestParameters = null;

			List<QueryParameter> parameters = GetQueryParameters(url.Query);
			parameters.Add(new QueryParameter(OAuthVersionKey, OAuthVersion));
			parameters.Add(new QueryParameter(OAuthNonceKey, nonce));
			parameters.Add(new QueryParameter(OAuthTimestampKey, timeStamp));
			parameters.Add(new QueryParameter(OAuthSignatureMethodKey, signatureType));
			parameters.Add(new QueryParameter(OAuthConsumerKeyKey, consumerKey));

			if (!string.IsNullOrEmpty(token))
			{
				parameters.Add(new QueryParameter(OAuthTokenKey, token));
			}

			parameters.Sort(new QueryParameterComparer());

			normalizedUrl = string.Format("{0}://{1}", url.Scheme, url.Host);
			if (!((url.Scheme == "http" && url.Port == 80) || (url.Scheme == "https" && url.Port == 443)))
			{
				normalizedUrl += ":" + url.Port;
			}
			normalizedUrl += url.AbsolutePath;
			normalizedRequestParameters = NormalizeRequestParameters(parameters);

			StringBuilder signatureBase = new StringBuilder();
			signatureBase.AppendFormat("{0}&", httpMethod.ToUpper());
			signatureBase.AppendFormat("{0}&", UrlEncode(normalizedUrl));
			signatureBase.AppendFormat("{0}", UrlEncode(normalizedRequestParameters));

			return signatureBase.ToString();
		}

		public string GenerateSignature(Uri url, string consumerKey, string consumerSecret, string token, string tokenSecret, string httpMethod, string timeStamp, string nonce, out string normalizedUrl, out string normalizedRequestParameters)
		{
			return GenerateSignature(url, consumerKey, consumerSecret, token, tokenSecret, httpMethod, timeStamp, nonce, SignatureTypes.HMACSHA1, out normalizedUrl, out normalizedRequestParameters);
		}

		public string GenerateSignature(Uri url, string consumerKey, string consumerSecret, string token, string tokenSecret, string httpMethod, string timeStamp, string nonce, SignatureTypes signatureType, out string normalizedUrl, out string normalizedRequestParameters)
		{
			normalizedUrl = null;
			normalizedRequestParameters = null;

			switch (signatureType)
			{
				case SignatureTypes.PLAINTEXT:
					return WebUtility.UrlEncode(string.Format("{0}&{1}", consumerSecret, tokenSecret));
				case SignatureTypes.HMACSHA1:
					string signatureBase = GenerateSignatureBase(url, consumerKey, token, tokenSecret, httpMethod, timeStamp, nonce, HMACSHA1SignatureType, out normalizedUrl, out normalizedRequestParameters);
					System.Diagnostics.Debug.WriteLine(signatureBase);

					return GenerateSignatureUsingHash_HMACSHA1(string.Format("{0}&{1}", UrlEncode(consumerSecret), string.IsNullOrEmpty(tokenSecret) ? "" : UrlEncode(tokenSecret)), signatureBase);
				case SignatureTypes.RSASHA1:
					throw new NotImplementedException();
				default:
					throw new ArgumentException("Unknown signature type", "signatureType");
			}
		}

		private string GenerateSignatureUsingHash_HMACSHA1(string Key, string data)
		{
			var crypt = Windows.Security.Cryptography.Core.MacAlgorithmProvider.OpenAlgorithm("HMAC_SHA1");
			var keyBuffer = Windows.Security.Cryptography.CryptographicBuffer.CreateFromByteArray(Encoding.UTF8.GetBytes(Key));
			var cryptKey = crypt.CreateKey(keyBuffer);

			var dataBuffer = Windows.Security.Cryptography.CryptographicBuffer.CreateFromByteArray(Encoding.UTF8.GetBytes(data));
			var signBuffer = Windows.Security.Cryptography.Core.CryptographicEngine.Sign(cryptKey, dataBuffer);

			byte[] value;
			Windows.Security.Cryptography.CryptographicBuffer.CopyToByteArray(signBuffer, out value);

			return Convert.ToBase64String(value);
		}

		/// <summary>
		/// Generate the timestamp for the signature        
		/// </summary>
		/// <returns></returns>
		public virtual string GenerateTimeStamp()
		{
			// Default implementation of UNIX time of the current UTC time
			TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
			return Convert.ToInt64(ts.TotalSeconds).ToString();
		}

		/// <summary>
		/// Generate a nonce
		/// </summary>
		/// <returns></returns>
		public virtual string GenerateNonce_()
		{
			// Just a simple implementation of a random number between 123400 and 9999999
			return random.Next(123400, 9999999).ToString();
		}

		/// <summary>
		/// Generate a nonce
		/// </summary>
		/// <returns></returns>
		public virtual string GenerateNonce()
		{
			// Just a simple implementation of a random number between 123400 and 9999999
			string strings = "0123456789abcdefghijklmnopqrstuvwxyABCDEFGHIJKLMNOPQRSTUVWXYZ";
			string res = "";
			for (int i = 0; i < 32; i++)
				res += strings[random.Next(0, strings.Length - 1)];
			return res;
		}

	}
}
