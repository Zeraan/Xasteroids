using System;
using System.Globalization;

namespace Asteroids_of_Beyaan
{
	static class Utility
	{
		public static int GetIntValue(string value, Random r)
		{
			//There are three possible types of "value"
			//The first is a basic value, like "5", it is always returned
			//The second is a range of values, like "0,5", randomly pick one inside the range
			//The third is a weighted range of values, like "0,1,0.05" which leans toward 0 with 5% chance of picking 1
			string[] parts = value.Split(new[] { ',' });
			if (parts.Length == 1)
			{
				//Just return the value
				return int.Parse(parts[0]);
			}
			if (parts.Length == 2)
			{
				//return a value in the range
				return r.Next(int.Parse(parts[0]), int.Parse(parts[1]) + 1);
			}
			if (parts.Length == 3)
			{
				//return a value in the weighted range
				double randVal;
				do
				{
					randVal = r.NextDouble();
				} while (randVal == 0); //Make sure it's not 0, otherwise it'd throw an exception in the next line of code

				double weight = NormalCDFInverse(randVal);
				int min = int.Parse(parts[0]);
				int max = int.Parse(parts[1]);
				float shift = float.Parse(parts[2], CultureInfo.InvariantCulture); //Shift moves the standard distribution left or right (does not skew it, just moves it, 0.5 is default)
				int newValue = (int)(((min + max) * shift) + (weight * (max - min)));
				if (newValue < min)
				{
					return min;
				}
				if (newValue > max)
				{
					return max;
				}
				return newValue;
			}
			throw new Exception("GetIntValue cannot parse the value of '" + value + "'");
		}

		private static double RationalApproximation(double t)
		{
			// Abramowitz and Stegun formula 26.2.23.
			// The absolute value of the error should be less than 4.5 e-4.
			double[] c = { 2.515517, 0.802853, 0.010328 };
			double[] d = { 1.432788, 0.189269, 0.001308 };
			return t - ((c[2] * t + c[1]) * t + c[0]) /
						(((d[2] * t + d[1]) * t + d[0]) * t + 1.0);
		}

		private static double NormalCDFInverse(double p)
		{
			if (p <= 0.0 || p >= 1.0)
			{
				string msg = String.Format("Invalid input argument: {0}.", p);
				throw new ArgumentOutOfRangeException(msg);
			}

			// See article above for explanation of this section.
			if (p < 0.5)
			{
				// F^-1(p) = - G^-1(p)
				return -RationalApproximation(Math.Sqrt(-2.0 * Math.Log(p)));
			}
			// F^-1(p) = G^-1(1-p)
			return RationalApproximation(Math.Sqrt(-2.0 * Math.Log(1.0 - p)));
		}

	}
}
