using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleTranslateFreeApi
{
	class LanguageNotSupportException: Exception
	{
		public readonly Language Language;

		public LanguageNotSupportException(Language language)
			:base("Language not support:")
		{
			Language = language;
		}

		public override string Message => base.Message + " " + Language.ToString();
	}
}
