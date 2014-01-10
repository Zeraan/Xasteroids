using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xasteroids
{
	public interface IConfigurable
	{
		string[] Configuration { get; set; }
	}
}
