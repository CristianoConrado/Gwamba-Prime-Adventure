using Cysharp.Threading.Tasks;
using System.Threading;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace GwambaPrimeAdventure
{
	[DisallowMultipleComponent, RequireComponent( typeof( CinemachineCamera ), typeof( CinemachinePositionComposer ), typeof( Rigidbody2D ) )]
	[RequireComponent( typeof( BoxCollider2D ) )]
	internal sealed class CameraOccluder : StateController, IConnector
	{
		private static
			CameraOccluder _instance;
		private
			CinemachinePositionComposer _cinemachineFollow;
		private Vector2
			_posiontDamping = Vector2.zero;
		[SerializeField, Tooltip( "The scene of the menu." ), Header( "Interactions" )]
		private
			SceneField _menuScene;
		public MessagePath Path =>
			MessagePath.System;
		private new void Awake()
		{
			base.Awake();
			if ( _instance )
			{
				Destroy( gameObject, WorldBuild.MINIMUM_TIME_SPACE_LIMIT );
				return;
			}
			_instance = this;
			_cinemachineFollow = GetComponent<CinemachinePositionComposer>();
			_posiontDamping = _cinemachineFollow.Damping;
			GetComponent<BoxCollider2D>().size = BuildMathemathics.OrthographicToRealSize( GetComponent<CinemachineCamera>().Lens.OrthographicSize );
			SceneManager.sceneLoaded += SceneLoaded;
			Sender.Include( this );
		}
		private new void OnDestroy()
		{
			base.OnDestroy();
			if ( !_instance || this != _instance )
				return;
			StopAllCoroutines();
			SceneManager.sceneLoaded -= SceneLoaded;
			Sender.Exclude( this );
		}
		private void OnEnable()
		{
			if ( !_instance || this != _instance )
				return;
			_cinemachineFollow.Damping = _posiontDamping;
			_cinemachineFollow.enabled = true;
		}
		private void OnDisable()
		{
			if ( !_instance || this != _instance )
				return;
			_cinemachineFollow.enabled = false;
		}
		private async void Start()
		{
			if ( !_instance || this != _instance )
				return;
			CancellationToken destroyToken = this.GetCancellationTokenOnDestroy();
			await UniTask.WaitWhile( () => SceneInitiator.IsInTrancision(), PlayerLoopTiming.Update, destroyToken, true ).SuppressCancellationThrow();
			if ( destroyToken.IsCancellationRequested )
				return;
			DontDestroyOnLoad( gameObject );
		}
		private void SceneLoaded( Scene scene, LoadSceneMode loadMode )
		{
			if ( scene.name == _menuScene )
			{
				Destroy( gameObject );
				return;
			}
			_cinemachineFollow.Damping = Vector3.zero;
			_cinemachineFollow.enabled = true;
		}
		private void SetOtherChildren( GameObject gameObject, bool activate )
		{
			if ( !_instance || this != _instance )
				return;
			if ( gameObject.TryGetComponent<OcclusionObject>( out var occlusion ) )
				occlusion.Execution( activate );
		}
		private void OnTriggerEnter2D( Collider2D other ) => SetOtherChildren( other.gameObject, true );
		private void OnTriggerExit2D( Collider2D other ) => SetOtherChildren( other.gameObject, false );
		public void Receive( MessageData message )
		{
			if ( MessageFormat.Event == message.Format && message.ToggleValue.HasValue )
				_cinemachineFollow.Damping = message.ToggleValue.Value ? _posiontDamping : Vector3.zero;
		}
	};
};
