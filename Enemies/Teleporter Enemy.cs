using UnityEngine;
using System.Collections;
using GwambaPrimeAdventure.Enemy.Supply;
namespace GwambaPrimeAdventure.Enemy
{
	[DisallowMultipleComponent]
	internal sealed class TeleporterEnemy : EnemyProvider, ILoader, ITeleporter
	{
		private float _teleportTime = 0F;
		private ushort _teleportIndex = 0;
		private bool _canTeleport = true;
		[Header( "Teleporter Enemy" )]
		[SerializeField, Tooltip( "The teleporter statitics of this enemy." )] private TeleporterStatistics _statistics;
		public IEnumerator Load()
		{
			for ( ushort i = 0; _statistics.TeleportPointStructures.Length > i; i++ )
				Instantiate( _statistics.TeleportPointStructures[ i ].TeleportPointObject, _statistics.TeleportPointStructures[ i ].InstancePoint, Quaternion.identity ).GetTouch( this, i );
			yield return null;
		}
		private void Update()
		{
			if ( IsStunned )
				return;
			if ( 0F < _teleportTime )
				_canTeleport = 0F >= ( _teleportTime -= Time.deltaTime );
		}
		public void OnTeleport( ushort teleportIndex )
		{
			if ( _canTeleport )
			{
				if ( _statistics.TeleportPointStructures[ teleportIndex ].RandomTeleports )
					_teleportIndex = (ushort) Random.Range( 0, _statistics.TeleportPointStructures[ teleportIndex ].TeleportPoints.Length );
				transform.position = _statistics.TeleportPointStructures[ teleportIndex ].TeleportPoints[ _teleportIndex ];
				if ( !_statistics.TeleportPointStructures[ teleportIndex ].RandomTeleports )
					_teleportIndex = (ushort) ( _teleportIndex < _statistics.TeleportPointStructures[ teleportIndex ].TeleportPoints.Length - 1 ? _teleportIndex + 1 : 0 );
				(_canTeleport, _teleportTime) = (false, _statistics.TimeToUse);
			}
		}
	};
};
