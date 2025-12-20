using UnityEngine;
using NaughtyAttributes;
using GwambaPrimeAdventure.Character;
namespace GwambaPrimeAdventure.Item.EventItem
{
	[DisallowMultipleComponent, RequireComponent( typeof( Transform ), typeof( Collider2D ), typeof( Receptor ) )]
	internal sealed class Teleporter : StateController, IReceptorSignal, IInteractable
	{
		private readonly Sender _sender = Sender.Create();
		private ushort _index = 0;
		private float _timer = 0F;
		private bool
			_active = false,
			_use = false,
			_returnActive = false;
		[Header( "Teleporter" )]
		[SerializeField, Tooltip( "The locations that Guwba can teleport to." )] private Vector2[] _locations;
		[SerializeField, Tooltip( "If it have to interact to teleport." )] private bool _isInteractive;
		[SerializeField, Tooltip( "If it have to receive a signal to work." )] private bool _isReceptor;
		[SerializeField, Tooltip( "If it teleports at the touch." )] private bool _onCollision;
		[SerializeField, Tooltip( "If it have to waits to teleport." )] private bool _useTimer;
		[SerializeField, ShowIf( nameof( _useTimer ) ), Tooltip( "The amount of time it have to waits to teleport." )] private float _timeToUse;
		private new void Awake()
		{
			base.Awake();
			_active = !_isReceptor;
		}
		private void Update()
		{
			if ( 0F < _timer )
				if ( 0F >= ( _timer -= Time.deltaTime ) )
					if ( _use )
					{
						_use = false;
						Teleport();
						_sender.SetFormat( MessageFormat.State );
						_sender.SetAdditionalData( gameObject );
						_sender.SetToggle( true );
						_sender.Send( MessagePath.Hud );
					}
					else
						_active = _returnActive;
		}
		private void Teleport()
		{
			_sender.SetFormat( MessageFormat.Event );
			_sender.SetAdditionalData( _locations[ _index ] );
			_sender.SetToggle( false );
			_sender.Send( MessagePath.Character );
			_index = (ushort) ( _index < _locations.Length - 1 ? _index + 1 : 0 );
		}
		private void Timer()
		{
			_sender.SetFormat( MessageFormat.State );
			_sender.SetAdditionalData( gameObject );
			_sender.SetToggle( false );
			_sender.Send( MessagePath.Hud );
			(_timer, _use) = (_timeToUse, true);
		}
		private void OnTriggerEnter2D( Collider2D other )
		{
			if ( _active && _onCollision )
				if ( _useTimer )
					Timer();
				else if ( CharacterExporter.EqualGwamba( other.gameObject ) )
					Teleport();
		}
		public void Execute()
		{
			_active = !_active;
			if ( _useTimer )
				(_timer, _returnActive) = (_timeToUse, !_active);
			else
				Teleport();
		}
		public void Interaction()
		{
			if ( _active && _isInteractive )
				if ( _useTimer )
					Timer();
				else
					Teleport();
		}
	};
};
