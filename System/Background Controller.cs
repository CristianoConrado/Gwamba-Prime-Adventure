using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D;
using Unity.Cinemachine;
using System.Collections;
namespace GwambaPrimeAdventure
{
	[DisallowMultipleComponent, RequireComponent( typeof( Camera ), typeof( CinemachineBrain ) )]
	internal sealed class BackgroundController : StateController, ILoader
	{
		private static BackgroundController _instance;
		private Transform[] _childrenTransforms;
		private SpriteRenderer[] _childrenRederers;
		private Vector2[] _startPosition;
		private Vector2
			_speed = Vector2.zero,
			_movement = Vector2.zero;
		[Header( "Background Interaction" )]
		[SerializeField, Tooltip( "The object that handles the backgrounds." )] private Transform _backgroundObject;
		[SerializeField, Tooltip( "The handler of the background." )] private SpriteAtlas _backgroundHandler;
		[SerializeField, Tooltip( "The amount of speed that the background will move." )] private Vector2 _backgroundSpeed;
		[SerializeField, Tooltip( "The amount to slow for each layer that is after the first." )] private Vector2 _slowSpeed;
		[SerializeField, Tooltip( "The offset of the camera relative to center of the screen." )] private Vector2 _positionOffset;
		[SerializeField, Tooltip( "The name of the images that are placed in each background." )] private string[] _backgroundImages;
		private new void Awake()
		{
			base.Awake();
			if ( _instance )
			{
				Destroy( gameObject, WorldBuild.MINIMUM_TIME_SPACE_LIMIT );
				return;
			}
			_instance = this;
		}
		public IEnumerator Load()
		{
			_childrenTransforms = new Transform[ _backgroundImages.Length ];
			_childrenRederers = new SpriteRenderer[ _backgroundImages.Length ];
			_startPosition = new Vector2[ _backgroundImages.Length ];
			Transform child;
			SpriteRenderer childRenderer;
			Vector3 childOffset = Vector2.zero;
			ushort childIndex;
			for ( ushort i = 0; i < _backgroundImages.Length; i++ )
			{
				_childrenTransforms[ i ] = Instantiate( _backgroundObject, transform );
				_startPosition[ i ] = _childrenTransforms[ i ].transform.position = (Vector2) _childrenTransforms[ i ].transform.position;
				_childrenRederers[ i ] = _childrenTransforms[ i ].GetComponent<SpriteRenderer>();
				_childrenRederers[ i ].sprite = _backgroundHandler.GetSprite( _backgroundImages[ i ] );
				_childrenRederers[ i ].sortingOrder = _backgroundImages.Length - 1 - i;
				_childrenTransforms[ i ].GetComponent<SortingGroup>().sortingOrder = _childrenRederers[ i ].sortingOrder;
				childIndex = 0;
				for ( short x = -1; 1 >= x; x++ )
					for ( short y = -1; 1 >= y; y++ )
						if ( 0 != x || 0 != y )
						{
							child = _childrenTransforms[ i ].transform.GetChild( childIndex++ );
							childOffset.Set( _childrenRederers[ i ].bounds.size.x * x, _childrenRederers[ i ].bounds.size.y * y, 0F );
							child.position += childOffset;
							childRenderer = child.GetComponent<SpriteRenderer>();
							childRenderer.sprite = _childrenRederers[ i ].sprite;
							childRenderer.sortingOrder = _childrenRederers[ i ].sortingOrder;
						}
			}
			yield return null;
		}
		private void LateUpdate()
		{
			if ( SceneInitiator.IsInTrancision() )
				return;
			for ( ushort i = 0; _childrenTransforms.Length > i; i++ )
			{
				_speed = _backgroundSpeed - ( i * _slowSpeed );
				_movement.Set( ( transform.position.x + _positionOffset.x ) * ( 1F - _speed.x ), ( transform.position.y + _positionOffset.y ) * ( 1F - _speed.y ) );
				_childrenTransforms[ i ].transform.position = _startPosition[ i ] + _movement;
				if ( transform.position.x * _speed.x > _startPosition[ i ].x + _childrenRederers[ i ].bounds.size.x )
					_startPosition[ i ].x += _childrenRederers[ i ].bounds.size.x;
				else if ( transform.position.x * _speed.x < _startPosition[ i ].x - _childrenRederers[ i ].bounds.size.x )
					_startPosition[ i ].x -= _childrenRederers[ i ].bounds.size.x;
				if ( transform.position.y * _speed.y > _startPosition[ i ].y + _childrenRederers[ i ].bounds.size.y )
					_startPosition[ i ].y += _childrenRederers[ i ].bounds.size.y;
				else if ( transform.position.y * _speed.y < _startPosition[ i ].y - _childrenRederers[ i ].bounds.size.y )
					_startPosition[ i ].y -= _childrenRederers[ i ].bounds.size.y;
			}
		}
	};
};
