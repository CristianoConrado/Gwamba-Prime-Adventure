using UnityEngine;
using NaughtyAttributes;
namespace GwambaPrimeAdventure.Enemy.Supply
{
	[CreateAssetMenu( fileName = "Flying Enemy", menuName = "Enemy Statistics/Flying", order = 4 )]
	public sealed class FlyingStatistics : MovingStatistics
	{
		[field: SerializeField, Tooltip( "The target this enemy have to pursue." ), Header( "Flying Enemy", order = 0 ), Space( WorldBuild.FIELD_SPACE_LENGTH * 2F, order = 1 )]
		public Transform Target
		{
			get;
			private set;
		}
		[field: SerializeField, Min( 0F ), ShowIf( EConditionOperator.Or, nameof( LookPerception ), nameof( EndlessPursue ) )]
		[field: Tooltip( "The amount of speed to this enemy rotate towards." )]
		public float RotationSpeed
		{
			get;
			private set;
		}
		[field: SerializeField, Min( 0F ), ShowIf( nameof( LookPerception ) ), HideIf( nameof( EndlessPursue ) ), Tooltip( "The distance to stay away from the target." )]
		public float TargetDistance
		{
			get;
			private set;
		}
		[field: SerializeField, Min( 0F ), ShowIf( nameof( LookPerception ) ), HideIf( nameof( EndlessPursue ) )]
		[field: Tooltip( "The amount of speed that this enemy moves to go back to the original point." )]
		public float ReturnSpeed
		{
			get;
			private set;
		}
		[field: SerializeField, Min( 0F ), ShowIf( nameof( LookPerception ) ), HideIf( nameof( EndlessPursue ) )]
		[field: Tooltip( "The amount of time this enemy moves will be stopped after it attack." )]
		public float AfterTime
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "If this enemy will pursue the target until fade." )]
		public bool EndlessPursue
		{
			get;
			private set;
		}
	};
};