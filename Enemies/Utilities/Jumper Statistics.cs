using UnityEngine;
using NaughtyAttributes;
namespace GwambaPrimeAdventure.Enemy.Supply
{
	[CreateAssetMenu( fileName = "Jumper Enemy", menuName = "Enemy Statistics/Jumper", order = 3 )]
	public sealed class JumperStatistics : MovingStatistics
	{
		[field: SerializeField, Tooltip( "The collection of the objet that carry the jump." )]
        [field: Header( "Jumper Enemy", order = 0 ), Space( WorldBuild.FIELD_SPACE_LENGTH * 2F, order = 1 )]
		public JumpPointStructure[] JumpPointStructures
        {
            get;
            private set;
        }
        [field: SerializeField, Tooltip( "The collection of the objet that carry the jump." )]
		public JumpStats[] TimedJumps
        {
            get;
            private set;
        }
        [field: SerializeField, Tooltip( "If this enemy will use input." )]
		public bool UseInput
        {
            get;
            private set;
        }
        [field: SerializeField, Tooltip( "If the detection will be circular." )]
		public bool CircularDetection
        {
            get;
            private set;
        }
        [field: SerializeField, Tooltip( "If this enemy will not follow the target in the basic jump." )]
		public bool UnFollow
        {
            get;
            private set;
        }
        [field: SerializeField, Tooltip( "If the timmed jumps will be executed in a sequence." )]
		public bool SequentialTimmedJumps
        {
            get;
            private set;
        }
        [field: SerializeField, ShowIf( nameof( SequentialTimmedJumps ) ), Tooltip( "If the sequential timmed jumps will be repeated again and again." )]
		public bool RepeatTimmedJumps
        {
            get;
            private set;
        }
        [field: SerializeField, Tooltip( "If the react to damage will use other target." )]
		public bool UseTarget
        {
            get;
            private set;
        }
        [field: SerializeField, ShowIf( nameof( UseTarget ) ), Tooltip( "The other target to move to on jump." )]
		public float OtherTarget
        {
            get;
            private set;
        }
        [field: SerializeField, ShowIf( nameof( UseTarget ) ), Tooltip( "If the target to follow will be random." )]
		public bool RandomFollow
        {
            get;
            private set;
        }
        [field: SerializeField, Tooltip( "The amount of time to cancel the jump that is waiting to perform." )]
		public float TimeToCancel
        {
            get;
            private set;
        }
        [field: SerializeField, Min( 0F ), Tooltip( "The strenght of the basic jump." )]
		public float JumpStrenght
        {
            get;
            private set;
        }
        [field: SerializeField, Min( 0F ), Tooltip( "The amount of time to jump again." )]
		public float TimeToJump
        {
            get;
            private set;
        }
        [field: SerializeField, Min( 0F ), HideIf( nameof( CircularDetection ) ), Tooltip( "The angle of the detection ray." )]
		public float DetectionAngle
        {
            get;
            private set;
        }
        [field: SerializeField, Min( 0F ), Tooltip( "The distance this enemy will be to the follow target." )]
		public float DistanceToTarget
        {
            get;
            private set;
        }
        [field: SerializeField, ShowIf( nameof( ReactToDamage ) ), Tooltip( "The strenght of the jump on the react." )]
        [field: Header( "Reaction", order = 0 ), Space( WorldBuild.FIELD_SPACE_LENGTH * 2F, order = 1 )]
		public ushort StrenghtReact
        {
            get;
            private set;
        }
        [field: SerializeField, ShowIf( nameof( ReactToDamage ) ), Tooltip( "If the react to damage jump will follow a target." )]
		public bool FollowReact
        {
            get;
            private set;
        }
        [field: SerializeField, ShowIf( nameof( ReactToDamage ) ), Tooltip( "If the react to damage jump will turn on the target." )]
		public bool TurnFollowReact
        {
            get;
            private set;
        }
        [field: SerializeField, ShowIf( nameof( ReactToDamage ) ), Tooltip( "If it will stop moving on react to damage." )]
		public bool StopMoveReact
        {
            get;
            private set;
        }
    };
	[System.Serializable]
	public struct JumpPointStructure
	{
		[field: SerializeField, Tooltip( "The object to activate the jump." )]
		public JumpPoint JumpPointObject
        {
            get;
            private set;
        }
        [field: SerializeField, Tooltip( "The jump stats to use in this structure." )]
		public JumpStats JumpStats
        {
            get;
            private set;
        }
        [field: SerializeField, Tooltip( "Where the jump point will be." )]
		public Vector2 Point
        {
            get;
            private set;
        }
        [SerializeField, Tooltip( "The minimal amount of times the boss have to pass by to activate the jump." )]
		private
			ushort _minJumpCount;
		[SerializeField, Tooltip( "The maximum amount of times the boss have to pass by to activate the jump." )]
		private
			ushort _maxJumpCount;
		public readonly ushort JumpCount =>
			(ushort) Random.Range( _minJumpCount, _maxJumpCount + 1 );
	};
	[System.Serializable]
	public struct JumpStats
	{
		[field: SerializeField, Min( 0F ), Tooltip( "The strenght of the jump." )]
		public float Strength
        {
            get;
            private set;
        }
        [field: SerializeField, Tooltip( "If it will stop moving when jumping." )]
		public bool StopMove
        {
            get;
            private set;
        }
        [field: SerializeField, Tooltip( "If this is a follow jump." )]
		public bool Follow
        {
            get;
            private set;
        }
        [field: SerializeField, ShowIf( nameof( Follow ) ), Tooltip( "If this enemy will turn on the follow." )]
		public bool TurnFollow
        {
            get;
            private set;
        }
        [field: SerializeField, ShowIf( nameof( Follow ) ), Tooltip( "If for this jump it will use the other target." )]
		public bool UseTarget
        {
            get;
            private set;
        }
        [field: SerializeField, ShowIf( EConditionOperator.And, nameof( Follow ), nameof( UseTarget ) ), Tooltip( "To where this have to go if theres no target." )]
		public float OtherTarget
        {
            get;
            private set;
        }
        [field: SerializeField, ShowIf( nameof( Follow ) ), Min( 0F ), Tooltip( "The amount of time for this jump to execute." )]
		public float TimeToExecute
        {
            get;
            private set;
        }
    };
};
