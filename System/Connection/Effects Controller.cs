using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.U2D;
namespace GwambaPrimeAdventure.Connection
{
	[DisallowMultipleComponent, RequireComponent( typeof( Light2DBase ) )]
	public sealed class EffectsController : StateController
	{
		private static EffectsController _instance;
		private readonly List<Light2DBase>
			_lightsStack = new List<Light2DBase>();
		private CancellationToken _destroyToken;
		private bool
			_canHitStop = true;
		[SerializeField, Tooltip( "The source where the sounds came from." )] private AudioSource
			_sourceObject;
		private new void Awake()
		{
			base.Awake();
			if ( _instance )
			{
				Destroy( gameObject, WorldBuild.MINIMUM_TIME_SPACE_LIMIT );
				return;
			}
			_instance = this;
			_lightsStack.Add( GetComponent<Light2DBase>() );
			_destroyToken = this.GetCancellationTokenOnDestroy();
		}
		private new void OnDestroy()
		{
			base.OnDestroy();
			_lightsStack.Clear();
		}
		private void OnEnable() => AudioListener.pause = false;
		private void OnDisable() => AudioListener.pause = true;
		private async void PrvateHitStop( float stopTime, float slowTime )
		{
			if ( !_canHitStop )
				return;
			_canHitStop = false;
			Time.timeScale = slowTime;
			await UniTask.WaitForSeconds( stopTime, true, PlayerLoopTiming.Update, _destroyToken, true ).SuppressCancellationThrow();
			if ( _destroyToken.IsCancellationRequested )
				return;
			Time.timeScale = 1F;
			_canHitStop = true;
		}
		private void PrivateGlobalLight( Light2DBase globalLight, bool active )
		{
			if ( ( active && !_lightsStack.Contains( globalLight ) || !active && _lightsStack.Contains( globalLight ) ) && globalLight && _lightsStack[ 0 ] )
			{
				for ( ushort i = 0; _lightsStack.Count > i; i++ )
					if ( _lightsStack[ i ] )
						_lightsStack[ i ].enabled = false;
				if ( active )
					_lightsStack.Add( globalLight );
				else
					_lightsStack.Remove( globalLight );
				_lightsStack[ ^1 ].enabled = true;
			}
		}
		private async void PrivateSoundEffect( AudioClip clip, Vector2 originSound )
		{
			if ( !clip )
				return;
			AudioSource source = Instantiate( _sourceObject, originSound, Quaternion.identity );
			source.clip = clip;
			source.volume = 1F;
			source.Play();
			float time = clip.length;
			while ( 0F < time )
			{
				time -= Time.deltaTime;
				await UniTask.WaitUntil( () => isActiveAndEnabled, PlayerLoopTiming.Update, _destroyToken, true ).SuppressCancellationThrow();
				if ( _destroyToken.IsCancellationRequested )
					return;
			}
			Destroy( source.gameObject );
		}
		private void PrivateSurfaceSound( Vector2 originPosition ) => PrivateSoundEffect( Surface.CheckSurface( originPosition ), originPosition );
		public static void HitStop( float stopTime, float slowTime ) => _instance.PrvateHitStop( stopTime, slowTime );
		public static void OnGlobalLight( Light2DBase globalLight ) => _instance.PrivateGlobalLight( globalLight, true );
		public static void OffGlobalLight( Light2DBase globalLight ) => _instance.PrivateGlobalLight( globalLight, false );
		public static void SoundEffect( AudioClip clip, Vector2 originSound ) => _instance.PrivateSoundEffect( clip, originSound );
		public static void SurfaceSound( Vector2 originPosition ) => _instance.PrivateSurfaceSound( originPosition );
	};
};
