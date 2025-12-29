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
		private
			Projectile _projectile;
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
		private float
			_shootInterval = 0F,
			_timeStop = 0F;
		private int
			_castSize = 0;
		private bool
			_hasTarget = false,
			_canShoot = false,
			_isStopped = false;
		[SerializeField, Tooltip( "The shooter statitics of this enemy." ), Header( "Shooter Enemy" )]
		private ShooterStatistics
			_statistics;
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
		private void Shoot()
		{
			if ( !_statistics.PureInstance )
				_projectileRotation = _statistics.CircularDetection
					? Quaternion.AngleAxis( ( Mathf.Atan2( _targetDirection.y, _targetDirection.x ) * Mathf.Rad2Deg ) - 90F, Vector3.forward )
					: Quaternion.AngleAxis( _statistics.RayAngleDirection * ( _statistics.TurnRay ? transform.localScale.x.CompareTo( 0F ) : 1F ), Vector3.forward );
			for ( ushort i = 0; _statistics.Projectiles.Length > i; i++ )
				if ( _statistics.PureInstance )
				{
					_projectile = Instantiate( _statistics.Projectiles[ i ], _statistics.SpawnPoint, _statistics.Projectiles[ i ].transform.rotation, _projectileParameters );
					_projectile.transform.SetParent( null );
				}
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
			if ( _stopWorking || IsStunned )
				return;
			if ( 0F < _shootInterval && !_isStopped )
				_shootInterval -= Time.deltaTime;
			if ( 0F < _timeStop )
			{
				if ( _statistics.StopTime / 2F >= ( _timeStop -= Time.deltaTime ) && _statistics.Stop && _canShoot )
				{
					_canShoot = false;
					Shoot();
				}
				if ( 0F >= _timeStop && _isStopped )
				{
					_isStopped = false;
					_sender.SetFormat( MessageFormat.State );
					_sender.SetToggle( true );
					_sender.Send( MessagePath.Enemy );
					if ( _statistics.Paralyze )
						_controller.OnEnable();
				}
			}
		}
		private void FixedUpdate()
		{
			_hasTarget = false;
			if ( 0F >= _shootInterval )
				if ( _statistics.CircularDetection )
				{
					if ( _hasTarget = CharacterExporter.GwambaLocalization().InsideCircle( (Vector2) transform.position + _collider.offset, _statistics.PerceptionDistance ) )
					{
						_targetDirection = ( CharacterExporter.GwambaLocalization() - (Vector2) transform.position ).normalized;
						transform.TurnScaleX( ( CharacterExporter.GwambaLocalization().x < transform.position.x ? -1F : 1F ) * transform.right.x );
					}
				}
				else
				{
					_originCast = (Vector2) transform.position + _collider.offset;
					_originCast.x += _collider.bounds.extents.x * transform.localScale.x.CompareTo( 0F );
					_directionCast = Quaternion.AngleAxis( _statistics.RayAngleDirection, Vector3.forward ) * Vector2.up;
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
				if ( _statistics.InvencibleShoot )
				{
					_sender.SetFormat( MessageFormat.Event );
					_sender.SetToggle( false );
					_sender.Send( MessagePath.Enemy );
				}
				if ( _statistics.Stop )
				{
					_timeStop = _statistics.StopTime;
					_sender.SetFormat( MessageFormat.State );
					_sender.SetToggle( !( _isStopped = _canShoot = true ) );
					_sender.Send( MessagePath.Enemy );
					if ( _statistics.Paralyze )
						_controller.OnDisable();
				}
				else
					Shoot();
			}
		}
		public new bool Hurt( ushort damage )
		{
			if ( _statistics.ShootDamaged )
				Shoot();
			return base.Hurt( damage );
		}
		public void Receive( MessageData message )
		{
			if ( message.AdditionalData is not null && message.AdditionalData is EnemyProvider[] enemies && 0 < enemies.Length )
				foreach ( EnemyProvider enemy in enemies )
					if ( enemy && this == enemy && MessageFormat.Event == message.Format && _statistics.ReactToDamage )
					{
						_targetDirection = ( CharacterExporter.GwambaLocalization() - (Vector2) transform.position ).normalized;
						transform.TurnScaleX( CharacterExporter.GwambaLocalization().x < transform.position.x );
						Shoot();
						return;
					}
		}
	};
};
