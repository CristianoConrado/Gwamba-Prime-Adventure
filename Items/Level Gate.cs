using UnityEngine;
using UnityEngine.UIElements;
using Unity.Cinemachine;
using System.Collections;
using GwambaPrimeAdventure.Character;
using GwambaPrimeAdventure.Connection;
namespace GwambaPrimeAdventure.Item
{
	[DisallowMultipleComponent, Icon(WorldBuild.PROJECT_ICON), RequireComponent(typeof(Transform), typeof(SpriteRenderer), typeof(BoxCollider2D))]
	[RequireComponent(typeof(Transitioner), typeof(IInteractable))]
	internal sealed class LevelGate : MonoBehaviour, ILoader
	{
		private LevelGateHud _levelGate;
		private CinemachineCamera _gateCamera;
		private readonly Sender _sender = Sender.Create();
		private Vector2 _transitionSize = Vector2.zero;
		private Vector2 _worldSpaceSize = Vector2.zero;
		private Vector2 _activeSize = Vector2.one;
		private Vector2 _defaultUISize = new(WorldBuild.UI_SCALE_WIDTH, WorldBuild.UI_SCALE_HEIGHt);
		private bool _isOnInteraction = false;
		private bool _isOnTransicion = false;
		private short _defaultPriority;
		[Header("Scene Status")]
		[SerializeField, Tooltip("The brain responsable for controlling the camera.")] private CinemachineBrain _brain;
		[SerializeField, Tooltip("The object that handles the hud of the level gate.")] private LevelGateHud _levelGateObject;
		[SerializeField, Tooltip("The panel settings of the world space.")] private PanelSettings _worldSpacePanelSettings;
		[SerializeField, Tooltip("The panel settings of the screen space's overlay.")] private PanelSettings _screenOverlayPanelSettings;
		[SerializeField, Tooltip("The scene of the level.")] private SceneField _levelScene;
		[SerializeField, Tooltip("The scene of the boss.")] private SceneField _bossScene;
		[SerializeField, Tooltip("The offset that the hud will be.")] private Vector2 _offsetPosition;
		[SerializeField, Tooltip("Where the this camera have to be in the hierarchy.")] private short _overlayPriority;
		private void Awake()
		{
			_gateCamera = GetComponentInChildren<CinemachineCamera>();
			_sender.SetFormat(MessageFormat.Event);
			_sender.SetAdditionalData(gameObject);
			_levelGate = Instantiate(_levelGateObject, transform);	
		}
		private void OnDestroy()
		{
			_levelGate.Level.clicked -= EnterLevel;
			_levelGate.Boss.clicked -= EnterBoss;
			_levelGate.Scenes.clicked -= ShowScenes;
		}
		public IEnumerator Load()
		{
			_levelGate.transform.localPosition = _offsetPosition;
			_transitionSize = _worldSpaceSize = _levelGate.Document.worldSpaceSize;
			_activeSize = WorldBuild.OrthographicToScreenSize(_gateCamera.Lens.OrthographicSize);
			SaveController.Load(out SaveFile saveFile);
			_levelGate.Level.clicked += EnterLevel;
			if (saveFile.LevelsCompleted[ushort.Parse($"{_levelScene.SceneName[^1]}") - 1])
				_levelGate.Boss.clicked += EnterBoss;
			if (saveFile.DeafetedBosses[ushort.Parse($"{_levelScene.SceneName[^1]}") - 1])
				_levelGate.Scenes.clicked += ShowScenes;
			_defaultPriority = (short)_gateCamera.Priority.Value;
			yield return null;
		}
		private void EnterLevel() => GetComponent<Transitioner>().Transicion(_levelScene);
		private void EnterBoss() => GetComponent<Transitioner>().Transicion(_bossScene);
		private void ShowScenes() => _sender.Send(MessagePath.Story);
		private IEnumerator OnHud()
		{
			_isOnInteraction = true;
			while (_isOnTransicion)
				yield return null;
			_gateCamera.Priority.Value = _overlayPriority;
			_isOnTransicion = true;
			float time;
			float elapsedTime = 0F;
			while (_isOnInteraction && _levelGate.Document.worldSpaceSize != _activeSize)
			{
				time = elapsedTime / _brain.DefaultBlend.Time;
				_levelGate.Document.worldSpaceSize = Vector2.Lerp(_transitionSize, _activeSize, time);
				elapsedTime = elapsedTime >= _brain.DefaultBlend.Time ? _brain.DefaultBlend.Time : elapsedTime + Time.deltaTime;
				yield return null;
			}
			_transitionSize = _levelGate.Document.worldSpaceSize;
			_isOnTransicion = false;
			if (!_isOnInteraction)
				yield break;
			_levelGate.Level.SetEnabled(true);
			_levelGate.Boss.SetEnabled(true);
			_levelGate.Scenes.SetEnabled(true);
			_levelGate.Document.worldSpaceSize = _defaultUISize;
			_levelGate.Document.panelSettings = _screenOverlayPanelSettings;
		}
		private IEnumerator OffHud()
		{
			_isOnInteraction = false;
			while (_isOnTransicion)
				yield return null;
			_gateCamera.Priority.Value = _defaultPriority;
			_levelGate.Level.SetEnabled(false);
			_levelGate.Boss.SetEnabled(false);
			_levelGate.Scenes.SetEnabled(false);
			_levelGate.Document.panelSettings = _worldSpacePanelSettings;
			_levelGate.Document.worldSpaceSize = _activeSize;
			_isOnTransicion = true;
			float time;
			float elapsedTime = 0F;
			while (!_isOnInteraction && _levelGate.Document.worldSpaceSize != _worldSpaceSize)
			{
				time = elapsedTime / _brain.DefaultBlend.Time;
				_levelGate.Document.worldSpaceSize = Vector2.Lerp(_transitionSize, _worldSpaceSize, time);
				elapsedTime = elapsedTime >= _brain.DefaultBlend.Time ? _brain.DefaultBlend.Time : elapsedTime + Time.deltaTime;
				yield return null;
			}
			_transitionSize = _levelGate.Document.worldSpaceSize;
			_isOnTransicion = false;
		}
		private void OnTriggerEnter2D(Collider2D other)
		{
			if (_isOnInteraction || !GwambaStateMarker.EqualObject(other.gameObject))
				return;
			StartCoroutine(OnHud());
		}
		private void OnTriggerExit2D(Collider2D other)
		{
			if (!_isOnInteraction || !GwambaStateMarker.EqualObject(other.gameObject))
				return;
			StartCoroutine(OffHud());
		}
	};
};
