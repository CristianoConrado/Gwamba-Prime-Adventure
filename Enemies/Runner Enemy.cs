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
			_offEdge = false,
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
		private void Update()
		{
			if ( IsStunned || SceneInitiator.IsInTransition() )
				return;
			if ( _statistics.DetectionStop && _stopRunning )
				if ( 0F >= ( _stoppedTime -= Time.deltaTime ) )
				{
					_retreatTime = _statistics.TimeToRetreat;
					_dashedTime = _statistics.TimeDashing;
					_isDashing = !( _stopWorking = _stopRunning = false );
					_sender.SetFormat( MessageFormat.State );
					_sender.SetToggle( _statistics.JumpDash );
					_sender.Send( MessagePath.Enemy );
					InvencibleDash();
				}
			if ( _stopWorking )
				return;
			if ( _statistics.TimedDash && !_isDashing )
				if ( 0F >= ( _dashTime -= Time.deltaTime ) )
				{
					_dashedTime = _statistics.TimeDashing;
					_isDashing = true;
					InvencibleDash();
				}
			if ( _statistics.RunFromTarget )
			{
				if ( 0F < _timeRun && !_isDashing )
				{
					_isDashing = true;
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
					_isDashing = false;
					InvencibleDash();
				}
			}
			if ( !_retreat && 0F < _retreatTime )
				if ( 0F >= ( _retreatTime -= Time.deltaTime ) )
					_canRetreat = true;
			if ( _isDashing )
				if ( 0F >= ( _dashedTime -= Time.deltaTime ) )
				{
					_dashTime = _statistics.TimeToDash;
					_sender.SetFormat( MessageFormat.State );
					_sender.SetToggle( !( _detected = _isDashing = false ) );
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
			if ( _stopWorking )
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
			void RetreatUse()
			{
				_invencibility = _retreat = false;
				if ( _statistics.DetectionStop )
				{
					_stoppedTime = _statistics.StopTime;
					_stopWorking = _stopRunning = true;
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
					_retreatTime = _statistics.TimeToRetreat;
					_dashedTime = _statistics.TimeDashing;
					_isDashing = !( _stopWorking = _stopRunning = false );
					_sender.SetFormat( MessageFormat.State );
					_sender.SetToggle( _statistics.JumpDash );
					_sender.Send( MessagePath.Enemy );
					InvencibleDash();
				}
			}
			_originCast = Rigidbody.position + _collider.offset;
			_originCast.x += _collider.bounds.extents.x * ( _retreat ? -1F : 1F ) * _movementSide * transform.right.x;
			_originCast.y -= _collider.bounds.extents.y * transform.up.y;
			_offEdge = !Physics2D.Raycast( _originCast, -transform.up, WorldBuild.SNAP_LENGTH, WorldBuild.SCENE_LAYER_MASK );
			if ( OnGround && !_statistics.TurnOffEdge && _offEdge || _wayBlocked && Mathf.Abs( Rigidbody.linearVelocityX ) <= MINIMUM_VELOCITY )
				if ( _retreat )
					RetreatUse();
				else
					_movementSide *= -1;
			if ( _retreat )
			{
				Rigidbody.linearVelocityX = ( transform.right * _movementSide ).x * -_statistics.RetreatSpeed;
				if ( Mathf.Abs( transform.position.x - _retreatLocation ) >= _statistics.RetreatDistance )
					RetreatUse();
				return;
			}
			if ( _statistics.DetectionStop && _detected && !_isDashing )
			{
				_stoppedTime = _statistics.StopTime;
				_stopWorking = _stopRunning = true;
				_sender.SetFormat( MessageFormat.State );
				_sender.SetToggle( false );
				_sender.Send( MessagePath.Enemy );
				return;
			}
			else if ( _detected && !_isDashing )
			{
				_dashedTime = _statistics.TimeDashing;
				_isDashing = !( _stopWorking = _stopRunning = false );
				_sender.SetFormat( MessageFormat.State );
				_sender.SetToggle( _statistics.JumpDash );
				_sender.Send( MessagePath.Enemy );
				InvencibleDash();
			}
			transform.TurnScaleX( _movementSide );
			Rigidbody.linearVelocityX = ( transform.right * _movementSide ).x * ( _isDashing ? _statistics.DashSpeed : _statistics.MovementSpeed );
		}
		private new void OnCollisionStay2D( Collision2D collision )
		{
			base.OnCollisionStay2D( collision );
			if ( SceneInitiator.IsInTransition() || WorldBuild.SCENE_LAYER != collision.gameObject.layer )
				return;
			_collider.GetContacts( _groundContacts );
			//_originCast = (Vector2) transform.position + _collider.offset;
			//_originCast.x += _collider.bounds.extents.x * ( transform.localScale.x.CompareTo( 0F ) ) * transform.right.x;
			//_sizeCast.Set( WorldBuild.SNAP_LENGTH, _collider.bounds.size.y );
			_wayBlocked = _groundContacts.Exists( contact =>
			0F > contact.normal.x
			? -_statistics.CheckGroundLimit >= contact.normal.x
			: _statistics.CheckGroundLimit <= contact.normal.x );
		}
		public new bool Hurt( ushort damage )
		{
			if ( _invencibility )
				return false;
			if ( _statistics.ReactToDamage && _canRetreat )
			{
				_stoppedTime = 0F;
				_stopWorking = _stopRunning = _canRetreat = !( _invencibility = _retreat = true );
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
							_stoppedTime = 0F;
							_stopWorking = _stopRunning = _canRetreat = !( _invencibility = _retreat = true );
							_retreatLocation = transform.position.x;
							transform.TurnScaleX( _movementSide = (short) ( CharacterExporter.GwambaLocalization().x < transform.position.x ? -1 : 1 ) );
							_sender.SetFormat( MessageFormat.State );
							_sender.SetToggle( false );
							_sender.Send( MessagePath.Enemy );
						}
						return;
					}
		}
	};
};
