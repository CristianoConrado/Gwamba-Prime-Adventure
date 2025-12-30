using UnityEngine;
using GwambaPrimeAdventure.Character;
namespace GwambaPrimeAdventure.Enemy.Supply
{
	[DisallowMultipleComponent, RequireComponent( typeof( Collider2D ) )]
	public sealed class TeleportPoint : StateController
	{
		private
			ITeleporter _teleporter;
		private
			ushort _teleportIndex;
		[SerializeField, Tooltip( "If this point will destroy itself after use." ), Header( "Interactions" )]
		private
			bool _destroyAfter;
		[SerializeField, Tooltip( "If this point will trigger with other object." )]
		private
			bool _hasTarget;
		public void GetTouch( ITeleporter teleporter, ushort teleportIndex )
		{
			_teleporter = teleporter;
			_teleportIndex = teleportIndex;
		}
		private void OnTriggerEnter2D( Collider2D other )
		{
			if ( _hasTarget )
			{
				if ( CharacterExporter.EqualGwamba( other.gameObject ) )
					_teleporter.OnTeleport( _teleportIndex );
			}
			else if ( other.TryGetComponent<ITeleporter>( out _ ) )
				_teleporter.OnTeleport( _teleportIndex );
			if ( _destroyAfter )
				Destroy( gameObject );
		}
	};
};
