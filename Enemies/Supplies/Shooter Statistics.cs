using UnityEngine;
using NaughtyAttributes;
namespace GwambaPrimeAdventure.Enemy.Supply
{
	[CreateAssetMenu( fileName = "Shooter Enemy", menuName = "Enemy Statistics/Shooter", order = 5 )]
	public sealed class ShooterStatistics : ScriptableObject
	{
		[field: SerializeField, Tooltip( "The projectiles that this enemy can instantiate." )]
		[field: Header( "Shooter Enemy", order = 0 ), Space( WorldBuild.FIELD_SPACE_LENGTH * 2F, order = 1 )]
		public Projectile[] Projectiles
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "The point to where spawn the projectile relative to this enemy." )]
		public Vector2 SpawnPoint
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "Will shoot to infinity without any detection." )]
		public bool ShootInfinity
		{
			get;
			private set;
		}
		[field: SerializeField, HideIf( nameof( ShootInfinity ) ), Min( 0F ), Tooltip( "The distance this enemy can detect the target." )]
		public float PerceptionDistance
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "If the detection will be circular." )]
		public bool CircularUse
		{
			get;
			private set;
		}
		[field: SerializeField, HideIf( nameof( CircularUse ) ), Tooltip( "The angle of the ray's direction." )]
		public float DirectionAngle
		{
			get;
			private set;
		}
		[field: SerializeField, HideIf( nameof( CircularUse ) ), Tooltip( "If this enemy will turn the ray to the looking side." )]
		public bool TurnRay
		{
			get;
			private set;
		}
		[field: SerializeField, Min( 0F ), Tooltip( "The amount of time to wait to execute another shoot." )]
		public float IntervalToShoot
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "If this enemy will stop moving when shoot.\nRequires: Moving Enemy." )]
		public bool Stop
		{
			get;
			private set;
		}
		[field: SerializeField, ShowIf( nameof( Stop ) ), Min( WorldBuild.MINIMUM_TIME_SPACE_LIMIT )]
		[field: Tooltip( "The amount of time to stop this enemy to move.\nRequires: Moving Enemy." )]
		public float StopTime
		{
			get;
			private set;
		}
		[field: SerializeField, ShowIf( nameof( Stop ) ), Tooltip( "If this enemy will paralyze moving.\nRequires: Moving Enemy." )]
		public bool Paralyze
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "If this enemy will become invencible while shooting.\nRequires: Defender Enemy." )]
		public bool InvencibleShoot
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "If this enemy won't interfere in the projectile." )]
		public bool PureInstance
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "If this enemy gets hurt it will shoot." )]
		public bool ShootDamaged
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "If this enemy will react to any damage taken." )]
		public bool ReactToDamage
		{
			get;
			private set;
		}
	};
};
