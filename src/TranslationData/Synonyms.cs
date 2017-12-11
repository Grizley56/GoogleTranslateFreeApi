using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace GoogleTranslateFreeApi.TranslationData
{
	public sealed class Synonyms: TranslationInfoParser
	{
		public string[] Noun { get; internal set; }
		public string[] Exclamation { get; internal set; }
		public string[] Adjective { get; internal set; }
		public string[] Verb { get; internal set; }
		public string[] Adverb { get; internal set; }
		public string[] Preposition { get; internal set; }
		public string[] Conjunction { get; internal set; }
		public string[] Pronoun { get; internal set; }

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
			
			property.SetMethod.Invoke(this, new object[] { synonyms.ToArray() });
			
			return true;
		}

		internal override int ItemDataIndex => 1;
	}
}
