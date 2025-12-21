using UnityEngine;
using NaughtyAttributes;
namespace GwambaPrimeAdventure.Enemy.Supply
{
	[CreateAssetMenu( fileName = "Enemy Projectile", menuName = "Enemy Statistics/Projectile", order = 11 )]
	public sealed class ProjectileStatistics : ScriptableObject
	{
		[field: SerializeField, Tooltip( "The second projectile this will instantiate." ), Header( "Projectile Statistics", order = 0 ), Space( WorldBuild.FIELD_SPACE_LENGTH * 2F, order = 1 )]
		public Projectile SecondProjectile { get; private set; }
		[field: SerializeField, Tooltip( "If this peojectile will move in side ways." )] public bool SideMovement { get; private set; }
		[field: SerializeField, Tooltip( "If this projectile will move in the opposite way." )] public bool InvertSide { get; private set; }
		[field: SerializeField, Tooltip( "The angle the second projectile will be instantiated." )] public float BaseAngle { get; private set; }
		[field: SerializeField, Tooltip( "The angle the second projectile have to be spreaded." )] public float SpreadAngle { get; private set; }
		[field: SerializeField, Tooltip( "The amount of time this projectile will exists after fade away." )] public float TimeToFade { get; private set; }
		[field: SerializeField, Tooltip( "If this projectile won't move." )] public bool StayInPlace { get; private set; }
		[field: SerializeField, HideIf( nameof( StayInPlace ) ), Tooltip( "The velocity of the screen shake when colliding on the scene." )] public Vector2 CollideShake { get; private set; }
		[field: SerializeField, HideIf( nameof( StayInPlace ) ), Tooltip( "If this projectile will pursue the player endless." )] public bool EndlessPursue { get; private set; }
		[field: SerializeField, HideIf( EConditionOperator.Or, nameof( StayInPlace ), nameof( EndlessPursue ) ), Tooltip( "The fore mode to applied in the projectile." )]
		public ForceMode2D ForceMode { get; private set; }
		[field: SerializeField, HideIf( EConditionOperator.Or, nameof( StayInPlace ), nameof( EndlessPursue ) ), Tooltip( "If this projectile will use force mode to move." )]
		public bool UseForce { get; private set; }
		[field: SerializeField, HideIf( EConditionOperator.Or, nameof( StayInPlace ), nameof( EndlessPursue ) ), Tooltip( "If this projectile will use parabolic movement." )]
		public bool ParabolicMovement { get; private set; }
		[field: SerializeField, HideIf( nameof( StayInPlace ) ), Tooltip( "The amount of speed this projectile will move." )] public float MovementSpeed { get; private set; }
		[field: SerializeField, HideIf( nameof( StayInPlace ) ), Tooltip( "If the rotation of this projectile impacts its movement." )] public bool RotationMatter { get; private set; }
		[field: SerializeField, Tooltip( "If the rotation of this projectile will be used." )] public bool UseSelfRotation { get; private set; }
		[field: SerializeField, Tooltip( "The amount of speed the rotation spins." )] public float RotationSpeed { get; private set; }
		[field: SerializeField, Tooltip( "If this projectile will instantiate another after its death." )] public bool InDeath { get; private set; }
		[field: SerializeField, ShowIf( nameof( InDeath ) ), Tooltip( "The enemy that will be instantiate on death." )] public Control EnemyOnDeath { get; private set; }
		[field: SerializeField, Tooltip( "If this projectile will instantiate another ones in an amount of quantity." )] public bool UseQuantity { get; private set; }
		[field: SerializeField, ShowIf( EConditionOperator.Or, nameof( InDeath ), nameof( UseQuantity ) ), Tooltip( "The amount of second projectiles to instantiate." )]
		public ushort QuantityToSummon { get; private set; }
		[field: SerializeField, Tooltip( "If this projectile receives no type of damage." )] public bool NoDamage { get; private set; }
		[field: SerializeField, HideIf( nameof( NoDamage ) ), Tooltip( "The vitality of this projectile." )] public ushort Vitality { get; private set; }
		[field: SerializeField, HideIf( nameof( NoDamage ) ), Tooltip( "If this projectile won't get stunned." )] public bool NoStun { get; private set; }
		[field: SerializeField, Tooltip( "If this projectile won't hit at contact." )] public bool NoHit { get; private set; }
		[field: SerializeField, HideIf( nameof( NoHit ) ), Tooltip( "The velocity of the screen shake on the hurt." )] public Vector2 HurtShake { get; private set; }
		[field: SerializeField, HideIf( nameof( NoHit ) ), Tooltip( "If this projectile won't die when hit." )] public bool NoDeathHit { get; private set; }
		[field: SerializeField, HideIf( nameof( NoHit ) ), Tooltip( "If this projectile won't die when hit a wall." )] public bool NoDeathCollision { get; private set; }
		[field: SerializeField, HideIf( nameof( NoHit ) ), Tooltip( "The amount of damage this projectile will cause to a target." )] public ushort Damage { get; private set; }
		[field: SerializeField, HideIf( nameof( NoHit ) ), Tooltip( "The amount of time this projectile will stun." )] public float StunTime { get; private set; }
		[field: SerializeField, HideIf( nameof( NoHit ) ), Tooltip( "The amount of time to stop the game when hit is given." )] public float HitStopTime { get; private set; }
		[field: SerializeField, HideIf( nameof( NoHit ) ), Tooltip( "The amount of time to slow the game when hit is given." )] public float HitSlowTime { get; private set; }
		[field: SerializeField, Tooltip( "If the second projectile will be instantiated in a cell." ), Header( "Cell Statistics", order = 0 ), Space( WorldBuild.FIELD_SPACE_LENGTH * 2F, order = 1 )]
		public bool InCell { get; private set; }
		[field: SerializeField, ShowIf( nameof( InCell ) ), Tooltip( "If the second projectile will instantiate in a continuos sequence." )] public bool ContinuosSummon { get; private set; }
		[field: SerializeField, ShowIf( nameof( InCell ) ), Tooltip( "If the instantiation of the second projectile will break after a moment." )] public bool UseBreak { get; private set; }
		[field: SerializeField, ShowIf( nameof( InCell ) ), Tooltip( "If the instantiation of the second projectile will always break after the first." )] public bool AlwaysBreak { get; private set; }
		[field: SerializeField, ShowIf( nameof( InCell ) ), Tooltip( "If the points of break are randomized between the maximum and minimum." )] public bool RandomBreak { get; private set; }
		[field: SerializeField, ShowIf( nameof( InCell ) ), Tooltip( "If the break point is restricted at a specific break point." )] public bool ExtrictRandom { get; private set; }
		[field: SerializeField, ShowIf( nameof( InCell ) ), Tooltip( "The amount of cell points to jump the instantiation." )] public ushort JumpPoints { get; private set; }
		[field: SerializeField, ShowIf( nameof( InCell ) ), Tooltip( "The exact point where the break of the instantiantion start." )] public ushort BreakPoint { get; private set; }
		[field: SerializeField, ShowIf( nameof( InCell ) ), Tooltip( "The exact point where the instantiation returns." )] public ushort ReturnPoint { get; private set; }
		[field: SerializeField, ShowIf( nameof( InCell ) ), Tooltip( "The minimum value the break point can break." )] public ushort MinimumRandomValue { get; private set; }
		[field: SerializeField, ShowIf( nameof( InCell ) ), Tooltip( "The distance of the range ray to the instantiation." )] public float DistanceRay { get; private set; }
	};
};
