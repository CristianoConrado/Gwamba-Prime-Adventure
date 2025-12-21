using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;
using System.Collections;
namespace GwambaPrimeAdventure
{
	[DisallowMultipleComponent, RequireComponent( typeof( CinemachineCamera ), typeof( CinemachineFollow ), typeof( Rigidbody2D ) ), RequireComponent( typeof( BoxCollider2D ) )]
	internal sealed class CameraOccluder : StateController, IConnector
	{
		private static CameraOccluder _instance;
		private CinemachineFollow _cinemachineFollow;
		private Vector2 _posiontDamping = Vector2.zero;
		[Header( "Interactions" )]
		[SerializeField, Tooltip( "The scene of the menu." )] private SceneField _menuScene;
		public MessagePath Path => MessagePath.System;
		private new void Awake()
		{
			base.Awake();
			if ( _instance )
			{
				Destroy( gameObject, WorldBuild.MINIMUM_TIME_SPACE_LIMIT );
				return;
			}
			_instance = this;
			_cinemachineFollow = GetComponent<CinemachineFollow>();
			_posiontDamping = _cinemachineFollow.TrackerSettings.PositionDamping;
			GetComponent<BoxCollider2D>().size = WorldBuild.OrthographicToRealSize( GetComponent<CinemachineCamera>().Lens.OrthographicSize );
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
			(_cinemachineFollow.TrackerSettings.PositionDamping, _cinemachineFollow.enabled) = (_posiontDamping, true);
		}
		private void OnDisable()
		{
			if ( !_instance || this != _instance )
				return;
			_cinemachineFollow.enabled = false;
		}
		private IEnumerator Start()
		{
			if ( !_instance || this != _instance )
				yield break;
			yield return new WaitWhile( () => SceneInitiator.IsInTrancision() );
			DontDestroyOnLoad( gameObject );
		}
		private void SceneLoaded( Scene scene, LoadSceneMode loadMode )
		{
			if ( scene.name == _menuScene )
			{
				Destroy( gameObject );
				return;
			}
			(_cinemachineFollow.TrackerSettings.PositionDamping, _cinemachineFollow.enabled) = (Vector2.zero, true);
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
				if ( !message.ToggleValue.Value )
					_cinemachineFollow.TrackerSettings.PositionDamping = Vector2.zero;
				else if ( message.ToggleValue.Value )
					_cinemachineFollow.TrackerSettings.PositionDamping = _posiontDamping;
		}
	};
};
