using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using GorgonLibrary.Sound;
using Timer = System.Timers.Timer;

namespace Xasteroids
{
	public class Sounds
	{
		private GSContext _context;
		private SoundBuffer _musicSound;
		private SoundSource _musicSource;
		private SoundBuffer _buttonHoverSound;
		private SoundSource _buttonHoverSource;
		public Timer SoundTimer;
		private readonly string _currPath = Directory.GetCurrentDirectory();
		private string _audioPath = "";
		private Thread _workerThread;

		public Sounds()
		{
			SoundTimer = new Timer();
		}

		public void SoundSetup(SoundTypes sound)
		{

			if (_musicSource != null && _musicSource.State == SoundSource.SoundStates.PLAYING)
			{
				return;
			}
			_context = new GSContext();
			_audioPath = _currPath.Remove(_currPath.Length - 10);
			StartSound(sound);
		}

		private void StartSound(SoundTypes soundChoice)
		{

			switch (soundChoice)
			{
				case SoundTypes.MainTheme:
					_musicSound = new SoundBuffer(_audioPath + @"\\Sounds\\MainTheme.wav", SoundType.Wav);
					_musicSource = new SoundSource(_musicSound) { Loop = true };
					_musicSource.Play();
					break;
				case SoundTypes.ButtonHover:
					_buttonHoverSound = new SoundBuffer(_audioPath + @"\\Sounds\\ButtonHover.wav", SoundType.Wav);
					_buttonHoverSource = new SoundSource(_buttonHoverSound) { Loop = false };
					_buttonHoverSource.Play();
					break;
			}

		}
	}

	public enum SoundTypes { MainTheme, ShotsFired, ButtonHover, };

}
