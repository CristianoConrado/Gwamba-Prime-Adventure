using UnityEngine;
using Cysharp.Threading.Tasks;
using GwambaPrimeAdventure.Enemy.Supply;
namespace GwambaPrimeAdventure.Enemy
{
	[DisallowMultipleComponent]
	internal sealed class DefenderEnemy : EnemyProvider, ILoader, IConnector, IDestructible
	{
		private bool _invencible = false;
		private float _timeOperation = 0F;
		[Header( "Defender Enemy" )]
		[SerializeField, Tooltip( "The defender statitics of this enemy." )] private DefenderStatistics _statistics;
		private new void Awake()
		{
			base.Awake();
			_sender.SetFormat( MessageFormat.State );
			Sender.Include( this );
		}
		private new void OnDestroy()
		{
			base.OnDestroy();
			Sender.Exclude( this );
		}
		public async UniTask Load()
		{
			_timeOperation = _statistics.TimeToInvencible;
			await UniTask.WaitForEndOfFrame();
		}
		private void Update()
		{
			if ( _stopWorking || IsStunned || !_statistics.UseAlternatedTime && !_invencible )
				return;
			if ( 0F < _timeOperation )
				if ( 0F >= ( _timeOperation -= Time.deltaTime ) )
					if ( _invencible )
					{
						(_invencible, _timeOperation) = (false, _statistics.UseAlternatedTime ? _statistics.TimeToInvencible : _statistics.TimeToDestructible);
						if ( _statistics.InvencibleStop )
						{
							_sender.SetToggle( true );
							_sender.Send( MessagePath.Enemy );
						}
					}
					else
					{
						(_invencible, _timeOperation) = (true, _statistics.TimeToDestructible);
						if ( _statistics.InvencibleStop )
						{
							_sender.SetToggle( false );
							_sender.Send( MessagePath.Enemy );
						}
					}
		}
		public new bool Hurt( ushort damage )
		{
			bool isHurted = false;
			if ( !_invencible && _statistics.BiggerDamage <= damage )
				if ( ( isHurted = base.Hurt( damage ) ) && _statistics.InvencibleHurted )
				{
					(_timeOperation, _invencible) = (_statistics.TimeToDestructible, true);
					if ( _statistics.InvencibleStop )
					{
						_sender.SetToggle( true );
						_sender.Send( MessagePath.Enemy );
					}
				}
			return isHurted;
		}
		public void Receive( MessageData message )
		{
			if ( message.AdditionalData is not null && message.AdditionalData is EnemyProvider[] enemies && 0 < enemies.Length )
				foreach ( EnemyProvider enemy in enemies )
					if ( enemy && this == enemy )
					{
						if ( MessageFormat.Event == message.Format && _statistics.ReactToDamage && message.ToggleValue.HasValue )
							if ( _statistics.UseAlternatedTime && message.ToggleValue.Value )
								(_invencible, _timeOperation) = (true, _statistics.TimeToDestructible);
							else
								(_invencible, _timeOperation) = (message.ToggleValue.Value, _statistics.TimeToDestructible);
						if ( _statistics.InvencibleStop )
						{
							_sender.SetToggle( !_invencible );
							_sender.Send( MessagePath.Enemy );
						}
						return;
					}
		}
	};
};
