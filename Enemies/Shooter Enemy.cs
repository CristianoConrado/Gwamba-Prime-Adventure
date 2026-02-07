using Cysharp.Threading.Tasks;
using GwambaPrimeAdventure.Character;
using GwambaPrimeAdventure.Enemy.Supply;
using System.Threading;
using UnityEngine;
namespace GwambaPrimeAdventure.Enemy
{
	[DisallowMultipleComponent]
	internal sealed class ShooterEnemy : EnemyProvider, ILoader, IConnector, IDestructible
	{
		private Vector2
			_originCast = Vector2.zero,
			_directionCast = Vector2.zero,
			_targetDirection = Vector2.zero;
		private Quaternion
			_projectileRotation = Quaternion.identity;
		private
			InstantiateParameters _projectileParameters;
		private readonly RaycastHit2D[]
			_detectionRaycasts = new RaycastHit2D[ (uint) WorldBuild.PIXELS_PER_UNIT ];
		private readonly int
			Shoot = Animator.StringToHash( nameof( Shoot ) );
		private float
			_shootInterval = 0F,
			_timeStop = 0F;
		private int
			_castSize = 0;
		private bool
			_hasTarget = false,
			_isStopped = false;
		[SerializeField, Tooltip( "The shooter statitics of this enemy." ), Header( "Shooter Enemy" )]
		private
			ShooterStatistics _statistics;
		private new void Awake()
		{
			base.Awake();
			Sender.Include( this );
		}
		private new void OnDestroy()
		{
			base.OnDestroy();
			Sender.Exclude( this );
		}
		public async UniTask Load()
		{
			CancellationToken destroyToken = this.GetCancellationTokenOnDestroy();
			await UniTask.Yield( PlayerLoopTiming.EarlyUpdate, destroyToken, true ).SuppressCancellationThrow();
			if ( destroyToken.IsCancellationRequested )
				return;
			_projectileParameters = new InstantiateParameters()
			{
				parent = transform,
				worldSpace = false
			};
		}
		private void Unstop()
		{
			Animator.SetBool( Stop, _isStopped = false );
			_sender.SetFormat( MessageFormat.State );
			_sender.SetToggle( true );
			_sender.Send( MessagePath.Enemy );
			if ( _statistics.Paralyze )
				_controller.OnEnable();
		}
		private void Shooted()
		{
			if ( !_statistics.PureInstance )
				_projectileRotation = _statistics.CircularUse
					? Quaternion.AngleAxis( Mathf.Atan2( _targetDirection.y, _targetDirection.x ) * Mathf.Rad2Deg - 90F, Vector3.forward )
					: Quaternion.AngleAxis( _statistics.DirectionAngle * ( _statistics.TurnRay ? transform.localScale.x.CompareTo( 0F ) : 1F ), Vector3.forward );
			for ( ushort i = 0; _statistics.Projectiles.Length > i; i++ )
				if ( _statistics.PureInstance )
					Instantiate(
						original: _statistics.Projectiles[ i ],
						position: _statistics.SpawnPoint,
						rotation: _statistics.Projectiles[ i ].transform.rotation,
						parameters: _projectileParameters ).transform.SetParent( null );
				else
					Instantiate( _statistics.Projectiles[ i ], _statistics.SpawnPoint, _projectileRotation, _projectileParameters ).transform.SetParent( null );
			if ( _statistics.InvencibleShoot )
			{
				_sender.SetFormat( MessageFormat.Event );
				_sender.SetToggle( true );
				_sender.Send( MessagePath.Enemy );
			}
		}
		private void Update()
		{
			if ( _stopWorking || IsStunned || SceneInitiator.IsInTransition() )
				return;
			if ( 0F < _shootInterval && !_isStopped )
				_shootInterval -= Time.deltaTime;
			if ( 0F < _timeStop )
				if ( 0F >= ( _timeStop -= Time.deltaTime ) )
				{
					Animator.SetTrigger( Shoot );
					if ( _statistics.InvencibleShoot )
					{
						_sender.SetFormat( MessageFormat.Event );
						_sender.SetToggle( false );
						_sender.Send( MessagePath.Enemy );
					}
				}
		}
		private void FixedUpdate()
		{
			if ( SceneInitiator.IsInTransition() )
				return;
			_hasTarget = false;
			if ( 0F >= _shootInterval )
				if ( _statistics.CircularUse &&
					( _statistics.ShootInfinity ||
					( _hasTarget = CharacterExporter.GwambaLocalization().InsideCircleCast( Rigidbody.position + _collider.offset, _statistics.PerceptionDistance ) ) ) )
				{
					transform.TurnScaleX( ( CharacterExporter.GwambaLocalization().x < Rigidbody.position.x ? -1F : 1F ) * transform.right.x );
					_targetDirection = ( CharacterExporter.GwambaLocalization() - _statistics.SpawnPoint ).normalized;
					_targetDirection.x *= transform.localScale.x.CompareTo( 0F );
				}
				else
				{
					_originCast = (Vector2) transform.position + _collider.offset;
					_originCast.x += _collider.bounds.extents.x * transform.localScale.x.CompareTo( 0F );
					_directionCast = Quaternion.AngleAxis( _statistics.DirectionAngle, Vector3.forward ) * Vector2.up;
					if ( _statistics.TurnRay )
						_directionCast *= transform.localScale.x.CompareTo( 0F );
					_castSize = Physics2D.RaycastNonAlloc( _originCast, _directionCast, _detectionRaycasts, _statistics.PerceptionDistance, WorldBuild.CHARACTER_LAYER_MASK );
					for ( int i = 0; _castSize > i; i++ )
						if ( _hasTarget = _detectionRaycasts[ i ].collider.TryGetComponent<IDestructible>( out _ ) )
							break;
				}
			if ( ( _hasTarget || _statistics.ShootInfinity ) && 0F >= _shootInterval )
			{
				_shootInterval = _statistics.IntervalToShoot;
				if ( _statistics.Stop )
				{
					Animator.SetBool( Stop, _isStopped = true );
					_timeStop = _statistics.StopTime;
					_sender.SetFormat( MessageFormat.State );
					_sender.SetToggle( false );
					_sender.Send( MessagePath.Enemy );
					if ( _statistics.Paralyze )
						_controller.OnDisable();
				}
				else
				{
					Animator.SetTrigger( Shoot );
					if ( _statistics.InvencibleShoot )
					{
						_sender.SetFormat( MessageFormat.Event );
						_sender.SetToggle( false );
						_sender.Send( MessagePath.Enemy );
					}
				}
			}
		}
		public new bool Hurt( ushort damage )
		{
			if ( _statistics.ShootDamaged )
				Animator.SetTrigger( Shoot );
			return base.Hurt( damage );
		}
		public void Receive( MessageData message )
		{
			if ( message.AdditionalData is not null && message.AdditionalData is EnemyProvider[] enemies && 0 < enemies.Length )
				foreach ( EnemyProvider enemy in enemies )
					if ( enemy && this == enemy )
						if ( MessageFormat.State == message.Format && message.ToggleValue.HasValue )
							_stopWorking = message.ToggleValue.Value;
						else if ( MessageFormat.Event == message.Format && _statistics.ReactToDamage )
						{
							transform.TurnScaleX( ( CharacterExporter.GwambaLocalization().x < Rigidbody.position.x ? -1F : 1F ) * transform.right.x );
							_targetDirection = ( CharacterExporter.GwambaLocalization() - _statistics.SpawnPoint ).normalized;
							_targetDirection.x *= transform.localScale.x.CompareTo( 0F );
							Animator.SetTrigger( Shoot );
							if ( _statistics.InvencibleShoot )
							{
								_sender.SetFormat( MessageFormat.Event );
								_sender.SetToggle( false );
								_sender.Send( MessagePath.Enemy );
							}
						}
		}
	};
};
