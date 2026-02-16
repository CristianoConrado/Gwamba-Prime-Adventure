using Cysharp.Threading.Tasks;
using GwambaPrimeAdventure.Character;
using GwambaPrimeAdventure.Enemy.Supply;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
namespace GwambaPrimeAdventure.Enemy
{
	[DisallowMultipleComponent]
	internal sealed class JumperEnemy : MovingEnemy, IEnemyLoader, IJumper, IConnector
	{
		private
			InputController _inputController;
		private
			CancellationToken _destroyToken;
		private Vector2
			_targetPosition = Vector2.zero,
			_direction = Vector2.zero;
		private readonly RaycastHit2D[]
			_perceptionRaycasts = new RaycastHit2D[ (byte) WorldBuild.PIXELS_PER_UNIT ];
		private readonly int
			Jump = Animator.StringToHash( nameof( Jump ) );
		private byte
			_sequentialJumpIndex = 0;
		private
			sbyte[] _jumpCount;
		private
			float[] _timedJumpTime;
		private float
			_jumpTime = 0F,
			_stopTime = 0F,
			_otherTarget = 0F;
		private ushort
			_castSize = 0;
		private bool
			_isJumping = false,
			_onJump = false,
			_stopJump = false,
			_follow = false,
			_contunuosFollow = false,
			_useTarget = false,
			_turnFollow = false,
			_isTimeout = false,
			_waitResult = false;
		[SerializeField, Tooltip( "The jumper statitics of this enemy." ), Header( "Jumper Enemy" )]
		private
			JumperStatistics _statistics;
		private new void Awake()
		{
			base.Awake();
			if ( _statistics.UseInput )
			{
				_inputController = new InputController();
				_inputController.Commands.Jump.started += Jumping;
				_inputController.Commands.Jump.Enable();
			}
			Sender.Include( this );
		}
		private new void OnDestroy()
		{
			base.OnDestroy();
			if ( _statistics.UseInput )
			{
				_inputController.Commands.Jump.started -= Jumping;
				_inputController.Commands.Jump.Disable();
				_inputController.Dispose();
			}
			Sender.Exclude( this );
		}
		public void Load()
		{
			_destroyToken = this.GetCancellationTokenOnDestroy();
			_timedJumpTime = new float[ _statistics.TimedJumps.Length ];
			_jumpCount = new sbyte[ _statistics.JumpPointStructures.Length ];
			for ( byte i = 0; _statistics.TimedJumps.Length > i; i++ )
				_timedJumpTime[ i ] = _statistics.TimedJumps[ i ].TimeToExecute;
			for ( byte i = 0; _statistics.JumpPointStructures.Length > i; i++ )
			{
				Instantiate( _statistics.JumpPointStructures[ i ].JumpPointObject, _statistics.JumpPointStructures[ i ].Point, Quaternion.identity ).GetTouch( this, i );
				_jumpCount[ i ] = (sbyte) _statistics.JumpPointStructures[ i ].JumpCount;
			}
		}
		private void Jumping( InputAction.CallbackContext jump )
		{
			if ( isActiveAndEnabled && !IsStunned && 0F >= _jumpTime )
			{
				_jumpTime = _statistics.TimeToJump;
				_targetPosition = CharacterExporter.GwambaLocalization();
				BasicJump();
			}
		}
		private void BasicJump()
		{
			_detected = true;
			if ( _statistics.DetectionStop )
			{
				Animator.SetBool( Stop, true );
				_sender.SetFormat( MessageFormat.State );
				_sender.SetToggle( false );
				_sender.Send( MessagePath.Enemy );
				_stopTime = _statistics.StopTime;
				return;
			}
			Animator.SetBool( Jump, _isJumping = true );
			Rigidbody.AddForceY( Rigidbody.mass * _statistics.JumpStrenght, ForceMode2D.Impulse );
			if ( _follow = !_statistics.UnFollow )
				transform.TurnScaleX( _movementSide = (sbyte) ( _targetPosition.x < transform.position.x ? -1 : 1 ) );
		}
		private async void TimedJump( byte jumpIndex )
		{
			if ( 0F < _timedJumpTime[ jumpIndex ] )
				if ( 0F >= ( _timedJumpTime[ jumpIndex ] -= Time.deltaTime ) )
				{
					Rigidbody.AddForceY( _statistics.TimedJumps[ jumpIndex ].Strength * Rigidbody.mass, ForceMode2D.Impulse );
					( _isTimeout, _waitResult ) = await UniTask.WaitWhile( () => OnGround, PlayerLoopTiming.Update, _destroyToken, true ).SuppressCancellationThrow()
						.TimeoutWithoutException( TimeSpan.FromSeconds( _statistics.TimeToCancel ), DelayType.DeltaTime, PlayerLoopTiming.Update );
					if ( _destroyToken.IsCancellationRequested || _isTimeout && !_waitResult )
						return;
					Animator.SetBool( Jump, _isJumping = true );
					_contunuosFollow = _follow = _statistics.TimedJumps[ jumpIndex ].Follow;
					_turnFollow = _statistics.TimedJumps[ jumpIndex ].TurnFollow;
					_useTarget = _statistics.TimedJumps[ jumpIndex ].UseTarget;
					_otherTarget = _statistics.TimedJumps[ jumpIndex ].OtherTarget;
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
			if ( IsStunned || Animator.GetBool( Stop ) || SceneInitiator.IsInTransition() )
				return;
			if ( 0F < _stopTime )
				if ( 0F >= ( _stopTime -= Time.deltaTime ) )
				{
					Animator.SetBool( Stop, false );
					Animator.SetBool( Jump, _isJumping = true );
					Rigidbody.AddForceY( Rigidbody.mass * _statistics.JumpStrenght, ForceMode2D.Impulse );
					if ( _follow = !_statistics.UnFollow )
						transform.TurnScaleX( _movementSide = (sbyte) ( _targetPosition.x < transform.position.x ? -1 : 1 ) );
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
					for ( byte i = 0; _timedJumpTime.Length > i; i++ )
						TimedJump( i );
		}
		private new void FixedUpdate()
		{
			base.FixedUpdate();
			if ( IsStunned || SceneInitiator.IsInTransition() )
				return;
			if ( _isJumping )
				if ( !Animator.GetBool( Jump ) && 0F < Rigidbody.linearVelocityY )
					Animator.SetBool( Jump, true );
				else if ( Animator.GetBool( Jump ) && 0F > Rigidbody.linearVelocityY || OnGround )
					Animator.SetBool( Jump, false );
			if ( _onJump && OnGround )
			{
				if ( _follow )
					Rigidbody.linearVelocityX = 0F;
				Animator.SetBool( Jump, _onJump = _isJumping = _detected = _contunuosFollow = _follow = false );
				_sender.SetFormat( MessageFormat.State );
				_sender.SetToggle( true );
				_sender.Send( MessagePath.Enemy );
			}
			else if ( !_onJump && _isJumping && !OnGround )
				_onJump = true;
			if ( Animator.GetBool( Stop ) )
				return;
			if ( OnGround )
			{
				if ( !_detected && _statistics.LookPerception )
					if ( _statistics.CircularDetection )
					{
						if ( CharacterExporter.GwambaLocalization().InsideCircleCast( (Vector2) transform.position + _collider.offset, _statistics.LookDistance ) )
						{
							_targetPosition = CharacterExporter.GwambaLocalization();
							BasicJump();
						}
					}
					else
					{
						_originCast.Set( transform.position.x + _collider.offset.x + _collider.bounds.extents.x * _movementSide, transform.position.y + _collider.offset.y );
						_direction = Quaternion.AngleAxis( _statistics.DetectionAngle, Vector3.forward ) * transform.right * transform.localScale.x.CompareTo( 0F );
						_castSize = (ushort) Physics2D.RaycastNonAlloc( _originCast, _direction, _perceptionRaycasts, _statistics.LookDistance, WorldBuild.CHARACTER_LAYER_MASK );
						for ( int i = 0; _castSize > i; i++ )
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
						? ( 0 <= UnityEngine.Random.Range( -1, 1 ) ? CharacterExporter.GwambaLocalization().x : _otherTarget )
						: ( _useTarget ? _otherTarget : CharacterExporter.GwambaLocalization().x );
					_movementSide = (sbyte) ( _targetPosition.x < transform.position.x ? -1 : 1 );
					if ( _turnFollow )
						transform.TurnScaleX( _movementSide );
				}
				Rigidbody.linearVelocityX = Mathf.Abs( _targetPosition.x - transform.position.x ) > _statistics.DistanceToTarget ? _movementSide * _statistics.MovementSpeed : 0F;
			}
		}
		public async void OnJump( byte jumpIndex )
		{
			( _isTimeout, _waitResult ) = await UniTask.WaitWhile( () => !OnGround || _detected || !isActiveAndEnabled || IsStunned, PlayerLoopTiming.Update, _destroyToken, true )
				.SuppressCancellationThrow().TimeoutWithoutException( TimeSpan.FromSeconds( _statistics.TimeToCancel ), DelayType.DeltaTime, PlayerLoopTiming.Update );
			if ( _destroyToken.IsCancellationRequested || _isTimeout && !_waitResult )
				return;
			if ( _stopJump || 0F < _jumpTime )
				return;
			if ( 0 >= _jumpCount[ jumpIndex ]-- )
			{
				Rigidbody.AddForceY( _statistics.JumpPointStructures[ jumpIndex ].JumpStats.Strength * Rigidbody.mass, ForceMode2D.Impulse );
				( _isTimeout, _waitResult ) = await UniTask.WaitWhile( () => OnGround, PlayerLoopTiming.Update, _destroyToken, true ).SuppressCancellationThrow()
					.TimeoutWithoutException( TimeSpan.FromSeconds( _statistics.TimeToCancel ), DelayType.DeltaTime, PlayerLoopTiming.Update );
				if ( _destroyToken.IsCancellationRequested || _isTimeout && !_waitResult )
					return;
				Animator.SetBool( Jump, _isJumping = true );
				_contunuosFollow = _follow = _statistics.JumpPointStructures[ jumpIndex ].JumpStats.Follow;
				_turnFollow = _statistics.JumpPointStructures[ jumpIndex ].JumpStats.TurnFollow;
				_useTarget = _statistics.JumpPointStructures[ jumpIndex ].JumpStats.UseTarget;
				_otherTarget = _statistics.JumpPointStructures[ jumpIndex ].JumpStats.OtherTarget;
				_jumpCount[ jumpIndex ] = (sbyte) _statistics.JumpPointStructures[ jumpIndex ].JumpCount;
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
							( _isTimeout, _waitResult ) = await UniTask.WaitWhile( () => OnGround, PlayerLoopTiming.Update, _destroyToken, true ).SuppressCancellationThrow()
								.TimeoutWithoutException( TimeSpan.FromSeconds( _statistics.TimeToCancel ), DelayType.DeltaTime, PlayerLoopTiming.Update );
							if ( _destroyToken.IsCancellationRequested || _isTimeout && !_waitResult )
								return;
							Animator.SetBool( Jump, _isJumping = true );
							_otherTarget = _statistics.OtherTarget;
							_contunuosFollow = _follow = _statistics.FollowReact;
							_turnFollow = _statistics.TurnFollowReact;
							_useTarget = _statistics.UseTarget;
							if ( _statistics.StopMoveReact )
							{
								_sender.SetFormat( MessageFormat.State );
								_sender.SetToggle( false );
								_sender.Send( MessagePath.Enemy );
							}
						}
					}
		}
	};
};
