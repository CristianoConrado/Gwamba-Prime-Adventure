using UnityEngine;
using NaughtyAttributes;
namespace GwambaPrimeAdventure.Enemy.Supply
{
	[CreateAssetMenu( fileName = "Defender Enemy", menuName = "Enemy Statistics/Defender", order = 8 )]
	public sealed class DefenderStatistics : ScriptableObject
	{
		[field: SerializeField, Tooltip( "The amount of damage that this object have to receive real damage." )]
		[field: Header( "Defender Enemy", order = 0 ), Space( WorldBuild.FIELD_SPACE_LENGTH * 2F, order = 1 )]
		public byte BiggerDamage
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "If this enemy will stop moving when become invencible.\nRequires: Moving Enemy." )]
		public bool InvencibleStop
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "If this enemy will become invencible when hurted." )]
		public bool InvencibleHurted
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "If this enemy will react to any damage taken to a event." )]
		public bool ReactToDamage
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "If the react to a event will be timed." )]
		public bool TimedReact
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "If this enemy will use time to become invencible/destructible." )]
		public bool UseAlternatedTime
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "The amount of time the enemy have to become destructible." )]
		public float TimeToDestructible
		{
			get;
			private set;
		}
		[field: SerializeField, ShowIf( nameof( UseAlternatedTime ) ), Tooltip( "The amount of time the enemy have to become invencible." )]
		public float TimeToInvencible
		{
			get;
			private set;
		}
	};
};
