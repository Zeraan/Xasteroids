using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
					Content
				};
			}
			set
			{
				if (value.Length == 0)
				{
					return;
				}
				Content = value[0];
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
					Content
				};
			}
			set
			{
				if (value.Length == 0)
				{
					return;
				}
				Content = value[0];
			}
		}
	}
}
