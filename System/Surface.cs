using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
namespace GwambaPrimeAdventure
{
	[DisallowMultipleComponent, Icon(WorldBuild.PROJECT_ICON), RequireComponent(typeof(Transform), typeof(Tilemap), typeof(TilemapRenderer)), RequireComponent(typeof(TilemapCollider2D))]
	public sealed class Surface : MonoBehaviour, ILoader
	{
		private Tilemap _tilemap;
		public IEnumerator Load()
		{
			_tilemap = GetComponent<Tilemap>();
			yield return null;
		}
		public Tile CheckForTile(Vector2 originPosition) => _tilemap.GetTile<Tile>(_tilemap.WorldToCell(originPosition));
	};
};
