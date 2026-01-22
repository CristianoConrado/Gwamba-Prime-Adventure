using UnityEngine;
using UnityEngine.Events;
namespace GwambaPrimeAdventure.Enemy
{
	[DisallowMultipleComponent, RequireComponent( typeof( Collider2D ) )]
	internal sealed class InnerEnemy : StateController, IDestructible
	{
		private
			UnityAction<Collider2D> _hitEvent;
		[SerializeField, Tooltip( "Is this enemy can hit other objects." )]
		private
			bool _isOnlyHit;
		public IDestructible Source
		{
			get;
			private set;
		}
		public short Health =>
			Source is not null ? Source.Health : default;
		internal void Deliver( IDestructible destructibleEnemy, UnityAction<Collider2D> hitEvent )
		{
			_hitEvent = hitEvent;
			if ( _isOnlyHit )
				return;
			Source = destructibleEnemy;
		}
		public bool Hurt( ushort damage ) => Source is not null && Source.Hurt( damage );
		public void Stun( ushort stunStrenght, float stunTime ) => Source?.Stun( stunStrenght, stunTime );
		private void OnTriggerEnter2D( Collider2D other ) => _hitEvent?.Invoke( other );
	};
};
