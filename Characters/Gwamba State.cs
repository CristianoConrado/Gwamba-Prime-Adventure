using NaughtyAttributes;
using System.Collections.Generic;
using System.Threading;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
namespace GwambaPrimeAdventure.Character
{
	[DisallowMultipleComponent, SelectionBase, RequireComponent( typeof( SortingGroup ), typeof( CircleCollider2D ) )]
	internal abstract class GwambaState<StateT> : StateController, IConnector where StateT : GwambaState<StateT>
	{
		protected GwambaCanvas _gwambaCanvas;
		protected GwambaDamager[] _gwambaDamagers;
		protected Animator _animator;
		protected Rigidbody2D _rigidbody;
		protected BoxCollider2D _collider;
		protected CinemachineImpulseSource _screenShaker;
		protected InputController _inputController;
		protected readonly Sender _sender = Sender.Create();
		protected readonly Collider2D[] _interactions = new Collider2D[ (uint) WorldBuild.PIXELS_PER_UNIT ];
		protected readonly List<ContactPoint2D> _groundContacts = new List<ContactPoint2D>( (int) WorldBuild.PIXELS_PER_UNIT );
		protected IInteractable[] _interactionsPerObject;
		protected CancellationToken _destroyToken;
		protected Vector2
			_localAtStart = Vector2.zero,
			_localAtEnd = Vector2.zero,
			_localAtSurface = Vector2.zero,
			_localAtLinearVelocity = Vector2.zero,
			_beginingPosition = Vector2.zero;
		protected Vector3 _localAtAny = Vector3.zero;
		protected RaycastHit2D _castHit;
		protected readonly ContactFilter2D _interactionFilter = new()
		{
			layerMask = WorldBuild.SYSTEM_LAYER_MASK + WorldBuild.CHARACTER_LAYER_MASK + WorldBuild.SCENE_LAYER_MASK + WorldBuild.ITEM_LAYER_MASK,
			useLayerMask = true,
			useTriggers = true
		};
		protected short
			_vitality = 0,
			_stunResistance = 0;
		protected ushort
			_recoverVitality = 0,
			_bunnyHopBoost = 0;
		protected float
			_timerOfInvencibility = 0F,
			_showInvencibilityTimer = 0F,
			_stunTimer = 0F,
			_fadeTimer = 0F,
			_walkValue = 0F,
			_lastGroundedTime = 0F,
			_lastJumpTime = 0F,
			_startOfFall = 0F,
			_fallDamage = 0F,
			_attackDelay = 0F;
		protected readonly float _minimumVelocity = WorldBuild.MINIMUM_TIME_SPACE_LIMIT * 10F;
		protected bool
			_isHubbyWorld = false,
			_turnLeft = false,
			_didStart = false,
			_isOnGround = false,
			_offGround = false,
			_downStairs = false,
			_isJumping = false,
			_canAirJump = true,
			_longJumping = false,
			_bunnyHopUsed = false,
			_offBunnyHop = false,
			_fallStarted = false,
			_invencibility = false,
			_reloadTransform = false,
			_deathLoad = false;
		protected readonly int
			IsOn = Animator.StringToHash( nameof( IsOn ) ),
			Idle = Animator.StringToHash( nameof( Idle ) ),
			Walk = Animator.StringToHash( nameof( Walk ) ),
			WalkSpeed = Animator.StringToHash( nameof( WalkSpeed ) ),
			Jump = Animator.StringToHash( nameof( Jump ) ),
			Fall = Animator.StringToHash( nameof( Fall ) ),
			AirJump = Animator.StringToHash( nameof( AirJump ) ),
			DashSlide = Animator.StringToHash( nameof( DashSlide ) ),
			Attack = Animator.StringToHash( nameof( Attack ) ),
			AttackCombo = Animator.StringToHash( nameof( AttackCombo ) ),
			AttackJump = Animator.StringToHash( nameof( AttackJump ) ),
			AttackAirJump = Animator.StringToHash( nameof( AttackAirJump ) ),
			AttackSlide = Animator.StringToHash( nameof( AttackSlide ) ),
			Stun = Animator.StringToHash( nameof( Stun ) ),
			Death = Animator.StringToHash( nameof( Death ) );
		[field: SerializeField, BoxGroup( "Control" ), Tooltip( "The scene of the hubby world." ), Space( WorldBuild.FIELD_SPACE_LENGTH * 2F )]
		protected SceneField HubbyWorldScene { get; private set; }
		[field: SerializeField, BoxGroup( "Control" ), Tooltip( "The scene of the menu." )] protected SceneField MenuScene { get; private set; }
		[field: SerializeField, BoxGroup( "Control" ), Tooltip( "The sound to play when Gwamba gets hurt." )] protected AudioClip HurtSound { get; private set; }
		[field: SerializeField, BoxGroup( "Control" ), Tooltip( "The sound to play when Gwamba gets stunned." )] protected AudioClip StunSound { get; private set; }
		[field: SerializeField, BoxGroup( "Control" ), Tooltip( "The sound to play when Gwamba die." )] protected AudioClip DeathSound { get; private set; }
		[field: SerializeField, BoxGroup( "Control" ), Tooltip( "The start position where Gwamba will be on the scene." )] protected Vector2 StartPosition { get; private set; }
		[field: SerializeField, BoxGroup( "Control" ), Tooltip( "The velocity of the shake on the fall." )] protected Vector2 FallShake { get; private set; }
		[field: SerializeField, BoxGroup( "Control" ), Tooltip( "The amount of distance to get down stairs." )] protected ushort DownStairsDistance { get; private set; }
		[field: SerializeField, BoxGroup( "Control" ), Tooltip( "The size of the detector to climb the stairs." )] protected float UpStairsLength { get; private set; }
		[field: SerializeField, BoxGroup( "Control" ), Tooltip( "The gravity applied to Gwamba." )] protected float GravityScale { get; private set; }
		[field: SerializeField, BoxGroup( "Control" ), Min( 0F ), Tooltip( "The amount of time the fall screen shake will be applied." )] protected float FallShakeTime { get; private set; }
		[field: SerializeField, BoxGroup( "Control" ), Min( 0F ), Tooltip( "The amount of gravity to multiply on the fall." )] protected float FallGravityMultiply { get; private set; }
		[field: SerializeField, BoxGroup( "Control" ), Min( 0F ), Tooltip( "The amount of fall's distance to take damage." )] protected float FallDamageDistance { get; private set; }
		[field: SerializeField, BoxGroup( "Control" ), Min( 0F ), Tooltip( "The amount of time to fade the show of fall's damage." )] protected float TimeToFadeShow { get; private set; }
		[field: SerializeField, BoxGroup( "Control" ), Range( 0F, 1F ), Tooltip( "The amount of fall's distance to start show the fall damage." )] protected float FallDamageShowMultiply { get; private set; }
		[field: SerializeField, BoxGroup( "Control" ), Min( 0F ), Tooltip( "The amount of time that Gwamba gets invencible." )] protected float InvencibilityTime { get; private set; }
		[field: SerializeField, BoxGroup( "Control" ), Range( 0F, 1F ), Tooltip( "The value applied to visual when a hit is taken." )] protected float InvencibilityValue { get; private set; }
		[field: SerializeField, BoxGroup( "Control" ), Min( 0F ), Tooltip( "The amount of time that Gwamba has to stay before fade." )] protected float TimeStep { get; private set; }
		[field: SerializeField, BoxGroup( "Control" ), Min( 0F ), Tooltip( "The amount of time taht Gwamba will be stunned after recover." )] protected float StunnedTime { get; private set; }
		[field: SerializeField, BoxGroup( "Control" ), Tooltip( "If Gwamba will be facing left on the begining of the scene." )] protected bool TurnToLeft { get; private set; }
		[field: SerializeField, BoxGroup( "Movement" ), Tooltip( "The sound to play when Gwamba executes the air jump." ), Space( WorldBuild.FIELD_SPACE_LENGTH * 2F )]
		protected AudioClip AirJumpSound { get; private set; }
		[field: SerializeField, BoxGroup( "Movement" ), Tooltip( "The sound to play when Gwamba executes the dash slide." )] protected AudioClip DashSlideSound { get; private set; }
		[field: SerializeField, BoxGroup( "Movement" ), Range( 1E-1F, 1F ), Tooltip( "The amount of speed that Gwamba moves yourself." )] protected float MovementInputZone { get; private set; }
		[field: SerializeField, BoxGroup( "Movement" ), Range( 1E-1F, 1F ), Tooltip( "The amount of speed that Gwamba moves yourself." )] protected float AirJumpInputZone { get; private set; }
		[field: SerializeField, BoxGroup( "Movement" ), Range( -1E-1F, -1F ), Tooltip( "The amount of speed that Gwamba moves yourself." )] protected float DashSlideInputZone { get; private set; }
		[field: SerializeField, BoxGroup( "Movement" ), Min( 0F ), Tooltip( "The amount of speed that Gwamba moves yourself." )] protected float MovementSpeed { get; private set; }
		[field: SerializeField, BoxGroup( "Movement" ), Min( 0F ), Tooltip( "The amount of acceleration Gwamba will apply to the movement." )] protected float Acceleration { get; private set; }
		[field: SerializeField, BoxGroup( "Movement" ), Min( 0F ), Tooltip( "The amount of decceleration Gwamba will apply to the movement." )] protected float Decceleration { get; private set; }
		[field: SerializeField, BoxGroup( "Movement" ), Min( 0F ), Tooltip( "The amount of power the velocity Gwamba will apply to the movement." )] protected float VelocityPower { get; private set; }
		[field: SerializeField, BoxGroup( "Movement" ), Min( 0F ), Tooltip( "The amount of friction Gwamba will apply to the end of movement." )] protected float FrictionAmount { get; private set; }
		[field: SerializeField, BoxGroup( "Movement" ), Min( 0F ), Tooltip( "The amount of speed that the dash will apply." )] protected float DashSpeed { get; private set; }
		[field: SerializeField, BoxGroup( "Movement" ), Min( 0F ), Tooltip( "The amount of distance Gwamba will go in both dashes." )] protected float DashDistance { get; private set; }
		[field: SerializeField, BoxGroup( "Movement" ), Min( 0F ), Tooltip( "The amount of max speed to increase on the bunny hop." )] protected float VelocityBoost { get; private set; }
		[field: SerializeField, BoxGroup( "Movement" ), Min( 0F ), Tooltip( "The amount of acceleration/decceleration to increase on the bunny hop." )] protected float PotencyBoost { get; private set; }
		[field: SerializeField, BoxGroup( "Jump" ), Tooltip( "The sound to play when Gwamba execute a jump." ), Space( WorldBuild.FIELD_SPACE_LENGTH * 2F )]
		protected AudioClip JumpSound { get; private set; }
		[field: SerializeField, BoxGroup( "Jump" ), Min( 0F ), Tooltip( "The amount of strenght that Gwamba can Jump." )] protected float JumpStrenght { get; private set; }
		[field: SerializeField, BoxGroup( "Jump" ), Min( 0F ), Tooltip( "The amount of strenght that Gwamba can Jump on the air." )] protected float AirJumpStrenght { get; private set; }
		[field: SerializeField, BoxGroup( "Jump" ), Min( 0F ), Tooltip( "The amount of strenght that will be added on the bunny hop." )] protected float JumpBoost { get; private set; }
		[field: SerializeField, BoxGroup( "Jump" ), Min( 0F ), Tooltip( "The amount of time that Gwamba can Jump before thouching ground." )] protected float JumpBufferTime { get; private set; }
		[field: SerializeField, BoxGroup( "Jump" ), Min( 0F ), Tooltip( "The amount of time that Gwamba can Jump when get out of the ground." )] protected float JumpCoyoteTime { get; private set; }
		[field: SerializeField, BoxGroup( "Jump" ), Range( 0F, 1F ), Tooltip( "The amount of cut that Gwamba's jump will suffer at up." )] protected float JumpCut { get; private set; }
		[field: SerializeField, BoxGroup( "Attack" ), Tooltip( "The sound to play when Gwamba attack." ), Space( WorldBuild.FIELD_SPACE_LENGTH * 2F )]
		protected AudioClip AttackSound { get; private set; }
		[field: SerializeField, BoxGroup( "Attack" ), Tooltip( "The sound to play when Gwamba damages something." )] protected AudioClip DamageAttackSound { get; private set; }
		[field: SerializeField, BoxGroup( "Attack" ), Range( 0F, 1F ), Tooltip( "The amount of velocity to cut during the attack." )] protected float AttackVelocityCut { get; private set; }
		[field: SerializeField, BoxGroup( "Attack" ), Min( 0F ), Tooltip( "The amount of time to stop the game when hit is given." )] protected float HitStopTime { get; private set; }
		[field: SerializeField, BoxGroup( "Attack" ), Min( 0F ), Tooltip( "The amount of time to slow the game when hit is given." )] protected float HitSlowTime { get; private set; }
		[field: SerializeField, BoxGroup( "Attack" ), Min( 0F ), Tooltip( "The amount of time the attack will be inactive after attack's hit." )] protected float DelayAfterAttack { get; private set; }
		[field: SerializeField, BoxGroup( "Attack" ), Tooltip( "If Gwamba is attacking in the moment." )] protected bool AttackUsage { get; private set; }
		[field: SerializeField, BoxGroup( "Attack" ), Tooltip( "The buffer moment that Gwamba have to execute a combo attack." )] protected bool ComboAttackBuffer { get; private set; }
		public static StateT Instance { get; protected set; }
		protected Vector2 Local => (Vector2) transform.position + _collider.offset;
		public MessagePath Path => MessagePath.Character;
		public abstract void Receive( MessageData message );
	};
};
