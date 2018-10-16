using System;
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
  public class GoogleTranslator: ITranslator
  {
    private readonly GoogleKeyTokenGenerator _generator;
	  private readonly HttpClient _httpClient;

		protected Uri _address;
		protected TimeSpan _timeOut;
		protected IWebProxy _proxy;
		
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
			get { return _address.AbsoluteUri.GetTextBetween("https://", "/translate_a/single"); }
			set { _address = new Uri($"https://{value}/translate_a/single"); }
		}

		/// <summary>
		/// An Array of supported languages by google translate
		/// </summary>
		public static Language[] LanguagesSupported { get; }

		/// <param name="language">Full name of the required language</param>
		/// <example>GoogleTranslator.GetLangaugeByName("English")</example>
		/// <returns>Language object from the LanguagesSupported array</returns>
		public static Language GetLanguageByName(string language)
			=> LanguagesSupported.FirstOrDefault(i
				=> i.FullName.Equals(language, StringComparison.OrdinalIgnoreCase));

		public static Language GetLanguageByISO(string iso)
			=> LanguagesSupported.FirstOrDefault(i
				=> i.ISO639.Equals(iso, StringComparison.OrdinalIgnoreCase));

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
			_address = new Uri($"https://{domain}/translate_a/single");
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
		/// <exception cref="LanguageNotSupportException">Language is not supported</exception>
		/// <exception cref="InvalidOperationException">Thrown when target language is auto</exception>
	  /// <exception cref="GoogleTranslateIPBannedException">Thrown when the IP used for requests is banned </exception>
		/// <exception cref="WebException">Thrown when getting an error with response</exception>
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
		/// <exception cref="LanguageNotSupportException">Language is not supported</exception>
		/// <exception cref="InvalidOperationException">Thrown when target language is auto</exception>
		/// <exception cref="GoogleTranslateIPBannedException">Thrown when the IP used for requests is banned </exception>
		/// <exception cref="WebException">Thrown when getting an error with response</exception>
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
	  /// <exception cref="LanguageNotSupportException">Language is not supported</exception>
	  /// <exception cref="InvalidOperationException">Thrown when target language is auto</exception>
	  /// <exception cref="GoogleTranslateIPBannedException">Thrown when the IP used for requests is banned </exception>
	  /// <exception cref="WebException">Thrown when getting an error with response</exception>
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
	  /// <exception cref="LanguageNotSupportException">Language is not supported</exception>
	  /// <exception cref="InvalidOperationException">Thrown when target language is auto</exception>
	  /// <exception cref="GoogleTranslateIPBannedException">Thrown when the IP used for requests is banned </exception>
	  /// <exception cref="WebException">Thrown when getting an error with response</exception>
	  public async Task<TranslationResult> TranslateLiteAsync(ITranslatable item)
	  {
		  return await TranslateLiteAsync(item.OriginalText, item.FromLanguage, item.ToLanguage);
	  }
	  
	  protected async virtual Task<TranslationResult> GetTranslationResultAsync(string originalText, Language fromLanguage,
		  Language toLanguage, bool additionInfo)
	  {
		  if (!IsLanguageSupported(fromLanguage))
				throw new LanguageNotSupportException(fromLanguage);
			if (!IsLanguageSupported(toLanguage))
				throw new LanguageNotSupportException(toLanguage);
			if (toLanguage.Equals(Language.Auto))
				throw new InvalidOperationException("A destination Language is auto");

			if (originalText.Trim() == String.Empty)
				return new TranslationResult();

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
			  result = await _httpClient.GetStringAsync($"{_address}?{postData}");
		  }
		  catch(WebException ex)
		  {
				if (_generator.IsExternalKeyObsolete)
					return await TranslateAsync(originalText, fromLanguage, toLanguage);

			  if ((int)ex.Status == 7) //ProtocolError
				  throw new GoogleTranslateIPBannedException(GoogleTranslateIPBannedException.Operation.Translation);
			  
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
			string[] translate;

			var mainTranslationInfo = tmp[0];

			GetMainTranslationInfo(mainTranslationInfo, out translate,
				ref originalTextTranscription, ref translatedTextTranscription);
			
			translationResult.FragmentedTranslation = translate;
			translationResult.OriginalText = sourceText;

			translationResult.OriginalTextTranscription = originalTextTranscription;
			translationResult.TranslatedTextTranscription = translatedTextTranscription;

			translationResult.Corrections = GetTranslationCorrections(tmp);

			translationResult.SourceLanguage = sourceLanguage.Equals(Language.Auto) 
				? GetLanguageByISO( (string)tmp[8][0][0] ) 
				: sourceLanguage;
			
			translationResult.TargetLanguage = targetLanguage;

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
					  $"class {typeof(T).Name} dont contains a member for a part " +
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

			bool transcriptionAviable = translationInfo.Count() > 1;

			translate = new string[translationInfo.Count() - (transcriptionAviable ? 1 : 0)];

			for (int i = 0; i < translate.Length; i++)
				translate[i] = (string)translationInfo[i][0];


			if (!transcriptionAviable)
				return;

			var transcriptionInfo = translationInfo[translationInfo.Count() - 1];
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

			string selectedLangauge = (string)response[2];
			string detectedLanguage = (string)(response[8])[0][0];

			if (selectedLangauge != detectedLanguage)
			{
				corrections.LanguageWasCorrected = true;
				corrections.CorrectedLanguage = LanguagesSupported.FirstOrDefault(language =>
					language.ISO639 == detectedLanguage);
			}
			corrections.Confidence =  (double)response[6];
			
			return corrections;
		}
  }
}