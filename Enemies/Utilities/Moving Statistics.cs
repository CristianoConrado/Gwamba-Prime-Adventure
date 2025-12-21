using UnityEngine;
using NaughtyAttributes;
namespace GwambaPrimeAdventure.Enemy.Supply
{
	public abstract class MovingStatistics : ScriptableObject
	{
		[field: SerializeField, Min( 0F ), Tooltip( "The speed of the enemy to moves towards." ), Header( "Moving Statistics", order = 0 ), Space( WorldBuild.FIELD_SPACE_LENGTH * 2F, order = 1 )]
		public float MovementSpeed { get; private set; }
		[field: SerializeField, Min( 0F ), Tooltip( "The amount of speed of the dash." )] public float DashSpeed { get; private set; }
		[field: SerializeField, Tooltip( "If this enemy will invert the side of movement." )] public bool InvertMovementSide { get; private set; }
		[field: SerializeField, Tooltip( "If this enemy will do some action when look to a target." )] public bool LookPerception { get; private set; }
		[field: SerializeField, ShowIf( nameof( LookPerception ) ), Min( 0F ), Tooltip( "The distance of the detection of target." )] public float LookDistance { get; private set; }
		[field: SerializeField, ShowIf( nameof( LookPerception ) ), Tooltip( "If this enemy will stop on detection of the target." )] public bool DetectionStop { get; private set; }
		[field: SerializeField, ShowIf( nameof( LookPerception ) ), Min( WorldBuild.MINIMUM_TIME_SPACE_LIMIT ), Tooltip( "The amount of time this enemy will stop on detection." )]
		public float StopTime { get; private set; }
		[field: SerializeField, Tooltip( "If this enemy will react to any damage taken." )] public bool ReactToDamage { get; private set; }
	};
};
