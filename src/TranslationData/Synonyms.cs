using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace GoogleTranslateFreeApi.TranslationData
{
	public sealed class Synonyms: TranslationInfoParser
	{
		public IEnumerable<string> Noun { get; internal set; }
		public IEnumerable<string> Exclamation { get; internal set; }
		public IEnumerable<string> Adjective { get; internal set; }
		public IEnumerable<string> Verb { get; internal set; }
		public IEnumerable<string> Adverb { get; internal set; }
		public IEnumerable<string> Preposition { get; internal set; }
		public IEnumerable<string> Conjunction { get; internal set; }
		public IEnumerable<string> Pronoun { get; internal set; }

		internal Synonyms() { }

		public override string ToString()
		{
			string info = String.Empty;
			info += FormatOutput(Noun, nameof(Noun));
			info += FormatOutput(Verb, nameof(Verb));
			info += FormatOutput(Pronoun, nameof(Pronoun));
			info += FormatOutput(Adverb, nameof(Adverb));
			info += FormatOutput(Adjective, nameof(Adjective));
			info += FormatOutput(Conjunction, nameof(Conjunction));
			info += FormatOutput(Preposition, nameof(Preposition));
			info += FormatOutput(Exclamation, nameof(Exclamation));

			return info.TrimEnd();
		}

		private string FormatOutput(IEnumerable<string> partOfSpretchData, string partOfSpretchName)
		{
			if (partOfSpretchData == null)
				return String.Empty;

			return !partOfSpretchData.Any()
				? String.Empty
				: $"{partOfSpretchName}: {string.Join(", ", partOfSpretchData)} \n";
		}

		internal override bool TryParseMemberAndAdd(string memberName, JToken parseInformation)
		{
			PropertyInfo property = this.GetType().GetRuntimeProperty(memberName.ToCamelCase());
			if (property == null)
				return false;
			
			List<string> synonyms = new List<string>();
			foreach (var synonymsSet in parseInformation)
				synonyms.AddRange(synonymsSet[0].ToObject<string[]>());
			
			property.SetMethod.Invoke(this, new object[] { synonyms });
			
			return true;
		}

		internal override int ItemDataIndex => 1;
	}
}
