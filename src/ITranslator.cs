using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleTranslateFreeApi
{
	public interface ITranslator
	{
		Task<TranslationResult> TranslateAsync(ITranslatable item);
		Task<TranslationResult> TranslateAsync(string text, Language from, Language to);
	}
}
