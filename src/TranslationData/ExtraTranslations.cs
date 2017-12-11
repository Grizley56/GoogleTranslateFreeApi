using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace GoogleTranslateFreeApi.TranslationData
{
	public sealed class ExtraTranslations : TranslationInfoParser
	{
		public class ExtraTranslation
		{
			public string Phrase { get; }
			public string[] PhraseTranslations { get; }

			internal ExtraTranslation(string phrase, string[] phraseTranslations)
			{
				Phrase = phrase;
				PhraseTranslations = phraseTranslations;
			}

			public override string ToString() => $"{Phrase}: {String.Join(", ", PhraseTranslations)}";
		}
		
		public ExtraTranslation[] Noun { get; internal set; }
		public ExtraTranslation[] Verb { get; internal set; }
		public ExtraTranslation[] Pronoun { get;  internal set; }
		public ExtraTranslation[] Adverb { get; internal set; }
		public ExtraTranslation[] AuxiliaryVerb { get; internal set; }
		public ExtraTranslation[] Adjective { get; internal set; }
		public ExtraTranslation[] Conjunction { get; internal set; }
		public ExtraTranslation[] Preposition { get; internal set; }
		public ExtraTranslation[] Interjection { get; internal set; }
		public ExtraTranslation[] Suffix { get; internal set; }
		public ExtraTranslation[] Prefix { get; internal set; }
		public ExtraTranslation[] Abbreviation { get; internal set; }
		public ExtraTranslation[] Particle { get; internal set; }
		public ExtraTranslation[] Phrase { get; internal set; }
		
		public ExtraTranslations() { }

		
		private string FormatOutput(IEnumerable<ExtraTranslation> formatData, string partOfSpretchName)
		{
			if(formatData == null)
				return String.Empty;
			
			string result = partOfSpretchName + ":\n";

			return formatData.Aggregate(result, (current, data) 
				=> current + $"{data.Phrase}: {String.Join(", ", data.PhraseTranslations)}\n");
		}

		public override string ToString()
		{
			string result = String.Empty;
			
			result += FormatOutput(Noun, nameof(Noun));
			result += FormatOutput(Verb, nameof(Verb));
			result += FormatOutput(Pronoun, nameof(Pronoun));
			result += FormatOutput(Adverb, nameof(Adverb));
			result += FormatOutput(AuxiliaryVerb, nameof(AuxiliaryVerb));
			result += FormatOutput(Adjective, nameof(Adjective));
			result += FormatOutput(Conjunction, nameof(Conjunction));
			result += FormatOutput(Preposition, nameof(Preposition));
			result += FormatOutput(Interjection, nameof(Interjection));
			result += FormatOutput(Suffix, nameof(Suffix));
			result += FormatOutput(Prefix, nameof(Prefix));
			result += FormatOutput(Abbreviation, nameof(Abbreviation));
			result += FormatOutput(Particle, nameof(Particle));
			result += FormatOutput(Phrase, nameof(Phrase));

			return result.Trim();
		}


		internal override bool TryParseMemberAndAdd(string memberName, JToken parseInformation)
		{
			PropertyInfo property = this.GetType().GetRuntimeProperty(memberName.ToCamelCase());
			if (property == null)
				return false;
			
			var extraTranslations = new ExtraTranslation[parseInformation.Count()];

			for (int i = 0; i < parseInformation.Count(); i++)
				extraTranslations[i] = new ExtraTranslation(
					(string) parseInformation[i][0], parseInformation[i][1].ToObject<string[]>());
			
//			ExtraTranslation extraTranslation = new ExtraTranslation(
//				(string)parseInformation[0], parseInformation[1].ToObject<string[]>());
			
//			var translations = parseInformation.ToDictionary(translation => 
//				(string) translation[0], translation => translation[1].ToObject<string[]>());

			property.SetMethod.Invoke(this,
				new object[] { extraTranslations } );

			return true;
		}

		internal override int ItemDataIndex => 2;
	}
}
