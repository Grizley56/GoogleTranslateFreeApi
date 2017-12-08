using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoogleTranslateFreeApi.TranslationData;

namespace GoogleTranslateFreeApi
{
	public class TranslationResult
	{
		public string[] FragmentedTranslation { get; internal set; }
		public string MergedTranslation => String.Concat(FragmentedTranslation);
		public string OriginalText { get; internal set; }
		public string TranslatedTextTranscription { get; internal set; }
		public string OriginalTextTranscription { get; internal set; }

		public string[] SeeAlso { get; internal set; }
		
		public Language SourceLanguage { get; internal set; }
		public Language TargetLanguage { get; internal set; }

		public Synonyms Synonyms { get; internal set; }
		public Corrections Corrections { get; internal set; }
		public Definitions Definitions { get; internal set; }
		public ExtraTranslations ExtraTranslations { get; internal set; }

		internal TranslationResult() { }
	}
}
