using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading;
using Cysharp.Threading.Tasks;
using GwambaPrimeAdventure.Character;
using GwambaPrimeAdventure.Enemy.Supply;
namespace GwambaPrimeAdventure.Enemy
{
	[DisallowMultipleComponent]
	internal sealed class JumperEnemy : MovingEnemy, ILoader, IJumper, IConnector
	{
		private InputController _inputController;
		private readonly CancellationTokenSource
			_cancellationSource = new CancellationTokenSource(),
			_cancelTimerSource = new CancellationTokenSource();
		private Vector2
			_targetPosition = Vector2.zero,
			_direction = Vector2.zero;
		private readonly RaycastHit2D[] _perceptionRaycasts = new RaycastHit2D[ (uint) WorldBuild.PIXELS_PER_UNIT ];
		private ushort _sequentialJumpIndex = 0;
		private short[] _jumpCount;
		private float[] _timedJumpTime;
		private float
			_jumpTime = 0F,
			_stopTime = 0F,
			_otherTarget = 0F;
		private bool
			_isJumping = false,
			_onJump = false,
			_stopJump = false,
			_follow = false,
			_contunuosFollow = false,
			_useTarget = false,
			_turnFollow = false,
			_cancelTimerActivated = false;
		[Header( "Jumper Enemy" )]
		[SerializeField, Tooltip( "The jumper statitics of this enemy." )] private JumperStatistics _statistics;
		private new void Awake()
		{
			base.Awake();
			if ( _statistics.UseInput )
			{
				_inputController = new InputController();
				_inputController.Commands.Jump.started += Jump;
				_inputController.Commands.Jump.Enable();
			}
			Sender.Include( this );
		}
		private new void OnDestroy()
		{
			base.OnDestroy();
			_cancellationSource?.Cancel();
			_cancelTimerSource?.Cancel();
			_cancellationSource?.Dispose();
			_cancelTimerSource?.Dispose();
			if ( _statistics.UseInput )
			{
				_inputController.Commands.Jump.started -= Jump;
				_inputController.Commands.Jump.Disable();
				_inputController.Dispose();
			}
			Sender.Exclude( this );
		}
		private void OnEnable() => _cancelTimerSource.Cancel( !( _cancelTimerActivated = false ) );
		public async UniTask Load()
		{
			CancellationToken destroyToken = this.GetCancellationTokenOnDestroy();
			await UniTask.Yield( PlayerLoopTiming.EarlyUpdate, destroyToken ).SuppressCancellationThrow();
			if ( destroyToken.IsCancellationRequested )
				return;
			_cancellationSource.RegisterRaiseCancelOnDestroy( gameObject );
			_cancelTimerSource.RegisterRaiseCancelOnDestroy( gameObject );
			(_timedJumpTime, _jumpCount) = (new float[ _statistics.TimedJumps.Length ], new short[ _statistics.JumpPointStructures.Length ]);
			for ( ushort i = 0; _statistics.TimedJumps.Length > i; i++ )
				_timedJumpTime[ i ] = _statistics.TimedJumps[ i ].TimeToExecute;
			for ( ushort i = 0; _statistics.JumpPointStructures.Length > i; i++ )
			{
				Instantiate( _statistics.JumpPointStructures[ i ].JumpPointObject, _statistics.JumpPointStructures[ i ].Point, Quaternion.identity ).GetTouch( this, i );
				_jumpCount[ i ] = (short) _statistics.JumpPointStructures[ i ].JumpCount;
			}
		}
		private void Jump( InputAction.CallbackContext jump )
		{
			if ( isActiveAndEnabled && !IsStunned && 0F >= _jumpTime )
			{
				(_jumpTime, _targetPosition) = (_statistics.TimeToJump, CharacterExporter.GwambaLocalization());
				BasicJump();
			}
		}
		private void BasicJump()
		{
			_detected = true;
			if ( _statistics.DetectionStop )
			{
				_sender.SetFormat( MessageFormat.State );
				_sender.SetToggle( false );
				_sender.Send( MessagePath.Enemy );
				_stopTime = _statistics.StopTime;
				return;
			}
			_isJumping = true;
			Rigidbody.AddForceY( Rigidbody.mass * _statistics.JumpStrenght, ForceMode2D.Impulse );
			if ( _follow = !_statistics.UnFollow )
				transform.TurnScaleX( _movementSide = (short) ( _targetPosition.x < transform.position.x ? -1 : 1 ) );
		}
		private async void WaitToCancel()
		{
			if ( !gameObject.activeSelf && !_cancelTimerActivated )
			{
				_cancelTimerActivated = true;
				await UniTask.WaitForSeconds( _statistics.TimeToCancel, false, PlayerLoopTiming.Update, _cancelTimerSource.Token ).SuppressCancellationThrow();
				if ( _cancelTimerSource.IsCancellationRequested )
					return;
				_cancellationSource.Cancel( !( _cancelTimerActivated = false ) );
			}
		}
		private async void TimedJump( ushort jumpIndex )
		{
			if ( 0F < _timedJumpTime[ jumpIndex ] )
				if ( 0F >= ( _timedJumpTime[ jumpIndex ] -= Time.deltaTime ) )
				{
					Rigidbody.AddForceY( _statistics.TimedJumps[ jumpIndex ].Strength * Rigidbody.mass, ForceMode2D.Impulse );
					await UniTask.WaitWhile( () =>
					{
						WaitToCancel();
						return OnGround;
					}, PlayerLoopTiming.Update, _cancellationSource.Token ).SuppressCancellationThrow();
					if ( _cancellationSource.IsCancellationRequested )
						return;
					(_isJumping, _contunuosFollow, _turnFollow) = (true, _follow = _statistics.TimedJumps[ jumpIndex ].Follow, _statistics.TimedJumps[ jumpIndex ].TurnFollow);
					(_useTarget, _otherTarget) = (_statistics.TimedJumps[ jumpIndex ].UseTarget, _statistics.TimedJumps[ jumpIndex ].OtherTarget);
					if ( _statistics.SequentialTimmedJumps )
						_sequentialJumpIndex++;
					else
						_timedJumpTime[ jumpIndex ] = _statistics.TimedJumps[ jumpIndex ].TimeToExecute;
					if ( _statistics.TimedJumps[ jumpIndex ].StopMove )
					{
						_sender.SetFormat( MessageFormat.State );
						_sender.SetToggle( false );
						_sender.Send( MessagePath.Enemy );
						Rigidbody.linearVelocityX = 0F;
					}
				}
		}
		private void Update()
		{
			if ( IsStunned || _stopWorking )
				return;
			if ( 0F < _stopTime )
				if ( 0F >= ( _stopTime -= Time.deltaTime ) )
				{
					_isJumping = true;
					Rigidbody.AddForceY( Rigidbody.mass * _statistics.JumpStrenght, ForceMode2D.Impulse );
					if ( _follow = !_statistics.UnFollow )
						transform.TurnScaleX( _movementSide = (short) ( _targetPosition.x < transform.position.x ? -1 : 1 ) );
				}
			if ( !OnGround || _detected || _isJumping )
				return;
			if ( 0F < _jumpTime )
				_jumpTime -= Time.deltaTime;
			if ( !_isJumping )
				if ( _statistics.SequentialTimmedJumps )
				{
					if ( _statistics.TimedJumps.Length - 1 <= _sequentialJumpIndex )
						if ( _statistics.RepeatTimmedJumps )
							_sequentialJumpIndex = 0;
						else
							return;
					TimedJump( _sequentialJumpIndex );
				}
				else
					for ( ushort i = 0; _timedJumpTime.Length > i; i++ )
						TimedJump( i );
		}
		private void FixedUpdate()
		{
			if ( IsStunned )
				return;
			if ( _onJump && OnGround )
			{
				if ( _follow )
					Rigidbody.linearVelocityX = 0F;
				_sender.SetFormat( MessageFormat.State );
				_sender.SetToggle( !( _onJump = _isJumping = _detected = _contunuosFollow = _follow = false ) );
				_sender.Send( MessagePath.Enemy );
			}
			else if ( !_onJump && _isJumping && !OnGround )
				_onJump = true;
			if ( _stopWorking )
				return;
			if ( OnGround )
			{
				if ( !_detected && _statistics.LookPerception )
					if ( _statistics.CircularDetection )
					{
						if ( CharacterExporter.GwambaLocalization().InsideCircle( (Vector2) transform.position + _collider.offset, _statistics.LookDistance ) )
						{
							_targetPosition = CharacterExporter.GwambaLocalization();
							BasicJump();
						}
					}
					else
					{
						_originCast.Set( transform.position.x + _collider.offset.x + _collider.bounds.extents.x * _movementSide, transform.position.y + _collider.offset.y );
						_direction = Quaternion.AngleAxis( _statistics.DetectionAngle, Vector3.forward ) * transform.right * transform.localScale.x.CompareTo( 0F );
						for ( int i = Physics2D.RaycastNonAlloc( _originCast, _direction, _perceptionRaycasts, _statistics.LookDistance, WorldBuild.CHARACTER_LAYER_MASK ); 0 < i; i-- )
							if ( _perceptionRaycasts[ i ].collider.TryGetComponent<IDestructible>( out _ ) )
							{
								_targetPosition = _perceptionRaycasts[ i ].collider.transform.position;
								BasicJump();
								break;
							}
					}
			}
			else if ( _follow )
			{
				if ( _contunuosFollow )
				{
					_targetPosition.x = _statistics.RandomFollow
						? ( 0 <= Random.Range( -1, 1 ) ? CharacterExporter.GwambaLocalization().x : _otherTarget )
						: ( _useTarget ? _otherTarget : CharacterExporter.GwambaLocalization().x );
					_movementSide = (short) ( _targetPosition.x < transform.position.x ? -1 : 1 );
					if ( _turnFollow )
						transform.TurnScaleX( _movementSide );
				}
				Rigidbody.linearVelocityX = Mathf.Abs( _targetPosition.x - transform.position.x ) > _statistics.DistanceToTarget ? _movementSide * _statistics.MovementSpeed : 0F;
			}
		}
		public async void OnJump( ushort jumpIndex )
		{
			await UniTask.WaitWhile( () =>
			{
				WaitToCancel();
				return !OnGround || _detected || !isActiveAndEnabled || IsStunned;
			}, PlayerLoopTiming.Update, _cancellationSource.Token ).SuppressCancellationThrow();
			if ( _cancellationSource.IsCancellationRequested )
				return;
			if ( _stopJump || 0F < _jumpTime )
				return;
			if ( 0 >= _jumpCount[ jumpIndex ]-- )
			{
				Rigidbody.AddForceY( _statistics.JumpPointStructures[ jumpIndex ].JumpStats.Strength * Rigidbody.mass, ForceMode2D.Impulse );
				await UniTask.WaitWhile( () =>
				{
					WaitToCancel();
					return OnGround;
				}, PlayerLoopTiming.Update, _cancellationSource.Token ).SuppressCancellationThrow();
				if ( _cancellationSource.IsCancellationRequested )
					return;
				(_isJumping, _contunuosFollow) = (true, _follow = _statistics.JumpPointStructures[ jumpIndex ].JumpStats.Follow);
				(_turnFollow, _useTarget) = (_statistics.JumpPointStructures[ jumpIndex ].JumpStats.TurnFollow, _statistics.JumpPointStructures[ jumpIndex ].JumpStats.UseTarget);
				(_otherTarget, _jumpCount[ jumpIndex ]) = (_statistics.JumpPointStructures[ jumpIndex ].JumpStats.OtherTarget, (short) _statistics.JumpPointStructures[ jumpIndex ].JumpCount);
				_jumpTime = _statistics.TimeToJump;
				if ( _statistics.JumpPointStructures[ jumpIndex ].JumpStats.StopMove )
				{
					_sender.SetFormat( MessageFormat.State );
					_sender.SetToggle( false );
					_sender.Send( MessagePath.Enemy );
					Rigidbody.linearVelocityX = 0F;
				}
			}
		}
		public new async void Receive( MessageData message )
		{
			if ( message.AdditionalData is not null && message.AdditionalData is EnemyProvider[] enemies && 0 < enemies.Length )
				foreach ( EnemyProvider enemy in enemies )
					if ( enemy && this == enemy )
					{
						base.Receive( message );
						if ( MessageFormat.State == message.Format && message.ToggleValue.HasValue )
							_stopJump = !message.ToggleValue.Value;
						else if ( MessageFormat.Event == message.Format && _statistics.ReactToDamage )
						{
							Rigidbody.AddForceY( _statistics.StrenghtReact * Rigidbody.mass, ForceMode2D.Impulse );
							await UniTask.WaitWhile( () =>
							{
								WaitToCancel();
								return OnGround;
							}, PlayerLoopTiming.Update, _cancellationSource.Token ).SuppressCancellationThrow();
							if ( _cancellationSource.IsCancellationRequested )
								return;
							(_otherTarget, _contunuosFollow, _turnFollow) = (_statistics.OtherTarget, _follow = _statistics.FollowReact, _statistics.TurnFollowReact);
							(_useTarget, _isJumping) = (_statistics.UseTarget, true);
							if ( _statistics.StopMoveReact )
							{
								_sender.SetFormat( MessageFormat.State );
								_sender.SetToggle( false );
								_sender.Send( MessagePath.Enemy );
							}
						}
						return;
					}
		}
	};
};
