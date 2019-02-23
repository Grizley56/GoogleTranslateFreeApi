using System.Runtime.Serialization;

namespace GoogleTranslateFreeApi.TranslationData
{
	[DataContract]
	public sealed class Corrections
	{
		[DataMember] public bool TextWasCorrected { get; internal set; }
		[DataMember] public bool LanguageWasCorrected { get; internal set; }
		[DataMember] public string CorrectedText { get; internal set; }
		[DataMember] public string[] CorrectedWords { get; internal set; }
		[DataMember] public Language CorrectedLanguage { get; internal set; }
		
		[DataMember(EmitDefaultValue = false)] 
		public double Confidence { get; internal set; } = 1.0;

		internal Corrections() { }
	}
}
