namespace GoogleTranslateFreeApi.TranslationData
{
	public sealed class Corrections
	{
		public bool TextWasCorrected { get; internal set; }
		public bool LanguageWasCorrected { get; internal set; }
		public string CorrectedText { get; internal set; }
		public string[] CorrectedWords { get; internal set; }
		public Language CorrectedLanguage { get; internal set; }
		public double Confidence { get; internal set; } = 1.0;

		internal Corrections() { }
	}
}
