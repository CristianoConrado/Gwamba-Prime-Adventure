using UnityEngine;
using System.IO;
namespace GwambaPrimeAdventure.Connection
{
	public struct Settings
	{
		public Vector2Int ScreenResolution;
		public FullScreenMode FullScreenMode;
		public bool DialogToggle;
		public bool GeneralVolumeToggle;
		public bool EffectsVolumeToggle;
		public bool MusicVolumeToggle;
		public bool InfinityFPS;
		public float DialogSpeed;
		public float ScreenBrightness;
		public float GeneralVolume;
		public float EffectsVolume;
		public float MusicVolume;
		public ushort FrameRate;
		public ushort VSync;
	};
	public static class SettingsController
	{
		private static readonly string SettingsPath = $@"{Application.persistentDataPath}\Settings.txt";
		public static bool FileExists() => File.Exists(SettingsPath);
		public static void Load(out Settings settings)
		{
			if (File.Exists(SettingsPath))
				settings = FileEncoder.ReadData<Settings>(SettingsPath);
			else
				settings = new Settings()
				{
					ScreenResolution = new Vector2Int(WorldBuild.PixelPerfectResolutions()[^1].width, WorldBuild.PixelPerfectResolutions()[^1].height),
					FullScreenMode = FullScreenMode.FullScreenWindow,
					DialogToggle = true,
					GeneralVolumeToggle = true,
					EffectsVolumeToggle = true,
					MusicVolumeToggle = true,
					InfinityFPS = false,
					DialogSpeed = 5E-2F,
					ScreenBrightness = 1F,
					GeneralVolume = 1F,
					EffectsVolume = 1F,
					MusicVolume = 1F,
					FrameRate = 60,
					VSync = 1
				};
		}
		public static void WriteSave(Settings settings) => FileEncoder.WriteData(settings, SettingsPath);
	};
};
