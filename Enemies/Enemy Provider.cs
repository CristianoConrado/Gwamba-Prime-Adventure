using GwambaPrimeAdventure.Connection;
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
		protected readonly int
			Fall = Animator.StringToHash( nameof( Fall ) ),
			Stop = Animator.StringToHash( nameof( Stop ) ),
			Stunned = Animator.StringToHash( nameof( Stunned ) ),
			Death = Animator.StringToHash( nameof( Death ) );
		[SerializeField, Tooltip( "The enemies to send messages." ), Header( "Enemy Provider" )]
		private
			EnemyProvider[] _enemiesToSend;
		[field: SerializeField, Tooltip( "The level of priority to use the destructible side." )]
		internal byte DestructilbePriority
		{
			get;
			private set;
		}
		protected Animator Animator =>
			  _controller.Animator;
		protected Rigidbody2D Rigidbody =>
			  _controller.Rigidbody;
		public IDestructible Source => this;
		public MessagePath Path =>
			MessagePath.Enemy;
		public byte Health =>
			_controller.Health;
		protected new void Awake()
		{
			base.Awake();
			_controller = GetComponent<EnemyController>();
			_collider = GetComponent<Collider2D>();
			_sender.SetAdditionalData( _enemiesToSend );
		}
		private new void OnDestroy()
		{
			base.OnDestroy();
			if ( !_controller.ProvidenceStatistics.IsLevelBoss )
				return;
			SaveController.Load( out SaveFile saveFile );
			byte bossIndex = (byte) ( byte.Parse( $"{gameObject.scene.name[ ^1 ]}" ) - 1 );
			if ( !saveFile.DeafetedBosses[ bossIndex ] )
				saveFile.DeafetedBosses[ bossIndex ] = true;
		}
		public bool Hurt( byte damage )
		{
			if ( _controller.ProvidenceStatistics.ReactToDamage )
			{
				if ( _controller.ProvidenceStatistics.HasIndex )
					_sender.SetNumber( _controller.ProvidenceStatistics.IndexEvent );
				_sender.SetFormat( MessageFormat.Event );
				_sender.Send( MessagePath.Enemy );
			}
			if ( 0 >= ( _controller.Vitality = (byte) ( 0 <= _controller.Vitality - damage ? _controller.Vitality - damage : 0 ) ) )
				Animator.SetTrigger( Death );
			return true;
		}
		public void Stun( byte stunStength, float stunTime )
		{
			if ( !_controller.ProvidenceStatistics.NoHitStun )
			{
				Animator.SetBool( Stunned, true );
				_controller.OnDisable();
				_controller.StunTimer = stunTime;
			}
			if ( 0 >= ( _controller.ArmorResistance = (byte) ( 0 <= _controller.ArmorResistance - stunStength ? _controller.ArmorResistance - stunStength : 0 ) ) )
			{
				_controller.OnDisable();
				_controller.StunTimer = _controller.ProvidenceStatistics.StunnedTime;
				_controller.ArmorResistance = (byte) _controller.ProvidenceStatistics.HitResistance;
			}
		}
	};
};
