using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleTranslateFreeApi
{
	public interface ITranslatable
	{
		string OriginalText { get; }
		Language FromLanguage { get; }
		Language ToLanguage { get; }
	}
}
