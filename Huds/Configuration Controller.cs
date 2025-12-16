using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using GwambaPrimeAdventure.Connection;
namespace GwambaPrimeAdventure.Hud
{
	[DisallowMultipleComponent, Icon(WorldBuild.PROJECT_ICON), RequireComponent(typeof(Transform), typeof(Transitioner))]
	internal sealed class ConfigurationController : MonoBehaviour, IConnector
	{
		private ConfigurationHud _configurationHud;
		private InputController _inputController;
		private bool _isActive = true;
		[Header("Interaction Objects")]
		[SerializeField, Tooltip("The object that handles the hud of the configurations.")] private ConfigurationHud _configurationHudObject;
		[SerializeField, Tooltip("The scene of the menu.")] private SceneField _menuScene;
		[SerializeField, Tooltip("The scene of the level selector.")] private SceneField _levelSelectorScene;
		[SerializeField, Tooltip("The mixer of the sounds.")] private AudioMixer _mixer;
		internal static ConfigurationController Instance { get; private set; }
		public MessagePath Path => MessagePath.Hud;
		private void Awake()
		{
			if (Instance)
			{
				Destroy(gameObject, WorldBuild.MINIMUM_TIME_SPACE_LIMIT);
				return;
			}
			(Instance, _configurationHud) = (this, Instantiate(_configurationHudObject, transform));
			SceneManager.sceneLoaded += SceneLoaded;
			Sender.Include(this);
		}
		private void OnDestroy()
		{
			if (!Instance || this != Instance)
				return;
			_configurationHud.Close.clicked -= CloseConfigurations;
			_configurationHud.OutLevel.clicked -= OutLevel;
			_configurationHud.SaveGame.clicked -= SaveGame;
			_configurationHud.ScreenResolution.UnregisterValueChangedCallback(ScreenResolution);
			_configurationHud.FullScreenModes.UnregisterValueChangedCallback(FullScreenModes);
			_configurationHud.DialogToggle.UnregisterValueChangedCallback(DialogToggle);
			_configurationHud.GeneralVolumeToggle.UnregisterValueChangedCallback(GeneralVolumeToggle);
			_configurationHud.EffectsVolumeToggle.UnregisterValueChangedCallback(EffectsVolumeToggle);
			_configurationHud.MusicVolumeToggle.UnregisterValueChangedCallback(MusicVolumeToggle);
			_configurationHud.InfinityFPS.UnregisterValueChangedCallback(InfinityFPS);
			_configurationHud.DialogSpeed.UnregisterValueChangedCallback(DialogSpeed);
			_configurationHud.ScreenBrightness.UnregisterValueChangedCallback(ScreenBrightness);
			_configurationHud.GeneralVolume.UnregisterValueChangedCallback(GeneralVolume);
			_configurationHud.EffectsVolume.UnregisterValueChangedCallback(EffectsVolume);
			_configurationHud.MusicVolume.UnregisterValueChangedCallback(MusicVolume);
			_configurationHud.FrameRate.UnregisterValueChangedCallback(FrameRate);
			_configurationHud.VSync.UnregisterValueChangedCallback(VSync);
			_configurationHud.Yes.clicked -= YesBackLevel;
			_configurationHud.No.clicked -= NoBackLevel;
			SceneManager.sceneLoaded -= SceneLoaded;
			Sender.Exclude(this);
		}
		private void OnEnable()
		{
			if (!Instance || this != Instance)
				return;
			_inputController = new InputController();
			_inputController.Commands.HideHud.canceled += HideHudAction;
			_inputController.Commands.HideHud.Enable();
		}
		private void OnDisable()
		{
			if (!Instance || this != Instance)
				return;
			_inputController.Commands.HideHud.canceled -= HideHudAction;
			_inputController.Commands.HideHud.Disable();
			_inputController.Dispose();
		}
		private IEnumerator Start()
		{
			if (!Instance || this != Instance)
				yield break;
			yield return StartCoroutine(StartLoad());
			yield return _configurationHud.LoadHud();
			_configurationHud.Close.clicked += CloseConfigurations;
			_configurationHud.OutLevel.clicked += OutLevel;
			_configurationHud.SaveGame.clicked += SaveGame;
			_configurationHud.ScreenResolution.RegisterValueChangedCallback(ScreenResolution);
			_configurationHud.FullScreenModes.RegisterValueChangedCallback(FullScreenModes);
			_configurationHud.DialogToggle.RegisterValueChangedCallback(DialogToggle);
			_configurationHud.GeneralVolumeToggle.RegisterValueChangedCallback(GeneralVolumeToggle);
			_configurationHud.EffectsVolumeToggle.RegisterValueChangedCallback(EffectsVolumeToggle);
			_configurationHud.MusicVolumeToggle.RegisterValueChangedCallback(MusicVolumeToggle);
			_configurationHud.InfinityFPS.RegisterValueChangedCallback(InfinityFPS);
			_configurationHud.ScreenBrightness.RegisterValueChangedCallback(ScreenBrightness);
			_configurationHud.DialogSpeed.RegisterValueChangedCallback(DialogSpeed);
			_configurationHud.GeneralVolume.RegisterValueChangedCallback(GeneralVolume);
			_configurationHud.EffectsVolume.RegisterValueChangedCallback(EffectsVolume);
			_configurationHud.MusicVolume.RegisterValueChangedCallback(MusicVolume);
			_configurationHud.FrameRate.RegisterValueChangedCallback(FrameRate);
			_configurationHud.VSync.RegisterValueChangedCallback(VSync);
			_configurationHud.Yes.clicked += YesBackLevel;
			_configurationHud.No.clicked += NoBackLevel;
			SettingsController.Load(out Settings settings);
			_mixer.SetFloat(nameof(GeneralVolume), Mathf.Log10(settings.GeneralVolume) * 20F);
			_mixer.SetFloat(nameof(EffectsVolume), Mathf.Log10(settings.EffectsVolume) * 20F);
			_mixer.SetFloat(nameof(MusicVolume), Mathf.Log10(settings.MusicVolume) * 20F);
			DontDestroyOnLoad(gameObject);
		}
		private IEnumerator StartLoad()
		{
			_configurationHud.RootElement.style.display = DisplayStyle.None;
			yield return new WaitUntil(() => _isActive = !SceneInitiator.IsInTrancision());
		}
		private void SceneLoaded(Scene scene, LoadSceneMode loadMode) => StartCoroutine(StartLoad());
		private void HideHudAction(InputAction.CallbackContext hideHud) => OpenCloseConfigurations();
		private void CloseConfigurations()
		{
			(_configurationHud.Confirmation.style.display, _configurationHud.Settings.style.display) = (_configurationHud.RootElement.style.display = DisplayStyle.None, DisplayStyle.Flex);
			StateController.SetState(true);
		}
		private void OutLevel() => (_configurationHud.Settings.style.display, _configurationHud.Confirmation.style.display) = (DisplayStyle.None, DisplayStyle.Flex);
		private void SaveGame() => SaveController.SaveData();
		private void ScreenResolution(ChangeEvent<string> resolution)
		{
			SettingsController.Load(out Settings settings);
			ReadOnlySpan<string> dimensions = resolution.newValue.Split(new char[] { 'x', ' ' }, StringSplitOptions.RemoveEmptyEntries);
			settings.ScreenResolution.Set(ushort.Parse(dimensions[0]), ushort.Parse(dimensions[1]));
			Screen.SetResolution(settings.ScreenResolution.x, settings.ScreenResolution.y, settings.FullScreenMode);
			SettingsController.WriteSave(settings);
		}
		private void FullScreenModes(ChangeEvent<string> screenMode)
		{
			SettingsController.Load(out Settings settings);
			Screen.fullScreenMode = settings.FullScreenMode = Enum.Parse<FullScreenMode>(screenMode.newValue);
			SettingsController.WriteSave(settings);
		}
		private void DialogToggle(ChangeEvent<bool> toggle)
		{
			SettingsController.Load(out Settings settings);
			settings.DialogToggle = toggle.newValue;
			SettingsController.WriteSave(settings);
		}
		private void GeneralVolumeToggle(ChangeEvent<bool> toggle)
		{
			SettingsController.Load(out Settings settings);
			settings.GeneralVolumeToggle = toggle.newValue;
			SettingsController.WriteSave(settings);
		}
		private void EffectsVolumeToggle(ChangeEvent<bool> toggle)
		{
			SettingsController.Load(out Settings settings);
			settings.EffectsVolumeToggle = toggle.newValue;
			SettingsController.WriteSave(settings);
		}
		private void MusicVolumeToggle(ChangeEvent<bool> toggle)
		{
			SettingsController.Load(out Settings settings);
			settings.MusicVolumeToggle = toggle.newValue;
			SettingsController.WriteSave(settings);
		}
		private void InfinityFPS(ChangeEvent<bool> toggle)
		{
			SettingsController.Load(out Settings settings);
			settings.InfinityFPS = toggle.newValue;
			Application.targetFrameRate = settings.InfinityFPS ? -1 : settings.FrameRate;
			SettingsController.WriteSave(settings);
		}
		private void DialogSpeed(ChangeEvent<float> value)
		{
			SettingsController.Load(out Settings settings);
			settings.DialogSpeed = value.newValue;
			SettingsController.WriteSave(settings);
		}
		private void ScreenBrightness(ChangeEvent<float> brightness)
		{
			SettingsController.Load(out Settings settings);
			Screen.brightness = settings.ScreenBrightness = brightness.newValue;
			SettingsController.WriteSave(settings);
		}
		private void GeneralVolume(ChangeEvent<float> volume)
		{
			SettingsController.Load(out Settings settings);
			_mixer.SetFloat(nameof(GeneralVolume), Mathf.Log10(settings.GeneralVolume = volume.newValue) * 20F);
			SettingsController.WriteSave(settings);
		}
		private void EffectsVolume(ChangeEvent<float> volume)
		{
			SettingsController.Load(out Settings settings);
			_mixer.SetFloat(nameof(EffectsVolume), Mathf.Log10(settings.EffectsVolume = volume.newValue) * 20F);
			SettingsController.WriteSave(settings);
		}
		private void MusicVolume(ChangeEvent<float> volume)
		{
			SettingsController.Load(out Settings settings);
			_mixer.SetFloat(nameof(MusicVolume), Mathf.Log10(settings.MusicVolume = volume.newValue) * 20F);
			SettingsController.WriteSave(settings);
		}
		private void FrameRate(ChangeEvent<int> frameRate)
		{
			SettingsController.Load(out Settings settings);
			settings.FrameRate = (ushort)frameRate.newValue;
			if (!settings.InfinityFPS)
				Application.targetFrameRate = settings.FrameRate;
			_configurationHud.FrameRateText.text = frameRate.newValue.ToString();
			SettingsController.WriteSave(settings);
		}
		private void VSync(ChangeEvent<int> vsync)
		{
			SettingsController.Load(out Settings settings);
			QualitySettings.vSyncCount = settings.VSync = (ushort)vsync.newValue;
			SettingsController.WriteSave(settings);
		}
		private void YesBackLevel()
		{
			CloseConfigurations();
			_isActive = false;
			if (SceneManager.GetActiveScene().name != _levelSelectorScene)
				GetComponent<Transitioner>().Transicion(_levelSelectorScene);
			else
				GetComponent<Transitioner>().Transicion(_menuScene);
		}
		private void NoBackLevel() => (_configurationHud.Settings.style.display, _configurationHud.Confirmation.style.display) = (DisplayStyle.Flex, DisplayStyle.None);
		internal void OpenCloseConfigurations()
		{
			if (!_isActive)
				return;
			if (DisplayStyle.Flex == _configurationHud.RootElement.style.display)
				CloseConfigurations();
			else
			{
				StateController.SetState(false);
				if (SceneManager.GetActiveScene().name == _menuScene)
					_configurationHud.SaveGame.style.display = _configurationHud.OutLevel.style.display = DisplayStyle.None;
				else if (SceneManager.GetActiveScene().name == _levelSelectorScene)
					_configurationHud.SaveGame.style.display = _configurationHud.OutLevel.style.display = DisplayStyle.Flex;
				else
					(_configurationHud.OutLevel.style.display, _configurationHud.SaveGame.style.display) = (DisplayStyle.Flex, DisplayStyle.None);
				_configurationHud.RootElement.style.display = DisplayStyle.Flex;
			}
		}
		internal void SetActive(bool isActive) => _isActive = isActive;
		public void Receive(MessageData message)
		{
			if (MessageFormat.State == message.Format && message.ToggleValue.HasValue)
				_isActive = message.ToggleValue.Value;
		}
	};
};
