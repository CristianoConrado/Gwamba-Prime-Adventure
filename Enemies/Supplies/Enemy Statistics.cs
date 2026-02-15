using UnityEngine;
using NaughtyAttributes;
namespace GwambaPrimeAdventure.Enemy.Supply
{
	[CreateAssetMenu( fileName = "Provider Enemy", menuName = "Enemy Statistics/Provider", order = 1 )]
	public sealed class EnemyStatistics : ScriptableObject
	{
		[field: SerializeField, Tooltip( "The gravity applied to pull down the object." )]
		[field: Header( "Providence Statistics", order = 0 ), Space( WorldBuild.FIELD_SPACE_LENGTH * 2F, order = 1 )]
		public float GravityScale
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "If this enemy receives no type of damage." )]
		public bool NoDamage
		{
			get;
			private set;
		}
		[field: SerializeField, HideIf( nameof( NoDamage ) ), Tooltip( "The vitality of the enemy." )]
		public byte Vitality
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "If this enemy won't get stunned." )]
		public bool NoStun
		{
			get;
			private set;
		}
		[field: SerializeField, HideIf( nameof( NoStun ) ), Tooltip( "The amount of stun that this enemy can resists." )]
		public byte HitResistance
		{
			get;
			private set;
		}
		[field: SerializeField, HideIf( nameof( NoStun ) ), Tooltip( "If this enemy won't get stunned when hitted." )]
		public bool NoHitStun
		{
			get;
			private set;
		}
		[field: SerializeField, HideIf( nameof( NoStun ) ), Tooltip( "The amount of time this enemy will be stunned when armor be broken." )]
		public float StunnedTime
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "If this enemy won't hit at contact." )]
		public bool NoHit
		{
			get;
			private set;
		}
		[field: SerializeField, HideIf( nameof( NoHit ) ), Tooltip( "The velocity of the screen shake on the hurt." )]
		public Vector2 HurtShake
		{
			get;
			private set;
		}
		[field: SerializeField, HideIf( nameof( NoHit ) ), Tooltip( "The amount of damage that the enemy hit." )]
		public byte Damage
		{
			get;
			private set;
		}
		[field: SerializeField, HideIf( nameof( NoHit ) ), Tooltip( "The amount of time this enemy will stun." )]
		public float StunTime
		{
			get;
			private set;
		}
		[field: SerializeField, HideIf( nameof( NoHit ) ), Tooltip( "The amount of time to stop the game when hit is given." )]
		public float HitStopTime
		{
			get;
			private set;
		}
		[field: SerializeField, HideIf( nameof( NoHit ) ), Tooltip( "The amount of time to slow the game when hit is given." )]
		public float HitSlowTime
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "If this enemy will fade away over time." )]
		public bool FadeOverTime
		{
			get;
			private set;
		}
		[field: SerializeField, ShowIf( nameof( FadeOverTime ) ), Tooltip( "The amount of time this enemy will fade away." )]
		public float TimeToFadeAway
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
		[field: SerializeField, ShowIf( nameof( ReactToDamage ) ), Tooltip( "If this enemy has a index atribute to use." )]
		public bool HasIndex
		{
			get;
			private set;
		}
		[field: SerializeField, ShowIf( nameof( HasIndex ) ), Tooltip( "The index to a event to a enemy make." )]
		public byte IndexEvent
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "If this enemy is the boss of the level." )]
		public bool IsLevelBoss
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "If this object will be saved as already existent object." )]
		public bool SaveOnSpecifics
		{
			get;
			private set;
		}
	};
};
