using Cysharp.Threading.Tasks;
using GwambaPrimeAdventure.Connection;
using GwambaPrimeAdventure.Enemy.Supply;
using System.Threading;
using Unity.Cinemachine;
using UnityEngine;
namespace GwambaPrimeAdventure.Enemy
{
	[RequireComponent( typeof( Animator ), typeof( Rigidbody2D ), typeof( Collider2D ) ), RequireComponent( typeof( CinemachineImpulseSource ) )]
	internal sealed class EnemyController : Control, ILoader, IConnector, IOccludee, IDestructible
	{
		private
			EnemyProvider[] _selfEnemies;
		private readonly int
			IsOn = Animator.StringToHash( nameof( IsOn ) ),
			Stunned = Animator.StringToHash( nameof( Stunned ) ),
			Fade = Animator.StringToHash( nameof( Fade ) );
		[ field: SerializeField, Tooltip( "The control statitics of this enemy." ), Header( "Enemy Statistics" )]
		internal EnemyStatistics ProvidenceStatistics
		{
			get;
			private set;
		}
		internal Animator Animator =>
			_animator;
		internal Rigidbody2D Rigidbody =>
			_rigidbody;
		public IDestructible Source => this;
		public MessagePath Path =>
			MessagePath.Enemy;
		public byte Health =>
			_vitality;
		internal byte Vitality
		{
			get => _vitality;
			set => _vitality = value;
		}
		internal byte ArmorResistance
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
			_animator = GetComponent<Animator>();
			_rigidbody = GetComponent<Rigidbody2D>();
			_screenShaker = GetComponent<CinemachineImpulseSource>();
			_destructibleEnemy = _selfEnemies[ 0 ];
			for ( byte i = 1; _selfEnemies.Length > i; i++ )
				if ( _selfEnemies[ i ].DestructilbePriority > _destructiblePriority )
				{
					_destructiblePriority = _selfEnemies[ i ].DestructilbePriority;
					_destructibleEnemy = _selfEnemies[ i ];
				}
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
			_animator.SetFloat( IsOn, 1F );
			if ( RigidbodyType2D.Static != _rigidbody.bodyType )
			{
				_rigidbody.gravityScale = ProvidenceStatistics.GravityScale;
				_rigidbody.linearVelocity = _guardedLinearVelocity;
			}
		}
		internal void OnDisable()
		{
			_animator.SetFloat( IsOn, 0F );
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
			_animator.SetFloat( IsOn, 0F );
			CancellationToken destroyToken = this.GetCancellationTokenOnDestroy();
			await UniTask.WaitWhile( () => SceneInitiator.IsInTransition(), PlayerLoopTiming.Update, destroyToken, true ).SuppressCancellationThrow();
			if ( destroyToken.IsCancellationRequested )
				return;
			_vitality = (byte) ProvidenceStatistics.Vitality;
			_armorResistance = (byte) ProvidenceStatistics.HitResistance;
			_fadeTime = ProvidenceStatistics.TimeToFadeAway;
			foreach ( EnemyProvider enemy in _selfEnemies )
				enemy.enabled = true;
			_animator.SetFloat( IsOn, 1F );
		}
		public async UniTask Load()
		{
			foreach ( EnemyProvider enemy in _selfEnemies )
				if (enemy.TryGetComponent<IEnemyLoader>( out var enemyLoader ) )
					enemyLoader.Load();
			CancellationToken destroyToken = this.GetCancellationTokenOnDestroy();
			await UniTask.Yield( PlayerLoopTiming.EarlyUpdate, destroyToken, true ).SuppressCancellationThrow();
			if ( destroyToken.IsCancellationRequested )
				return;
		}
		private void DeathExecution() => Destroy( gameObject );
		private void Update()
		{
			if ( SceneInitiator.IsInTransition() )
				return;
			if ( ProvidenceStatistics.FadeOverTime )
				if ( 0F >= ( _fadeTime -= Time.deltaTime ) )
				{
					foreach ( EnemyProvider enemy in _selfEnemies )
						enemy.enabled = false;
					Animator.SetTrigger( Fade );
				}
			if ( _stunned )
				if ( 0F >= ( _stunTimer -= Time.deltaTime ) )
				{
					Animator.SetBool( Stunned, _stunned = false );
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
		public bool Hurt( byte damage ) => !ProvidenceStatistics.NoDamage && 0 < damage && _destructibleEnemy.Hurt( damage );
		public void Stun( byte stunStength, float stunTime )
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
				for ( byte i = 0; _selfEnemies.Length > i; i++ )
					_selfEnemies[ i ].enabled = message.ToggleValue.Value;
			}
		}
	};
};
