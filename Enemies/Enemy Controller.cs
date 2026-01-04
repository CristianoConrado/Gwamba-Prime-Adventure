using Cysharp.Threading.Tasks;
using GwambaPrimeAdventure.Connection;
using GwambaPrimeAdventure.Enemy.Supply;
using System.Threading;
using Unity.Cinemachine;
using UnityEngine;
namespace GwambaPrimeAdventure.Enemy
{
	[RequireComponent( typeof( Rigidbody2D ), typeof( Collider2D ), typeof( CinemachineImpulseSource ) )]
	internal sealed class EnemyController : Control, IConnector, IOccludee, IDestructible
	{
		private
			EnemyProvider[] _selfEnemies;
		[field: SerializeField, Tooltip( "The control statitics of this enemy." ), Header( "Enemy Statistics" )]
		internal EnemyStatistics ProvidenceStatistics
		{
			get;
			private set;
		}
		internal Rigidbody2D Rigidbody =>
			_rigidbody;
		public IDestructible Source => this;
		public MessagePath Path =>
			MessagePath.Enemy;
		public short Health =>
			_vitality;
		internal short Vitality
		{
			get => _vitality;
			set => _vitality = value;
		}
		internal short ArmorResistance
		{
			get => _armorResistance;
			set => _armorResistance = value;
		}
		internal float StunTimer
		{
			get => _stunTimer;
			set => _stunTimer = value;
		}
		internal bool IsStunned
		{
			get => _stunned;
			set => _stunned = value;
		}
		public bool Occlude =>
			!ProvidenceStatistics.FadeOverTime;
		private new void Awake()
		{
			base.Awake();
			_selfEnemies = GetComponents<EnemyProvider>();
			_rigidbody = GetComponent<Rigidbody2D>();
			_screenShaker = GetComponent<CinemachineImpulseSource>();
			_destructibleEnemy = _selfEnemies[ 0 ];
			for ( ushort i = 0; _selfEnemies.Length - 1 > i; i++ )
				if ( _selfEnemies[ i + 1 ].DestructilbePriority > _selfEnemies[ i ].DestructilbePriority )
					_destructibleEnemy = _selfEnemies[ i + 1 ];
			foreach ( InnerEnemy innerEnemy in GetComponentsInChildren<InnerEnemy>( true ) )
				innerEnemy.Deliver( this, OnTriggerEnter2D );
			Sender.Include( this );
		}
		private new void OnDestroy()
		{
			base.OnDestroy();
			SaveController.Load( out SaveFile saveFile );
			if ( ProvidenceStatistics.SaveOnSpecifics && !saveFile.GeneralObjects.Contains( name ) )
			{
				saveFile.GeneralObjects.Add( name );
				SaveController.WriteSave( saveFile );
			}
			Sender.Exclude( this );
		}
		internal void OnEnable()
		{
			if ( RigidbodyType2D.Static != _rigidbody.bodyType )
			{
				_rigidbody.linearVelocity = _guardedLinearVelocity;
				_rigidbody.gravityScale = ProvidenceStatistics.GravityScale;
			}
		}
		internal void OnDisable()
		{
			if ( RigidbodyType2D.Static != _rigidbody.bodyType )
			{
				_rigidbody.gravityScale = 0F;
				(_guardedLinearVelocity, _rigidbody.linearVelocity) = (_rigidbody.linearVelocity, Vector2.zero);
			}
		}
		private async void Start()
		{
			SaveController.Load( out SaveFile saveFile );
			if ( ProvidenceStatistics.SaveOnSpecifics && saveFile.GeneralObjects.Contains( name ) )
			{
				Destroy( gameObject );
				return;
			}
			foreach ( EnemyProvider enemy in _selfEnemies )
				enemy.enabled = false;
			CancellationToken destroyToken = this.GetCancellationTokenOnDestroy();
			await UniTask.WaitWhile( () => SceneInitiator.IsInTrancision(), PlayerLoopTiming.Update, destroyToken, true ).SuppressCancellationThrow();
			if ( destroyToken.IsCancellationRequested )
				return;
			_vitality = (short) ProvidenceStatistics.Vitality;
			_armorResistance = (short) ProvidenceStatistics.HitResistance;
			_fadeTime = ProvidenceStatistics.TimeToFadeAway;
			foreach ( EnemyProvider enemy in _selfEnemies )
				enemy.enabled = true;
		}
		private void Update()
		{
			if ( SceneInitiator.IsInTrancision() )
				return;
			if ( ProvidenceStatistics.FadeOverTime )
				if ( 0F >= ( _fadeTime -= Time.deltaTime ) )
					Destroy( gameObject );
			if ( _stunned )
				if ( 0F >= ( _stunTimer -= Time.deltaTime ) )
				{
					_stunned = false;
					OnEnable();
				}
		}
		private void OnTriggerEnter2D( Collider2D other )
		{
			if ( !ProvidenceStatistics.NoHit && other.TryGetComponent<IDestructible>( out var destructible ) && destructible.Hurt( ProvidenceStatistics.Damage ) )	
			{
				destructible.Stun( ProvidenceStatistics.Damage, ProvidenceStatistics.StunTime );
				_screenShaker.GenerateImpulse( ProvidenceStatistics.HurtShake );
				EffectsController.HitStop( ProvidenceStatistics.HitStopTime, ProvidenceStatistics.HitSlowTime );
			}
		}
		public bool Hurt( ushort damage ) => !ProvidenceStatistics.NoDamage && 0 < damage && _destructibleEnemy.Hurt( damage );
		public void Stun( ushort stunStength, float stunTime )
		{
			if ( ProvidenceStatistics.NoStun || _stunned )
				return;
			_destructibleEnemy.Stun( stunStength, stunTime );
		}
		public void Receive( MessageData message )
		{
			if ( MessageFormat.None == message.Format && message.ToggleValue.HasValue )
			{
				OnDisable();
				for ( ushort i = 0; _selfEnemies.Length > i; i++ )
					_selfEnemies[ i ].enabled = message.ToggleValue.Value;
			}
		}
	};
};
