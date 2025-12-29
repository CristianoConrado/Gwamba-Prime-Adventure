using UnityEngine;
using System;
namespace GwambaPrimeAdventure.Enemy.Supply
{
	[CreateAssetMenu( fileName = "Teleporter Enemy", menuName = "Enemy Statistics/Teleporter", order = 9 )]
	public sealed class TeleporterStatistics : ScriptableObject
	{
		[field: SerializeField, Tooltip( "The collection of the summon places." ), Header( "Teleporter Enemy", order = 0 ), Space( WorldBuild.FIELD_SPACE_LENGTH * 2F, order = 1 )]
		public TeleportPointStructure[] TeleportPointStructures
		{
			get;
			private set;
		}
		[field: SerializeField, Tooltip( "The amount of time to use the teleport again." )]
		public float TimeToUse
        {
            get;
            private set;
        }
    };
	[Serializable]
	public struct TeleportPointStructure
	{
		[field: SerializeField, Tooltip( "The teleport point to be instantiated." )]
		public TeleportPoint TeleportPointObject
        {
            get;
            private set;
        }
        [field: SerializeField, Tooltip( "The points where the point will teleport to." )]
		public Vector2[] TeleportPoints
        {
            get;
            private set;
        }
        [field: SerializeField, Tooltip( "The point where the teleport point will be." )]
		public Vector2 InstancePoint
        {
            get;
            private set;
        }
        [field: SerializeField, Tooltip( "If the points to teleport will be random." )]
		public bool RandomTeleports
        {
            get;
            private set;
        }
    }
};
