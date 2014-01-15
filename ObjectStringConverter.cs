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

		public static Regex SimpleValueRegex = new Regex(@"(" +
																@"[^,\[]" +				//anything not a comma or a left bracket...
																@"|((?<=\\)\[)" +		//or an escaped left bracket...
																@"|((?<=\\),)" +		//or an escaped comma...
															")+" +						//...one or more of them
															@"|((?<=[^\\],)(?=,))",		//Or an empty string between two commas
			RegexOptions.Compiled
		);

		public static Regex ConfigurationRegex = new Regex(@"(?<!\\)\[" +
																"(" +
																	@"[^\[\]]+" +
																	"|" +
																	@"(?'open'(?<!\\)\[)" +
																	"|" +
																	@"(?'-open'(?<!\\)\])" +
																")*" +
																"(?(open) (?!))" +
															@"\]",
			RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled
		);

		public static Regex MixedRegex = new Regex("(" + SimpleValueRegex.ToString() + ")|(" + ConfigurationRegex.ToString() + ")", RegexOptions.Compiled);

		public string ObjectToString(IConfigurable obj)
		{
			string[] configuration = obj.Configuration;
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
			MatchCollection valueMatches = MixedRegex.Matches(configurationString);
			string[] configurationArray = new string[valueMatches.Count];
			for (int j = 0; j < configurationArray.Length; ++j)
			{
				configurationArray[j] = valueMatches[j].Value;
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

		public static string[] ConfigurationFromStringOfConfigurations(string configurations)
		{
			MatchCollection matches = ConfigurationRegex.Matches(configurations);
			string[] configuration = new string[matches.Count];
			for (int j = 0; j < matches.Count; ++j)
			{
				configuration[j] = matches[j].Value;
			}
			return configuration;
		}

		public static string[] ConfigurationFromMixedString(string mixed)
		{
			MatchCollection matches = MixedRegex.Matches(mixed);
			string[] configuration = new string[matches.Count];
			for (int j = 0; j < matches.Count; ++j)
			{
				configuration[j] = matches[j].Value;
			}
			return configuration;
		}

		public static string IConfigurableListToArrayString<T>(IList<T> configurableCollection)
			where T : IConfigurable
		{
			if (configurableCollection == null || configurableCollection.Count == 0)
			{
				return "[]";
			}
			else
			{
				string[] itemArrayStrings = new string[configurableCollection.Count];
				for (int j = 0; j <configurableCollection.Count; ++j)
				{
					IConfigurable item = configurableCollection[j];
					string itemArrayString = "[" + string.Join(",", item.Configuration) + "]";
					itemArrayStrings[j] = itemArrayString;
				}
				return "[" + string.Join(",", itemArrayStrings) + "]";
			}
		}
	}
}
