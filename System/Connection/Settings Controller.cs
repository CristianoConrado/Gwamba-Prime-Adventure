using UnityEngine;
using System.IO;
namespace GwambaPrimeAdventure.Connection
{
	public struct Settings
	{
		public Vector2Int ScreenResolution;
		public FullScreenMode FullScreenMode;
		public float
			DialogSpeed,
			ScreenBrightness,
			GeneralVolume,
			EffectsVolume,
			MusicVolume;
		public ushort
			FrameRate,
			SimulationHertz,
			VSync;
		public bool
			DialogToggle,
			GeneralVolumeToggle,
			EffectsVolumeToggle,
			MusicVolumeToggle,
			InfinityFPS;
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
					DialogSpeed = 5E-2F,
					ScreenBrightness = 1F,
					GeneralVolume = 1F,
					EffectsVolume = 1F,
					MusicVolume = 1F,
					FrameRate = 60,
					SimulationHertz = WorldBuild.DEFAULT_HERTZ,
					VSync = 1,
					DialogToggle = true,
					GeneralVolumeToggle = true,
					EffectsVolumeToggle = true,
					MusicVolumeToggle = true,
					InfinityFPS = false
				};
		}
		public static void WriteSave(Settings settings) => FileEncoder.WriteData(settings, SettingsPath);
	};
};
