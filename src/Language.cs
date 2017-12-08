using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleTranslateFreeApi
{
	public class Language
	{
		public static Language Auto = new Language("Automatic", "auto");
		public string FullName { get; }
		public string ISO639 { get; }

		public Language(string fullName, string iso639)
		{
			FullName = fullName;
			ISO639 = iso639;
		}

		protected bool Equals(Language other)
		{
			return string.Equals(ISO639, other.ISO639, StringComparison.OrdinalIgnoreCase);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			
			return Equals((Language) obj);
		}

		public override int GetHashCode()
		{
			return StringComparer.OrdinalIgnoreCase.GetHashCode(ISO639);
		}


		public override string ToString()
		{
			return $"FullName: {FullName}, ISO639: {ISO639}";
		}
	}
}
