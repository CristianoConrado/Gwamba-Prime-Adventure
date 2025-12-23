using UnityEngine;
using UnityEngine.UIElements;
using Unity.Cinemachine;
using System.Threading;
using Cysharp.Threading.Tasks;
using GwambaPrimeAdventure.Character;
using GwambaPrimeAdventure.Connection;
namespace GwambaPrimeAdventure.Item
{
	[DisallowMultipleComponent, Icon( WorldBuild.PROJECT_ICON ), RequireComponent( typeof( Transform ), typeof( SpriteRenderer ), typeof( BoxCollider2D ) )]
	[RequireComponent( typeof( Transitioner ), typeof( IInteractable ) )]
	internal sealed class LevelGate : MonoBehaviour, ILoader
	{
		private LevelGateHud
			_levelGateWorld,
			_levelGateScreen;
		private CinemachineCamera _gateCamera;
		private readonly Sender _sender = Sender.Create();
		private CancellationToken _destroyToken;
		private Vector2
			_transitionSize = Vector2.zero,
			_worldSpaceSize = Vector2.zero,
			_activeSize = Vector2.zero;
		private short _defaultPriority = 0;
		private bool
			_isOnInteraction = false,
			_isOnTransicion = false;
		[Header( "Scene Status" )]
		[SerializeField, Tooltip( "The brain responsable for controlling the camera." )] private CinemachineBrain _brain;
		[SerializeField, Tooltip( "The handler of the world hud of the level gate." )] private LevelGateHud _levelGateWorldObject;
		[SerializeField, Tooltip( "The handler of the screen hud of the level gate." )] private LevelGateHud _levelGateScreenObject;
		[SerializeField, Tooltip( "The scene of the level." )] private SceneField _levelScene;
		[SerializeField, Tooltip( "The scene of the boss." )] private SceneField _bossScene;
		[SerializeField, Tooltip( "The offset that the hud will be." )] private Vector2 _offsetPosition;
		[SerializeField, Tooltip( "Where the this camera have to be in the hierarchy." )] private short _overlayPriority;
		private void Awake()
		{
			(_gateCamera, _destroyToken) = (GetComponentInChildren<CinemachineCamera>(), this.GetCancellationTokenOnDestroy());
			_sender.SetFormat( MessageFormat.Event );
			_sender.SetAdditionalData( gameObject );
		}
		private void OnDestroy()
		{
			if ( _levelGateWorld )
				Destroy( _levelGateWorld.gameObject );
			if ( _levelGateScreen )
			{
				_levelGateScreen.Level.clicked -= EnterLevel;
				_levelGateScreen.Boss.clicked -= EnterBoss;
				_levelGateScreen.Scenes.clicked -= ShowScenes;
				Destroy( _levelGateScreen.gameObject );
			}
		}
		public async UniTask Load()
		{
			CancellationToken destroyToken = this.GetCancellationTokenOnDestroy();
			await UniTask.Yield( PlayerLoopTiming.EarlyUpdate, destroyToken, true ).SuppressCancellationThrow();
			if ( destroyToken.IsCancellationRequested )
				return;
			(_levelGateWorld, _levelGateScreen) = (Instantiate( _levelGateWorldObject, transform ), Instantiate( _levelGateScreenObject, transform ));
			_levelGateScreen.RootElement.style.display = DisplayStyle.None;
			_levelGateScreen.Level.SetEnabled( true );
			_levelGateScreen.Boss.SetEnabled( true );
			_levelGateScreen.Scenes.SetEnabled( true );
			_levelGateWorld.transform.localPosition = _offsetPosition;
			_levelGateScreen.transform.localPosition = _offsetPosition;
			_transitionSize = _worldSpaceSize = _levelGateWorld.Document.worldSpaceSize;
			_activeSize = BuildWorker.OrthographicToScreenSize( _gateCamera.Lens.OrthographicSize );
			SaveController.Load( out SaveFile saveFile );
			_levelGateScreen.Level.clicked += EnterLevel;
			if ( saveFile.LevelsCompleted[ ushort.Parse( $"{_levelScene.SceneName[ ^1 ]}" ) - 1 ] )
				_levelGateScreen.Boss.clicked += EnterBoss;
			if ( saveFile.DeafetedBosses[ ushort.Parse( $"{_levelScene.SceneName[ ^1 ]}" ) - 1 ] )
				_levelGateScreen.Scenes.clicked += ShowScenes;
			_defaultPriority = (short) _gateCamera.Priority.Value;
		}
		private void EnterLevel() => GetComponent<Transitioner>().Transicion( _levelScene );
		private void EnterBoss() => GetComponent<Transitioner>().Transicion( _bossScene );
		private void ShowScenes() => _sender.Send( MessagePath.Story );
		private async UniTask OnHud()
		{
			_isOnInteraction = true;
			await UniTask.WaitWhile( () => _isOnTransicion, PlayerLoopTiming.Update, _destroyToken ).SuppressCancellationThrow();
			if ( _destroyToken.IsCancellationRequested )
				return;
			_gateCamera.Priority.Value = _overlayPriority;
			_isOnTransicion = true;
			float time;
			float elapsedTime = 0F;
			while ( _isOnInteraction && _levelGateWorld.Document.worldSpaceSize != _activeSize )
			{
				time = elapsedTime / _brain.DefaultBlend.Time;
				_levelGateWorld.Document.worldSpaceSize = Vector2.Lerp( _transitionSize, _activeSize, time );
				elapsedTime = elapsedTime >= _brain.DefaultBlend.Time ? _brain.DefaultBlend.Time : elapsedTime + Time.deltaTime;
				await UniTask.WaitUntil( () => isActiveAndEnabled, PlayerLoopTiming.Update, _destroyToken ).SuppressCancellationThrow();
				if ( _destroyToken.IsCancellationRequested )
					return;
			}
			_transitionSize = _levelGateWorld.Document.worldSpaceSize;
			_isOnTransicion = false;
			if ( !_isOnInteraction )
				return;
			_levelGateWorld.RootElement.style.display = DisplayStyle.None;
			_levelGateScreen.RootElement.style.display = DisplayStyle.Flex;
		}
		private async UniTask OffHud()
		{
			_isOnInteraction = false;
			await UniTask.WaitWhile( () => _isOnTransicion, PlayerLoopTiming.Update, _destroyToken ).SuppressCancellationThrow();
			if ( _destroyToken.IsCancellationRequested )
				return;
			_gateCamera.Priority.Value = _defaultPriority;
			_levelGateScreen.RootElement.style.display = DisplayStyle.None;
			_levelGateWorld.RootElement.style.display = DisplayStyle.Flex;
			_isOnTransicion = true;
			float time;
			float elapsedTime = 0F;
			while ( !_isOnInteraction && _levelGateWorld.Document.worldSpaceSize != _worldSpaceSize )
			{
				time = elapsedTime / _brain.DefaultBlend.Time;
				_levelGateWorld.Document.worldSpaceSize = Vector2.Lerp( _transitionSize, _worldSpaceSize, time );
				elapsedTime = elapsedTime >= _brain.DefaultBlend.Time ? _brain.DefaultBlend.Time : elapsedTime + Time.deltaTime;
				await UniTask.WaitUntil( () => isActiveAndEnabled, PlayerLoopTiming.Update, _destroyToken ).SuppressCancellationThrow();
				if ( _destroyToken.IsCancellationRequested )
					return;
			}
			_transitionSize = _levelGateWorld.Document.worldSpaceSize;
			_isOnTransicion = false;
		}
		private void OnTriggerEnter2D( Collider2D other )
		{
			if ( _isOnInteraction || !CharacterExporter.EqualGwamba( other.gameObject ) )
				return;
			OnHud().Forget();
		}
		private void OnTriggerExit2D( Collider2D other )
		{
			if ( !_isOnInteraction || !CharacterExporter.EqualGwamba( other.gameObject ) )
				return;
			OffHud().Forget();
		}
	};
};
