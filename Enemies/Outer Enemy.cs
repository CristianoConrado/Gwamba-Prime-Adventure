using UnityEngine;
using UnityEngine.Events;
namespace GwambaPrimeAdventure.Enemy
{
	[DisallowMultipleComponent, RequireComponent( typeof( Collider2D ) )]
	internal sealed class OuterEnemy : StateController, IDestructible
	{
		private IDestructible _destructibleEnemy;
		private UnityAction<Collider2D> _hitEvent;
		[SerializeField, Tooltip( "Is this enemy can hit other objects." )]
		private
			bool _isOnlyHit;
		public IDestructible Source => _destructibleEnemy;
		public short Health => (short) _destructibleEnemy?.Health;
		internal void Deliver( IDestructible destructibleEnemy, UnityAction<Collider2D> hitEvent )
		{
			_hitEvent = hitEvent;
			if ( _isOnlyHit )
				return;
			_destructibleEnemy = destructibleEnemy;
		}
		public bool Hurt( ushort damage ) => (bool) _destructibleEnemy?.Hurt( damage );
		public void Stun( ushort stunStrenght, float stunTime ) => _destructibleEnemy?.Stun( stunStrenght, stunTime );
		private void OnTriggerEnter2D( Collider2D other ) => _hitEvent?.Invoke( other );
	};
};