using Cysharp.Threading.Tasks;
using GwambaPrimeAdventure.Character;
using GwambaPrimeAdventure.Enemy.Supply;
using System.Threading;
using UnityEngine;
namespace GwambaPrimeAdventure.Enemy
{
	[DisallowMultipleComponent]
	internal sealed class RunnerEnemy : MovingEnemy, ILoader, IConnector, IDestructible
	{
		private readonly RaycastHit2D[]
			_detections = new RaycastHit2D[ (uint) WorldBuild.PIXELS_PER_UNIT ];
		private readonly int
			Retreat = Animator.StringToHash( nameof( Retreat ) );
		private ushort
			_runnedTimes = 0;
		private int
			_castSize = 0;
		private float
			_timeRun = 0F,
			_dashedTime = 0F,
			_dashTime = 0F,
			_retreatTime = 0F,
			_retreatLocation = 0F;
		private bool
			_stopRunning = false,
			_wayBlocked = false,
			_invencibility = false,
			_canRetreat = true,
			_retreat = false,
			_runTowards = false;
		[SerializeField, Tooltip( "The runner statitics of this enemy." ), Header( "Runner Enemy" )]
		private
			RunnerStatistics _statistics;
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
		public async new UniTask Load()
		{
			CancellationToken destroyToken = this.GetCancellationTokenOnDestroy();
			await UniTask.Yield( PlayerLoopTiming.EarlyUpdate, destroyToken, true ).SuppressCancellationThrow();
			if ( destroyToken.IsCancellationRequested )
				return;
			_timeRun = _statistics.RunOfTime;
			_dashTime = _statistics.TimeToDash;
		}
		private void InvencibleDash()
		{
			if ( _statistics.InvencibleDash )
			{
				_sender.SetFormat( MessageFormat.Event );
				_sender.SetToggle( _isDashing );
				_sender.Send( MessagePath.Enemy );
			}
		}
		private void RetreatUse()
		{
			Animator.SetBool( Retreat, false );
			_invencibility = _retreat = false;
			if ( _statistics.DetectionStop )
			{
				Animator.SetBool( Stop, _stopRunning = true );
				_stoppedTime = _statistics.StopTime;
				Rigidbody.linearVelocityX = 0F;
			}
			else if ( _statistics.EventRetreat )
			{
				_retreatTime = _statistics.TimeToRetreat;
				_sender.SetFormat( MessageFormat.State );
				_sender.SetToggle( true );
				_sender.Send( MessagePath.Enemy );
				_sender.SetFormat( MessageFormat.Event );
				_sender.SetNumber( _statistics.EventIndex );
				_sender.Send( MessagePath.Enemy );
			}
			else
			{
				Animator.SetBool( Stop, _stopRunning = false );
				Animator.SetBool( Move, false );
				Animator.SetBool( Dash, _isDashing = true );
				_retreatTime = _statistics.TimeToRetreat;
				_dashedTime = _statistics.TimeDashing;
				_sender.SetFormat( MessageFormat.State );
				_sender.SetToggle( _statistics.JumpDash );
				_sender.Send( MessagePath.Enemy );
				InvencibleDash();
			}
		}
		private void Update()
		{
			if ( IsStunned || SceneInitiator.IsInTransition() )
				return;
			if ( _statistics.DetectionStop && _stopRunning )
				if ( 0F >= ( _stoppedTime -= Time.deltaTime ) )
				{
					Animator.SetBool( Stop, _stopRunning = false );
					Animator.SetBool( Move, false );
					Animator.SetBool( Dash, _isDashing = true );
					_retreatTime = _statistics.TimeToRetreat;
					_dashedTime = _statistics.TimeDashing;
					_sender.SetFormat( MessageFormat.State );
					_sender.SetToggle( _statistics.JumpDash );
					_sender.Send( MessagePath.Enemy );
					InvencibleDash();
				}
			if ( Animator.GetBool( Stop ) )
				return;
			if ( _statistics.TimedDash && !_isDashing )
				if ( 0F >= ( _dashTime -= Time.deltaTime ) )
				{
					Animator.SetBool( Move, false );
					Animator.SetBool( Dash, _isDashing = true );
					_dashedTime = _statistics.TimeDashing;
					InvencibleDash();
				}
			if ( _statistics.RunFromTarget )
			{
				if ( 0F < _timeRun && !_isDashing )
				{
					Animator.SetBool( Dash, _isDashing = true );
					InvencibleDash();
				}
				if ( 0F >= ( _timeRun -= Time.deltaTime ) && _isDashing )
				{
					if ( _statistics.RunTowardsAfter && _runnedTimes >= _statistics.TimesToRun )
					{
						_runnedTimes = 0;
						_runTowards = true;
					}
					else if ( _statistics.RunTowardsAfter )
						_runnedTimes++;
					Animator.SetBool( Dash, _isDashing = false );
					Animator.SetBool( Move, true );
					InvencibleDash();
				}
			}
			if ( !_retreat && 0F < _retreatTime )
				if ( 0F >= ( _retreatTime -= Time.deltaTime ) )
					_canRetreat = true;
			if ( _isDashing )
				if ( 0F >= ( _dashedTime -= Time.deltaTime ) )
				{
					Animator.SetBool( Dash, _detected = _isDashing = false );
					Animator.SetBool( Move, true );
					_dashTime = _statistics.TimeToDash;
					_sender.SetFormat( MessageFormat.State );
					_sender.SetToggle( true );
					_sender.Send( MessagePath.Enemy );
					InvencibleDash();
				}
		}
		private new void FixedUpdate()
		{
			base.FixedUpdate();
			if ( IsStunned || SceneInitiator.IsInTransition() )
				return;
			if ( _statistics.DetectionStop && _detected && !_isDashing && OnGround && !_retreat )
				Rigidbody.linearVelocityX = 0F;
			if ( Animator.GetBool( Stop ) )
				return;
			if ( _statistics.LookPerception && !_detected )
			{
				_originCast.Set( transform.position.x + _collider.offset.x + _collider.bounds.extents.x * _movementSide, transform.position.y + _collider.offset.y );
				_castSize = Physics2D.RaycastNonAlloc( _originCast, transform.right * _movementSide, _detections, _statistics.LookDistance, WorldBuild.CHARACTER_LAYER_MASK );
				for ( int i = 0; _castSize > i; i++ )
					if ( _detected = _detections[ i ].collider.TryGetComponent<IDestructible>( out _ ) )
						break;
			}
			if ( _statistics.RunFromTarget && 0F >= _timeRun && _detected )
			{
				_timeRun = _statistics.RunOfTime;
				if ( _runTowards )
					_runTowards = false;
				else
					_movementSide *= -1;
			}
			if ( _retreat )
			{
				Rigidbody.linearVelocityX = ( transform.right * _movementSide ).x * -_statistics.RetreatSpeed;
				if ( Mathf.Abs( transform.position.x - _retreatLocation ) >= _statistics.RetreatDistance )
					RetreatUse();
				return;
			}
			if ( _detected && !_isDashing )
				if ( _statistics.DetectionStop )
				{
					Animator.SetBool( Move, false );
					Animator.SetBool( Dash, false );
					Animator.SetBool( Stop, _stopRunning = true );
					_stoppedTime = _statistics.StopTime;
					_sender.SetFormat( MessageFormat.State );
					_sender.SetToggle( false );
					_sender.Send( MessagePath.Enemy );
					return;
				}
				else
				{
					Animator.SetBool( Stop, _stopRunning = false );
					Animator.SetBool( Move, false );
					Animator.SetBool( Dash, _isDashing = true );
					_dashedTime = _statistics.TimeDashing;
					_sender.SetFormat( MessageFormat.State );
					_sender.SetToggle( _statistics.JumpDash );
					_sender.Send( MessagePath.Enemy );
					InvencibleDash();
				}
			transform.TurnScaleX( _movementSide );
			Rigidbody.linearVelocityX = transform.right.x * _movementSide * ( _isDashing ? _statistics.DashSpeed : _statistics.MovementSpeed );
			if ( !Animator.GetBool( Move ) && !_isDashing )
				Animator.SetBool( Move, true );
			else if ( Animator.GetBool( Move ) && _isDashing )
				Animator.SetBool( Move, false );
			if ( !Animator.GetBool( Dash ) && _isDashing )
				Animator.SetBool( Dash, true );
			else if ( Animator.GetBool( Dash ) && !_isDashing )
				Animator.SetBool( Dash, false );
		}
		private new void OnCollisionStay2D( Collision2D collision )
		{
			base.OnCollisionStay2D( collision );
			if ( SceneInitiator.IsInTransition() || WorldBuild.SCENE_LAYER != collision.gameObject.layer )
				return;
			_collider.GetContacts( _groundContacts );
			_wayBlocked = _groundContacts.Exists( contact =>
			{
				return 0F < _movementSide
				? -_statistics.CheckGroundLimit >= contact.normal.x
				: _statistics.CheckGroundLimit <= contact.normal.x;
			} );
			_originCast = Rigidbody.position + _collider.offset;
			_originCast.x += _collider.bounds.extents.x * ( _retreat ? -1F : 1F ) * _movementSide * transform.right.x;
			_originCast.y -= _collider.bounds.extents.y * transform.up.y;
			_sizeCast.Set( WorldBuild.SNAP_LENGTH * _statistics.OffEdgeSize, WorldBuild.SNAP_LENGTH );
			_groundContacts.RemoveAll( contact =>
			{
				return contact.point.OutsideBoxCast( _originCast, _sizeCast )
				&& ( contact.point - contact.relativeVelocity * Time.fixedDeltaTime ).OutsideBoxCast( _originCast, _sizeCast );
			} );
			if ( !_statistics.TurnOffEdge && OnGround && 0 >= _groundContacts.Count || _wayBlocked && Mathf.Abs( Rigidbody.linearVelocityX ) <= MINIMUM_VELOCITY )
				if ( _retreat )
					RetreatUse();
				else
					_movementSide *= -1;
		}
		public new bool Hurt( ushort damage )
		{
			if ( _invencibility )
				return false;
			if ( _statistics.ReactToDamage && _canRetreat )
			{
				Animator.SetBool( Stop, _stopRunning = _canRetreat = !( _invencibility = _retreat = true ) );
				Animator.SetBool( Move, false );
				Animator.SetBool( Retreat, true );
				_stoppedTime = 0F;
				_retreatLocation = transform.position.x;
				transform.TurnScaleX( _movementSide = (short) ( CharacterExporter.GwambaLocalization().x < transform.position.x ? -1 : 1 ) );
				_sender.SetFormat( MessageFormat.State );
				_sender.SetToggle( false );
				_sender.Send( MessagePath.Enemy );
				return false;
			}
			return base.Hurt( damage );
		}
		public new void Receive( MessageData message )
		{
			if ( message.AdditionalData is not null && message.AdditionalData is EnemyProvider[] enemies && 0 < enemies.Length )
				foreach ( EnemyProvider enemy in enemies )
					if ( enemy && this == enemy )
					{
						base.Receive( message );
						if ( MessageFormat.State == message.Format && message.ToggleValue.HasValue && !message.ToggleValue.Value )
							Rigidbody.linearVelocityX = 0F;
						if ( MessageFormat.Event == message.Format && _statistics.ReactToDamage && _canRetreat )
						{
							Animator.SetBool( Stop, _stopRunning = _canRetreat = !( _invencibility = _retreat = true ) );
							Animator.SetBool( Move, false );
							Animator.SetBool( Retreat, true );
							_stoppedTime = 0F;
							_retreatLocation = transform.position.x;
							transform.TurnScaleX( _movementSide = (short) ( CharacterExporter.GwambaLocalization().x < transform.position.x ? -1 : 1 ) );
							_sender.SetFormat( MessageFormat.State );
							_sender.SetToggle( false );
							_sender.Send( MessagePath.Enemy );
						}
					}
		}
	};
};
