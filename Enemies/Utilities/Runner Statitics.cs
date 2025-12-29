using UnityEngine;
using NaughtyAttributes;
namespace GwambaPrimeAdventure.Enemy.Supply
{
	[CreateAssetMenu( fileName = "Runner Enemy", menuName = "Enemy Statistics/Runner", order = 2 )]
	public sealed class RunnerStatistics : MovingStatistics
	{
		[field: SerializeField, Tooltip( "If the off edge verifier will be turned off." ), Header( "Runner Enemy", order = 0 ), Space( WorldBuild.FIELD_SPACE_LENGTH * 2F, order = 1 )]
		public bool TurnOffEdge
        {
            get;
            private set;
        }
        [field: SerializeField, Tooltip( "If the boss can jump while dashing.\nRequires: Jumper Enemy." )]
		public bool JumpDash
        {
            get;
            private set;
        }
        [field: SerializeField, Tooltip( "If the boss will turn invencible while dashing.\nRequires: Defender Enemy." )]
		public bool InvencibleDash
        {
            get;
            private set;
        }
        [field: SerializeField, Tooltip( "If the dash is timed to start when the boss is instantiate." )]
		public bool TimedDash
        {
            get;
            private set;
        }
        [field: SerializeField, ShowIf( nameof( TimedDash ) ), Min( 0F ), Tooltip( "The amount of time to wait the timed dash to go." )]
		public float TimeToDash
        {
            get;
            private set;
        }
        [field: SerializeField, Min( 0F ), Tooltip( "The amount of time this enemy will be dashing upon the target." )]
		public float TimeDashing
        {
            get;
            private set;
        }
        [field: SerializeField, Tooltip( "If this enemy will run away from the target." )]
		public bool RunFromTarget
        {
            get;
            private set;
        }
        [field: SerializeField, ShowIf( nameof( RunFromTarget ) ), Tooltip( "If this enemy will run toward the target after the run." ), Space( WorldBuild.FIELD_SPACE_LENGTH * 2F )]
		public bool RunTowardsAfter
        {
            get;
            private set;
        }
        [field: SerializeField, ShowIf( nameof( RunFromTarget ) ), Min( 0F ), Tooltip( "The amount of time this enemy will run away from or pursue the target." )]
		public float RunOfTime
        {
            get;
            private set;
        }
        [field: SerializeField, ShowIf( nameof( RunFromTarget ) ), Tooltip( "The amount of times this enemy have to run away from the target." )]
		public ushort TimesToRun
        {
            get;
            private set;
        }
        [field: SerializeField, ShowIf( nameof( ReactToDamage ) ), Min( 0F ), Tooltip( "The amount of speed to run in the retreat." ), Space( WorldBuild.FIELD_SPACE_LENGTH * 2F )]
		public float RetreatSpeed
        {
            get;
            private set;
        }
        [field: SerializeField, ShowIf( nameof( ReactToDamage ) ), Min( 0F ), Tooltip( "The amount of time to wait for retreat again." )]
		public float TimeToRetreat
        {
            get;
            private set;
        }
        [field: SerializeField, ShowIf( nameof( ReactToDamage ) ), Min( 0F ), Tooltip( "The amount of distance to run in the retreat." )]
		public float RetreatDistance
        {
            get;
            private set;
        }
        [field: SerializeField, ShowIf( nameof( ReactToDamage ) ), Tooltip( "If this enemy will execute other event than dashing." )]
		public bool EventRetreat
        {
            get;
            private set;
        }
        [field: SerializeField, ShowIf( nameof( ReactToDamage ) ), Tooltip( "The index of the event executed in the retreat." )]
		public ushort EventIndex
        {
            get;
            private set;
        }
    };
};
