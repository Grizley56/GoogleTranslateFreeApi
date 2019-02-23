using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;

namespace GoogleTranslateFreeApi.TranslationData
{
	[DataContract]
	public sealed class Definitions : TranslationInfoParser
	{
		[DataContract]
		public sealed class Definition
		{
			[DataMember] public string Explanation { get; private set; }
			[DataMember] public string Example { get; private set; }
			
			internal Definition(string explantion, string example)
			{
				Explanation = explantion;
				Example = example;
			}
			
			public override string ToString() => $"Explantion: {Explanation} Example: {Example}";
		}

		[DataMember] public Definition[] Noun { get; internal set; }
		[DataMember] public Definition[] Verb { get; internal set; }
		[DataMember] public Definition[] Exclamation { get; internal set; }
		[DataMember] public Definition[] Adjective { get; internal set; }
		[DataMember] public Definition[] Adverb { get; internal set; }
		[DataMember] public Definition[] Abbreviation { get; internal set; }
		[DataMember] public Definition[] Article { get; internal set; }
		[DataMember] public Definition[] Preposition { get; internal set; }
		[DataMember] public Definition[] Suffix { get; internal set; }
		[DataMember] public Definition[] Conjunction { get; internal set; }
		[DataMember] public Definition[] Pronoun { get; internal set; }
		[DataMember] public Definition[] Prefix { get; internal set; }
		[DataMember] public Definition[] Symbol { get; internal set; }
		[DataMember] public Definition[] Contraction { get; internal set; }


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

		private string FormatOutput(IEnumerable<Definition> formatData, string partOfSpeechName)
		{
			if (formatData == null || !formatData.Any())
				return String.Empty;
			
			int i = 1;
			string tmp = '\n' + partOfSpeechName + ':';
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