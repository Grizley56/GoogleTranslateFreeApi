using System.Runtime.Serialization;

namespace GoogleTranslateFreeApi.TranslationData
{
	[DataContract]
	public sealed class Corrections
	{
		[DataMember] public bool TextWasCorrected { get; internal set; }
		[DataMember] public string CorrectedText { get; internal set; }
		[DataMember] public string[] CorrectedWords { get; internal set; }

		internal Corrections() { }
	}
}
