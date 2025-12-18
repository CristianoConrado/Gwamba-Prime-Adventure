using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;
namespace GwambaPrimeAdventure
{
	[DisallowMultipleComponent, Icon(WorldBuild.PROJECT_ICON), RequireComponent(typeof(Transform), typeof(Tilemap), typeof(TilemapRenderer)), RequireComponent(typeof(TilemapCollider2D))]
	public sealed class Surface : MonoBehaviour, ILoader
	{
		private static readonly List<Surface> _surfaces = new();
		private Tilemap _tilemap;
		private Tile _returnedTile;
		private void OnEnable() => _surfaces.Add(this);
		private void OnDisable() => _surfaces.Remove(this);
		public IEnumerator Load()
		{
			_tilemap = GetComponent<Tilemap>();
			yield return null;
		}
		public static Tile CheckForTile(Vector2 originPosition)
		{
			foreach (Surface surface in _surfaces)
				if ((surface._returnedTile = surface._tilemap.GetTile<Tile>(surface._tilemap.WorldToCell(originPosition))) && surface._tilemap.ContainsTile(surface._returnedTile))
					return surface._returnedTile;
			return null;
		}
	};
};
