using UnityEngine;
namespace GwambaPrimeAdventure.Enemy.Supply
{
	[CreateAssetMenu( fileName = "Death Enemy", menuName = "Enemy Statistics/Death", order = 10 )]
	public sealed class DeathStatistics : ScriptableObject
	{
		[field: SerializeField, Tooltip( "The enemy that this enemy will spawn in death." ), Header( "Death Enemy", order = 0 ), Space( WorldBuild.FIELD_SPACE_LENGTH * 2F, order = 1 )]
		public Control ChildEnemy { get; private set; }
		[field: SerializeField, Tooltip( "The projectile that this enemy will spawn in death." )] public Projectile ChildProjectile { get; private set; }
		[field: SerializeField, Tooltip( "The point to where spawn the object on death relative to this enemy." )] public Vector2 SpawnPoint { get; private set; }
		[field: SerializeField, Tooltip( "If this enemy will die on touch." )] public bool OnTouch { get; private set; }
		[field: SerializeField, Tooltip( "The time to this enemy die." )] public float TimeToDie { get; private set; }
	};
};
