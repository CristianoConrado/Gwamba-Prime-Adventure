using UnityEngine;
using System;
namespace GwambaPrimeAdventure.Enemy.Supply
{
	[CreateAssetMenu( fileName = "Summoner Enemy", menuName = "Enemy Statistics/Summoner", order = 6 )]
	public sealed class SummonerStatistics : ScriptableObject
	{
		[field: SerializeField, Tooltip( "The summons that will be activate on an event." )]
		[field: Header( "Summoner Enemy", order = 0 ), Space( WorldBuild.FIELD_SPACE_LENGTH * 2F, order = 1 )]
		public SummonObject[] EventSummons
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "The summons that will be activate with time." )]
		public SummonObject[] TimedSummons
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "The collection of the summon point structure." )]
		public SummonPointStructure[] SummonPointStructures
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "If this enemy will summon randomized in the react." )]
		public bool RandomReactSummons
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "If this enemy will summon randomized timed." )]
		public bool RandomTimedSummons
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "If this enemy will react to any damage taken." )]
		public bool HasEventSummon
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "The amount of time to cancel the summon that is waiting to perform." )]
		public float TimeToCancel
		{
			get;
			private set;
		}
	};
	[Serializable]
	public struct SummonPointStructure
	{
		[field: SerializeField, Tooltip( "The object to activate the summon." )]
		public SummonPoint SummonPointObject
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "Which summon event the summon point will activate." )]
		public SummonObject Summon
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "The point where the summon point will be." )]
		public Vector2 Point
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "The amount of time to summon again." )]
		public float TimeToUse
		{
			get;
			private set;
		}
	};
};