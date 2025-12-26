using Cysharp.Threading.Tasks;
using GwambaPrimeAdventure.Connection;
using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace GwambaPrimeAdventure.Hud
{
	[DisallowMultipleComponent, Icon( WorldBuild.PROJECT_ICON ), RequireComponent( typeof( Transform ), typeof( UIDocument ) )]
	internal sealed class ConfigurationHud : MonoBehaviour
	{
		private static ConfigurationHud _instance;
		internal VisualElement RootElement { get; private set; }
		internal GroupBox Settings { get; private set; }
		internal GroupBox Confirmation { get; private set; }
		internal DropdownField ScreenResolution { get; private set; }
		internal DropdownField FullScreenModes { get; private set; }
		internal DropdownField SimulationHertz { get; private set; }
		internal Toggle GeneralVolumeToggle { get; private set; }
		internal Toggle EffectsVolumeToggle { get; private set; }
		internal Toggle MusicVolumeToggle { get; private set; }
		internal Toggle InfinityFPS { get; private set; }
		internal Toggle DialogToggle { get; private set; }
		internal Slider ScreenBrightness { get; private set; }
		internal Slider GeneralVolume { get; private set; }
		internal Slider EffectsVolume { get; private set; }
		internal Slider MusicVolume { get; private set; }
		internal SliderInt FrameRate { get; private set; }
		internal SliderInt VSync { get; private set; }
		internal SliderInt SpeachDelay { get; private set; }
		internal Button Close { get; private set; }
		internal Button OutLevel { get; private set; }
		internal Button SaveGame { get; private set; }
		internal Button Yes { get; private set; }
		internal Button No { get; private set; }
		internal Label ScreenBrightnessText { get; private set; }
		internal Label GeneralVolumeText { get; private set; }
		internal Label EffectsVolumeText { get; private set; }
		internal Label MusicVolumeText { get; private set; }
		internal Label SpeachDelayText { get; private set; }
		internal Label FrameRateText { get; private set; }
		internal Label VSyncText { get; private set; }
		private void Awake()
		{
			if ( _instance )
			{
				Destroy( gameObject, WorldBuild.MINIMUM_TIME_SPACE_LIMIT );
				return;
			}
			_instance = this;
			RootElement = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>( nameof( RootElement ) );
			Settings = RootElement.Q<GroupBox>( nameof( Settings ) );
			Confirmation = RootElement.Q<GroupBox>( nameof( Confirmation ) );
			ScreenResolution = RootElement.Q<DropdownField>( nameof( ScreenResolution ) );
			FullScreenModes = RootElement.Q<DropdownField>( nameof( FullScreenModes ) );
			SimulationHertz = RootElement.Q<DropdownField>( nameof( SimulationHertz ) );
			DialogToggle = RootElement.Q<Toggle>( nameof( DialogToggle ) );
			GeneralVolumeToggle = RootElement.Q<Toggle>( nameof( GeneralVolumeToggle ) );
			EffectsVolumeToggle = RootElement.Q<Toggle>( nameof( EffectsVolumeToggle ) );
			MusicVolumeToggle = RootElement.Q<Toggle>( nameof( MusicVolumeToggle ) );
			InfinityFPS = RootElement.Q<Toggle>( nameof( InfinityFPS ) );
			ScreenBrightness = RootElement.Q<Slider>( nameof( ScreenBrightness ) );
			GeneralVolume = RootElement.Q<Slider>( nameof( GeneralVolume ) );
			EffectsVolume = RootElement.Q<Slider>( nameof( EffectsVolume ) );
			MusicVolume = RootElement.Q<Slider>( nameof( MusicVolume ) );
			FrameRate = RootElement.Q<SliderInt>( nameof( FrameRate ) );
			VSync = RootElement.Q<SliderInt>( nameof( VSync ) );
			SpeachDelay = RootElement.Q<SliderInt>( nameof( SpeachDelay ) );
			Close = RootElement.Q<Button>( nameof( Close ) );
			OutLevel = RootElement.Q<Button>( nameof( OutLevel ) );
			SaveGame = RootElement.Q<Button>( nameof( SaveGame ) );
			Yes = RootElement.Q<Button>( nameof( Yes ) );
			No = RootElement.Q<Button>( nameof( No ) );
			ScreenBrightnessText = RootElement.Q<Label>( nameof( ScreenBrightnessText ) );
			GeneralVolumeText = RootElement.Q<Label>( nameof( GeneralVolumeText ) );
			EffectsVolumeText = RootElement.Q<Label>( nameof( EffectsVolumeText ) );
			MusicVolumeText = RootElement.Q<Label>( nameof( MusicVolumeText ) );
			SpeachDelayText = RootElement.Q<Label>( nameof( SpeachDelayText ) );
			FrameRateText = RootElement.Q<Label>( nameof( FrameRateText ) );
			VSyncText = RootElement.Q<Label>( nameof( VSyncText ) );
		}
		internal async UniTask LoadHud()
		{
			await UniTask.Yield( PlayerLoopTiming.EarlyUpdate );
			SettingsController.Load( out Settings settings );
			if ( !SettingsController.FileExists() )
				SettingsController.WriteSave( settings );
			SpeachDelay.highValue = 100;
			MusicVolume.highValue = EffectsVolume.highValue = GeneralVolume.highValue = ScreenBrightness.highValue = 1F;
			FrameRate.highValue = 120;
			VSync.highValue = 2;
			MusicVolume.lowValue = EffectsVolume.lowValue = GeneralVolume.lowValue = WorldBuild.MINIMUM_TIME_SPACE_LIMIT;
			FrameRate.lowValue = 10;
			ScreenBrightness.lowValue = VSync.lowValue = SpeachDelay.lowValue = 0;
			foreach ( Resolution resolution in BuildMathemathics.PixelPerfectResolutions() )
				ScreenResolution.choices.Add( $@"{resolution.width} x {resolution.height}" );
			foreach ( FullScreenMode mode in Enum.GetValues( typeof( FullScreenMode ) ) )
				FullScreenModes.choices.Add( $"{mode}" );
			SimulationHertz.choices.Add( $"{WorldBuild.DEFAULT_HERTZ} hertz" );
			SimulationHertz.choices.Add( $"{WorldBuild.MEDIUM_HERTZ} hertz" );
			SimulationHertz.choices.Add( $"{WorldBuild.MAXIMUM_HERTZ} hertz" );
			ScreenResolution.value = $@"{settings.ScreenResolution.x} x {settings.ScreenResolution.y}";
			FullScreenModes.value = settings.FullScreenMode.ToString();
			SimulationHertz.value = $"{settings.SimulationHertz} hertz";
			DialogToggle.value = settings.DialogToggle;
			GeneralVolumeToggle.value = settings.GeneralVolumeToggle;
			EffectsVolumeToggle.value = settings.EffectsVolumeToggle;
			MusicVolumeToggle.value = settings.MusicVolumeToggle;
			InfinityFPS.value = settings.InfinityFPS;
			SpeachDelay.value = (ushort) ( settings.SpeachDelay * 1000F );
			ScreenBrightness.value = settings.ScreenBrightness;
			GeneralVolume.value = settings.GeneralVolume;
			EffectsVolume.value = settings.EffectsVolume;
			MusicVolume.value = settings.MusicVolume;
			FrameRate.value = settings.FrameRate;
			VSync.value = settings.VSync;
			ScreenBrightnessText.text = $"{settings.ScreenBrightness}";
			GeneralVolumeText.text = $"{( WorldBuild.MINIMUM_TIME_SPACE_LIMIT < settings.GeneralVolume ? settings.GeneralVolume / 1F : 0 )}";
			EffectsVolumeText.text = $"{(WorldBuild.MINIMUM_TIME_SPACE_LIMIT < settings.EffectsVolume ? settings.EffectsVolume / 1F : 0)}";
			MusicVolumeText.text = $"{(WorldBuild.MINIMUM_TIME_SPACE_LIMIT < settings.MusicVolume ? settings.MusicVolume / 1F : 0)}";
			SpeachDelayText.text = $"{settings.SpeachDelay * 10F}";
			FrameRateText.text = $"{settings.FrameRate}";
			VSyncText.text = $"{settings.VSync}";
		}
	};
};
