using ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Xasteroids
{
	public class ObjectStringConverter
	{
		private static Regex _objectRegex = new Regex(@"^\{Type:([a-zA-Z0-9._]+),\[(.+)[^\\]\}$", RegexOptions.Compiled);
		private static Regex _unescapedCommaRegex = new Regex(@"(?<=[^\\]),", RegexOptions.Compiled);
		private static Regex _unescapedCurlyBracketRegex = new Regex(@"(?<=[^\\](\{|\})),", RegexOptions.Compiled);
		
		/* The way I've got the data formatted, an unescaped right curly bracket ends
		 * the string representation of an object.
		 */
		private static Regex _objectEndRegex = new Regex(@"(?<=[^\\]\})", RegexOptions.Compiled);
		public static Regex ObjectEndRegex { get { return _objectEndRegex; } }
		private static Regex _objectStartRegex = new Regex(@"(^\{|(?<=[^\\])\{)", RegexOptions.Compiled);
		public static Regex ObjectStartRegex { get { return _objectStartRegex; } }
		private static Regex _spaceBetweenObjectsRegex = new Regex(@"(?<=[^\\]\}).*?(?=[^\\]\{)", RegexOptions.Compiled);
		public static Regex SpaceBetweenObjectsRegex { get { return _spaceBetweenObjectsRegex; } }

		public string ObjectToString(IConfigurable obj)
		{
			string[] configuration = obj.Configuration;
			for (int j = 0; j < configuration.Length; ++j)
			{
				configuration[j] = configuration[j].EscapeCommas().EscapeCurlyBrackets();
			}
			string arrayString = "[" + string.Join(",", configuration) + "]";
			return "{" +
						"Type:" + obj.GetType().FullName + "," +
						arrayString +
 					"}";
		}

		public IConfigurable StringToObject(string str)
		{
			if (ObjectStringMalformed(str))
			{
				return null;
			}

			Match theMatch = _objectRegex.Match(str);
			string fullTypeName = theMatch.Groups[1].Value;
			string configurationString = theMatch.Groups[2].Value;
			string[] configurationArray = _unescapedCommaRegex.Split(configurationString);
			for (int j = 0; j < configurationArray.Length; ++j)
			{
				configurationArray[j] = configurationArray[j].UnEscapeCommas().UnEscapeEscapeCurlyBrackets();
			}
			IConfigurable configurableGuy = null;
			if (fullTypeName.Equals(typeof(NetworkMessage).FullName))
			{
				configurableGuy = new NetworkMessage();
			}
			else if (fullTypeName.Equals(typeof(GameMessage).FullName))
			{
				configurableGuy = new GameMessage();
			}

			if (null != configurableGuy)
			{
				configurableGuy.Configuration = configurationArray;
			}
			return configurableGuy;
		}

		public bool ObjectStringMalformed(string objectString)
		{
			Match theMatch = _objectRegex.Match(objectString);
			if (!theMatch.Success)
			{
				return true;
			}
			string configurationString = theMatch.Groups[2].Value;
			return _unescapedCurlyBracketRegex.Match(configurationString).Success;
		}
	}
}
