using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
namespace GwambaPrimeAdventure
{
	[DisallowMultipleComponent, Icon( WorldBuild.PROJECT_ICON ), RequireComponent( typeof( Transform ), typeof( Tilemap ), typeof( TilemapRenderer ) )]
	[RequireComponent( typeof( TilemapCollider2D ) )]
	public sealed class Surface : MonoBehaviour, ILoader
	{
		private
			Tilemap _tilemap;
		private
			TilemapCollider2D _collider;
		private
			Tile _returnedTile;
		private readonly Dictionary<Tile, AudioClip>
			_tiles = new Dictionary<Tile, AudioClip>();
		private static
			AudioClip _surfaceSoundClip;
		private static
			UnityAction<Vector2> _getSurface;
		[SerializeField, Tooltip( "The sounds of the surfaces that will be played." )]
		private SurfaceSound[]
			_surfaceSounds;
		private void OnEnable() => _getSurface += CheckPoint;
		private void OnDisable() => _getSurface -= CheckPoint;
		public async UniTask Load()
		{
			CancellationToken destroyToken = this.GetCancellationTokenOnDestroy();
			await UniTask.Yield( PlayerLoopTiming.EarlyUpdate, destroyToken, true ).SuppressCancellationThrow();
			if ( destroyToken.IsCancellationRequested )
				return;
			_tilemap = GetComponent<Tilemap>();
            _collider = GetComponent<TilemapCollider2D>();
			foreach ( SurfaceSound surfaceSound in _surfaceSounds )
				foreach ( Tile tile in surfaceSound.Tiles )
					if ( !_tiles.ContainsKey( tile ) )
						_tiles.Add( tile, surfaceSound.Clip );
		}
		private void CheckPoint( Vector2 originPosition )
		{
			if ( _collider.OverlapPoint( originPosition ) )
				if ( ( _returnedTile = _tilemap.GetTile<Tile>( _tilemap.WorldToCell( originPosition ) ) ) && _tiles.ContainsKey( _returnedTile ) )
					_surfaceSoundClip = _tiles[ _returnedTile ];
		}
		public static AudioClip CheckSurface( Vector2 originPosition )
		{
			_getSurface.Invoke( originPosition );
			return _surfaceSoundClip;
		}
	};
};
