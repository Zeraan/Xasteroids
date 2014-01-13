using ExtensionMethods;

namespace Xasteroids
{
	public class NetworkMessage : IConfigurable
	{
		public string Content { get; set; }
		public string[] Configuration
		{
			get
			{
				return new string[] {
					Content.EscapeCommas().EscapeSquareBrackets().EscapeCurlyBrackets()
				};
			}
			set
			{
				if (value.Length == 0)
				{
					return;
				}
				Content = value[0].UnEscapeCommas().UnEscapeSquareBrackets().UnEscapeCurlyBrackets();
			}
		}
	}

	public class GameMessage : IConfigurable
	{
		public string Content { get; set; }
		public string[] Configuration
		{
			get
			{
				return new string[] {
					Content.EscapeCommas().EscapeSquareBrackets().EscapeCurlyBrackets()
				};
			}
			set
			{
				if (value.Length == 0)
				{
					return;
				}
				Content = value[0].UnEscapeCommas().UnEscapeSquareBrackets().UnEscapeCurlyBrackets();
			}
		}
	}
}
