using Cysharp.Threading.Tasks;
using GwambaPrimeAdventure.Connection;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
namespace GwambaPrimeAdventure.Character
{
	[DisallowMultipleComponent, RequireComponent( typeof( Animator ), typeof( Rigidbody2D ), typeof( BoxCollider2D ) ), RequireComponent( typeof( CinemachineImpulseSource ) )]
	internal sealed class GwambaMarker : GwambaState<GwambaMarker>, ILoader, IConnector
	{
		private new void Awake()
		{
			base.Awake();
			if ( _instance )
			{
				if ( !_instance._isHubbyWorld )
				{
					_instance._beginingPosition = StartPosition;
					_instance._turnLeft = TurnToLeft;
					_instance._loadState = true;
				}
				Destroy( gameObject, WorldBuild.MINIMUM_TIME_SPACE_LIMIT );
				return;
			}
			_instance = this;
			_gwambaCanvas = GetComponentInChildren<GwambaCanvas>();
			_gwambaDamagers = GetComponentsInChildren<GwambaDamager>();
			_animator = GetComponent<Animator>();
			_screenShaker = GetComponent<CinemachineImpulseSource>();
			_rigidbody = GetComponent<Rigidbody2D>();
			_collider = GetComponent<BoxCollider2D>();
			_inputController = new InputController();
			_inputController.Commands.Movement.started += MovementInput;
			_inputController.Commands.Movement.performed += MovementInput;
			_inputController.Commands.Movement.canceled += MovementInput;
			_inputController.Commands.Jump.started += JumpInput;
			_inputController.Commands.Jump.canceled += JumpInput;
			_inputController.Commands.AttackUse.started += AttackUseInput;
			_inputController.Commands.AttackUse.canceled += AttackUseInput;
			_inputController.Commands.Interaction.started += InteractionInput;
			SceneManager.sceneLoaded += SceneLoaded;
			Sender.Include( this );
		}
		private new void OnDestroy()
		{
			base.OnDestroy();
			if ( !_instance || this != _instance )
				return;
			for ( ushort i = 0; _gwambaDamagers.Length > i; i++ )
			{
				_gwambaDamagers[ i ].DamagerHurt -= DamagerHurt;
				_gwambaDamagers[ i ].DamagerStun -= DamagerStun;
				_gwambaDamagers[ i ].DamagerAttack -= DamagerAttack;
				_gwambaDamagers[ i ].Alpha = 1F;
			}
			_inputController.Commands.Movement.started -= MovementInput;
			_inputController.Commands.Movement.performed -= MovementInput;
			_inputController.Commands.Movement.canceled -= MovementInput;
			_inputController.Commands.Jump.started -= JumpInput;
			_inputController.Commands.Jump.canceled -= JumpInput;
			_inputController.Commands.AttackUse.started -= AttackUseInput;
			_inputController.Commands.AttackUse.canceled -= AttackUseInput;
			_inputController.Commands.Interaction.started -= InteractionInput;
			_inputController.Dispose();
			SceneManager.sceneLoaded -= SceneLoaded;
			Sender.Exclude( this );
		}
		private void OnEnable()
		{
			if ( !_instance || this != _instance )
				return;
			_animator.SetFloat( IsOn, 1F );
			_animator.SetFloat( WalkSpeed, 1F );
			EnableInputs();
		}
		private void OnDisable()
		{
			if ( !_instance || this != _instance )
				return;
			_animator.SetFloat( IsOn, 0F );
			_animator.SetFloat( WalkSpeed, 0F );
			DisableInputs();
		}
		private void EnableInputs()
		{
			_inputController.Commands.Movement.Enable();
			_inputController.Commands.Jump.Enable();
			_inputController.Commands.AttackUse.Enable();
			_inputController.Commands.Interaction.Enable();
			_rigidbody.linearVelocity = _localAtLinearVelocity;
			_rigidbody.gravityScale = GravityScale;
		}
		private void DisableInputs()
		{
			_inputController.Commands.Movement.Disable();
			_inputController.Commands.Jump.Disable();
			_inputController.Commands.AttackUse.Disable();
			_inputController.Commands.Interaction.Disable();
			(_localAtLinearVelocity, _rigidbody.linearVelocity) = (_rigidbody.linearVelocity, Vector2.zero);
			_rigidbody.gravityScale = _walkValue = 0;
		}
		private async void Start()
		{
			if ( !_instance || this != _instance )
				return;
			_destroyToken = this.GetCancellationTokenOnDestroy();
			_beginingPosition = StartPosition;
			_turnLeft = TurnToLeft;
			_loadState = true;
			await StartLoad().SuppressCancellationThrow();
			if ( _destroyToken.IsCancellationRequested )
				return;
			DontDestroyOnLoad( gameObject );
			_didStart = true;
		}
		private async UniTask StartLoad()
		{
			DisableInputs();
			await UniTask.WaitUntil( () => _loadState, PlayerLoopTiming.Update, _destroyToken, true ).SuppressCancellationThrow();
			if ( _destroyToken.IsCancellationRequested )
				return;
			transform.TurnScaleX( _turnLeft );
			transform.position = _beginingPosition;
			if ( _animator.GetBool( Death ) )
				_animator.SetBool( Death, _bunnyHopUsed = _offBunnyHop = !( _deathLoad = true ) );
			await UniTask.WaitWhile( () => SceneInitiator.IsInTrancision(), PlayerLoopTiming.Update, _destroyToken, true ).SuppressCancellationThrow();
			if ( _destroyToken.IsCancellationRequested )
				return;
			if ( _deathLoad )
				OnEnable();
			else
				EnableInputs();
			_deathLoad = _loadState = false;
		}
		public async UniTask Load()
		{
			if ( !_instance || _instance != this )
				return;
			await _gwambaCanvas.LoadCanvas().AttachExternalCancellation( _destroyToken ).SuppressCancellationThrow();
			if ( _destroyToken.IsCancellationRequested )
				return;
			SaveController.Load( out SaveFile saveFile );
			_gwambaCanvas.LifeText.text = $"X {saveFile.Lifes}";
			_gwambaCanvas.CoinText.text = $"X {saveFile.Coins}";
			_vitality = (short) _gwambaCanvas.Vitality.Length;
			_stunResistance = (short) _gwambaCanvas.StunResistance.Length;
			for ( ushort i = 0; _gwambaDamagers.Length > i; i++ )
			{
				_gwambaDamagers[ i ].DamagerHurt += DamagerHurt;
				_gwambaDamagers[ i ].DamagerStun += DamagerStun;
				_gwambaDamagers[ i ].DamagerAttack += DamagerAttack;
			}
			SceneLoaded( SceneManager.GetActiveScene(), LoadSceneMode.Single );
			await UniTask.Yield( PlayerLoopTiming.Update, _destroyToken, true ).SuppressCancellationThrow();
			if ( _destroyToken.IsCancellationRequested )
				return;
		}
		private void SceneLoaded( Scene scene, LoadSceneMode loadMode )
		{
			if ( scene.name == MenuScene )
			{
				Destroy( gameObject );
				return;
			}
			if ( _isHubbyWorld = scene.name == HubbyWorldScene && !_loadState )
			{
				_beginingPosition = PointSetter.CheckedPoint;
				_turnLeft = PointSetter.TurnToLeft;
				_loadState = true;
			}
			if ( _didStart )
			{
				RestartState();
				StartLoad().Forget();
			}
		}
		private void RestartState()
		{
			for ( ushort i = 0; ( _vitality = (short) _gwambaCanvas.Vitality.Length ) > i; i++ )
			{
				_gwambaCanvas.Vitality[ i ].style.backgroundColor = _gwambaCanvas.BackgroundColor;
				_gwambaCanvas.Vitality[ i ].style.borderBottomColor = _gwambaCanvas.BorderColor;
				_gwambaCanvas.Vitality[ i ].style.borderLeftColor = _gwambaCanvas.BorderColor;
				_gwambaCanvas.Vitality[ i ].style.borderRightColor = _gwambaCanvas.BorderColor;
				_gwambaCanvas.Vitality[ i ].style.borderTopColor = _gwambaCanvas.BorderColor;
			}
			for ( ushort i = _recoverVitality = 0; _gwambaCanvas.RecoverVitality.Length > i; i++ )
				_gwambaCanvas.RecoverVitality[ i ].style.backgroundColor = _gwambaCanvas.MissingColor;
			for ( ushort i = 0; ( _stunResistance = (short) _gwambaCanvas.StunResistance.Length ) > i; i++ )
				_gwambaCanvas.StunResistance[ i ].style.backgroundColor = _gwambaCanvas.StunResistanceColor;
			for ( ushort i = _bunnyHopBoost = 0; _gwambaCanvas.BunnyHop.Length > i; i++ )
				_gwambaCanvas.BunnyHop[ i ].style.backgroundColor = _gwambaCanvas.MissingColor;
		}
		private void MovementInput( InputAction.CallbackContext movement )
		{
			if ( !isActiveAndEnabled || _animator.GetBool( Stun ) )
				return;
			if ( 0 != ( _walkValue = ( _localAtStart = movement.ReadValue<Vector2>() ).x.RangeNormalize( MovementInputZone ) ) && ( !AttackUsage || ComboAttackBuffer ) )
				if ( _localAtStart.y > AirJumpInputZone && !_isOnGround && _canAirJump && !_animator.GetBool( AirJump ) )
				{
					_animator.SetBool( AirJump, !( _canAirJump = false ) );
					_animator.SetBool( AttackAirJump, ComboAttackBuffer );
					transform.TurnScaleX( _localAtAny.z = _walkValue );
					_rigidbody.linearVelocity = Vector2.zero;
					_isJumping = false;
					_rigidbody.AddForceX( ( AirJumpStrenght + BunnyHop( JumpBoost ) ) * _localAtAny.z * _rigidbody.mass, ForceMode2D.Impulse );
					_rigidbody.AddForceY( ( AirJumpStrenght + BunnyHop( JumpBoost ) ) * _rigidbody.mass, ForceMode2D.Impulse );
					EffectsController.SoundEffect( AirJumpSound, transform.position );
					if ( ComboAttackBuffer )
						StartAttackSound();
				}
				else if ( _localAtStart.y < DashSlideInputZone && _isOnGround && !_animator.GetBool( DashSlide ) )
				{
					_animator.SetBool( DashSlide, true );
					_animator.SetBool( AttackSlide, ComboAttackBuffer );
					transform.TurnScaleX( _localAtAny.z = _walkValue );
					_localAtAny.x = transform.position.x;
					EffectsController.SoundEffect( DashSlideSound, transform.position );
					if ( ComboAttackBuffer )
						StartAttackSound();
				}
		}
		private void GroundSound( float stepPositionX )
		{
			_localAtSurface.Set( Local.x + stepPositionX, Local.y - _collider.bounds.extents.y );
			EffectsController.SurfaceSound( _localAtSurface );
		}
		private void JumpInput( InputAction.CallbackContext jump )
		{
			if ( !isActiveAndEnabled || _animator.GetBool( Stun ) )
				return;
			if ( jump.started )
			{
				_lastJumpTime = JumpBufferTime;
				if ( !_isOnGround && !_bunnyHopUsed && !_animator.GetBool( AirJump ) )
				{
					_bunnyHopUsed = true;
					_bunnyHopBoost = (ushort) ( _gwambaCanvas.BunnyHop.Length > _bunnyHopBoost + 1 ? _bunnyHopBoost + 1 : _gwambaCanvas.BunnyHop.Length );
				}
			}
			else if ( jump.canceled && _isJumping && 0F < _rigidbody.linearVelocityY )
			{
				_isJumping = false;
				_lastJumpTime = 0F;
				_rigidbody.AddForceY( _rigidbody.linearVelocityY * JumpCut * -_rigidbody.mass, ForceMode2D.Impulse );
			}
		}
		private void AttackUseInput( InputAction.CallbackContext attackUse )
		{
			if ( ( 0F < _attackDelay && !ComboAttackBuffer ) || _animator.GetBool( AirJump ) || _animator.GetBool( DashSlide ) || !isActiveAndEnabled || _animator.GetBool( Stun ) )
				return;
			if ( attackUse.started && !AttackUsage )
				_animator.SetTrigger( Attack );
			if ( attackUse.canceled && ComboAttackBuffer )
				_animator.SetTrigger( AttackCombo );
		}
		private void StartAttackSound() => EffectsController.SoundEffect( AttackSound, transform.position );
		private void InteractionInput( InputAction.CallbackContext interaction )
		{
			if ( !_isOnGround || 0 != _walkValue || !isActiveAndEnabled || _animator.GetBool( AirJump ) || _animator.GetBool( DashSlide ) || _animator.GetBool( Stun ) )
				return;
			for ( int i = Physics2D.OverlapCollider( _collider, _interactionFilter, _interactions ) - 1; 0 < i; i-- )
				if ( _interactions[ i ].TryGetComponent<IInteractable>( out _ ) )
				{
					_interactionsPerObject = _interactions[ i ].GetComponents<IInteractable>();
					for ( ushort j = 0; _interactionsPerObject.Length > j; j++ )
						_interactionsPerObject[ j ]?.Interaction();
					return;
				}
		}
		public bool DamagerHurt( ushort damage )
		{
			if ( _invencibility || 0 >= damage || _animator.GetBool( Death ) )
				return false;
			EffectsController.SoundEffect( HurtSound, transform.position );
			_vitality = (short) ( _vitality - damage );
			_timerOfInvencibility = InvencibilityTime;
			_invencibility = true;
			for ( ushort i = (ushort) _gwambaCanvas.Vitality.Length; ( 0 <= _vitality ? _vitality : 0 ) < i; i-- )
			{
				_gwambaCanvas.Vitality[ i - 1 ].style.backgroundColor = _gwambaCanvas.MissingColor;
				_gwambaCanvas.Vitality[ i - 1 ].style.borderBottomColor = _gwambaCanvas.MissingColor;
				_gwambaCanvas.Vitality[ i - 1 ].style.borderLeftColor = _gwambaCanvas.MissingColor;
				_gwambaCanvas.Vitality[ i - 1 ].style.borderRightColor = _gwambaCanvas.MissingColor;
				_gwambaCanvas.Vitality[ i - 1 ].style.borderTopColor = _gwambaCanvas.MissingColor;
			}
			if ( 0 >= _vitality )
			{
				OnDisable();
				EffectsController.SoundEffect( DeathSound, transform.position );
				SaveController.Load( out SaveFile saveFile );
				_gwambaCanvas.LifeText.text = $"X {saveFile.Lifes -= 1}";
				_localAtLinearVelocity = Vector2.zero;
				_rigidbody.gravityScale = GravityScale;
				_invencibility = false;
				SaveController.WriteSave( saveFile );
				for ( ushort i = 0; _gwambaDamagers.Length > i; i++ )
					_gwambaDamagers[ i ].Alpha = 1F;
				_animator.SetBool( Idle, false );
				_animator.SetBool( Walk, false );
				_animator.SetBool( Jump, false );
				_animator.SetBool( Fall, false );
				_animator.SetBool( AirJump, false );
				_animator.SetBool( DashSlide, false );
				_animator.SetBool( AttackJump, false );
				_animator.SetBool( AttackAirJump, false );
				_animator.SetBool( AttackSlide, false );
				_animator.SetBool( Stun, false );
				_animator.SetBool( Death, true );
				_sender.SetToggle( false );
				_sender.SetFormat( MessageFormat.State );
				_sender.Send( MessagePath.Hud );
				_sender.SetFormat( MessageFormat.Event );
				_sender.Send( MessagePath.Hud );
				_sender.SetFormat( MessageFormat.None );
				_sender.Send( MessagePath.Enemy );
			}
			return true;
		}
		public void DamagerStun( ushort stunStrength, float stunTime )
		{
			_stunResistance -= (short) stunStrength;
			for ( ushort i = (ushort) _gwambaCanvas.StunResistance.Length; ( 0 <= _stunResistance ? _stunResistance : 0 ) < i; i-- )
				_gwambaCanvas.StunResistance[ i - 1 ].style.backgroundColor = _gwambaCanvas.MissingColor;
			if ( 0 >= _stunResistance && !_animator.GetBool( Death ) )
			{
				DisableInputs();
				_localAtLinearVelocity = Vector2.zero;
				_rigidbody.gravityScale = GravityScale;
				_stunTimer = stunTime;
				_animator.SetBool( AirJump, false );
				_animator.SetBool( DashSlide, false );
				_animator.SetBool( AttackJump, false );
				_animator.SetBool( AttackAirJump, false );
				_animator.SetBool( AttackSlide, false );
				_animator.SetBool( Stun, !( _invencibility = false ) );
				for ( ushort i = 0; _gwambaDamagers.Length > i; i++ )
					_gwambaDamagers[ i ].Alpha = 1F;
				for ( ushort i = 0; ( _stunResistance = (short) _gwambaCanvas.StunResistance.Length ) > i; i++ )
					_gwambaCanvas.StunResistance[ i ].style.backgroundColor = _gwambaCanvas.StunResistanceColor;
				EffectsController.SoundEffect( StunSound, transform.position );
			}
		}
		private void DamagerAttack( GwambaDamager gwambaDamager, IDestructible destructible )
		{
			if ( destructible.Hurt( gwambaDamager.AttackDamage ) )
			{
				destructible.Stun( gwambaDamager.AttackDamage, gwambaDamager.StunTime );
				EffectsController.HitStop( HitStopTime, HitSlowTime );
				EffectsController.SoundEffect( DamageAttackSound, gwambaDamager.transform.position );
				_attackDelay = DelayAfterAttack;
				_screenShaker.ImpulseDefinition.ImpulseDuration = gwambaDamager.AttackShakeTime;
				_screenShaker.GenerateImpulse( gwambaDamager.AttackShake );
				gwambaDamager.damagedes.Add( destructible.Source );
				for ( ushort amount = 0; ( destructible.Health <= 0 ? gwambaDamager.AttackDamage + 1 : gwambaDamager.AttackDamage ) > amount; amount++ )
					if ( _gwambaCanvas.RecoverVitality.Length <= _recoverVitality && _gwambaCanvas.Vitality.Length > _vitality )
					{
						_vitality += 1;
						for ( ushort i = 0; _vitality > i; i++ )
						{
							_gwambaCanvas.Vitality[ i ].style.backgroundColor = _gwambaCanvas.BackgroundColor;
							_gwambaCanvas.Vitality[ i ].style.borderBottomColor = _gwambaCanvas.BorderColor;
							_gwambaCanvas.Vitality[ i ].style.borderLeftColor = _gwambaCanvas.BorderColor;
							_gwambaCanvas.Vitality[ i ].style.borderRightColor = _gwambaCanvas.BorderColor;
							_gwambaCanvas.Vitality[ i ].style.borderTopColor = _gwambaCanvas.BorderColor;
						}
						for ( ushort i = _recoverVitality = 0; _gwambaCanvas.RecoverVitality.Length > i; i++ )
							_gwambaCanvas.RecoverVitality[ i ].style.backgroundColor = _gwambaCanvas.MissingColor;
						_stunResistance = (short) ( _stunResistance < _gwambaCanvas.StunResistance.Length ? _stunResistance + 1 : _stunResistance );
						for ( ushort i = 0; _stunResistance > i; i++ )
							_gwambaCanvas.StunResistance[ i ].style.backgroundColor = _gwambaCanvas.StunResistanceColor;
					}
					else if ( _gwambaCanvas.RecoverVitality.Length > _recoverVitality )
					{
						_recoverVitality += 1;
						for ( ushort i = 0; _recoverVitality > i; i++ )
							_gwambaCanvas.RecoverVitality[ i ].style.backgroundColor = _gwambaCanvas.BorderColor;
					}
			}
		}
		private void Update()
		{
			if ( !_instance || _instance != this || !_didStart || _animator.GetBool( Death ) )
				return;
			if ( _invencibility )
			{
				_invencibility = 0F < ( _timerOfInvencibility -= Time.deltaTime );
				if ( _invencibility && 0F >= ( _showInvencibilityTimer -= Time.deltaTime ) )
				{
					for ( ushort i = 0; _gwambaDamagers.Length > i; i++ )
						_gwambaDamagers[ i ].Alpha = _gwambaDamagers[ i ].Alpha >= 1F ? InvencibilityValue : 1F;
					_showInvencibilityTimer = TimeStep;
				}
				if ( !_invencibility )
					for ( ushort i = 0; _gwambaDamagers.Length > i; i++ )
						_gwambaDamagers[ i ].Alpha = 1F;
			}
			if ( _animator.GetBool( Stun ) )
				if ( 0F >= ( _stunTimer -= Time.deltaTime ) )
				{
					_animator.SetBool( Stun, !( _invencibility = true ) );
					EnableInputs();
				}
			if ( 0F < _fadeTimer )
				if ( 0F >= ( _fadeTimer -= Time.deltaTime ) )
				{
					_gwambaCanvas.FallDamageText.style.opacity = 0F;
					_gwambaCanvas.FallDamageText.text = $"X 0";
				}
			if ( !_animator.GetBool( DashSlide ) && !_isOnGround && !_downStairs && ( 0F < _lastGroundedTime || 0F < _lastJumpTime ) )
			{
				_lastGroundedTime -= Time.deltaTime;
				_lastJumpTime -= Time.deltaTime;
			}
			if ( 0F < _attackDelay )
				if ( 0F >= ( _attackDelay -= Time.deltaTime ) )
					for ( ushort i = 0; _gwambaDamagers.Length > i; i++ )
						_gwambaDamagers[ i ].damagedes.Clear();
		}
		private float BunnyHop( float callBackValue ) => 0 < _bunnyHopBoost ? _bunnyHopBoost * callBackValue : 0F;
		private void FixedUpdate()
		{
			if ( !_instance || _instance != this || !_didStart || _animator.GetBool( Stun ) || _animator.GetBool( Death ) )
				return;
			if ( _animator.GetBool( DashSlide ) )
				if ( Mathf.Abs( transform.position.x - _localAtAny.x ) > DashDistance || !_isOnGround || _isJumping )
				{
					_animator.SetBool( DashSlide, false );
					_animator.SetBool( AttackSlide, false );
				}
				else
					_rigidbody.linearVelocityX = DashSpeed * _localAtAny.z;
			else
			{
				if ( _animator.GetBool( AirJump ) )
					if ( _isJumping )
					{
						_animator.SetBool( AirJump, false );
						_animator.SetBool( AttackAirJump, false );
					}
					else
						_lastGroundedTime = JumpCoyoteTime;
				else
				{
					if ( !_isOnGround && !_downStairs && MINIMUM_VELOCITY < Mathf.Abs( _rigidbody.linearVelocityY ) )
					{
						if ( _animator.GetBool( Idle ) )
							_animator.SetBool( Idle, false );
						if ( _animator.GetBool( Walk ) )
							_animator.SetBool( Walk, false );
						if ( !_animator.GetBool( Jump ) && 0F < _rigidbody.linearVelocityY )
							_animator.SetBool( Jump, true );
						else if ( _animator.GetBool( Jump ) && 0F > _rigidbody.linearVelocityY )
							_animator.SetBool( Jump, false );
						if ( !_animator.GetBool( Fall ) && 0F > _rigidbody.linearVelocityY )
							_animator.SetBool( Fall, true );
						else if ( _animator.GetBool( Fall ) && 0F < _rigidbody.linearVelocityY )
							_animator.SetBool( Fall, false );
						if ( _animator.GetBool( AttackJump ) && 0F > _rigidbody.linearVelocityY )
							_animator.SetBool( AttackJump, false );
						if ( _animator.GetBool( Fall ) )
						{
							if ( _rigidbody.gravityScale < FallGravityMultiply * GravityScale )
								_rigidbody.gravityScale = FallGravityMultiply * GravityScale;
							if ( _fallStarted && !_isHubbyWorld )
							{
								_fallDamage = Mathf.Abs( _startOfFall - transform.position.y );
								if ( _fallDamage >= FallDamageDistance * FallDamageShowMultiply )
								{
									_gwambaCanvas.FallDamageText.style.opacity = 1F;
									_gwambaCanvas.FallDamageText.text = $"X {_fallDamage / FallDamageDistance:F1}";
								}
								else if ( !_invencibility )
								{
									_gwambaCanvas.FallDamageText.style.opacity = 0F;
									_gwambaCanvas.FallDamageText.text = $"X 0";
								}
							}
							else if ( !_isHubbyWorld )
							{
								_startOfFall = transform.position.y;
								_fallDamage = 0F;
								_fallStarted = true;
							}
						}
						else
						{
							if ( !_invencibility )
							{
								_gwambaCanvas.FallDamageText.style.opacity = 0F;
								_gwambaCanvas.FallDamageText.text = $"X 0";
							}
							if ( _rigidbody.gravityScale > GravityScale )
								_rigidbody.gravityScale = GravityScale;
							if ( _fallStarted )
							{
								_fallStarted = false;
								_fallDamage = 0F;
							}
						}
						if ( AttackUsage )
							_rigidbody.linearVelocityY *= AttackVelocityCut;
					}
					_localAtAny.x = ( _longJumping ? DashSpeed : MovementSpeed + BunnyHop( VelocityBoost ) ) * ( AttackUsage ? AttackVelocityCut : 1F );
					_localAtAny.y = _localAtAny.x * _walkValue - _rigidbody.linearVelocityX;
					_localAtAny.z = 0F < Mathf.Abs( _localAtAny.x * _walkValue ) ? Acceleration : Decceleration;
					_rigidbody.AddForceX( Mathf.Pow( Mathf.Abs( _localAtAny.y ) * _localAtAny.z, VelocityPower ) * Mathf.Sign( _localAtAny.y ) * _rigidbody.mass );
					if ( 0 != _walkValue && !AttackUsage )
					{
						if ( MINIMUM_VELOCITY < Mathf.Abs( _rigidbody.linearVelocityX ) )
							transform.TurnScaleX( 0F > _rigidbody.linearVelocityX );
						else
							transform.TurnScaleX( _walkValue );
						if ( _isOnGround )
						{
							_localAtAny.z = MINIMUM_VELOCITY >= Mathf.Abs( _rigidbody.linearVelocityX ) ? 1F : Mathf.Abs( _rigidbody.linearVelocityX ) / _localAtAny.x;
							_animator.SetFloat( WalkSpeed, _localAtAny.z );
						}
					}
				}
				if ( _isOnGround )
				{
					if ( 0 == _walkValue )
					{
						_localAtAny.x = Mathf.Min( Mathf.Abs( _rigidbody.linearVelocityX ), Mathf.Abs( FrictionAmount ) ) * Mathf.Sign( _rigidbody.linearVelocityX );
						_rigidbody.AddForceX( -_localAtAny.x * _rigidbody.mass, ForceMode2D.Impulse );
						_localAtAny.x = _longJumping ? DashSpeed : MovementSpeed + BunnyHop( VelocityBoost );
						_animator.SetFloat( WalkSpeed, Mathf.Abs( _rigidbody.linearVelocityX ) / _localAtAny.x );
					}
					if ( !_animator.GetBool( Idle ) && ( 0 == _walkValue || MINIMUM_VELOCITY >= Mathf.Abs( _rigidbody.linearVelocityX ) || _animator.GetBool( Fall ) ) )
						_animator.SetBool( Idle, true );
					else if ( _animator.GetBool( Idle ) || MINIMUM_VELOCITY < Mathf.Abs( _rigidbody.linearVelocityX ) )
						_animator.SetBool( Idle, false );
					if ( !_animator.GetBool( Walk ) && 0 != _walkValue )
						_animator.SetBool( Walk, true );
					else if ( _animator.GetBool( Walk ) && 0 == _walkValue && MINIMUM_VELOCITY >= Mathf.Abs( _rigidbody.linearVelocityX ) )
						_animator.SetBool( Walk, false );
				}
			}
			if ( !_isJumping && 0F < _lastJumpTime && 0F < _lastGroundedTime )
			{
				_animator.SetBool( AttackJump, ComboAttackBuffer );
				_longJumping = _animator.GetBool( DashSlide );
				_isJumping = !( _bunnyHopUsed = false );
				_rigidbody.gravityScale = GravityScale;
				_rigidbody.linearVelocityY = 0F;
				if ( 0 < _bunnyHopBoost )
				{
					_offBunnyHop = true;
					for ( ushort i = 0; _bunnyHopBoost > i; i++ )
						_gwambaCanvas.BunnyHop[ i ].style.backgroundColor = _gwambaCanvas.BunnyHopColor;
				}
				_rigidbody.AddForceY( ( JumpStrenght + BunnyHop( JumpBoost ) ) * _rigidbody.mass, ForceMode2D.Impulse );
				EffectsController.SoundEffect( JumpSound, transform.position );
				if ( ComboAttackBuffer )
					StartAttackSound();
			}
			_downStairs = false;
		}
		private void OnCollisionStay2D( Collision2D collision )
		{
			if ( !_instance || this != _instance || !isActiveAndEnabled || !_didStart || WorldBuild.SCENE_LAYER != collision.gameObject.layer )
				return;
			if ( _animator.GetBool( AirJump ) || _animator.GetBool( DashSlide ) && ( !_animator.GetBool( Stun ) || !_animator.GetBool( Death ) ) )
			{
				_collider.GetContacts( _groundContacts );
				_localAtStart.Set( Local.x + _collider.bounds.extents.x * _localAtAny.z, Local.y );
				_localAtEnd.Set( WorldBuild.SNAP_LENGTH, _collider.size.y );
				_groundContacts.RemoveAll( contact => contact.point.OutsideRectangle( _localAtStart, _localAtEnd ) );
				if ( 0 < _groundContacts.Count )
				{
					_animator.SetBool( AirJump, false );
					_animator.SetBool( DashSlide, false );
					_animator.SetBool( AttackAirJump, false );
					_animator.SetBool( AttackSlide, false );
					EffectsController.SurfaceSound( _groundContacts[ 0 ].point );
				}
			}
			if ( _isOnGround && !_offGround && 0 == _walkValue && MINIMUM_VELOCITY >= _rigidbody.linearVelocityX && MINIMUM_VELOCITY >= _rigidbody.linearVelocityY )
				return;
			_offGround = !_isOnGround;
			_collider.GetContacts( _groundContacts );
			_localAtStart.Set( Local.x, Local.y - _collider.bounds.extents.y );
			_localAtEnd.Set( _collider.size.x, WorldBuild.SNAP_LENGTH );
			_groundContacts.RemoveAll( contact => contact.point.OutsideRectangle( _localAtStart, _localAtEnd ) );
			if ( _isOnGround = 0 < _groundContacts.Count )
			{
				if ( _animator.GetBool( AirJump ) )
				{
					_animator.SetBool( AirJump, false );
					_animator.SetBool( AttackAirJump, false );
					EffectsController.SurfaceSound( _groundContacts[ 0 ].point );
				}
				if ( _offGround )
				{
					if ( _animator.GetBool( Jump ) )
						_animator.SetBool( Jump, false );
					if ( _animator.GetBool( Fall ) )
						_animator.SetBool( Fall, false );
					_lastGroundedTime = JumpCoyoteTime;
					_canAirJump = !( _offGround = _longJumping = _isJumping = false );
					_bunnyHopBoost = 0F < _lastJumpTime ? _bunnyHopBoost : (ushort) 0;
					if ( 0 >= _bunnyHopBoost && _offBunnyHop )
					{
						_bunnyHopUsed = _offBunnyHop = false;
						for ( ushort i = 0; _gwambaCanvas.BunnyHop.Length > i; i++ )
							_gwambaCanvas.BunnyHop[ i ].style.backgroundColor = _gwambaCanvas.MissingColor;
					}
					if ( _fallStarted && 0 >= _bunnyHopBoost && !_isHubbyWorld )
					{
						_screenShaker.ImpulseDefinition.ImpulseDuration = FallShakeTime;
						_screenShaker.GenerateImpulse( _fallDamage / FallDamageDistance * FallShake );
						DamagerHurt( (ushort) Mathf.FloorToInt( _fallDamage / FallDamageDistance ) );
						GroundSound( 0F );
						_fallStarted = false;
						_fallDamage = 0F;
						if ( _invencibility && 0F >= _fadeTimer )
							_fadeTimer = TimeToFadeShow;
						else
						{
							_gwambaCanvas.FallDamageText.style.opacity = 0F;
							_gwambaCanvas.FallDamageText.text = $"X 0";
						}
					}
				}
				if ( !_animator.GetBool( AirJump ) && !_animator.GetBool( DashSlide ) && 0 != _walkValue && ( !_animator.GetBool( Stun ) || !_animator.GetBool( Death ) ) )
					if ( MINIMUM_VELOCITY >= Mathf.Abs( _rigidbody.linearVelocityX ) )
					{
						_collider.GetContacts( _groundContacts );
						_localAtStart.Set( Local.x + _collider.bounds.extents.x * transform.localScale.x.CompareTo( 0F ), Local.y + WorldBuild.SNAP_LENGTH / 2F );
						_localAtEnd.Set( WorldBuild.SNAP_LENGTH, _collider.size.y + WorldBuild.SNAP_LENGTH );
						_groundContacts.RemoveAll( contact => contact.point.OutsideRectangle( _localAtStart, _localAtEnd ) );
						if ( 0 >= _groundContacts.Count )
							return;
						_localAtStart.Set( Local.x + _collider.bounds.extents.x * transform.localScale.x.CompareTo( 0F ), Local.y - ( _collider.size.y - UpStairsLength ) / 2F );
						_localAtEnd.Set( WorldBuild.SNAP_LENGTH, UpStairsLength );
						if ( _groundContacts.TrueForAll( contact => contact.point.InsideRectangle( _localAtStart, _localAtEnd ) ) )
						{
							_localAtAny.x = ( _collider.bounds.extents.x + WorldBuild.SNAP_LENGTH / 2F ) * transform.localScale.x.CompareTo( 0F );
							_localAtStart.Set( Local.x + _localAtAny.x, Local.y + _collider.bounds.extents.y );
							_localAtEnd.Set( Local.x + _localAtAny.x, Local.y - _collider.bounds.extents.y );
							if ( _castHit = Physics2D.Linecast( _localAtStart, _localAtEnd, WorldBuild.SCENE_LAYER_MASK ) )
							{
								_localAtAny.y = Mathf.Abs( _castHit.point.y - ( transform.position.y - _collider.bounds.extents.y ) );
								_localAtSurface.x = transform.position.x + WorldBuild.SNAP_LENGTH * transform.localScale.x.CompareTo( 0F );
								_localAtSurface.y = transform.position.y + _localAtAny.y;
								transform.position = _localAtSurface;
								_rigidbody.linearVelocityX = MovementSpeed * _walkValue * ( AttackUsage ? AttackVelocityCut : 1F );
							}
						}
					}
					else if ( 0F >= _lastJumpTime )
					{
						_localAtStart.x = Local.x - ( _collider.bounds.extents.x - WorldBuild.SNAP_LENGTH / 2F * DownStairsDistance ) * transform.localScale.x.CompareTo( 0F );
						_localAtEnd.x = WorldBuild.SNAP_LENGTH * DownStairsDistance;
						if ( _groundContacts.TrueForAll( contact => contact.point.x.InsideRange( _localAtStart.x, _localAtEnd.x ) ) )
						{
							_localAtAny.x = ( _collider.bounds.extents.x - WorldBuild.SNAP_LENGTH * DownStairsDistance ) * transform.localScale.x.CompareTo( 0F );
							_localAtStart.Set( Local.x - _localAtAny.x, Local.y - _collider.bounds.extents.y );
							if ( _downStairs = _castHit = Physics2D.Raycast( _localAtStart, -transform.up, WorldBuild.SNAP_LENGTH + 1F, WorldBuild.SCENE_LAYER_MASK ) )
							{
								_localAtSurface.x = transform.position.x + WorldBuild.SNAP_LENGTH * DownStairsDistance * _localAtAny.x;
								_localAtSurface.y = transform.position.y - _castHit.distance;
								transform.position = _localAtSurface;
							}
						}
					}
			}
		}
		private void OnCollisionExit2D( Collision2D collision )
		{
			if ( _instance && this == _instance && _didStart && WorldBuild.SCENE_LAYER == collision.gameObject.layer )
				_offGround = !( _isOnGround = false );
		}
		private void OnTriggerEnter2D( Collider2D other )
		{
			if ( _didStart && other.TryGetComponent<ICollectable>( out var collectable ) )
			{
				collectable.Collect();
				SaveController.Load( out SaveFile saveFile );
				_gwambaCanvas.LifeText.text = $"X {saveFile.Lifes}";
				_gwambaCanvas.CoinText.text = $"X {saveFile.Coins}";
			}
		}
		internal bool EqualObject( params GameObject[] others )
		{
			if ( _didStart && ( !_animator.GetBool( Stun ) || !_animator.GetBool( Death ) ) )
				for ( ushort i = 0; others.Length > i; i++ )
					if ( gameObject == others[ i ] )
						return true;
			return false;
		}
		public override void Receive( MessageData message )
		{
			if ( MessageFormat.Event == message.Format && message.ToggleValue.HasValue )
				if ( message.ToggleValue.Value )
				{
					OnEnable();
					_timerOfInvencibility = InvencibilityTime;
					_invencibility = true;
				}
				else
				{
					RestartState();
					_animator.SetBool( Death, _bunnyHopUsed = _offBunnyHop = false );
					transform.TurnScaleX( PointSetter.TurnToLeft );
					transform.position = PointSetter.CheckedPoint;
				}
		}
	};
};
