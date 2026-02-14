using Cysharp.Threading.Tasks;
using GwambaPrimeAdventure.Character;
using GwambaPrimeAdventure.Connection;
using NaughtyAttributes;
using System.Threading;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;
namespace GwambaPrimeAdventure.Item.EventItem
{
	[DisallowMultipleComponent, RequireComponent( typeof( Tilemap ), typeof( TilemapRenderer ), typeof( TilemapCollider2D ) )]
	[RequireComponent( typeof( CompositeCollider2D ), typeof( Light2DBase ), typeof( Receptor ) )]
	internal sealed class HiddenPlace : StateController, ILoader, ISignalReceptor
	{
		private
			Tilemap _tilemap;
		private
			TilemapRenderer _tilemapRenderer;
		private
			TilemapCollider2D _tilemapCollider;
		private Light2DBase
			_selfLight,
			_followLight;
		private readonly Sender
			_sender = Sender.Create();
		private
			CancellationToken _destroyToken;
		private bool
			_activation = false,
			_follow = false,
			_taskOnGoing = false,
			_cancelTask = false;
		[SerializeField, Tooltip( "Other hidden place to activate." ), Header( "Hidden Place" )]
		private
			HiddenPlace _otherPlace;
		[SerializeField, Tooltip( "The occlusion object to reveal/hide." )]
		private
			OcclusionObject _occlusionObject;
		[SerializeField, Tooltip( "If this object will receive a signal." )]
		private
			bool _isReceptor;
		[SerializeField, ShowIf( nameof( _isReceptor ) ), Tooltip( "The amount o time to appear/fade again after the activation." )]
		private
			float _timeToFadeAppearAgain;
		[SerializeField, ShowIf( nameof( _isReceptor ) ), Tooltip( "If the activation of the receive signal will fade the place." )]
		private
			bool _fadeActivation;
		[SerializeField, ShowIf( nameof( _isReceptor ) ), Tooltip( "If this place won't use his own collider." )]
		private
			bool _useOtherCollider;
		[SerializeField, Tooltip( "If the other hidden place will appear first." )]
		private
			bool _appearFirst;
		[SerializeField, Tooltip( "If the other hidden place will fade first." )]
		private
			bool _fadeFirst;
		[SerializeField, Tooltip( "If this object will appear/fade instantly." )]
		private
			bool _instantly;
		[SerializeField, Tooltip( "If the place has any inferior collider." )]
		private
			bool _haveColliders;
		[SerializeField, Tooltip( "If theres a follow light." )]
		private
			bool _hasFollowLight;
		private new void Awake()
		{
			base.Awake();
			_tilemap = GetComponent<Tilemap>();
			_tilemapRenderer = GetComponent<TilemapRenderer>();
			_tilemapCollider = GetComponent<TilemapCollider2D>();
			_selfLight = GetComponent<Light2DBase>();
			_followLight = GetComponentInChildren<Light2DBase>();
			_sender.SetFormat( MessageFormat.State );
			_sender.SetAdditionalData( _occlusionObject );
		}
		private new void OnDestroy()
		{
			base.OnDestroy();
			EffectsController.OffGlobalLight( _selfLight );
		}
		public async UniTask Load()
		{
			_destroyToken = this.GetCancellationTokenOnDestroy();
			await UniTask.Yield( PlayerLoopTiming.EarlyUpdate, _destroyToken, true ).SuppressCancellationThrow();
			if ( _destroyToken.IsCancellationRequested )
				return;
			_activation = !_fadeActivation;
			if ( _isReceptor )
			{
				_tilemapRenderer.enabled = _fadeActivation;
				_tilemapCollider.enabled = _fadeActivation && !_useOtherCollider;
			}
		}
		private void FixedUpdate()
		{
			if ( _follow )
				_followLight.transform.position = CharacterExporter.GwambaLocalization();
		}
		private async UniTask Fade( bool appear )
		{
			if ( _taskOnGoing )
			{
				_cancelTask = true;
				await UniTask.WaitWhile( () => _taskOnGoing, PlayerLoopTiming.Update, _destroyToken, true ).SuppressCancellationThrow();
				if ( _destroyToken.IsCancellationRequested )
					return;
				_cancelTask = false;
			}
			bool onFirst = false;
			_taskOnGoing = true;
			if ( _otherPlace )
				if ( onFirst = _otherPlace._appearFirst && _otherPlace._activation )
					await _otherPlace.Fade( true ).AttachExternalCancellation( _destroyToken ).SuppressCancellationThrow();
				else if ( onFirst = _otherPlace._fadeFirst && !_otherPlace._activation )
					await _otherPlace.Fade( false ).AttachExternalCancellation( _destroyToken ).SuppressCancellationThrow();
			if ( _destroyToken.IsCancellationRequested || _cancelTask )
			{
				_taskOnGoing = false;
				return;
			}
			if ( _isReceptor )
				_activation = !_activation;
			if ( appear )
				EffectsController.OffGlobalLight( _selfLight );
			else
				EffectsController.OnGlobalLight( _selfLight );
			if ( _hasFollowLight )
				_follow = !appear;
			void Occlusion()
			{
				if ( _occlusionObject )
				{
					_sender.SetToggle( appear );
					_sender.Send( MessagePath.System );
				}
			}
			async UniTask OpacityLevel( float alpha )
			{
				await UniTask.WaitUntil( () => isActiveAndEnabled, PlayerLoopTiming.Update, _destroyToken, true ).SuppressCancellationThrow();
				if ( _destroyToken.IsCancellationRequested )
					return;
				if ( _cancelTask )
				{
					_taskOnGoing = false;
					return;
				}
				Color color = _tilemap.color;
				color.a = alpha;
				_tilemap.color = color;
			}
			if ( appear )
			{
				Occlusion();
				_tilemapRenderer.enabled = true;
				if ( _instantly )
				{
					Color color = _tilemap.color;
					color.a = 1F;
					_tilemap.color = color;
				}
				else
					for ( float i = 0F; 1F > _tilemap.color.a; i += 1E-1F )
					{
						await OpacityLevel( i ).SuppressCancellationThrow();
						if ( _destroyToken.IsCancellationRequested || _cancelTask )
						{
							_taskOnGoing = false;
							return;
						}
					}
			}
			else
			{
				if ( _instantly )
				{
					Color color = _tilemap.color;
					color.a = 0F;
					_tilemap.color = color;
				}
				else
					for ( float i = 1F; 0F < _tilemap.color.a; i -= 1E-1F )
					{
						await OpacityLevel( i ).SuppressCancellationThrow();
						if ( _destroyToken.IsCancellationRequested || _cancelTask )
						{
							_taskOnGoing = false;
							return;
						}
					}
				_tilemapRenderer.enabled = false;
				Occlusion();
			}
			if ( _haveColliders )
				_tilemapCollider.enabled = appear;
			if ( _otherPlace && !onFirst )
				if ( !_otherPlace._appearFirst && _otherPlace._activation )
					_otherPlace.Fade( true ).Forget();
				else if ( !_otherPlace._fadeFirst && !_otherPlace._activation )
					_otherPlace.Fade( false ).Forget();
			_taskOnGoing = false;
		}
		private void OnTriggerEnter2D( Collider2D other )
		{
			if ( !_isReceptor && CharacterExporter.EqualGwamba( other.gameObject ) )
				Fade( false ).Forget();
		}
		private void OnTriggerExit2D( Collider2D other )
		{
			if ( !_isReceptor && CharacterExporter.EqualGwamba( other.gameObject ) )
				Fade( true ).Forget();
		}
		public void Execute()
		{
			if ( 0F < _timeToFadeAppearAgain )
				FadeTimed( _activation );
			else
				Fade( _activation ).Forget();
			async void FadeTimed( bool appear )
			{
				await Fade( appear ).SuppressCancellationThrow();
				if ( _destroyToken.IsCancellationRequested )
					return;
				await UniTask.WaitForSeconds( _timeToFadeAppearAgain, true, PlayerLoopTiming.Update, _destroyToken, true ).SuppressCancellationThrow();
				if ( _destroyToken.IsCancellationRequested )
					return;
				Fade( !appear ).Forget();
			}
		}
	};
};
