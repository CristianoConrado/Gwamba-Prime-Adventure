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
			_rigidbody = GetComponent<Rigidbody2D>();
			_collider = GetComponent<BoxCollider2D>();
			_screenShaker = GetComponent<CinemachineImpulseSource>();
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
			for ( byte i = 0; _gwambaDamagers.Length > i; i++ )
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
			_normalColliderSize = _collider.size;
			_turnLeft = TurnToLeft;
			_loadState = true;
			await StartLoad().SuppressCancellationThrow();
			if ( _destroyToken.IsCancellationRequested )
				return;
			_didStart = true;
			DontDestroyOnLoad( gameObject );
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
			await UniTask.WaitWhile( () => SceneInitiator.IsInTransition(), PlayerLoopTiming.Update, _destroyToken, true ).SuppressCancellationThrow();
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
			_vitality = (byte) _gwambaCanvas.Vitality.Length;
			_stunResistance = (byte) _gwambaCanvas.StunResistance.Length;
			for ( byte i = 0; _gwambaDamagers.Length > i; i++ )
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
			for ( byte i = 0; ( _vitality = (byte) _gwambaCanvas.Vitality.Length ) > i; i++ )
			{
				_gwambaCanvas.Vitality[ i ].style.backgroundColor = _gwambaCanvas.BackgroundColor;
				_gwambaCanvas.Vitality[ i ].style.borderBottomColor = _gwambaCanvas.BorderColor;
				_gwambaCanvas.Vitality[ i ].style.borderLeftColor = _gwambaCanvas.BorderColor;
				_gwambaCanvas.Vitality[ i ].style.borderRightColor = _gwambaCanvas.BorderColor;
				_gwambaCanvas.Vitality[ i ].style.borderTopColor = _gwambaCanvas.BorderColor;
			}
			for ( byte i = _recoverVitality = 0; _gwambaCanvas.RecoverVitality.Length > i; i++ )
				_gwambaCanvas.RecoverVitality[ i ].style.backgroundColor = _gwambaCanvas.MissingColor;
			for ( byte i = 0; ( _stunResistance = (byte) _gwambaCanvas.StunResistance.Length ) > i; i++ )
				_gwambaCanvas.StunResistance[ i ].style.backgroundColor = _gwambaCanvas.StunResistanceColor;
			for ( byte i = _bunnyHopBoost = 0; _gwambaCanvas.BunnyHop.Length > i; i++ )
				_gwambaCanvas.BunnyHop[ i ].style.backgroundColor = _gwambaCanvas.MissingColor;
		}
		private void MovementInput( InputAction.CallbackContext movement )
		{
			if ( !isActiveAndEnabled || _animator.GetBool( Stun ) )
				return;
			_walkValue = ( _localAtStart = movement.ReadValue<Vector2>() ).x.RangeNormalize( MovementInputZone );
			if ( ( !AttackUsage || ComboAttackBuffer ) && !_animator.GetBool( AttackCombo ) )
				if ( 0 != _walkValue && _localAtStart.y > UpInputZone && _offGround && _canAirJump && !_animator.GetBool( AirJump ) )
				{
					_animator.SetBool( AirJump, !( _isJumping = _canAirJump = false ) );
					_animator.SetBool( AttackAirJump, ComboAttackBuffer );
					transform.TurnScaleX( _localAtAny.z = _walkValue );
					_rigidbody.linearVelocity = Vector2.zero;
					_rigidbody.AddForceX( ( ImpulseStrenght + BunnyHop( JumpBoost ) ) * _localAtAny.z * _rigidbody.mass, ForceMode2D.Impulse );
					_rigidbody.AddForceY( ( ImpulseStrenght + BunnyHop( JumpBoost ) ) * _rigidbody.mass, ForceMode2D.Impulse );
					EffectsController.SoundEffect( AirJumpSound, transform.position );
					if ( ComboAttackBuffer )
						StartAttackSound();
				}
				else if ( _localAtStart.y < DownInputZone )
					if ( 0 != _walkValue && _isOnGround && !_animator.GetBool( DashSlide ) )
					{
						_animator.SetBool( DashSlide, true );
						_animator.SetBool( AttackSlide, ComboAttackBuffer );
						transform.TurnScaleX( _localAtAny.z = _walkValue );
						_localAtAny.x = transform.position.x;
						EffectsController.SoundEffect( DashSlideSound, transform.position );
						if ( ComboAttackBuffer )
							StartAttackSound();
					}
					else if ( _offGround && !_animator.GetBool( AttackDrop ) && ComboAttackBuffer && _canAttackDrop )
					{
						_animator.SetBool( AttackDrop, !( _isJumping = _canAttackDrop = false ) );
						_rigidbody.linearVelocity = Vector2.zero;
						_rigidbody.AddForceY( ( ImpulseStrenght + BunnyHop( JumpBoost ) ) * -_rigidbody.mass, ForceMode2D.Impulse );
						StartAttackSound();
					}
		}
		private void GroundSound( float stepPositionX )
		{
			_localAtSurface.Set( Local.x + stepPositionX * transform.localScale.x.CompareTo( 0F ), Local.y - _collider.bounds.extents.y );
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
					_bunnyHopBoost = (byte) ( _gwambaCanvas.BunnyHop.Length > _bunnyHopBoost + 1 ? _bunnyHopBoost + 1 : _gwambaCanvas.BunnyHop.Length );
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
			for ( byte i = (byte) Physics2D.OverlapCollider( _collider, _interactionFilter, _interactions ); 0 < i; i-- )
				if ( _interactions[ i - 1 ].TryGetComponent<IInteractable>( out _ ) )
				{
					_interactionsPerObject = _interactions[ i - 1 ].GetComponents<IInteractable>();
					for ( byte j = 0; _interactionsPerObject.Length > j; j++ )
						_interactionsPerObject[ j ]?.Interaction();
					return;
				}
		}
		public bool DamagerHurt( byte damage )
		{
			if ( _invencibility || 0 >= damage || _animator.GetBool( Death ) )
				return false;
			EffectsController.SoundEffect( HurtSound, transform.position );
			_vitality = (byte) ( 0 <= _vitality - damage ? _vitality - damage : 0 );
			_timerOfInvencibility = InvencibilityTime;
			_invencibility = true;
			for ( byte i = (byte) _gwambaCanvas.Vitality.Length; _vitality < i; i-- )
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
				for ( byte i = 0; _gwambaDamagers.Length > i; i++ )
					_gwambaDamagers[ i ].Alpha = 1F;
				_animator.SetBool( Idle, false );
				_animator.SetBool( Walk, false );
				_animator.SetBool( Jump, false );
				_animator.SetBool( Fall, false );
				_animator.SetBool( AirJump, false );
				_animator.SetBool( DashSlide, false );
				_animator.SetBool( AttackJump, false );
				_animator.SetBool( AttackSlide, false );
				_animator.SetBool( AttackAirJump, false );
				_animator.SetBool( AttackDrop, false );
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
		public void DamagerStun( byte stunStrength, float stunTime )
		{
			_stunResistance = (byte) ( 0 <= _stunResistance - stunStrength ? _stunResistance - stunStrength : 0 );
			for ( byte i = (byte) _gwambaCanvas.StunResistance.Length; _stunResistance < i; i-- )
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
				_animator.SetBool( AttackSlide, false );
				_animator.SetBool( AttackAirJump, false );
				_animator.SetBool( AttackDrop, false );
				_animator.SetBool( Stun, !( _invencibility = false ) );
				for ( byte i = 0; _gwambaDamagers.Length > i; i++ )
					_gwambaDamagers[ i ].Alpha = 1F;
				for ( byte i = 0; ( _stunResistance = (byte) _gwambaCanvas.StunResistance.Length ) > i; i++ )
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
				for ( byte amount = 0; ( destructible.Health <= 0 ? gwambaDamager.AttackDamage + 1 : gwambaDamager.AttackDamage ) > amount; amount++ )
					if ( _gwambaCanvas.RecoverVitality.Length <= _recoverVitality && _gwambaCanvas.Vitality.Length > _vitality )
					{
						_vitality += 1;
						for ( byte i = 0; _vitality > i; i++ )
						{
							_gwambaCanvas.Vitality[ i ].style.backgroundColor = _gwambaCanvas.BackgroundColor;
							_gwambaCanvas.Vitality[ i ].style.borderBottomColor = _gwambaCanvas.BorderColor;
							_gwambaCanvas.Vitality[ i ].style.borderLeftColor = _gwambaCanvas.BorderColor;
							_gwambaCanvas.Vitality[ i ].style.borderRightColor = _gwambaCanvas.BorderColor;
							_gwambaCanvas.Vitality[ i ].style.borderTopColor = _gwambaCanvas.BorderColor;
						}
						for ( byte i = _recoverVitality = 0; _gwambaCanvas.RecoverVitality.Length > i; i++ )
							_gwambaCanvas.RecoverVitality[ i ].style.backgroundColor = _gwambaCanvas.MissingColor;
						_stunResistance = (byte) ( _stunResistance < _gwambaCanvas.StunResistance.Length ? _stunResistance + 1 : _stunResistance );
						for ( byte i = 0; _stunResistance > i; i++ )
							_gwambaCanvas.StunResistance[ i ].style.backgroundColor = _gwambaCanvas.StunResistanceColor;
					}
					else if ( _gwambaCanvas.RecoverVitality.Length > _recoverVitality )
					{
						_recoverVitality += 1;
						for ( byte i = 0; _recoverVitality > i; i++ )
							_gwambaCanvas.RecoverVitality[ i ].style.backgroundColor = _gwambaCanvas.BorderColor;
					}
			}
		}
		private void Update()
		{
			if ( !_instance || _instance != this || !_didStart || _animator.GetBool( Death ) || SceneInitiator.IsInTransition() )
				return;
			if ( _invencibility )
			{
				_invencibility = 0F < ( _timerOfInvencibility -= Time.deltaTime );
				if ( _invencibility && 0F >= ( _showInvencibilityTimer -= Time.deltaTime ) )
				{
					for ( byte i = 0; _gwambaDamagers.Length > i; i++ )
						_gwambaDamagers[ i ].Alpha = _gwambaDamagers[ i ].Alpha >= 1F ? InvencibilityValue : 1F;
					_showInvencibilityTimer = TimeStep;
				}
				if ( !_invencibility )
					for ( byte i = 0; _gwambaDamagers.Length > i; i++ )
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
					for ( byte i = 0; _gwambaDamagers.Length > i; i++ )
						_gwambaDamagers[ i ].damagedes.Clear();
			_deltaTimeFPS += ( Time.unscaledDeltaTime - _deltaTimeFPS ) * 1E-1F;
			_gwambaCanvas.ShowFPS.text = $"{1F / _deltaTimeFPS:F0} FPS";
		}
		private float BunnyHop( float callBackValue ) => 0 < _bunnyHopBoost ? _bunnyHopBoost * callBackValue : 0F;
		private void FixedUpdate()
		{
			if ( !_instance || _instance != this || !_didStart || _animator.GetBool( Stun ) || _animator.GetBool( Death ) || SceneInitiator.IsInTransition() )
				return;
			if ( _animator.GetBool( DashSlide ) )
				if ( Mathf.Abs( transform.position.x - _localAtAny.x ) > DashDistance || _offGround || _isJumping )
				{
					_animator.SetBool( DashSlide, false );
					_animator.SetBool( AttackSlide, false );
				}
				else
					_rigidbody.linearVelocityX = DashSpeed * _localAtAny.z;
			else
			{
				if ( _animator.GetBool( AirJump ) || _animator.GetBool( AttackDrop ) )
					if ( _isJumping )
					{
						_animator.SetBool( AirJump, false );
						_animator.SetBool( AttackAirJump, false );
						_animator.SetBool( AttackDrop, false );
					}
					else
						_lastGroundedTime = JumpCoyoteTime;
				else
				{
					if ( _offGround && !_downStairs && MINIMUM_VELOCITY < Mathf.Abs( _rigidbody.linearVelocityY ) )
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
						if ( AttackUsage && !_animator.GetBool( AttackJump ) && !_animator.GetBool( AttackDrop ) )
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
				if ( !_animator.GetBool( AttackCombo ) )
					_animator.SetBool( AttackJump, ComboAttackBuffer );
				_longJumping = _animator.GetBool( DashSlide );
				_isJumping = !( _bunnyHopUsed = false );
				_rigidbody.gravityScale = GravityScale;
				_rigidbody.linearVelocityY = 0F;
				if ( _offBunnyHop = 0 < _bunnyHopBoost )
					for ( byte i = 0; _bunnyHopBoost > i; i++ )
						_gwambaCanvas.BunnyHop[ i ].style.backgroundColor = _gwambaCanvas.BunnyHopColor;
				_rigidbody.AddForceY( ( JumpStrenght + BunnyHop( JumpBoost ) ) * _rigidbody.mass, ForceMode2D.Impulse );
				EffectsController.SoundEffect( JumpSound, transform.position );
				if ( ComboAttackBuffer )
					StartAttackSound();
			}
			_downStairs = false;
		}
		private void OnCollisionStay2D( Collision2D collision )
		{
			if ( !_instance || this != _instance || !isActiveAndEnabled || !_didStart || WorldBuild.SCENE_LAYER != collision.gameObject.layer || SceneInitiator.IsInTransition() )
				return;
			if ( ( _animator.GetBool( AirJump ) || _animator.GetBool( DashSlide ) ) && ( !_animator.GetBool( Stun ) || !_animator.GetBool( Death ) ) )
			{
				_collider.GetContacts( _groundContacts );
				if ( _groundContacts.Exists( contact => 0F < _localAtAny.z ? -CheckGroundLimit >= contact.normal.x : CheckGroundLimit <= contact.normal.x ) )
				{
					_animator.SetBool( AirJump, false );
					_animator.SetBool( DashSlide, false );
					_animator.SetBool( AttackAirJump, false );
					_animator.SetBool( AttackSlide, false );
					EffectsController.SurfaceSound( _groundContacts[ 0 ].point );
				}
			}
			if ( _isOnGround && !_offGround && 0 == _walkValue && _rigidbody.linearVelocity.Abs().LessOrEqual( Vector2.one * MINIMUM_VELOCITY ) )
				return;
			_offGround = !_isOnGround;
			_collider.GetContacts( _groundContacts );
			if ( _isOnGround = _groundContacts.Exists( contact => CheckGroundLimit <= contact.normal.y ) )
			{
				if ( _offGround )
				{
					if ( _animator.GetBool( Jump ) )
						_animator.SetBool( Jump, false );
					if ( _animator.GetBool( Fall ) )
						_animator.SetBool( Fall, false );
					if ( _animator.GetBool( AirJump ) )
					{
						_animator.SetBool( AirJump, false );
						_animator.SetBool( AttackAirJump, false );
						EffectsController.SurfaceSound( _groundContacts[ 0 ].point );
					}
					_lastGroundedTime = JumpCoyoteTime;
					_canAttackDrop = _canAirJump = !( _offGround = _longJumping = _isJumping = false );
					_bunnyHopBoost = 0F < _lastJumpTime ? _bunnyHopBoost : (byte) 0;
					if ( 0 >= _bunnyHopBoost && _offBunnyHop )
					{
						_bunnyHopUsed = _offBunnyHop = false;
						for ( byte i = 0; _gwambaCanvas.BunnyHop.Length > i; i++ )
							_gwambaCanvas.BunnyHop[ i ].style.backgroundColor = _gwambaCanvas.MissingColor;
					}
					if ( _fallStarted && 0 >= _bunnyHopBoost && !_isHubbyWorld )
					{
						_screenShaker.ImpulseDefinition.ImpulseDuration = FallShakeTime;
						_screenShaker.GenerateImpulse( _fallDamage / FallDamageDistance * FallShake );
						if ( !_animator.GetBool( AttackDrop ) )
							DamagerHurt( (byte) Mathf.FloorToInt( _fallDamage / FallDamageDistance ) );
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
					if ( _animator.GetBool( AttackDrop ) )
					{
						_animator.SetBool( AttackDrop, false );
						EffectsController.SurfaceSound( _groundContacts[ 0 ].point );
					}
				}
				if ( _animator.GetBool( AirJump ) || _animator.GetBool( DashSlide ) || _animator.GetBool( Stun ) || _animator.GetBool( Death ) )
					return;
				if ( MINIMUM_VELOCITY >= Mathf.Abs( _rigidbody.linearVelocityX ) )
				{
					_localAtStart.Set( Local.x + _normalColliderSize.x / 2F * transform.localScale.x.CompareTo( 0F ), Local.y + WorldBuild.SNAP_LENGTH / 2F );
					_localAtEnd.Set( WorldBuild.SNAP_LENGTH, _normalColliderSize.y + WorldBuild.SNAP_LENGTH );
					_groundContacts.RemoveAll( contact =>
					{
						return contact.point.OutsideBoxCast( _localAtStart, _localAtEnd )
						&& ( contact.point - contact.relativeVelocity * Time.fixedDeltaTime ).OutsideBoxCast( _localAtStart, _localAtEnd );
					} );
					if ( 0 >= _groundContacts.Count )
						return;
					_localAtStart.Set( Local.x + _normalColliderSize.x / 2F * transform.localScale.x.CompareTo( 0F ), Local.y - ( _normalColliderSize.y - UpStairsLength ) / 2F );
					_localAtEnd.Set( WorldBuild.SNAP_LENGTH, UpStairsLength );
					if ( _groundContacts.Exists( contact =>
					{
						return contact.point.OutsideBoxCast( _localAtStart, _localAtEnd )
						&& ( contact.point - contact.relativeVelocity * Time.fixedDeltaTime ).OutsideBoxCast( _localAtStart, _localAtEnd );
					} ) )
						return;
					_localAtStart.y += _localAtEnd.y;
					_localAtEnd = _groundContacts[ 0 ].point;
					for ( byte i = 1; _groundContacts.Count > i; i++ )
						if ( Vector2.Distance( _localAtStart, _groundContacts[ i ].point ) < Vector2.Distance( _localAtStart, _localAtEnd ) )
							_localAtEnd = _groundContacts[ i ].point;
					_localAtSurface.x = transform.position.x + WorldBuild.SNAP_LENGTH * transform.localScale.x.CompareTo( 0F );
					_localAtSurface.y = transform.position.y + Mathf.Abs( _localAtEnd.y - ( Local.y - _normalColliderSize.y / 2F ) );
					transform.position = _localAtSurface;
					_rigidbody.linearVelocityX = MovementSpeed * _walkValue * ( AttackUsage ? AttackVelocityCut : 1F );
				}
				else if ( 0F >= _lastJumpTime )
				{
					_localAtAny.x = _collider.bounds.extents.x - WorldBuild.SNAP_LENGTH * DownStairsDistance / 2F + WorldBuild.SNAP_LENGTH / 2F;
					_localAtStart.Set( Local.x - _localAtAny.x * transform.localScale.x.CompareTo( 0F ), Local.y - _collider.bounds.extents.y );
					_localAtEnd.Set( WorldBuild.SNAP_LENGTH * DownStairsDistance + WorldBuild.SNAP_LENGTH, WorldBuild.SNAP_LENGTH );
					if ( _groundContacts.Exists( contact =>
					{
						return contact.point.OutsideBoxCast( _localAtStart, _localAtEnd )
						&& ( contact.point - contact.relativeVelocity * Time.fixedDeltaTime ).OutsideBoxCast( _localAtStart, _localAtEnd );
					} ) )
						return;
					_localAtStart.x = Local.x - ( _collider.bounds.extents.x - WorldBuild.SNAP_LENGTH * DownStairsDistance ) * transform.localScale.x.CompareTo( 0F );
					if ( _downStairs = 0 < Physics2D.RaycastNonAlloc( _localAtStart, -transform.up, _castHits, WorldBuild.SNAP_LENGTH + 1F, WorldBuild.SCENE_LAYER_MASK ) )
					{
						_localAtSurface.x = transform.position.x + WorldBuild.SNAP_LENGTH * DownStairsDistance * _localAtAny.x;
						_localAtSurface.y = transform.position.y - _castHits[ 0 ].distance;
						transform.position = _localAtSurface;
					}
				}
			}
		}
		private void OnCollisionExit2D( Collision2D collision )
		{
			if ( _instance && this == _instance && _didStart && WorldBuild.SCENE_LAYER == collision.gameObject.layer || SceneInitiator.IsInTransition() )
			{
				_collider.GetContacts( _groundContacts );
				if ( _groundContacts.Exists( contact => CheckGroundLimit <= contact.normal.y ) )
					return;
				_offGround = !( _isOnGround = false );
			}
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
				for ( byte i = 0; others.Length > i; i++ )
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
