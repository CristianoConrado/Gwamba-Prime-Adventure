using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Unity.Cinemachine;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using GwambaPrimeAdventure.Connection;
namespace GwambaPrimeAdventure.Character
{
	[DisallowMultipleComponent, SelectionBase, Icon("Assets/Huds/Designs/Gwamba Prime Series Logo.png"), RequireComponent(typeof(Transform), typeof(Animator), typeof(SortingGroup))]
	[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(CircleCollider2D)), RequireComponent(typeof(CinemachineImpulseSource))]
	public sealed class GwambaStateMarker : StateController, ILoader, IConnector
	{
		private static GwambaStateMarker _instance;
		private GwambaCanvas _gwambaCanvas;
		private GwambaDamager[] _gwambaDamagers;
		private Animator _animator;
		private Rigidbody2D _rigidbody;
		private BoxCollider2D _collider;
		private CinemachineImpulseSource _screenShaker;
		private InputController _inputController;
		private readonly Sender _sender = Sender.Create();
		private readonly Collider2D[] _interactions = new Collider2D[(uint)WorldBuild.PIXELS_PER_UNIT];
		private readonly List<ContactPoint2D> _groundContacts = new((int)WorldBuild.PIXELS_PER_UNIT);
		private IInteractable[] _interactionsPerObject;
		private Vector2 _localOfStart = Vector2.zero;
		private Vector2 _localOfEnd = Vector2.zero;
		private Vector2 _localOfSurface = Vector2.zero;
		private Vector2 _guardedLinearVelocity = Vector2.zero;
		private Vector3 _localOfAny = Vector3.zero;
		private RaycastHit2D _castHit;
		private readonly ContactFilter2D _interactionFilter = new()
		{
			layerMask = WorldBuild.SYSTEM_LAYER_MASK + WorldBuild.CHARACTER_LAYER_MASK + WorldBuild.SCENE_LAYER_MASK + WorldBuild.ITEM_LAYER_MASK,
			useLayerMask = true,
			useTriggers = true
		};
		private readonly int IsOn = Animator.StringToHash(nameof(IsOn));
		private readonly int Idle = Animator.StringToHash(nameof(Idle));
		private readonly int Walk = Animator.StringToHash(nameof(Walk));
		private readonly int WalkSpeed = Animator.StringToHash(nameof(WalkSpeed));
		private readonly int Jump = Animator.StringToHash(nameof(Jump));
		private readonly int Fall = Animator.StringToHash(nameof(Fall));
		private readonly int AirJump = Animator.StringToHash(nameof(AirJump));
		private readonly int DashSlide = Animator.StringToHash(nameof(DashSlide));
		private readonly int Attack = Animator.StringToHash(nameof(Attack));
		private readonly int AttackCombo = Animator.StringToHash(nameof(AttackCombo));
		private readonly int AttackJump = Animator.StringToHash(nameof(AttackJump));
		private readonly int AttackAirJump = Animator.StringToHash(nameof(AttackAirJump));
		private readonly int AttackSlide = Animator.StringToHash(nameof(AttackSlide));
		private readonly int Stun = Animator.StringToHash(nameof(Stun));
		private readonly int Death = Animator.StringToHash(nameof(Death));
		private short _vitality = 0;
		private short _stunResistance = 0;
		private ushort _recoverVitality = 0;
		private ushort _bunnyHopBoost = 0;
		private float _timerOfInvencibility = 0F;
		private float _showInvencibilityTimer = 0F;
		private float _stunTimer = 0F;
		private float _fadeTimer = 0F;
		private float _movementAction = 0F;
		private float _lastGroundedTime = 0F;
		private float _lastJumpTime = 0F;
		private float _startOfFall = 0F;
		private float _fallDamage = 0F;
		private float _attackDelay = 0F;
		private readonly float _minimumVelocity = WorldBuild.MINIMUM_TIME_SPACE_LIMIT * 10F;
		private bool _isHubbyWorld = false;
		private bool _didStart = false;
		private bool _isOnGround = false;
		private bool _offGround = false;
		private bool _downStairs = false;
		private bool _isJumping = false;
		private bool _canAirJump = true;
		private bool _longJumping = false;
		private bool _bunnyHopUsed = false;
		private bool _offBunnyHop = false;
		private bool _fallStarted = false;
		private bool _invencibility = false;
		[Space(WorldBuild.FIELD_SPACE_LENGTH * 2F)]
		[SerializeField, BoxGroup("Control"), Tooltip("The scene of the hubby world.")] private SceneField _hubbyWorldScene;
		[SerializeField, BoxGroup("Control"), Tooltip("The scene of the menu.")] private SceneField _menuScene;
		[SerializeField, BoxGroup("Control"), Tooltip("The sound to play when Gwamba gets hurt.")] private AudioClip _hurtSound;
		[SerializeField, BoxGroup("Control"), Tooltip("The sound to play when Gwamba gets stunned.")] private AudioClip _stunSound;
		[SerializeField, BoxGroup("Control"), Tooltip("The sound to play when Gwamba die.")] private AudioClip _deathSound;
		[SerializeField, BoxGroup("Control"), Tooltip("The velocity of the shake on the fall.")] private Vector2 _fallShake;
		[SerializeField, BoxGroup("Control"), Tooltip("The amount of distance to get down stairs.")] private ushort _downStairsDistance;
		[SerializeField, BoxGroup("Control"), Tooltip("The size of the detector to climb the stairs.")] private float _upStairsLength;
		[SerializeField, BoxGroup("Control"), Tooltip("The gravity applied to Gwamba.")] private float _gravityScale;
		[SerializeField, BoxGroup("Control"), Min(0F), Tooltip("The amount of time the fall screen shake will be applied.")] private float _fallShakeTime;
		[SerializeField, BoxGroup("Control"), Min(0F), Tooltip("The amount of gravity to multiply on the fall.")] private float _fallGravityMultiply;
		[SerializeField, BoxGroup("Control"), Min(0F), Tooltip("The amount of fall's distance to take damage.")] private float _fallDamageDistance;
		[SerializeField, BoxGroup("Control"), Min(0F), Tooltip("The amount of time to fade the show of fall's damage.")] private float _timeToFadeShow;
		[SerializeField, BoxGroup("Control"), Range(0F, 1F), Tooltip("The amount of fall's distance to start show the fall damage.")] private float _fallDamageShowMultiply;
		[SerializeField, BoxGroup("Control"), Min(0F), Tooltip("The amount of time that Gwamba gets invencible.")] private float _invencibilityTime;
		[SerializeField, BoxGroup("Control"), Range(0F, 1F), Tooltip("The value applied to visual when a hit is taken.")] private float _invencibilityValue;
		[SerializeField, BoxGroup("Control"), Min(0F), Tooltip("The amount of time that Gwamba has to stay before fade.")] private float _timeStep;
		[SerializeField, BoxGroup("Control"), Min(0F), Tooltip("The amount of time taht Gwamba will be stunned after recover.")] private float _stunnedTime;
		[Space(WorldBuild.FIELD_SPACE_LENGTH * 2F)]
		[SerializeField, BoxGroup("Movement"), Tooltip("The sound to play when Gwamba executes the air jump.")] private AudioClip _airJumpSound;
		[SerializeField, BoxGroup("Movement"), Tooltip("The sound to play when Gwamba executes the dash slide.")] private AudioClip _dashSlideSound;
		[SerializeField, BoxGroup("Movement"), Range(1E-1F, 1F), Tooltip("The amount of speed that Gwamba moves yourself.")] private float _movementInputZone;
		[SerializeField, BoxGroup("Movement"), Range(1E-1F, 1F), Tooltip("The amount of speed that Gwamba moves yourself.")] private float _airJumpInputZone;
		[SerializeField, BoxGroup("Movement"), Range(-1E-1F, -1F), Tooltip("The amount of speed that Gwamba moves yourself.")] private float _dashSlideInputZone;
		[SerializeField, BoxGroup("Movement"), Min(0F), Tooltip("The amount of speed that Gwamba moves yourself.")] private float _movementSpeed;
		[SerializeField, BoxGroup("Movement"), Min(0F), Tooltip("The amount of acceleration Gwamba will apply to the movement.")] private float _acceleration;
		[SerializeField, BoxGroup("Movement"), Min(0F), Tooltip("The amount of decceleration Gwamba will apply to the movement.")] private float _decceleration;
		[SerializeField, BoxGroup("Movement"), Min(0F), Tooltip("The amount of power the velocity Gwamba will apply to the movement.")] private float _velocityPower;
		[SerializeField, BoxGroup("Movement"), Min(0F), Tooltip("The amount of friction Gwamba will apply to the end of movement.")] private float _frictionAmount;
		[SerializeField, BoxGroup("Movement"), Min(0F), Tooltip("The amount of speed that the dash will apply.")] private float _dashSpeed;
		[SerializeField, BoxGroup("Movement"), Min(0F), Tooltip("The amount of distance Gwamba will go in both dashes.")] private float _dashDistance;
		[SerializeField, BoxGroup("Movement"), Min(0F), Tooltip("The amount of max speed to increase on the bunny hop.")] private float _velocityBoost;
		[SerializeField, BoxGroup("Movement"), Min(0F), Tooltip("The amount of acceleration/decceleration to increase on the bunny hop.")] private float _potencyBoost;
		[Space(WorldBuild.FIELD_SPACE_LENGTH * 2F)]
		[SerializeField, BoxGroup("Jump"), Tooltip("The sound to play when Gwamba execute a jump.")] private AudioClip _jumpSound;
		[SerializeField, BoxGroup("Jump"), Min(0F), Tooltip("The amount of strenght that Gwamba can Jump.")] private float _jumpStrenght;
		[SerializeField, BoxGroup("Jump"), Min(0F), Tooltip("The amount of strenght that Gwamba can Jump on the air.")] private float _airJumpStrenght;
		[SerializeField, BoxGroup("Jump"), Min(0F), Tooltip("The amount of strenght that will be added on the bunny hop.")] private float _jumpBoost;
		[SerializeField, BoxGroup("Jump"), Min(0F), Tooltip("The amount of time that Gwamba can Jump before thouching ground.")] private float _jumpBufferTime;
		[SerializeField, BoxGroup("Jump"), Min(0F), Tooltip("The amount of time that Gwamba can Jump when get out of the ground.")] private float _jumpCoyoteTime;
		[SerializeField, BoxGroup("Jump"), Range(0F, 1F), Tooltip("The amount of cut that Gwamba's jump will suffer at up.")] private float _jumpCut;
		[Space(WorldBuild.FIELD_SPACE_LENGTH * 2F)]
		[SerializeField, BoxGroup("Attack"), Tooltip("The sound to play when Gwamba attack.")] private AudioClip _attackSound;
		[SerializeField, BoxGroup("Attack"), Tooltip("The sound to play when Gwamba damages something.")] private AudioClip _damageAttackSound;
		[SerializeField, BoxGroup("Attack"), Range(0F, 1F), Tooltip("The amount of velocity to cut during the attack.")] private float _attackVelocityCut;
		[SerializeField, BoxGroup("Attack"), Min(0F), Tooltip("The amount of time to stop the game when hit is given.")] private float _hitStopTime;
		[SerializeField, BoxGroup("Attack"), Min(0F), Tooltip("The amount of time to slow the game when hit is given.")] private float _hitSlowTime;
		[SerializeField, BoxGroup("Attack"), Min(0F), Tooltip("The amount of time the attack will be inactive after attack's hit.")] private float _delayAfterAttack;
		[SerializeField, BoxGroup("Attack"), Tooltip("If Gwamba is attacking in the moment.")] private bool _attackUsage;
		[SerializeField, BoxGroup("Attack"), Tooltip("The buffer moment that Gwamba have to execute a combo attack.")] private bool _comboAttackBuffer;
		private Vector2 Local => (Vector2)transform.position + _collider.offset;
		public static Vector2 Localization => _instance ? _instance.transform.position : Vector2.zero;
		public MessagePath Path => MessagePath.Character;
		private new void Awake()
		{
			base.Awake();
			if (_instance)
			{
				Destroy(gameObject, WorldBuild.MINIMUM_TIME_SPACE_LIMIT);
				return;
			}
			_instance = this;
			_animator = GetComponent<Animator>();
			_rigidbody = GetComponent<Rigidbody2D>();
			_collider = GetComponent<BoxCollider2D>();
			_screenShaker = GetComponent<CinemachineImpulseSource>();
			_gwambaCanvas = GetComponentInChildren<GwambaCanvas>();
			_gwambaDamagers = GetComponentsInChildren<GwambaDamager>();
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
			Sender.Include(this);
		}
		private new void OnDestroy()
		{
			base.OnDestroy();
			if (!_instance || this != _instance)
				return;
			StopAllCoroutines();
			for (ushort i = 0; _gwambaDamagers.Length > i; i++)
			{
				_gwambaDamagers[i].DamagerHurt -= DamagerHurt;
				_gwambaDamagers[i].DamagerStun -= DamagerStun;
				_gwambaDamagers[i].DamagerAttack -= DamagerAttack;
				_gwambaDamagers[i].Alpha = 1F;
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
			Sender.Exclude(this);
		}
		private void OnEnable()
		{
			if (!_instance || this != _instance)
				return;
			if (_gwambaCanvas.RootElement is not null)
				_gwambaCanvas.RootElement.style.display = DisplayStyle.Flex;
			_animator.SetFloat(IsOn, 1F);
			_animator.SetFloat(WalkSpeed, 1F);
			EnableInputs();
		}
		private void OnDisable()
		{
			if (!_instance || this != _instance)
				return;
			_gwambaCanvas.RootElement.style.display = DisplayStyle.None;
			_animator.SetFloat(IsOn, 0F);
			_animator.SetFloat(WalkSpeed, 0F);
			DisableInputs();
		}
		private void EnableInputs()
		{
			_inputController.Commands.Movement.Enable();
			_inputController.Commands.Jump.Enable();
			_inputController.Commands.AttackUse.Enable();
			_inputController.Commands.Interaction.Enable();
			(_rigidbody.linearVelocity, _rigidbody.gravityScale) = (_guardedLinearVelocity, _gravityScale);
		}
		private void DisableInputs()
		{
			_inputController.Commands.Movement.Disable();
			_inputController.Commands.Jump.Disable();
			_inputController.Commands.AttackUse.Disable();
			_inputController.Commands.Interaction.Disable();
			(_guardedLinearVelocity, _rigidbody.linearVelocity, _rigidbody.gravityScale, _movementAction) = (_rigidbody.linearVelocity, Vector2.zero, 0F, 0F);
		}
		private IEnumerator Start()
		{
			if (!_instance || this != _instance)
				yield break;
			yield return StartCoroutine(StartLoad());
			_didStart = true;
			DontDestroyOnLoad(gameObject);
		}
		public IEnumerator StartLoad()
		{
			DisableInputs();
			transform.TurnScaleX(EffectsController.TurnToLeft);
			transform.position = EffectsController.BeginingPosition;
			yield return new WaitWhile(() => SceneInitiator.IsInTrancision());
			if (_animator.GetBool(Death))
			{
				Reanimate();
				OnEnable();
			}
			else
				EnableInputs();
		}
		public IEnumerator Load()
		{
			if (!_instance || _instance != this)
				yield break;
			yield return _gwambaCanvas.LoadHud();
			SaveController.Load(out SaveFile saveFile);
			(_gwambaCanvas.LifeText.text, _gwambaCanvas.CoinText.text) = ($"X {saveFile.Lifes}", $"X {saveFile.Coins}");
			(_vitality, _stunResistance) = ((short)_gwambaCanvas.Vitality.Length, (short)_gwambaCanvas.StunResistance.Length);
			for (ushort i = 0; _gwambaDamagers.Length > i; i++)
			{
				_gwambaDamagers[i].DamagerHurt += DamagerHurt;
				_gwambaDamagers[i].DamagerStun += DamagerStun;
				_gwambaDamagers[i].DamagerAttack += DamagerAttack;
			}
			SceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
			yield return null;
		}
		private void SceneLoaded(Scene scene, LoadSceneMode loadMode)
		{
			if (scene.name == _menuScene)
			{
				Destroy(gameObject);
				return;
			}
			_isHubbyWorld = scene.name == _hubbyWorldScene;
			if (_didStart)
				StartCoroutine(StartLoad());
		}
		private void Reanimate()
		{
			for (ushort i = 0; (_vitality = (short)_gwambaCanvas.Vitality.Length) > i; i++)
			{
				_gwambaCanvas.Vitality[i].style.backgroundColor = _gwambaCanvas.BackgroundColor;
				_gwambaCanvas.Vitality[i].style.borderBottomColor = _gwambaCanvas.BorderColor;
				_gwambaCanvas.Vitality[i].style.borderLeftColor = _gwambaCanvas.BorderColor;
				_gwambaCanvas.Vitality[i].style.borderRightColor = _gwambaCanvas.BorderColor;
				_gwambaCanvas.Vitality[i].style.borderTopColor = _gwambaCanvas.BorderColor;
			}
			for (ushort i = _recoverVitality = 0; _gwambaCanvas.RecoverVitality.Length > i; i++)
				_gwambaCanvas.RecoverVitality[i].style.backgroundColor = _gwambaCanvas.MissingColor;
			for (ushort i = 0; (_stunResistance = (short)_gwambaCanvas.StunResistance.Length) > i; i++)
				_gwambaCanvas.StunResistance[i].style.backgroundColor = _gwambaCanvas.StunResistanceColor;
			for (ushort i = _bunnyHopBoost = 0; _gwambaCanvas.BunnyHop.Length > i; i++)
				_gwambaCanvas.BunnyHop[i].style.backgroundColor = _gwambaCanvas.MissingColor;
			_animator.SetBool(Death, _bunnyHopUsed = _offBunnyHop = false);
		}
		private void MovementInput(InputAction.CallbackContext movement)
		{
			if (!isActiveAndEnabled || _animator.GetBool(Stun))
				return;
			_movementAction = 0F;
			if (Mathf.Abs(movement.ReadValue<Vector2>().x) > _movementInputZone)
				_movementAction = movement.ReadValue<Vector2>().x > 0F ? 1F : -1F;
			if (0F != _movementAction && (!_attackUsage || _comboAttackBuffer))
				if (movement.ReadValue<Vector2>().y > _airJumpInputZone && !_isOnGround && _canAirJump && !_animator.GetBool(AirJump))
				{
					_animator.SetBool(AirJump, !(_canAirJump = false));
					_animator.SetBool(AttackAirJump, _comboAttackBuffer);
					transform.TurnScaleX(_localOfAny.z = _movementAction);
					(_isJumping, _rigidbody.linearVelocity) = (false, Vector2.zero);
					_rigidbody.AddForceX((_airJumpStrenght + BunnyHop(_jumpBoost)) * _localOfAny.z * _rigidbody.mass, ForceMode2D.Impulse);
					_rigidbody.AddForceY((_airJumpStrenght + BunnyHop(_jumpBoost)) * _rigidbody.mass, ForceMode2D.Impulse);
					EffectsController.SoundEffect(_airJumpSound, transform.position);
					if (_comboAttackBuffer)
						StartAttackSound();
				}
				else if (movement.ReadValue<Vector2>().y < _dashSlideInputZone && _isOnGround && !_animator.GetBool(DashSlide))
				{
					_animator.SetBool(DashSlide, true);
					_animator.SetBool(AttackSlide, _comboAttackBuffer);
					transform.TurnScaleX(_localOfAny.z = _movementAction);
					_localOfAny.x = transform.position.x;
					EffectsController.SoundEffect(_dashSlideSound, transform.position);
					if (_comboAttackBuffer)
						StartAttackSound();
				}
		}
		private void FootStepSound(float stepPositionX)
		{
			_localOfSurface.Set(transform.position.x + stepPositionX, transform.position.y - _collider.bounds.extents.y);
			EffectsController.SurfaceSound(_localOfSurface);
		}
		private void JumpInput(InputAction.CallbackContext jump)
		{
			if (jump.started)
			{
				_lastJumpTime = _jumpBufferTime;
				if (!_isOnGround && !_bunnyHopUsed && !_animator.GetBool(AirJump))
				{
					_bunnyHopUsed = true;
					if (_gwambaCanvas.BunnyHop.Length <= (_bunnyHopBoost += 1))
						_bunnyHopBoost = (ushort)_gwambaCanvas.BunnyHop.Length;
				}
			}
			else if (jump.canceled && _isJumping && 0F < _rigidbody.linearVelocityY)
			{
				(_isJumping, _lastJumpTime) = (false, 0F);
				_rigidbody.AddForceY(_rigidbody.linearVelocityY * _jumpCut * -_rigidbody.mass, ForceMode2D.Impulse);
			}
		}
		private void AttackUseInput(InputAction.CallbackContext attackUse)
		{
			if ((0F < _attackDelay && !_comboAttackBuffer) || _animator.GetBool(AirJump) || _animator.GetBool(DashSlide) || !isActiveAndEnabled || _animator.GetBool(Stun))
				return;
			if (attackUse.started && !_attackUsage)
				_animator.SetTrigger(Attack);
			if (attackUse.canceled && _comboAttackBuffer)
				_animator.SetTrigger(AttackCombo);
		}
		private void StartAttackSound() => EffectsController.SoundEffect(_attackSound, transform.position);
		private void InteractionInput(InputAction.CallbackContext interaction)
		{
			if (!_isOnGround || 0F != _movementAction || !isActiveAndEnabled || _animator.GetBool(AirJump) || _animator.GetBool(DashSlide) || _animator.GetBool(Stun))
				return;
			for (int i = Physics2D.OverlapCollider(_collider, _interactionFilter, _interactions); 0 < i; i--)
				if (_interactions[i - 1].TryGetComponent<IInteractable>(out _))
				{
					_interactionsPerObject = _interactions[i - 1].GetComponents<IInteractable>();
					for (ushort j = 0; _interactionsPerObject.Length > j; j++)
						_interactionsPerObject[j]?.Interaction();
					return;
				}
		}
		public bool DamagerHurt(ushort damage)
		{
			if (_invencibility || 0 >= damage)
				return false;
			EffectsController.SoundEffect(_hurtSound, transform.position);
			_vitality -= (short)damage;
			for (ushort i = (ushort)_gwambaCanvas.Vitality.Length; (0 <= _vitality ? _vitality : 0) < i; i--)
			{
				_gwambaCanvas.Vitality[i - 1].style.backgroundColor = _gwambaCanvas.MissingColor;
				_gwambaCanvas.Vitality[i - 1].style.borderBottomColor = _gwambaCanvas.MissingColor;
				_gwambaCanvas.Vitality[i - 1].style.borderLeftColor = _gwambaCanvas.MissingColor;
				_gwambaCanvas.Vitality[i - 1].style.borderRightColor = _gwambaCanvas.MissingColor;
				_gwambaCanvas.Vitality[i - 1].style.borderTopColor = _gwambaCanvas.MissingColor;
			}
			(_timerOfInvencibility, _invencibility) = (_invencibilityTime, true);
			if (0 >= _vitality)
			{
				EffectsController.SoundEffect(_deathSound, transform.position);
				SaveController.Load(out SaveFile saveFile);
				_gwambaCanvas.LifeText.text = $"X {saveFile.Lifes -= 1}";
				SaveController.WriteSave(saveFile);
				_invencibility = false;
				for (ushort i = 0; _gwambaDamagers.Length > i; i++)
					_gwambaDamagers[i].Alpha = 1F;
				OnDisable();
				StopAllCoroutines();
				_animator.SetBool(Idle, false);
				_animator.SetBool(Walk, false);
				_animator.SetBool(Jump, false);
				_animator.SetBool(Fall, false);
				_animator.SetBool(AirJump, false);
				_animator.SetBool(DashSlide, false);
				_animator.SetBool(AttackJump, false);
				_animator.SetBool(AttackAirJump, false);
				_animator.SetBool(AttackSlide, false);
				_animator.SetBool(Stun, false);
				_animator.SetBool(Death, true);
				_rigidbody.gravityScale = _fallGravityMultiply * _gravityScale;
				_sender.SetToggle(false);
				_sender.SetFormat(MessageFormat.State);
				_sender.Send(MessagePath.Hud);
				_sender.SetFormat(MessageFormat.Event);
				_sender.Send(MessagePath.Hud);
				_sender.SetFormat(MessageFormat.None);
				_sender.Send(MessagePath.Enemy);
			}
			return true;
		}
		public void DamagerStun(ushort stunStrength, float stunTime)
		{
			_stunResistance -= (short)stunStrength;
			for (ushort i = (ushort)_gwambaCanvas.StunResistance.Length; (0 <= _stunResistance ? _stunResistance : 0) > i; i--)
				_gwambaCanvas.StunResistance[i - 1].style.backgroundColor = _gwambaCanvas.MissingColor;
			if (0 >= _stunResistance && !_animator.GetBool(Death))
			{
				_animator.SetBool(AirJump, false);
				_animator.SetBool(DashSlide, false);
				_animator.SetBool(AttackJump, false);
				_animator.SetBool(AttackAirJump, false);
				_animator.SetBool(AttackSlide, false);
				_animator.SetBool(Stun, !(_invencibility = false));
				for (ushort i = 0; _gwambaDamagers.Length > i; i++)
					_gwambaDamagers[i].Alpha = 1F;
				_stunTimer = stunTime;
				for (ushort i = 0; (_stunResistance = (short)_gwambaCanvas.StunResistance.Length) > i; i++)
					_gwambaCanvas.StunResistance[i].style.backgroundColor = _gwambaCanvas.StunResistanceColor;
				EffectsController.SoundEffect(_stunSound, transform.position);
				DisableInputs();
			}
		}
		private void DamagerAttack(GwambaDamager gwambaDamager, IDestructible destructible)
		{
			if (destructible.Hurt(gwambaDamager.AttackDamage))
			{
				EffectsController.SoundEffect(_damageAttackSound, gwambaDamager.transform.position);
				destructible.Stun(gwambaDamager.AttackDamage, gwambaDamager.StunTime);
				_screenShaker.ImpulseDefinition.ImpulseDuration = gwambaDamager.AttackShakeTime;
				_screenShaker.GenerateImpulse(gwambaDamager.AttackShake);
				EffectsController.HitStop(_hitStopTime, _hitSlowTime);
				gwambaDamager.damagedes.Add(destructible);
				_attackDelay = _delayAfterAttack;
				for (ushort amount = 0; (destructible.Health <= 0 ? gwambaDamager.AttackDamage + 1 : gwambaDamager.AttackDamage) > amount; amount++)
					if (_gwambaCanvas.RecoverVitality.Length <= _recoverVitality && _gwambaCanvas.Vitality.Length > _vitality)
					{
						_vitality += 1;
						for (ushort i = 0; _vitality > i; i++)
						{
							_gwambaCanvas.Vitality[i].style.backgroundColor = _gwambaCanvas.BackgroundColor;
							_gwambaCanvas.Vitality[i].style.borderBottomColor = _gwambaCanvas.BorderColor;
							_gwambaCanvas.Vitality[i].style.borderLeftColor = _gwambaCanvas.BorderColor;
							_gwambaCanvas.Vitality[i].style.borderRightColor = _gwambaCanvas.BorderColor;
							_gwambaCanvas.Vitality[i].style.borderTopColor = _gwambaCanvas.BorderColor;
						}
						for (ushort i = _recoverVitality = 0; _gwambaCanvas.RecoverVitality.Length > i; i++)
							_gwambaCanvas.RecoverVitality[i].style.backgroundColor = _gwambaCanvas.MissingColor;
						_stunResistance = (short)(_stunResistance < _gwambaCanvas.StunResistance.Length ? _stunResistance + 1 : _stunResistance);
						for (ushort i = 0; _stunResistance > i; i++)
							_gwambaCanvas.StunResistance[i].style.backgroundColor = _gwambaCanvas.StunResistanceColor;
					}
					else if (_gwambaCanvas.RecoverVitality.Length > _recoverVitality)
					{
						_recoverVitality += 1;
						for (ushort i = 0; _recoverVitality > i; i++)
							_gwambaCanvas.RecoverVitality[i].style.backgroundColor = _gwambaCanvas.BorderColor;
					}
			}
		}
		private void Update()
		{
			if (_invencibility)
			{
				_invencibility = 0F < (_timerOfInvencibility -= Time.deltaTime);
				if (_invencibility && 0F >= (_showInvencibilityTimer -= Time.deltaTime))
				{
					for (ushort i = 0; _gwambaDamagers.Length > i; i++)
						_gwambaDamagers[i].Alpha = _gwambaDamagers[i].Alpha >= 1F ? _invencibilityValue : 1F;
					_showInvencibilityTimer = _timeStep;
				}
				if (!_invencibility)
					for (ushort i = 0; _gwambaDamagers.Length > i; i++)
						_gwambaDamagers[i].Alpha = 1F;
			}
			if (_animator.GetBool(Stun))
				if (0F >= (_stunTimer -= Time.deltaTime))
				{
					_animator.SetBool(Stun, !(_invencibility = true));
					EnableInputs();
				}
			if (0F < _fadeTimer)
				if (0F >= (_fadeTimer -= Time.deltaTime))
					(_gwambaCanvas.FallDamageText.style.opacity, _gwambaCanvas.FallDamageText.text) = (0F, $"X 0");
			if (!_animator.GetBool(DashSlide) && !_isOnGround && Mathf.Abs(_rigidbody.linearVelocityY) != 0F && !_downStairs && (0F < _lastGroundedTime || 0F < _lastJumpTime))
				(_lastGroundedTime, _lastJumpTime) = (_lastGroundedTime - Time.deltaTime, _lastJumpTime - Time.deltaTime);
			if (0F < _attackDelay)
				if (0F >= (_attackDelay -= Time.deltaTime))
					for (ushort i = 0; _gwambaDamagers.Length > i; i++)
						_gwambaDamagers[i].damagedes.Clear();
		}
		private float BunnyHop(float callBackValue) => 0 < _bunnyHopBoost ? _bunnyHopBoost * callBackValue : 0F;
		private void FixedUpdate()
		{
			if (!_instance || _instance != this)
				return;
			if (_animator.GetBool(DashSlide))
				if (Mathf.Abs(transform.position.x - _localOfAny.x) > _dashDistance || !_isOnGround || _isJumping || _animator.GetBool(Stun) || _animator.GetBool(Death))
				{
					_animator.SetBool(DashSlide, false);
					_animator.SetBool(AttackSlide, false);
				}
				else
					_rigidbody.linearVelocityX = _dashSpeed * _localOfAny.z;
			else
			{
				if (!_isOnGround && !_downStairs && Mathf.Abs(_rigidbody.linearVelocityY) > _minimumVelocity && !_animator.GetBool(AirJump))
				{
					if (_animator.GetBool(Idle))
						_animator.SetBool(Idle, false);
					if (_animator.GetBool(Walk))
						_animator.SetBool(Walk, false);
					if (!_animator.GetBool(Jump) && 0F < _rigidbody.linearVelocityY)
						_animator.SetBool(Jump, true);
					else if (_animator.GetBool(Jump) && 0F > _rigidbody.linearVelocityY)
						_animator.SetBool(Jump, false);
					if (!_animator.GetBool(Fall) && 0F > _rigidbody.linearVelocityY)
						_animator.SetBool(Fall, true);
					else if (_animator.GetBool(Fall) && 0F < _rigidbody.linearVelocityY)
						_animator.SetBool(Fall, false);
					if (_animator.GetBool(AttackJump) && 0F > _rigidbody.linearVelocityY)
						_animator.SetBool(AttackJump, false);
					if (_animator.GetBool(Fall))
					{
						if (_rigidbody.gravityScale < _fallGravityMultiply * _gravityScale)
							_rigidbody.gravityScale = _fallGravityMultiply * _gravityScale;
						if (_fallStarted && !_isHubbyWorld)
						{
							_fallDamage = Mathf.Abs(_startOfFall - transform.position.y);
							if (_fallDamage >= _fallDamageDistance * _fallDamageShowMultiply)
								(_gwambaCanvas.FallDamageText.style.opacity, _gwambaCanvas.FallDamageText.text) = (1F, $"X {_fallDamage / _fallDamageDistance:F1}");
							else if (!_invencibility)
								(_gwambaCanvas.FallDamageText.style.opacity, _gwambaCanvas.FallDamageText.text) = (0F, $"X 0");
						}
						else if (!_isHubbyWorld)
							(_fallStarted, _startOfFall, _fallDamage) = (true, transform.position.y, 0F);
					}
					else
					{
						if (!_invencibility)
							(_gwambaCanvas.FallDamageText.style.opacity, _gwambaCanvas.FallDamageText.text) = (0F, $"X 0");
						if (_rigidbody.gravityScale > _gravityScale)
							_rigidbody.gravityScale = _gravityScale;
						if (_fallStarted)
							(_fallStarted, _fallDamage) = (false, 0F);
					}
					if (_attackUsage && !_animator.GetBool(AttackJump))
						_rigidbody.linearVelocityY *= _attackVelocityCut;
				}
				if (_animator.GetBool(AirJump))
					if (_isJumping || _animator.GetBool(Stun) || _animator.GetBool(Death))
					{
						_animator.SetBool(AirJump, false);
						_animator.SetBool(AttackAirJump, false);
					}
					else
						_lastGroundedTime = _jumpCoyoteTime;
				else
				{
					_localOfAny.x = _longJumping ? _dashSpeed : _movementSpeed + BunnyHop(_velocityBoost);
					_localOfAny.y = _localOfAny.x * _movementAction - _rigidbody.linearVelocityX;
					_localOfAny.z = (Mathf.Abs(_localOfAny.x * _movementAction) > 0F ? _acceleration : _decceleration) + BunnyHop(_potencyBoost);
					_rigidbody.AddForceX(Mathf.Pow(Mathf.Abs(_localOfAny.y) * _localOfAny.z, _velocityPower) * Mathf.Sign(_localOfAny.y) * _rigidbody.mass);
					if (0F != _movementAction && !_attackUsage)
					{
						if (Mathf.Abs(_rigidbody.linearVelocityX) > _minimumVelocity)
							transform.TurnScaleX(0F > _rigidbody.linearVelocityX);
						else if (Mathf.Abs(_rigidbody.linearVelocityX) <= _minimumVelocity)
							transform.TurnScaleX(_movementAction);
						if (_isOnGround)
							_animator.SetFloat(WalkSpeed, Mathf.Abs(_rigidbody.linearVelocityX) <= _minimumVelocity ? 1F : Mathf.Abs(_rigidbody.linearVelocityX) / _localOfAny.x);
					}
				}
				if (_attackUsage && !_animator.GetBool(AttackAirJump))
					_rigidbody.linearVelocityX *= _attackVelocityCut;
				if (_isOnGround && 0F == _movementAction && Mathf.Abs(_rigidbody.linearVelocityX) > _minimumVelocity)
				{
					_localOfAny.x = Mathf.Min(Mathf.Abs(_rigidbody.linearVelocityX), Mathf.Abs(_frictionAmount)) * Mathf.Sign(_rigidbody.linearVelocityX);
					_rigidbody.AddForceX(-_localOfAny.x * _rigidbody.mass, ForceMode2D.Impulse);
					_animator.SetFloat(WalkSpeed, Mathf.Abs(_rigidbody.linearVelocityX) / (_longJumping ? _dashSpeed : _movementSpeed + BunnyHop(_velocityBoost)));
				}
			}
			if (!_isJumping && 0F < _lastJumpTime && 0F < _lastGroundedTime)
			{
				_animator.SetBool(AttackJump, _comboAttackBuffer);
				(_isJumping, _longJumping, _rigidbody.gravityScale, _rigidbody.linearVelocityY) = (!(_bunnyHopUsed = false), _animator.GetBool(DashSlide), _gravityScale, 0F);
				if (0 < _bunnyHopBoost)
				{
					_offBunnyHop = true;
					for (ushort i = 0; _bunnyHopBoost > i; i++)
						_gwambaCanvas.BunnyHop[i].style.backgroundColor = _gwambaCanvas.BunnyHopColor;
				}
				_rigidbody.AddForceY((_jumpStrenght + BunnyHop(_jumpBoost)) * _rigidbody.mass, ForceMode2D.Impulse);
				EffectsController.SoundEffect(_jumpSound, transform.position);
				if (_comboAttackBuffer)
					StartAttackSound();
			}
			(_isOnGround, _offGround) = (_downStairs = false, !_isOnGround);
		}
		private void OnCollisionStay2D(Collision2D collision)
		{
			if (!_instance || this != _instance || WorldBuild.SCENE_LAYER != collision.gameObject.layer)
				return;
			if (_animator.GetBool(AirJump) || _animator.GetBool(DashSlide))
			{
				_groundContacts.Clear();
				collision.GetContacts(_groundContacts);
				_localOfStart.Set(Local.x + _collider.bounds.extents.x * _localOfAny.z, Local.y);
				_localOfEnd.Set(WorldBuild.SNAP_LENGTH, _collider.size.y);
				_groundContacts.RemoveAll(contact => contact.point.OutsideRectangle(_localOfStart, _localOfEnd));
				if (0 < _groundContacts.Count)
				{
					_animator.SetBool(AirJump, false);
					_animator.SetBool(DashSlide, false);
					_animator.SetBool(AttackAirJump, false);
					_animator.SetBool(AttackSlide, false);
					EffectsController.SurfaceSound(_groundContacts[0].point);
				}
			}
			_groundContacts.Clear();
			collision.GetContacts(_groundContacts);
			_localOfStart.Set(Local.x, Local.y - _collider.bounds.extents.y);
			_localOfEnd.Set(_collider.size.x, WorldBuild.SNAP_LENGTH);
			_groundContacts.RemoveAll(contact => contact.point.OutsideRectangle(_localOfStart, _localOfEnd));
			if (_isOnGround = 0 < _groundContacts.Count)
			{
				if (_animator.GetBool(AirJump))
				{
					_animator.SetBool(AirJump, false);
					_animator.SetBool(AttackAirJump, false);
					EffectsController.SurfaceSound(_groundContacts[0].point);
				}
				if (!_animator.GetBool(Idle) && (0F == _movementAction || Mathf.Abs(_rigidbody.linearVelocityX) <= _minimumVelocity || _animator.GetBool(Fall)))
					_animator.SetBool(Idle, true);
				else if (_animator.GetBool(Idle) || Mathf.Abs(_rigidbody.linearVelocityX) > _minimumVelocity)
					_animator.SetBool(Idle, false);
				if (!_animator.GetBool(Walk) && 0F != _movementAction)
					_animator.SetBool(Walk, true);
				else if (_animator.GetBool(Walk) && 0F == _movementAction)
					_animator.SetBool(Walk, false);
				if (_offGround)
				{
					_offGround = false;
					if (_animator.GetBool(Jump))
						_animator.SetBool(Jump, false);
					if (_animator.GetBool(Fall))
						_animator.SetBool(Fall, false);
					(_lastGroundedTime, _canAirJump, _bunnyHopBoost) = (_jumpCoyoteTime, !(_longJumping = _isJumping = false), 0F < _lastJumpTime ? _bunnyHopBoost : (ushort)0);
					if (0 >= _bunnyHopBoost && _offBunnyHop)
					{
						_bunnyHopUsed = _offBunnyHop = false;
						for (ushort i = 0; _gwambaCanvas.BunnyHop.Length > i; i++)
							_gwambaCanvas.BunnyHop[i].style.backgroundColor = _gwambaCanvas.MissingColor;
					}
					if (_fallStarted && 0 >= _bunnyHopBoost && !_isHubbyWorld)
					{
						_screenShaker.ImpulseDefinition.ImpulseDuration = _fallShakeTime;
						_screenShaker.GenerateImpulse(_fallDamage / _fallDamageDistance * _fallShake);
						DamagerHurt((ushort)Mathf.FloorToInt(_fallDamage / _fallDamageDistance));
						_localOfSurface.Set(transform.position.x, transform.position.y - _collider.bounds.extents.y);
						EffectsController.SurfaceSound(_localOfSurface);
						(_fallStarted, _fallDamage) = (false, 0F);
						if (_invencibility && 0F >= _fadeTimer)
							_fadeTimer = _timeToFadeShow;
						else
							(_gwambaCanvas.FallDamageText.style.opacity, _gwambaCanvas.FallDamageText.text) = (0F, $"X 0");
					}
				}
				if (!_animator.GetBool(AirJump) && !_animator.GetBool(DashSlide) && 0F != _movementAction)
					if (Mathf.Abs(_rigidbody.linearVelocityX) <= _minimumVelocity)
					{
						_groundContacts.Clear();
						collision.GetContacts(_groundContacts);
						_localOfStart.Set(Local.x + _collider.bounds.extents.x * (0F < transform.localScale.x ? 1F : -1F), Local.y - (_collider.size.y - _upStairsLength) / 2F);
						_localOfEnd.Set(WorldBuild.SNAP_LENGTH, _upStairsLength);
						_groundContacts.RemoveAll(contact => contact.point.OutsideRectangle(_localOfStart, _localOfEnd));
						if (0 < _groundContacts.Count)
						{
							_localOfAny.x = (_collider.bounds.extents.x + WorldBuild.SNAP_LENGTH / 2F) * (0F < transform.localScale.x ? 1F : -1F);
							_localOfAny.y = _localOfStart.y + _localOfEnd.y / 2F;
							_localOfAny.z = _localOfStart.y - _localOfEnd.y / 2F;
							_localOfStart.Set(Local.x + _localOfAny.x, Local.y + _collider.bounds.extents.y);
							_localOfEnd.Set(Local.x + _localOfAny.x, Local.y - _collider.bounds.extents.y);
							_castHit = Physics2D.Linecast(_localOfStart, _localOfEnd, WorldBuild.SCENE_LAYER_MASK);
							if (_castHit && _localOfAny.y >= _castHit.point.y && _localOfAny.z <= _castHit.point.y)
							{
								_localOfAny.y = Mathf.Abs(_castHit.point.y - (transform.position.y - _collider.bounds.extents.y));
								_localOfSurface.Set(transform.position.x + WorldBuild.SNAP_LENGTH * (0F < transform.localScale.x ? 1F : -1F), transform.position.y + _localOfAny.y);
								transform.position = _localOfSurface;
								_rigidbody.linearVelocityX = _movementSpeed * _movementAction;
							}
						}
					}
					else if (0F >= _lastJumpTime)
					{
						_localOfAny.x = Local.x - (_collider.bounds.extents.x - WorldBuild.SNAP_LENGTH / 2F * _downStairsDistance) * (0F < transform.localScale.x ? 1F : -1F);
						_localOfAny.y = WorldBuild.SNAP_LENGTH * _downStairsDistance;
						if (_groundContacts.TrueForAll(contact => _localOfAny.x + _localOfAny.y / 2F >= contact.point.x && _localOfAny.x - _localOfAny.y / 2F <= contact.point.x))
						{
							_localOfAny.x = (0F < transform.localScale.x ? 1F : -1F);
							_localOfStart.Set(Local.x - (_collider.bounds.extents.x - WorldBuild.SNAP_LENGTH * _downStairsDistance) * _localOfAny.x, Local.y - _collider.bounds.extents.y);
							if (_downStairs = _castHit = Physics2D.Raycast(_localOfStart, -transform.up, WorldBuild.SNAP_LENGTH + 1F, WorldBuild.SCENE_LAYER_MASK))
							{
								_localOfSurface.Set(transform.position.x + WorldBuild.SNAP_LENGTH * _downStairsDistance * _localOfAny.x, transform.position.y - _castHit.distance);
								transform.position = _localOfSurface;
							}
						}
					}
			}
		}
		private void OnTriggerEnter2D(Collider2D other)
		{
			if (other.TryGetComponent<ICollectable>(out var collectable))
			{
				collectable.Collect();
				SaveController.Load(out SaveFile saveFile);
				(_gwambaCanvas.LifeText.text, _gwambaCanvas.CoinText.text) = ($"X {saveFile.Lifes}", $"X {saveFile.Coins}");
			}
		}
		public static bool EqualObject(params GameObject[] othersObjects)
		{
			if (_instance)
				for (ushort i = 0; othersObjects.Length > i; i++)
					if (_instance.gameObject == othersObjects[i])
						return true;
			return false;
		}
		public void Receive(MessageData message)
		{
			if (MessageFormat.Event == message.Format && message.ToggleValue.HasValue)
				if (message.ToggleValue.Value)
				{
					Reanimate();
					transform.TurnScaleX(EffectsController.TurnToLeft);
				}
				else if (message.AdditionalData is Vector2 position)
					transform.position = position;
			if (MessageFormat.State == message.Format && message.ToggleValue.HasValue && message.ToggleValue.Value)
			{
				OnEnable();
				(_timerOfInvencibility, _invencibility) = (_invencibilityTime, true);
			}
		}
	};
};
