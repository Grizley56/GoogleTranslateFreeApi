using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleTranslateFreeApi
{
	class ExternalKeyParseException : Exception
	{
		public ExternalKeyParseException()
			:this("External key parse failed") { }

		public ExternalKeyParseException(string message)
			:base(message) { }
	}
}
