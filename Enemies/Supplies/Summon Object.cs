using UnityEngine;
using NaughtyAttributes;
namespace GwambaPrimeAdventure.Enemy.Supply
{
	[CreateAssetMenu( fileName = "Enemy Summon", menuName = "Enemy Statistics/Summon", order = 7 )]
	public sealed class SummonObject : ScriptableObject
	{
		[field: SerializeField, Tooltip( "The enemy that will be instantiate." )]
		[field: Header( "Components Statistics", order = 0 ), Space( WorldBuild.FIELD_SPACE_LENGTH * 2F, order = 1 )]
		public GameObject[] Summons
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "The points that the instance can be instantiate." )]
		public Vector2[] SummonPoints
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "If the points to summon are relative to it's parent." )]
		public bool LocalPoints
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "The amount of instance to be instantiate." )]
		public ushort QuantityToSummon
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "The amount of time to execute the instance." )]
		public float SummonTime
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "The amount of time to wait to execute the next instance." )]
		public float PostSummonTime
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "If the post summon time will be skipped." )]
		public bool SkipPostSummon
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "If the instanciation will be in the same point as the summoner." )]
		public bool Self
		{
			get;
			private set;
		}
		[field: SerializeField, HideIf( nameof( Self ) ), Tooltip( "If the instantiation will be randomized at one of the points." )]
		public bool Random
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "If the instantiator will stop during the summon." )]
		public bool StopToSummon
		{
			get;
			private set;
		}
		[field: SerializeField, ShowIf( nameof( StopToSummon ) ), Tooltip( "If the instantiator will paralyze during the summon." )]
		public bool ParalyzeToSummon
		{
			get;
			private set;
		}
		[field: SerializeField, ShowIf( nameof( StopToSummon ) ), Min( 0F ), Tooltip( "The amount of time to stop the instantiator." )]
		public float TimeToStop
		{
			get;
			private set;
		}
		[field: SerializeField, ShowIf( nameof( StopToSummon ) ), Tooltip( "If the instanciation will be instantly before the stop." )]
		public bool InstantlySummon
		{
			get;
			private set;
		}
		[field: SerializeField, ShowIf( nameof( StopToSummon ) ), HideIf( nameof( InstantlySummon ) ), Tooltip( "If the instanciation will wait return from the stop to summon." )]
		public bool WaitStop
		{
			get;
			private set;
		}
	};
};
