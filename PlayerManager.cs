using System;
using System.Collections.Generic;
using System.Drawing;

namespace Xasteroids
{
	public class PlayerManager
	{
		private List<Player> _players = new List<Player>();
		public List<Player> Players { get { return _players; } }
		public Player MainPlayer { get { return _players[0]; } } 

		public void AddPlayer(Player player)
		{
			_players.Add(player);
		}

		public void ClearMainPlayer(Random r)
		{
			if (_players.Count == 0)
			{
				_players.Add(new Player(1, 1, Color.Red, SpriteManager.GetShipSprite(1, 1, r)));
			}
			MainPlayer.Bank = 1000;
		}
	}

	public class Player
	{
		public float PositionX { get; private set; }
		public float PositionY { get; private set; }
		public float VelocityX { get; private set; }
		public float VelocityY { get; private set; }
		public float Angle { get; private set; }

		public int MaxEnergy { get; private set; }
		public float Energy { get; private set; }
		public float RechargeRate { get; private set; }

		public float CoolDownPeriod { get; private set; }
		public float CoolDown { get; private set; }

		public int ShipSize { get; set; }
		public int ShipStyle { get; set; }
		public Color ShipColor { get; set; }
		public BBSprite ShipSprite { get; set; }

		public int Bank { get; set; }

		public Player(int shipSize, int shipStyle, Color shipColor, BBSprite shipSprite)
		{
			ShipSize = shipSize;
			ShipStyle = shipStyle;
			ShipColor = shipColor;
			ShipSprite = shipSprite;
		}
	}
}
