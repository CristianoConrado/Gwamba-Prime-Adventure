using UnityEngine;
using GwambaPrimeAdventure.Character;
namespace GwambaPrimeAdventure.Enemy.Supply
{
	[DisallowMultipleComponent, RequireComponent( typeof( Collider2D ) )]
	public sealed class JumpPoint : StateController
	{
		private
			IJumper _jumper;
		private
			ushort _touchIndex;
		[SerializeField, Tooltip( "If this point will destroy itself after use." ), Header( "Interactions" )]
		private bool
			_destroyAfter;
		[SerializeField, Tooltip( "If this point will trigger with other object." )]
		private bool
			_hasTarget;
		public void GetTouch( IJumper jumperEnemy, ushort touchIndex )
		{
			_jumper = jumperEnemy;
			_touchIndex = touchIndex;
		}
		private void OnTriggerEnter2D( Collider2D other )
		{
			if ( _hasTarget )
			{
				if ( CharacterExporter.EqualGwamba( other.gameObject ) )
					_jumper.OnJump( _touchIndex );
			}
			else if ( other.TryGetComponent<IJumper>( out _ ) )
				_jumper.OnJump( _touchIndex );
			if ( _destroyAfter )
				Destroy( gameObject );
		}
	};
};
