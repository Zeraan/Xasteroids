using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtensionMethods
{
	public static class MyExtensions
	{
		public static string EscapeCommas(this String str)
		{
			return str.Replace(",", @"\,");
		}

		public static string UnEscapeCommas(this String str)
		{
			return str.Replace(@"\,", ",");
		}

		public static string EscapeCurlyBrackets(this String str)
		{
			str = str.Replace("{", @"\{");
			return str.Replace("}", @"\}");
		}

		public static string UnEscapeCurlyBrackets(this String str)
		{
			str = str.Replace(@"\{", "{");
			return str.Replace(@"\}", "}");
		}

		public static string EscapeSquareBrackets(this String str)
		{
			str = str.Replace("[", @"\[");
			return str.Replace("]", @"\]");
		}

		public static string UnEscapeSquareBrackets(this String str)
		{
			str = str.Replace(@"\[", "[");
			return str.Replace(@"\]", "]");
		}
	}
}
