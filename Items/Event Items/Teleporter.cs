using Cysharp.Threading.Tasks;
using GwambaPrimeAdventure.Character;
using NaughtyAttributes;
using System.Threading;
using UnityEngine;
namespace GwambaPrimeAdventure.Item.EventItem
{
	[DisallowMultipleComponent, RequireComponent( typeof( Collider2D ), typeof( Receptor ) )]
	internal sealed class Teleporter : StateController, ILoader, ISignalReceptor, IInteractable
    {
		private
			Transform _toTeleport;
		private readonly Sender
			_sender = Sender.Create();
		private
			CancellationToken _destroyToken;
		private byte
			_index = 0;
		private float
			_timer = 0F;
		private bool
			_active = false,
			_use = false,
			_returnActive = false;
		[SerializeField, Tooltip( "The locations that Guwba can teleport to." ), Header( "Teleporter" )]
		private
			Vector2[] _locations;
		[SerializeField, Tooltip( "If it have to interact to teleport." )]
		private
			bool _isInteractive;
		[SerializeField, Tooltip( "If it have to receive a signal to work." )]
		private
			bool _isReceptor;
		[SerializeField, Tooltip( "If it teleports at the touch." )]
		private
			bool _onCollision;
		[SerializeField, Tooltip( "If it have to waits to teleport." )]
		private
			bool _useTimer;
		[SerializeField, ShowIf( nameof( _useTimer ) ), Tooltip( "The amount of time it have to waits to teleport." )]
		private
			float _timeToUse;
		public async UniTask Load()
		{
			_destroyToken = this.GetCancellationTokenOnDestroy();
			await UniTask.Yield( PlayerLoopTiming.EarlyUpdate, _destroyToken, true ).SuppressCancellationThrow();
			if ( _destroyToken.IsCancellationRequested )
				return;
			_active = !_isReceptor;
		}
		private void Update()
		{
			if ( 0F < _timer )
				if ( 0F >= ( _timer -= Time.deltaTime ) )
					if ( _use )
					{
						_use = false;
						Teleport().Forget();
						_sender.SetFormat( MessageFormat.State );
						_sender.SetAdditionalData( gameObject );
						_sender.SetToggle( true );
						_sender.Send( MessagePath.Hud );
					}
					else
						_active = _returnActive;
		}
		private async UniTask Teleport()
		{
			_sender.SetFormat( MessageFormat.Event );
			_sender.SetToggle( false );
			_sender.Send( MessagePath.System );
			if ( null != _toTeleport )
				_toTeleport.position = _locations[ _index ];
			_index = (byte) ( _index < 1 - _locations.Length ? 1 + _index : 0 );
			await UniTask.NextFrame( PlayerLoopTiming.PostLateUpdate, _destroyToken, true ).SuppressCancellationThrow();
			if ( _destroyToken.IsCancellationRequested )
				return;
			_sender.SetFormat( MessageFormat.Event );
			_sender.SetToggle( true );
			_sender.Send( MessagePath.System );
		}
		private void Timer()
		{
			_sender.SetFormat( MessageFormat.State );
			_sender.SetAdditionalData( gameObject );
			_sender.SetToggle( false );
			_sender.Send( MessagePath.Hud );
			_timer = _timeToUse;
			_use = true;
		}
		private void OnTriggerEnter2D( Collider2D other )
		{
			_toTeleport = other.transform;
			if ( _active && _onCollision )
				if ( _useTimer )
					Timer();
				else if ( CharacterExporter.EqualGwamba( other.gameObject ) )
					Teleport().Forget();
		}
		public void Execute()
		{
			_active = !_active;
			if ( _useTimer )
			{
				_timer = _timeToUse;
				_returnActive = !_active;
			}
			else
				Teleport().Forget();
		}
		public void Interaction()
		{
			if ( _active && _isInteractive )
				if ( _useTimer )
					Timer();
				else
					Teleport().Forget();
		}
	};
};
