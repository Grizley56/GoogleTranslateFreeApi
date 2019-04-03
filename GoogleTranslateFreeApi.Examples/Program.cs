using System;
using System.Linq;

namespace GoogleTranslateFreeApi.Examples
{
	class Program
	{
		private static readonly GoogleTranslator Translator = new GoogleTranslator();

		static void Main(string[] args)
		{
			Language english = GoogleTranslator.GetLanguageByName("English"); // define language this way
			Language russian = Language.Russian; // or even this way
			Language french = GoogleTranslator.GetLanguageByISO("fr"); // you could also use ISO639 value

			TranslationResult result = Translator.TranslateAsync("Hello. How are you?", english, russian).GetAwaiter().GetResult();

			Console.WriteLine($"Result 1: {result.MergedTranslation}");

			TranslationResult result2 =
				Translator.TranslateAsync(new TranslateItem("The quick brown fox jumps over the lazy dog. Brown fox"))
					.GetAwaiter()
					.GetResult();

			for (var i = 0; i < result2.FragmentedTranslation.Length; i++)
			{
				var part = result2.FragmentedTranslation[i];
				Console.WriteLine($"Result 2.{i} = {part}");
			}

			TranslationResult result3 =
				Translator.TranslateAsync("Run", english, Language.Dutch)
					.GetAwaiter()
					.GetResult();

			foreach (var noun in result3.ExtraTranslations.Noun)
			{
				Console.WriteLine($"{noun.Phrase}: {string.Join(", ", noun.PhraseTranslations)}");
			}
		}

		private struct TranslateItem : ITranslatable
		{
			public string OriginalText { get; }
			public Language FromLanguage { get; }
			public Language ToLanguage { get; }

			// Some other user defined properties...

			public TranslateItem(string text)
			{
				OriginalText = text;
				FromLanguage = new Language("English", "en");
				ToLanguage = GoogleTranslator.GetLanguageByISO("fr");
			}
		}
	}
}
