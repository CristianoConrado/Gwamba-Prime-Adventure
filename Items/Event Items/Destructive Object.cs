using UnityEngine;
using GwambaPrimeAdventure.Connection;
namespace GwambaPrimeAdventure.Item.EventItem
{
	[DisallowMultipleComponent, RequireComponent( typeof( Collider2D ), typeof( Receptor ) )]
	internal sealed class DestructiveObject : StateController, IReceptorSignal, IDestructible
	{
		private readonly Sender _sender = Sender.Create();
		[Header( "Destructive Object" )]
		[SerializeField, Tooltip( "If there a object that will be instantiate after the destruction of " )] private OcclusionObject _occlusionObject;
		[SerializeField, Tooltip( "The vitality of this object before it destruction." )] private short _vitality;
		[SerializeField, Tooltip( "The amount of damage that this object have to receive real damage." )] private short _biggerDamage;
		[SerializeField, Tooltip( "If this object will be destructed on collision with another object." )] private bool _destroyOnCollision;
		[SerializeField, Tooltip( "If this object will be saved as already existent object." )] private bool _saveOnSpecifics;
		public short Health => _vitality;
		private new void Awake()
		{
			base.Awake();
			_sender.SetFormat( MessageFormat.State );
			_sender.SetAdditionalData( _occlusionObject );
			_sender.SetToggle( true );
		}
		private void Start()
		{
			SaveController.Load( out SaveFile saveFile );
			if ( _saveOnSpecifics && saveFile.GeneralObjects.Contains( name ) )
				Destroy( gameObject );
		}
		public void Execute()
		{
			if ( _occlusionObject )
				_sender.Send( MessagePath.System );
			SaveController.Load( out SaveFile saveFile );
			if ( _saveOnSpecifics && !saveFile.GeneralObjects.Contains( name ) )
			{
				saveFile.GeneralObjects.Add( name );
				SaveController.WriteSave( saveFile );
			}
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
		public void Stun( ushort stunStength, float stunTime ) { }
	};
};
