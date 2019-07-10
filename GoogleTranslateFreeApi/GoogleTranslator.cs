using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GoogleTranslateFreeApi.TranslationData;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GoogleTranslateFreeApi
{
	/// <summary>
	/// Represent a class for translate the text using <see href="http://translate.google.com"/>
	/// </summary>
	public class GoogleTranslator: ITranslator
  {
    private readonly GoogleKeyTokenGenerator _generator;
	  private readonly HttpClient _httpClient;
		private TimeSpan _timeOut;
		private IWebProxy _proxy;

		protected Uri Address;
		
		/// <summary>
		/// Requests timeout
		/// </summary>
		public TimeSpan TimeOut
		{
			get { return _timeOut; }
			set
			{
				_timeOut = value;
				_generator.TimeOut = value;
			}
		}
		
		/// <summary>
		/// Requests proxy
		/// </summary>
		public IWebProxy Proxy
		{
			get { return _proxy; }
			set
			{
				_proxy = value;
				_generator.Proxy = value;
			}
		}

		public string Domain
		{
			get { return Address.AbsoluteUri.GetTextBetween("https://", "/translate_a/single"); }
			set { Address = new Uri($"https://{value}/translate_a/single"); }
		}

		/// <summary>
		/// An Array of supported languages by google translate
		/// </summary>
		public static Language[] LanguagesSupported { get; }

		/// <param name="language">Full name of the required language</param>
		/// <example>GoogleTranslator.GetLanguageByName("English")</example>
		/// <returns>Language object from the LanguagesSupported array</returns>
		public static Language GetLanguageByName(string language)
			=> LanguagesSupported.FirstOrDefault(i
				=> i.FullName.Equals(language, StringComparison.OrdinalIgnoreCase));

	  /// <param name="iso">ISO of the required language</param>
	  /// <example>GoogleTranslator.GetLanguageByISO("en")</example>
	  /// <returns>Language object from the LanguagesSupported array</returns>
	  // ReSharper disable once InconsistentNaming
		public static Language GetLanguageByISO(string iso)
			=> LanguagesSupported.FirstOrDefault(i
				=> i.ISO639.Equals(iso, StringComparison.OrdinalIgnoreCase));

		/// <summary>
		/// Check is available language to translate
		/// </summary>
		/// <param name="language">Checked <see cref="Language"/> </param>
		/// <returns>Is it available language or not</returns>
		public static bool IsLanguageSupported(Language language)
		{
			if (language.Equals(Language.Auto))
				return true;
			
			return LanguagesSupported.Contains(language) ||
						 LanguagesSupported.FirstOrDefault(language.Equals) != null;
		}

		static GoogleTranslator()
		{
			var assembly = typeof(GoogleTranslator).GetTypeInfo().Assembly;
			Stream stream = assembly.GetManifestResourceStream("GoogleTranslateFreeApi.Languages.json");

			using (StreamReader reader = new StreamReader(stream))
			{
				string languages = reader.ReadToEnd();
				LanguagesSupported = JsonConvert
					.DeserializeObject<Language[]>(languages);
			}
		}

		/// <param name="domain">A Domain name which will be used to execute requests</param>
		public GoogleTranslator(string domain = "translate.google.com")
		{
			Address = new Uri($"https://{domain}/translate_a/single");
			_generator = new GoogleKeyTokenGenerator();
			_httpClient = new HttpClient();
		}

		/// <summary>
		/// <p>
		/// Async text translation from language to language. Include full information about the translation.
		/// </p>
		/// </summary>
		/// <param name="originalText">Text to translate</param>
		/// <param name="fromLanguage">Source language</param>
		/// <param name="toLanguage">Target language</param>
		/// <exception cref="LanguageIsNotSupportedException">Language is not supported</exception>
		/// <exception cref="InvalidOperationException">Thrown when target language is auto</exception>
		/// <exception cref="GoogleTranslateIPBannedException">Thrown when the IP used for requests is banned </exception>
		/// <exception cref="HttpRequestException">Thrown when getting the HTTP exception</exception>
		public async Task<TranslationResult> TranslateAsync(string originalText, Language fromLanguage, Language toLanguage)
		{
			return await GetTranslationResultAsync(originalText, fromLanguage, toLanguage, true);
		}

		/// <summary>
		/// <p>
		/// Async text translation from language to language. Include full information about the translation.
		/// </p>
		/// </summary>
		/// <param name="item">The object that implements the interface ITranslatable</param>
		/// <exception cref="LanguageIsNotSupportedException">Language is not supported</exception>
		/// <exception cref="InvalidOperationException">Thrown when target language is auto</exception>
		/// <exception cref="GoogleTranslateIPBannedException">Thrown when the IP used for requests is banned </exception>
		/// <exception cref="HttpRequestException">Thrown when getting the HTTP exception</exception>
		public async Task<TranslationResult> TranslateAsync(ITranslatable item)
		{
			return await TranslateAsync(item.OriginalText, item.FromLanguage, item.ToLanguage);
		}

		/// <summary>
		/// <p>
		/// Async text translation from language to language. 
		/// In contrast to the TranslateAsync doesn't include additional information such as ExtraTranslation and Definition.
		/// </p>
		/// </summary>
		/// <param name="originalText">Text to translate</param>
		/// <param name="fromLanguage">Source language</param>
		/// <param name="toLanguage">Target language</param>
		/// <exception cref="LanguageIsNotSupportedException">Language is not supported</exception>
		/// <exception cref="InvalidOperationException">Thrown when target language is auto</exception>
		/// <exception cref="GoogleTranslateIPBannedException">Thrown when the IP used for requests is banned </exception>
		/// <exception cref="HttpRequestException">Thrown when getting the HTTP exception</exception>
		public async Task<TranslationResult> TranslateLiteAsync(string originalText, Language fromLanguage, Language toLanguage)
	  {
		  return await GetTranslationResultAsync(originalText, fromLanguage, toLanguage, false);
	  }

	  /// <summary>
	  /// <p>
	  /// Async text translation from language to language. 
	  /// In contrast to the TranslateAsync doesn't include additional information such as ExtraTranslation and Definition.
	  /// </p>
	  /// </summary>
	  /// <param name="item">The object that implements the interface ITranslatable</param>
	  /// <exception cref="LanguageIsNotSupportedException">Language is not supported</exception>
	  /// <exception cref="InvalidOperationException">Thrown when target language is auto</exception>
	  /// <exception cref="GoogleTranslateIPBannedException">Thrown when the IP used for requests is banned</exception>
	  /// <exception cref="HttpRequestException">Thrown when getting the HTTP exception</exception>
	  public async Task<TranslationResult> TranslateLiteAsync(ITranslatable item)
	  {
		  return await TranslateLiteAsync(item.OriginalText, item.FromLanguage, item.ToLanguage);
	  }
	  
	  protected virtual async Task<TranslationResult> GetTranslationResultAsync(string originalText, Language fromLanguage,
		  Language toLanguage, bool additionInfo)
	  {
		  if (!IsLanguageSupported(fromLanguage))
				throw new LanguageIsNotSupportedException(fromLanguage);
			if (!IsLanguageSupported(toLanguage))
				throw new LanguageIsNotSupportedException(toLanguage);
			if (toLanguage.Equals(Language.Auto))
				throw new InvalidOperationException("A destination Language is auto");

			if (originalText.Trim() == string.Empty)
			{
				return new TranslationResult()
				{
					OriginalText = originalText, 
					FragmentedTranslation = new string[0], 
					SourceLanguage = fromLanguage, 
					TargetLanguage = toLanguage
				};
			}

			string token = await _generator.GenerateAsync(originalText);

			string postData = $"sl={fromLanguage.ISO639}&" +
												$"tl={toLanguage.ISO639}&" +
												$"hl=en&" +
												$"q={Uri.EscapeDataString(originalText)}&" +
												$"tk={token}&" +
												"client=t&" +
												"dt=at&dt=bd&dt=ex&dt=ld&dt=md&dt=qca&dt=rw&dt=rm&dt=ss&dt=t&" +
												"ie=UTF-8&" +
												"oe=UTF-8&" +
												"otf=1&" +
												"ssel=0&" +
												"tsel=0&" +
												"kc=7";

			string result;

			try
			{
				result = await _httpClient.GetStringAsync($"{Address}?{postData}");
			}
			catch (HttpRequestException ex) when (ex.Message.Contains("503"))
			{
				throw new GoogleTranslateIPBannedException(GoogleTranslateIPBannedException.Operation.Translation);
			}
			catch
			{
				if (_generator.IsExternalKeyObsolete)
					return await TranslateAsync(originalText, fromLanguage, toLanguage);

				throw;
			}


			return ResponseToTranslateResultParse(result, originalText, fromLanguage, toLanguage, additionInfo);
	  }
	  
		protected virtual TranslationResult ResponseToTranslateResultParse(string result, string sourceText, 
			Language sourceLanguage, Language targetLanguage, bool additionInfo)
		{
			TranslationResult translationResult = new TranslationResult();

			JToken tmp = JsonConvert.DeserializeObject<JToken>(result);
			
			string originalTextTranscription = null, translatedTextTranscription = null;

			var mainTranslationInfo = tmp[0];

			GetMainTranslationInfo(mainTranslationInfo, out var translation,
				ref originalTextTranscription, ref translatedTextTranscription);
			
			translationResult.FragmentedTranslation = translation;
			translationResult.OriginalText = sourceText;

			translationResult.OriginalTextTranscription = originalTextTranscription;
			translationResult.TranslatedTextTranscription = translatedTextTranscription;

			translationResult.Corrections = GetTranslationCorrections(tmp);

			translationResult.SourceLanguage = sourceLanguage;
			translationResult.TargetLanguage = targetLanguage;

			if (tmp[8] is JArray languageDetections)
				translationResult.LanguageDetections = GetLanguageDetections(languageDetections).ToArray();


			if (!additionInfo) 
				return translationResult;
			
			translationResult.ExtraTranslations = 
				TranslationInfoParse<ExtraTranslations>(tmp[1]);

			translationResult.Synonyms = tmp.Count() >= 12
				? TranslationInfoParse<Synonyms>(tmp[11])
				: null;

			translationResult.Definitions = tmp.Count() >= 13
				? TranslationInfoParse<Definitions>(tmp[12])
				: null;

			translationResult.SeeAlso = tmp.Count() >= 15
				? GetSeeAlso(tmp[14])
				: null;

			return translationResult;
		}


		protected static T TranslationInfoParse<T>(JToken response) where T : TranslationInfoParser
	  {
		  if (!response.HasValues)
			  return null;
			
		  T translationInfoObject = TranslationInfoParser.Create<T>();
			
		  foreach (var item in response)
		  {
			  string partOfSpeech = (string)item[0];

			  JToken itemToken = translationInfoObject.ItemDataIndex == -1 ? item : item[translationInfoObject.ItemDataIndex];
				
			  //////////////////////////////////////////////////////////////
			  // I delete the white spaces to work auxiliary verb as well //
			  //////////////////////////////////////////////////////////////
			  if (!translationInfoObject.TryParseMemberAndAdd(partOfSpeech.Replace(' ', '\0'), itemToken))
			  {
					#if DEBUG
				  //sometimes response contains members without name. Just ignore it.
				  Debug.WriteLineIf(partOfSpeech.Trim() != String.Empty, 
					  $"class {typeof(T).Name} doesn't contains a member for a part " +
					  $"of speech {partOfSpeech}");
					#endif
			  }
		  }
			
		  return translationInfoObject;
	  }

	  protected static string[] GetSeeAlso(JToken response)
	  {
		  return !response.HasValues ? new string[0] : response[0].ToObject<string[]>(); 
	  }
	  
		protected static void GetMainTranslationInfo(JToken translationInfo, out string[] translate, 
			ref string originalTextTranscription, ref string translatedTextTranscription)
		{
			List<string> translations = new List<string>();
			
			foreach (var item in translationInfo)
			{
				if (item.Count() >= 5)
					translations.Add(item.First.Value<string>());
				else
				{
					var transcriptionInfo = item;
					int elementsCount = transcriptionInfo.Count();

					if (elementsCount == 3)
					{
						translatedTextTranscription = (string)transcriptionInfo[elementsCount - 1];
					}
					else
					{
						if (transcriptionInfo[elementsCount - 2] != null)
							translatedTextTranscription = (string)transcriptionInfo[elementsCount - 2];
						else
							translatedTextTranscription = (string)transcriptionInfo[elementsCount - 1];

						originalTextTranscription = (string)transcriptionInfo[elementsCount - 1];
					}
				}
			}

			translate = translations.ToArray();
		}

		protected static Corrections GetTranslationCorrections(JToken response)
		{
			if (!response.HasValues)
				return new Corrections();

			Corrections corrections = new Corrections();

			JToken textCorrectionInfo = response[7];

			if (textCorrectionInfo.HasValues)
			{
				Regex pattern = new Regex(@"<b><i>(.*?)</i></b>");
				MatchCollection matches = pattern.Matches((string)textCorrectionInfo[0]);

				var correctedText = (string)textCorrectionInfo[1];
				var correctedWords = new string[matches.Count];

				for (int i = 0; i < matches.Count; i++)
					correctedWords[i] = matches[i].Groups[1].Value;

				corrections.CorrectedWords = correctedWords;
				corrections.CorrectedText = correctedText;
				corrections.TextWasCorrected = true;
			}

			return corrections;
		}

		protected IEnumerable<LanguageDetection> GetLanguageDetections(JArray item)
		{
			JArray languages = item[0] as JArray;
			JArray confidences = item[2] as JArray;

			if (languages == null || confidences == null || languages.Count != confidences.Count)
				yield break;

			for (int i = 0; i < languages.Count; i++)
			{
				yield return new LanguageDetection(GetLanguageByISO((string) languages[i]), (double) confidences[i]);
			}
		}
  }
}
