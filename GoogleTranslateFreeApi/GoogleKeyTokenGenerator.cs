using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Runtime;

namespace GoogleTranslateFreeApi
{
	/// <summary>
	/// GoogleTranslate token generator
	/// </summary>
	public class GoogleKeyTokenGenerator
	{
		/// <summary>
		/// GoogleTranslate token
		/// </summary>
		protected struct ExternalKey
		{
			/// <summary>
			/// Total hours
			/// </summary>
			public long Time { get; }

			/// <summary>
			/// Token value
			/// </summary>
			public long Value { get; }

			/// <param name="time">Unix-formatted total hours</param>
			/// <param name="value">Token value</param>
			public ExternalKey(long time, long value)
			{
				Time = time;
				Value = value;
			}
		}

		/// <summary>
		/// Using external key 
		/// </summary>
		protected ExternalKey CurrentExternalKey;
		/// <summary>
		/// Address for sending requests
		/// </summary>
		protected readonly Uri Address = new Uri("https://translate.google.com");

		/// <summary>
		/// 
		/// </summary>
		protected int UnixTotalHours
		{
			get
			{
				DateTime unixTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
				return (int)DateTime.UtcNow.Subtract(unixTime).TotalHours;
			}
		}
		/// <summary>
		/// True, if the current key cannot be used for a token generate
		/// </summary>
		public bool IsExternalKeyObsolete => CurrentExternalKey.Time != UnixTotalHours;

		
		/// <summary>
		/// The proxy server that is used to send requests
		/// </summary>
		public IWebProxy Proxy { get; set; }

		/// <summary>
		/// Requests timeout
		/// </summary>
		public TimeSpan TimeOut { get; set; } = TimeSpan.FromSeconds(10);

		/// <summary>
		/// Initializes a new instance of the <see cref="GoogleKeyTokenGenerator"/> class
		/// </summary>
		public GoogleKeyTokenGenerator()
		{
			CurrentExternalKey = new ExternalKey(0, 0);
		}

		/// <summary>
		/// <p>Generate the token for a given string</p>
		/// </summary>
		/// <param name="source">The string to receive the token</param>
		/// <returns>Token for the current string</returns>
		/// <exception cref="NotSupportedException">The method is no longer valid, or something went wrong</exception>
		/// <exception cref="HttpRequestException">Http exception</exception>
		/// <exception cref="GoogleTranslateIPBannedException"></exception>
		public virtual async Task<string> GenerateAsync(string source)
		{
			if (IsExternalKeyObsolete)
				try
				{
					CurrentExternalKey = await GetNewExternalKeyAsync();
				}
				catch (ExternalKeyParseException)
				{
					throw new NotSupportedException("The method is no longer valid, or something went wrong");
				}

			long time = DecrypthAlgorythm(source);

			return time.ToString() + '.' + (time ^ CurrentExternalKey.Time);
		}

		protected virtual async Task<ExternalKey> GetNewExternalKeyAsync()
		{
			HttpClient httpClient;

			if (Proxy == null)
				httpClient = new HttpClient();
			else
				httpClient = new HttpClient(
					new HttpClientHandler()
					{
						Proxy = Proxy,
						UseProxy = true,
					});

			httpClient.Timeout = TimeOut;

			string result;

			using (httpClient)
			{
				result = await httpClient.GetStringAsync(Address);

				HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, Address);
				//request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

				HttpResponseMessage response;

				try
				{
					response = await httpClient.SendAsync(request);
				}
				catch (HttpRequestException ex) when (ex.Message.Contains("503"))
				{
					throw new GoogleTranslateIPBannedException(
						GoogleTranslateIPBannedException.Operation.TokenGeneration);
				}
				
				result = await response.Content.ReadAsStringAsync();
			}

			long tkk;

			try
			{
				var tkkText = result.GetTextBetween(@"tkk:'", "',");

				if (tkkText == null)
					throw new ExternalKeyParseException("Unknown TKK position");

				var splitted = tkkText.Split('.');
				if (splitted.Length != 2 || !long.TryParse(splitted[1], out tkk))
					throw new ExternalKeyParseException($"Unknown TKK format. TKK: {tkkText}");

			}
			catch (ArgumentException)
			{
				throw new ExternalKeyParseException();
			}

			ExternalKey newExternalKey = new ExternalKey(UnixTotalHours, tkk);
			return newExternalKey;
		}
		
		[DebuggerHidden]
		private long DecrypthAlgorythm(string source)
		{
			List<long> code = new List<long>();

			for (int g = 0; g < source.Length; g++)
			{
				int l = source[g];
				if (l < 128)
				{
					code.Add(l);
				}
				else
				{
					if (l < 2048)
					{
						code.Add(l >> 6 | 192);
					}
					else
					{
						if (55296 == (l & 64512) && g + 1 < source.Length && 56320 == (source[g + 1] & 64512))
						{
							l = 65536 + ((l & 1023) << 10) + (source[++g] & 1023);
							code.Add(l >> 18 | 240);
							code.Add(l >> 12 & 63 | 128);
						}
						else
						{
							code.Add(l >> 12 | 224);
						}
						code.Add(l >> 6 & 63 | 128);
					}
					code.Add(l & 63 | 128);
				}
			}

			long time = CurrentExternalKey.Time;

			foreach (long i in code)
			{
				time += i;
				Xr(ref time, "+-a^+6");
			}

			Xr(ref time, "+-3^+b+-f");

			time ^= CurrentExternalKey.Value;

			if (time < 0)
				time = (time & int.MaxValue) + 2147483648;

			time %= (long)1e6;

			return time;
		}
		
		[DebuggerHidden]
		private static void Xr(ref long a, string b)
		{
			for (int c = 0; c < b.Length - 2; c += 3)
			{
				long d = b[c + 2];

				d = 'a' <= d ? d - 87 : (long) char.GetNumericValue((char) d);
				d = '+' == b[c + 1] ? (long)((ulong)a >> (int)d) : a << (int)d;
				a = '+' == b[c] ? a + d & 4294967295 : a ^ d;
			}
		}
	}
}
