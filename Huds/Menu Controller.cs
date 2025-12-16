using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using GwambaPrimeAdventure.Connection;
namespace GwambaPrimeAdventure.Hud
{
	[DisallowMultipleComponent, Icon(WorldBuild.PROJECT_ICON), RequireComponent(typeof(Transform), typeof(Transitioner))]
	internal sealed class MenuController : MonoBehaviour
	{
		private static MenuController _instance;
		private MenuHud _menuHud;
		private InputController _inputController;
		private bool _isPlay = false;
		[Header("Interaction Object")]
		[SerializeField, Tooltip("The object that handles the hud of the menu.")] private MenuHud _menuHudObject;
		private void Awake()
		{
			if (_instance)
			{
				Destroy(gameObject, WorldBuild.MINIMUM_TIME_SPACE_LIMIT);
				return;
			}
			(_instance, _menuHud) = (this, Instantiate(_menuHudObject, transform));
			_menuHud.SaveName[0].value = FilesController.Select(1);
			_menuHud.SaveName[1].value = FilesController.Select(2);
			_menuHud.SaveName[2].value = FilesController.Select(3);
			_menuHud.SaveName[3].value = FilesController.Select(4);
			_menuHud.Play.clicked += Play;
			_menuHud.Configurations.clicked += OpenConfigurations;
			_menuHud.Quit.clicked += Quit;
			_menuHud.Back.clicked += Back;
			_menuHud.SaveName[0].RegisterValueChangedCallback(ChangeName1);
			_menuHud.SaveName[1].RegisterValueChangedCallback(ChangeName2);
			_menuHud.SaveName[2].RegisterValueChangedCallback(ChangeName3);
			_menuHud.SaveName[3].RegisterValueChangedCallback(ChangeName4);
			_menuHud.RenameFile[0].clicked += RenameFile1;
			_menuHud.RenameFile[1].clicked += RenameFile2;
			_menuHud.RenameFile[2].clicked += RenameFile3;
			_menuHud.RenameFile[3].clicked += RenameFile4;
			_menuHud.Load[0].clicked += SelectSaveFile1;
			_menuHud.Load[1].clicked += SelectSaveFile2;
			_menuHud.Load[2].clicked += SelectSaveFile3;
			_menuHud.Load[3].clicked += SelectSaveFile4;
			_menuHud.Delete[0].clicked += DeleteSaveFile1;
			_menuHud.Delete[1].clicked += DeleteSaveFile2;
			_menuHud.Delete[2].clicked += DeleteSaveFile3;
			_menuHud.Delete[3].clicked += DeleteSaveFile4;
			_inputController = new InputController();
			_inputController.Commands.HideHud.canceled += HideHud;
			_inputController.Commands.HideHud.Enable();
		}
		private void OnDestroy()
		{
			if (!_instance || this != _instance)
				return;
			_menuHud.Play.clicked -= Play;
			_menuHud.Configurations.clicked -= OpenConfigurations;
			_menuHud.Quit.clicked -= Quit;
			_menuHud.Back.clicked -= Back;
			_menuHud.SaveName[0].UnregisterValueChangedCallback(ChangeName1);
			_menuHud.SaveName[1].UnregisterValueChangedCallback(ChangeName2);
			_menuHud.SaveName[2].UnregisterValueChangedCallback(ChangeName3);
			_menuHud.SaveName[3].UnregisterValueChangedCallback(ChangeName4);
			_menuHud.RenameFile[0].clicked -= RenameFile1;
			_menuHud.RenameFile[1].clicked -= RenameFile2;
			_menuHud.RenameFile[2].clicked -= RenameFile3;
			_menuHud.RenameFile[3].clicked -= RenameFile4;
			_menuHud.Load[0].clicked -= SelectSaveFile1;
			_menuHud.Load[1].clicked -= SelectSaveFile2;
			_menuHud.Load[2].clicked -= SelectSaveFile3;
			_menuHud.Load[3].clicked -= SelectSaveFile4;
			_menuHud.Delete[0].clicked -= DeleteSaveFile1;
			_menuHud.Delete[1].clicked -= DeleteSaveFile2;
			_menuHud.Delete[2].clicked -= DeleteSaveFile3;
			_menuHud.Delete[3].clicked -= DeleteSaveFile4;
			_inputController.Commands.HideHud.canceled -= HideHud;
			_inputController.Commands.HideHud.Disable();
			_inputController.Dispose();
		}
		private void HideHud(InputAction.CallbackContext hideHud) => Back();
		private void Play()
		{
			(_menuHud.Buttons.style.display, _menuHud.Saves.style.display, _isPlay) = (DisplayStyle.None, DisplayStyle.Flex, true);
			ConfigurationController.SetActive(false);
		}
		private void OpenConfigurations() => ConfigurationController.OpenConfigurations();
		private void Quit() => Application.Quit();
		private void Back()
		{
			if (!_isPlay)
				return;
			(_menuHud.Saves.style.display, _menuHud.Buttons.style.display, _isPlay) = (DisplayStyle.None, DisplayStyle.Flex, false);
			ConfigurationController.SetActive(true);
		}
		private void ChangeName1(ChangeEvent<string> write) => _menuHud.RenameFile[0].enabledSelf = write.newValue != FilesController.Select(1);
		private void ChangeName2(ChangeEvent<string> write) => _menuHud.RenameFile[1].enabledSelf = write.newValue != FilesController.Select(2);
		private void ChangeName3(ChangeEvent<string> write) => _menuHud.RenameFile[2].enabledSelf = write.newValue != FilesController.Select(3);
		private void ChangeName4(ChangeEvent<string> write) => _menuHud.RenameFile[3].enabledSelf = write.newValue != FilesController.Select(4);
		private void SetFileName(ushort fileIndex)
		{
			SaveController.RenameData(fileIndex, _menuHud.SaveName[fileIndex - 1].text);
			_menuHud.RenameFile[fileIndex - 1].enabledSelf = false;
		}
		private void RenameFile1() => SetFileName(1);
		private void RenameFile2() => SetFileName(2);
		private void RenameFile3() => SetFileName(3);
		private void RenameFile4() => SetFileName(4);
		private void SetSaveFile(ushort newSaveFile)
		{
			SaveController.SetActualSaveFile(newSaveFile);
			GetComponent<Transitioner>().Transicion();
			if (!SaveController.FileExists())
				SaveController.SaveData();
			if (!SettingsController.FileExists())
			{
				SettingsController.Load(out Settings settings);
				SettingsController.WriteSave(settings);
			}
		}
		private void SelectSaveFile1() => SetSaveFile(1);
		private void SelectSaveFile2() => SetSaveFile(2);
		private void SelectSaveFile3() => SetSaveFile(3);
		private void SelectSaveFile4() => SetSaveFile(4);
		private void DeleteSaveFile1() => _menuHud.SaveName[0].value = SaveController.DeleteData(1);
		private void DeleteSaveFile2() => _menuHud.SaveName[1].value = SaveController.DeleteData(2);
		private void DeleteSaveFile3() => _menuHud.SaveName[2].value = SaveController.DeleteData(3);
		private void DeleteSaveFile4() => _menuHud.SaveName[3].value = SaveController.DeleteData(4);
	};
};
