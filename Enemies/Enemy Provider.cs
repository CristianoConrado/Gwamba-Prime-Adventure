using UnityEngine;
namespace GwambaPrimeAdventure.Enemy
{
	[RequireComponent( typeof( EnemyController ), typeof( Collider2D ) )]
	internal abstract class EnemyProvider : StateController, IDestructible
	{
		protected
			EnemyController _controller;
		protected
			Collider2D _collider;
		protected readonly Sender
			_sender = Sender.Create();
		protected bool
			_stopWorking = false;
		[SerializeField, Tooltip( "The enemies to send messages." ), Header( "Enemy Provider" )]
		private
			EnemyProvider[] _enemiesToSend;
        [field: SerializeField, Tooltip( "The level of priority to use the destructible side." )]
        internal ushort DestructilbePriority
		{
			get;
			private set;
		}
        protected Rigidbody2D Rigidbody =>
			_controller.Rigidbody;
		public MessagePath Path =>
			MessagePath.Enemy;
		protected bool IsStunned =>
			_controller.IsStunned;
		public short Health =>
			_controller.Health;
		protected new void Awake()
		{
			base.Awake();
			_controller = GetComponent<EnemyController>();
			_collider = GetComponent<Collider2D>();
			_sender.SetAdditionalData( _enemiesToSend );
		}
		public bool Hurt( ushort damage )
		{
			if ( _controller.ProvidenceStatistics.ReactToDamage )
			{
				if ( _controller.ProvidenceStatistics.HasIndex )
					_sender.SetNumber( _controller.ProvidenceStatistics.IndexEvent );
				_sender.SetFormat( MessageFormat.Event );
				_sender.Send( MessagePath.Enemy );
			}
			if ( 0 >= ( _controller.Vitality -= (short) damage ) )
				Destroy( gameObject );
			return true;
		}
		public void Stun( ushort stunStength, float stunTime )
		{
			if ( _controller.IsStunned = !_controller.ProvidenceStatistics.NoHitStun )
			{
				_controller.OnDisable();
				_controller.StunTimer = stunTime;
			}
			if ( 0 >= ( _controller.ArmorResistance -= (short) stunStength ) )
			{
				_controller.OnDisable();
				_controller.StunTimer = _controller.ProvidenceStatistics.StunnedTime;
				_controller.ArmorResistance = (short) _controller.ProvidenceStatistics.HitResistance;
			}
		}
	};
};
