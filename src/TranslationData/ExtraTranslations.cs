using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace GoogleTranslateFreeApi.TranslationData
{
	public class ExtraTranslations : TranslationInfoParser
	{
		public struct ExtraTranslation
		{
			public string Phrase;
			public string[] PhraseTranslations;			
		}
		
		public ReadOnlyDictionary<string, string[]> Noun { get; internal set; }
		public ReadOnlyDictionary<string, string[]> Verb { get; internal set; }
		public ReadOnlyDictionary<string, string[]> Pronoun { get;  internal set; }
		public ReadOnlyDictionary<string, string[]> Adverb { get; internal set; }
		public ReadOnlyDictionary<string, string[]> AuxiliaryVerb { get; internal set; }
		public ReadOnlyDictionary<string, string[]> Adjective { get; internal set; }
		public ReadOnlyDictionary<string, string[]> Conjunction { get; internal set; }
		public ReadOnlyDictionary<string, string[]> Preposition { get; internal set; }
		public ReadOnlyDictionary<string, string[]> Interjection { get; internal set; }
		public ReadOnlyDictionary<string, string[]> Suffix { get; internal set; }
		public ReadOnlyDictionary<string, string[]> Prefix { get; internal set; }
		public ReadOnlyDictionary<string, string[]> Abbreviation { get; internal set; }
		public ReadOnlyDictionary<string, string[]> Particle { get; internal set; }
		public ReadOnlyDictionary<string, string[]> Phrase { get; internal set; }
		
		public ExtraTranslations() { }

		
		private string FormatOutput(IDictionary<string, string[]> formatData, string partOfSpretchName)
		{
			if(formatData == null)
				return String.Empty;
			
			string result = partOfSpretchName + ":\n";

			return formatData.Aggregate(result, (current, data) 
				=> current + $"{data.Key}: {String.Join(", ", data.Value)}\n");
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
			
			var translations = parseInformation.ToDictionary(translation => 
				(string) translation[0], translation => translation[1].ToObject<string[]>());

			property.SetMethod.Invoke(this,
				new object[] { new ReadOnlyDictionary<string, string[]>(translations) });

			return true;
		}

		internal override int ItemDataIndex => 2;
	}
}
