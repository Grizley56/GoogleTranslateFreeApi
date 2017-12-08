using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using GoogleTranslateFreeApi.TranslationData;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GoogleTranslateFreeApi
{
	public class GoogleTranslator : GoogleTranslatorLite
	{
		protected override TranslationResult ResponseToTranslateResultParse(string result, string sourceText, 
			Language sourceLanguage, Language targetLanguage)
		{
			TranslationResult translationResult = 
				base.ResponseToTranslateResultParse(result, sourceText, sourceLanguage, targetLanguage);

			JToken tmp = JsonConvert.DeserializeObject<JToken>(result);
			
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
				: new string[0];
			
			return translationResult;
		}

		protected static T TranslationInfoParse<T>(JToken response) where T : TranslationInfoParser
		{
			if (!response.HasValues)
				return null;
			
			T translationInfoObject = TranslationInfoParser.Create<T>();
			
			foreach (var item in response)
			{
				string partOfSpretch = (string)item[0];

				JToken itemToken = translationInfoObject.ItemDataIndex == -1 ? item : item[translationInfoObject.ItemDataIndex];
				
				//////////////////////////////////////////////////////////////
				// I delete the white spaces to work auxiliary verb as well //
				//////////////////////////////////////////////////////////////
				if (!translationInfoObject.TryParseMemberAndAdd(partOfSpretch.Replace(' ', '\0'), itemToken))
				{
					#if DEBUG
						//sometimes response contains members without name. Just ignore it.
						Debug.WriteLineIf(partOfSpretch.Trim() != String.Empty, 
							$"class {typeof(T).Name} dont contains a member for a part " +
							$"of spretch {partOfSpretch}");
					#endif
				}
			}
			
			return translationInfoObject;
		}

		protected static string[] GetSeeAlso(JToken response)
		{
			return !response.HasValues ? new string[0] : response[0].ToObject<string[]>();
		}
	}
}
