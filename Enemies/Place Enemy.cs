using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
namespace GwambaPrimeAdventure.Enemy
{
	[DisallowMultipleComponent, RequireComponent( typeof( Tilemap ), typeof( TilemapRenderer ), typeof( TilemapCollider2D ) ), RequireComponent( typeof( CompositeCollider2D ) )]
	internal sealed class PlaceEnemy : EnemyProvider, IConnector
	{
		private
			Tilemap _tilemap;
		private
			TilemapCollider2D _tilemapCollider;
		private
			WaitUntil _waitActive;
		private
			IEnumerator _appearFadeEvent;
		[SerializeField, Tooltip( "If anything can be hurt." ), Header( "Interactions" )]
		private
			bool _hurtEveryone;
		[SerializeField, Tooltip( "If this enemy will react to any damage taken." )]
		private
			bool _reactToDamage;
		private new void Awake()
		{
			base.Awake();
			_tilemap = GetComponent<Tilemap>();
			_tilemapCollider = GetComponent<TilemapCollider2D>();
			_waitActive = new WaitUntil( () => isActiveAndEnabled && !IsStunned );
			Sender.Include( this );
		}
		private new void OnDestroy()
		{
			base.OnDestroy();
			if ( _appearFadeEvent is not null )
				_appearFadeEvent = null;
			Sender.Exclude( this );
		}
		private void Update() => _appearFadeEvent?.MoveNext();
		public void Receive( MessageData message )
		{
			if ( message.AdditionalData is not null && message.AdditionalData is EnemyProvider[] enemies && 0 < enemies.Length )
				foreach ( EnemyProvider enemy in enemies )
					if ( enemy && this == enemy )
					{
						if ( MessageFormat.State == message.Format && message.ToggleValue.HasValue )
							_appearFadeEvent = AppearFade( message.ToggleValue.Value );
						else if ( MessageFormat.Event == message.Format && _reactToDamage )
							_appearFadeEvent = AppearFade( 0F >= _tilemap.color.a );
						IEnumerator AppearFade( bool appear )
						{
							Color color = _tilemap.color;
							if ( appear )
								for ( float i = 0F; 1F > _tilemap.color.a; i += 1E-1F )
								{
									yield return _waitActive;
									color.a = i;
									_tilemap.color = color;
									yield return null;
								}
							else
								for ( float i = 1F; 0F < _tilemap.color.a; i -= 1E-1F )
								{
									yield return _waitActive;
									color.a = i;
									_tilemap.color = color;
									yield return null;
								}
							_tilemapCollider.enabled = appear;
							_appearFadeEvent = null;
						}
						return;
					}
		}
	};
};
