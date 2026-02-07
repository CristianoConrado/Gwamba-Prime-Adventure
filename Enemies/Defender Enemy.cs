using Cysharp.Threading.Tasks;
using GwambaPrimeAdventure.Enemy.Supply;
using System.Threading;
using UnityEngine;
namespace GwambaPrimeAdventure.Enemy
{
	[DisallowMultipleComponent]
	internal sealed class DefenderEnemy : EnemyProvider, ILoader, IConnector, IDestructible
	{
		private readonly int
			Defend = Animator.StringToHash( nameof( Defend ) );
		private bool
			_invencible = false;
		private float
			_timeOperation = 0F;
		[SerializeField, Tooltip( "The defender statitics of this enemy." ), Header( "Defender Enemy" )]
		private
			DefenderStatistics _statistics;
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
			CancellationToken destroyToken = this.GetCancellationTokenOnDestroy();
			await UniTask.Yield( PlayerLoopTiming.EarlyUpdate, destroyToken, true ).SuppressCancellationThrow();
			if ( destroyToken.IsCancellationRequested )
				return;
			_timeOperation = _statistics.TimeToInvencible;
		}
		private void Update()
		{
			if ( _stopWorking || IsStunned || !_statistics.UseAlternatedTime && !_invencible || SceneInitiator.IsInTransition() )
				return;
			if ( 0F < _timeOperation )
				if ( 0F >= ( _timeOperation -= Time.deltaTime ) )
					if ( _invencible )
					{
						Animator.SetBool( Defend, _invencible = false );
						_timeOperation = _statistics.UseAlternatedTime ? _statistics.TimeToInvencible : _statistics.TimeToDestructible;
						if ( _statistics.InvencibleStop )
						{
							Animator.SetBool( Stop, false );
							_sender.SetToggle( true );
							_sender.Send( MessagePath.Enemy );
						}
					}
					else
					{
						Animator.SetBool( Defend, _invencible = true );
						_timeOperation = _statistics.TimeToDestructible;
						if ( _statistics.InvencibleStop )
						{
							Animator.SetBool( Stop, true );
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
					Animator.SetBool( Defend, _invencible = true );
					_timeOperation = _statistics.TimeToInvencible;
					if ( _statistics.InvencibleStop )
					{
						Animator.SetBool( Stop, false );
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
							{
								Animator.SetBool( Defend, _invencible = true );
								_timeOperation = _statistics.TimeToInvencible;
							}
							else if ( _statistics.TimedReact )
							{
								Animator.SetBool( Defend, _invencible = message.ToggleValue.Value );
								_timeOperation = _statistics.TimeToDestructible;
							}
							else
								Animator.SetBool( Defend, _invencible = message.ToggleValue.Value );
						if ( _statistics.InvencibleStop )
						{
							Animator.SetBool( Stop, _invencible );
							_sender.SetToggle( !_invencible );
							_sender.Send( MessagePath.Enemy );
						}
					}
		}
	};
};
