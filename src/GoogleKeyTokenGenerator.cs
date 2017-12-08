using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GoogleTranslateFreeApi
{
	public class GoogleKeyTokenGenerator
	{
		protected struct ExternalKey
		{
			public long Time { get; }
			public long Value { get; }

			public ExternalKey(long time, long value)
			{
				Time = time;
				Value = value;
			}
		}

		protected ExternalKey _currentExternalKey;

		protected readonly Uri _address = new Uri("https://translate.google.com");

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
		public bool IsExternalKeyObsolete => _currentExternalKey.Time != UnixTotalHours;

		
		/// <summary>
		/// The proxy server that is used to send requests
		/// </summary>
		public IWebProxy Proxy { get; set; }

		/// <summary>
		/// Requests timeout
		/// </summary>
		public TimeSpan TimeOut { get; set; } = TimeSpan.FromSeconds(10);

		public GoogleKeyTokenGenerator()
		{
			_currentExternalKey = new ExternalKey(0, 0);
		}

		/// <summary>
		/// <p>Generate the token for a string</p>
		/// </summary>
		/// <param name="source">The string to receive the token</param>
		/// <returns>Token for the current string</returns>
		/// <exception cref="NotSupportedException">The method is no longer valid, or something went wrong</exception>
		public virtual async Task<string> GenerateAsync(string source)
		{
			if (IsExternalKeyObsolete)
				try
				{
					_currentExternalKey = await GetNewExternalKeyAsync();
				}
				catch (ExternalKeyParseException)
				{
					throw new NotSupportedException();
				}

			long time = DecrypthAlgorythm(source);

			return time.ToString() + '.' + (time ^ _currentExternalKey.Time);
		}

		protected virtual async Task<ExternalKey> GetNewExternalKeyAsync()
		{
			HttpWebRequest request = WebRequest.CreateHttp(_address);
			request.Proxy = Proxy;
			request.ContinueTimeout = (int)TimeOut.TotalMilliseconds;
			request.ContentType = "application/x-www-form-urlencoded";

			var response = (HttpWebResponse) await request.GetResponseAsync();

			string result;

			using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
				result = await streamReader.ReadToEndAsync();


			long number1, number2;
			string textNumber1, textNumber2;

			try
			{
				int index = result.IndexOf((@"var a\x3d"), StringComparison.Ordinal);
				textNumber1 = result.GetTextBetween(@"var a\x3d", ";", index);
				textNumber2 = result.GetTextBetween(@"var b\x3d", ";", index);
				
				if(textNumber1 == null || textNumber2 == null)
					throw new ExternalKeyParseException();
			}
			catch (ArgumentException)
			{
				throw new ExternalKeyParseException();
			}


			if (!long.TryParse(textNumber1, out number1) || !long.TryParse(textNumber2, out number2))
				throw new ExternalKeyParseException();

			ExternalKey newExternalKey = new ExternalKey(UnixTotalHours, number1 + number2);
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

			long time = _currentExternalKey.Time;

			foreach (long i in code)
			{
				time += i;
				Xr(ref time, "+-a^+6");
			}

			Xr(ref time, "+-3^+b+-f");

			time ^= _currentExternalKey.Value;

			if (time < 0)
				time = (time & 2147483647) + 2147483648;

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
