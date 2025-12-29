using UnityEngine;
using System.IO;
namespace GwambaPrimeAdventure.Connection
{
	public struct Settings
	{
		public
			Vector2Int ScreenResolution;
		public
			FullScreenMode FullScreenMode;
		public float
			SpeachDelay,
			ScreenBrightness,
			GeneralVolume,
			EffectsVolume,
			MusicVolume;
		public ushort
			FrameRate,
			SimulationHertz,
			VSync;
		public bool
			GeneralVolumeToggle,
			EffectsVolumeToggle,
			MusicVolumeToggle,
			InfinityFPS,
			DialogToggle;
	};
	public static class SettingsController
	{
		private static readonly string
			SettingsPath = $@"{Application.persistentDataPath}\Settings.txt";
		public static bool FileExists() => File.Exists( SettingsPath );
		public static void Load( out Settings settings )
		{
			settings = File.Exists( SettingsPath )
				? FileEncoder.ReadData<Settings>( SettingsPath ) 
				: new Settings()
				{
					ScreenResolution = new Vector2Int( BuildMathemathics.PixelPerfectResolutions()[ ^1 ].width, BuildMathemathics.PixelPerfectResolutions()[ ^1 ].height ),
					FullScreenMode = FullScreenMode.FullScreenWindow,
					ScreenBrightness = 1F,
					GeneralVolume = 1F,
					EffectsVolume = 1F,
					MusicVolume = 1F,
					FrameRate = 60,
					SimulationHertz = WorldBuild.DEFAULT_HERTZ_PER_SECOND,
					VSync = 1,
					SpeachDelay = 5E-2F,
					GeneralVolumeToggle = true,
					EffectsVolumeToggle = true,
					MusicVolumeToggle = true,
					InfinityFPS = false,
					DialogToggle = true
				};
		}
		public static void WriteSave( Settings settings ) => FileEncoder.WriteData( settings, SettingsPath );
	};
};
