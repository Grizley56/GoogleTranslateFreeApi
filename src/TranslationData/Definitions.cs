using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace GoogleTranslateFreeApi.TranslationData
{
	public sealed class Definitions : TranslationInfoParser
	{
		public class Definition
		{
			public string Explanation { get; }
			public string Example { get; }
			internal Definition(string explantion, string example)
			{
				Explanation = explantion;
				Example = example;
			}
			
			public override string ToString() => $"Explantion: {Explanation} Example: {Example}";
		}

		public Definition[] Noun { get; internal set; }
		public Definition[] Verb { get; internal set; }
		public Definition[] Exclamation { get; internal set; }
		public Definition[] Adjective { get; internal set; }
		public Definition[] Adverb { get; internal set; }
		public Definition[] Abbreviation { get; internal set; }
		public Definition[] Article { get; internal set; }
		public Definition[] Preposition { get; internal set; }
		public Definition[] Suffix { get; internal set; }
		public Definition[] Conjunction { get; internal set; }
		public Definition[] Pronoun { get; internal set; }
		public Definition[] Prefix { get; internal set; }
		public Definition[] Symbol { get; internal set; }
		public Definition[] Contraction { get; internal set; }


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
			info += FormatOutput(Suffix, nameof(Suffix));
			info += FormatOutput(Prefix, nameof(Prefix));
			info += FormatOutput(Contraction, nameof(Contraction));
			info += FormatOutput(Abbreviation, nameof(Abbreviation));
			info += FormatOutput(Symbol, nameof(Symbol));
			info += FormatOutput(Article, nameof(Article));

			return info.Trim();
		}

		private string FormatOutput(IEnumerable<Definition> formatData, string partOfSpretchName)
		{
			if (formatData == null || !formatData.Any())
				return String.Empty;
			
			int i = 1;
			string tmp = '\n' + partOfSpretchName + ':';
			return formatData.Aggregate(
				tmp, (current, definition) => current + ($"\n{i++}) " + definition.ToString()));
		}


		internal override bool TryParseMemberAndAdd(string memberName, JToken parseInformation)
		{
			PropertyInfo property = this.GetType().GetRuntimeProperty(memberName.ToCamelCase());
			if (property == null)
				return false;
			
			var definitions = (
				from definitionUnformatted in parseInformation
				let explantion = (string) definitionUnformatted[0] 
				let example = (string) definitionUnformatted[definitionUnformatted.Count() - 1] 
				select new Definition(explantion, example)
				).ToArray();

			property.SetMethod.Invoke(this, new object[] { definitions });
			
			return true;
		}

		internal override int ItemDataIndex => 1;
	}

}