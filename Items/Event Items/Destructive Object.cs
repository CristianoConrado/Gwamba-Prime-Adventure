using UnityEngine;
namespace GwambaPrimeAdventure.Item.EventItem
{
	[DisallowMultipleComponent, RequireComponent( typeof( Collider2D ), typeof( Receptor ) )]
	internal sealed class DestructiveObject : StateController, ISignalReceptor, IDestructible
	{
		private readonly Sender
			_sender = Sender.Create();
		[SerializeField, Tooltip( "If there a object that will be instantiate after the destruction of " ), Header( "Destructive Object" )]
		private
			OcclusionObject _occlusionObject;
		[SerializeField, Tooltip( "The amount of damage that this object have to receive real damage." )]
		private
			byte _biggerDamage;
		[SerializeField, Tooltip( "If this object will be destructed on collision with another object." )]
		private
			bool _destroyOnCollision;
		[field: SerializeField, Tooltip( "The vitality of this object before it destruction." )]
		public byte Health
		{
			get;
			private set;
		}
		public IDestructible Source => this;
		private void Start()
		{
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
		public bool Hurt( byte damage )
		{
			if ( damage < _biggerDamage || 0 >= Health )
				return false;
			if ( 0 >= ( Health = (byte) ( 0 <= Health - damage ? Health - damage : 0 ) ) )
				Execute();
			return true;
		}
		public void Stun( byte stunStrength, float stunTime ) { }
	};
};
