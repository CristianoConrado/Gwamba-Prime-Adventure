using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;
namespace GwambaPrimeAdventure
{
	[DisallowMultipleComponent, Icon(WorldBuild.PROJECT_ICON), RequireComponent(typeof(Transform), typeof(Tilemap), typeof(TilemapRenderer)), RequireComponent(typeof(TilemapCollider2D))]
	public sealed class Surface : MonoBehaviour, ILoader
	{
		private Tilemap _tilemap;
		private TilemapCollider2D _collider;
		private Tile _returnedTile;
		private readonly Dictionary<Tile, AudioClip> _tiles = new();
		private static AudioClip _surfaceSoundClip;
		private static UnityAction<Vector2> _getSurface;
		[SerializeField, Tooltip("The sounds of the surfaces that will be played.")] private SurfaceSound[] _surfaceSounds;
		private void OnEnable() => _getSurface += CheckPoint;
		private void OnDisable() => _getSurface -= CheckPoint;
		public IEnumerator Load()
		{
			_tilemap = GetComponent<Tilemap>();
			_collider = GetComponent<TilemapCollider2D>();
			for (ushort i = 0; _surfaceSounds.Length > i; i++)
				for (ushort j = 0; _surfaceSounds[i].Tiles.Length > j; j++)
					if (!_tiles.ContainsKey(_surfaceSounds[i].Tiles[j]))
						_tiles.Add(_surfaceSounds[i].Tiles[j], _surfaceSounds[i].Clip);
			yield return null;
		}
		private void CheckPoint(Vector2 originPosition)
		{
			if (_collider.OverlapPoint(originPosition) && (_returnedTile = _tilemap.GetTile<Tile>(_tilemap.WorldToCell(originPosition))) && _tiles.ContainsKey(_returnedTile))
				_surfaceSoundClip = _tiles[_returnedTile];
		}
		public static AudioClip CheckSurface(Vector2 originPosition)
		{
			_getSurface.Invoke(originPosition);
			return _surfaceSoundClip;
		}
	};
};
