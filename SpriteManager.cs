using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Xasteroids
{
	public static class SpriteManager
	{
		public static Dictionary<string, BaseSprite> Sprites { get; private set; }
		private static bool _initalized;

		public static bool Initialize(out string reason)
		{
			if (_initalized)
			{
				reason = null;
				return true;
			}
			try
			{
				Sprites = new Dictionary<string, BaseSprite>();
				string file = Path.Combine(Environment.CurrentDirectory, "sprites.xml");
				string graphicDirectory = Path.Combine(Environment.CurrentDirectory, "graphics");
				if (!File.Exists(file))
				{
					reason = "Sprites.xml file does not exist";
					return false;
				}

				XDocument doc = XDocument.Load(file);
				XElement root = doc.Element("Sprites");
				foreach (XElement sprite in root.Elements())
				{
					var newSprite = new BaseSprite();
					if (!newSprite.LoadSprite(sprite, graphicDirectory, out reason))
					{
						return false;
					}
					Sprites.Add(newSprite.Name, newSprite);
				}
				_initalized = true;
				reason = null;
				return true;
			}
			catch (Exception e)
			{
				reason = e.Message;
				return false;
			}
		}

		public static BBSprite GetSprite(string name, Random r)
		{
			if (Sprites.ContainsKey(name))
			{
				return new BBSprite(Sprites[name], r);
			}
			return null;
		}

		public static BBSprite GetShipSprite(int size, int style, Random r)
		{
			string nameBuilder;
			string styleString;
			if (style <= 6)
			{
				//Human ship
				nameBuilder = "Human";
				styleString = style.ToString();
			}
			else if (style <= 12)
			{
				//Space Hamster ship
				nameBuilder = "SpaceHamster";
				styleString = (style - 6).ToString();
			}
			else
			{
				//Zero People ship
				nameBuilder = "ZeroPeople";
				styleString = (style - 12).ToString();
			}
			switch (size)
			{
				case 1:
					nameBuilder += "TinyShip" + styleString;
					return GetSprite(nameBuilder, r);
				case 2:
					nameBuilder += "SmallShip" + styleString;
					return GetSprite(nameBuilder, r);
				case 3:
					nameBuilder += "MediumShip" + styleString;
					return GetSprite(nameBuilder, r);
				case 4:
					nameBuilder += "LargeShip" + styleString;
					return GetSprite(nameBuilder, r);
				case 5:
					nameBuilder += "HugeShip" + styleString;
					return GetSprite(nameBuilder, r);
				case 6:
					nameBuilder += "TitanShip";
					return GetSprite(nameBuilder, r);
			}
			return null;
		}
	}
}
