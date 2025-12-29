using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
namespace GwambaPrimeAdventure.Item.EventItem
{
	[DisallowMultipleComponent, RequireComponent( typeof( Collider2D ), typeof( Receptor ) )]
	internal sealed class DestructiveObject : StateController, ILoader, IReceptorSignal, IDestructible
	{
		private readonly Sender
			_sender = Sender.Create();
		[SerializeField, Tooltip( "If there a object that will be instantiate after the destruction of " ), Header( "Destructive Object" )]
		private OcclusionObject
			_occlusionObject;
		[SerializeField, Tooltip( "The vitality of this object before it destruction." )]
		private short
			_vitality;
		[SerializeField, Tooltip( "The amount of damage that this object have to receive real damage." )]
		private short
			_biggerDamage;
		[SerializeField, Tooltip( "If this object will be destructed on collision with another object." )]
		private bool
			_destroyOnCollision;
		public short Health =>
			_vitality;
		public async UniTask Load()
		{
			CancellationToken destroyToken = this.GetCancellationTokenOnDestroy();
			await UniTask.Yield( PlayerLoopTiming.EarlyUpdate, destroyToken, true ).SuppressCancellationThrow();
			if ( destroyToken.IsCancellationRequested )
				return;
			_sender.SetFormat( MessageFormat.State );
			_sender.SetAdditionalData( _occlusionObject );
			_sender.SetToggle( true );
		}
		public void Execute()
		{
			if ( _occlusionObject )
				_sender.Send( MessagePath.System );
			Destroy( gameObject );
		}
		private void DestroyOnCollision()
		{
			if ( _destroyOnCollision )
				Execute();
		}
		private void OnCollisionEnter2D( Collision2D collision ) => DestroyOnCollision();
		private void OnTriggerEnter2D( Collider2D collision ) => DestroyOnCollision();
		public bool Hurt( ushort damage )
		{
			if ( damage < _biggerDamage || 0 >= _vitality )
				return false;
			if ( 0 >= ( _vitality -= (short) damage ) )
				Execute();
			return true;
		}
		public void Stun( ushort stunStrength, float stunTime ) { }
	};
};
