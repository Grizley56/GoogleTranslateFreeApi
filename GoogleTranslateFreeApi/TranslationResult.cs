using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GoogleTranslateFreeApi.TranslationData;

namespace GoogleTranslateFreeApi
{
	[DataContract]
	public class TranslationResult
	{
		[DataMember]
		public string[] FragmentedTranslation { get; internal set; }

		public string MergedTranslation => string.Concat(FragmentedTranslation);

		[DataMember]
		public string OriginalText { get; internal set; }
		[DataMember]
		public string TranslatedTextTranscription { get; internal set; }
		[DataMember]
		public string OriginalTextTranscription { get; internal set; }
		[DataMember]
		public string[] SeeAlso { get; internal set; }
		[DataMember]
		public Language SourceLanguage { get; internal set; }
		[DataMember]
		public Language TargetLanguage { get; internal set; }
		[DataMember]
		public Synonyms Synonyms { get; internal set; }
		[DataMember]
		public Corrections Corrections { get; internal set; }
		[DataMember]
		public Definitions Definitions { get; internal set; }
		[DataMember]
		public ExtraTranslations ExtraTranslations { get; internal set; }
		[DataMember]
		public LanguageDetection[] LanguageDetections { get; internal set; }

		internal TranslationResult()
		{

		}
	}
}
