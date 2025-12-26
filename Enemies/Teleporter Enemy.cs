using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using GwambaPrimeAdventure.Enemy.Supply;
namespace GwambaPrimeAdventure.Enemy
{
	[DisallowMultipleComponent]
	internal sealed class TeleporterEnemy : EnemyProvider, ILoader, ITeleporter
	{
		private float
			_teleportTime = 0F;
		private ushort
			_teleportIndex = 0;
		private bool
			_canTeleport = true;
		[SerializeField, Tooltip( "The teleporter statitics of this enemy." ), Header( "Teleporter Enemy" )] private TeleporterStatistics
			_statistics;
		public async UniTask Load()
		{
			CancellationToken destroyToken = this.GetCancellationTokenOnDestroy();
			await UniTask.Yield( PlayerLoopTiming.EarlyUpdate, destroyToken, true ).SuppressCancellationThrow();
			if ( destroyToken.IsCancellationRequested )
				return;
			TeleportPoint teleportPoint;
			for ( ushort i = 0; _statistics.TeleportPointStructures.Length > i; i++ )
			{
				teleportPoint = Instantiate( _statistics.TeleportPointStructures[ i ].TeleportPointObject, _statistics.TeleportPointStructures[ i ].InstancePoint, Quaternion.identity );
				teleportPoint.GetTouch( this, i );
			}
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
				_canTeleport = false;
				_teleportTime = _statistics.TimeToUse;
			}
		}
	};
};
