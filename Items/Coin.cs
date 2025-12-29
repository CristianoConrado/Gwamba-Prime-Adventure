using UnityEngine;
using GwambaPrimeAdventure.Connection;
namespace GwambaPrimeAdventure.Item
{
	[DisallowMultipleComponent, RequireComponent( typeof( SpriteRenderer ), typeof( Animator ), typeof( CircleCollider2D ) )]
	internal sealed class Coin : StateController, ICollectable, IConnector
	{
		private
			SpriteRenderer _spriteRenderer;
		private
			Animator _animator;
		private
			CircleCollider2D _collider;
		[SerializeField, Tooltip( "If this object will be saved as already existent object." ), Header( "Condition" )]
		private bool
			_saveOnSpecifics;
		public MessagePath Path =>
			MessagePath.Item;
		private new void Awake()
		{
			base.Awake();
			_spriteRenderer = GetComponent<SpriteRenderer>();
			_animator = GetComponent<Animator>();
			_collider = GetComponent<CircleCollider2D>();
			Sender.Include( this );
		}
		private new void OnDestroy()
		{
			base.OnDestroy();
			Sender.Exclude( this );
		}
		private void OnEnable()
		{
			if ( _spriteRenderer.enabled )
				_animator.enabled = true;
		}
		private void OnDisable()
		{
			if ( _spriteRenderer.enabled )
				_animator.enabled = false;
		}
		public void Collect()
		{
			SaveController.Load( out SaveFile saveFile );
			if ( 100 > saveFile.Coins )
				saveFile.Coins += 1;
			if ( 100 > saveFile.Lifes && 100 <= saveFile.Coins )
			{
				saveFile.Coins = 0;
				saveFile.Lifes += 1;
			}
			if ( 100 <= saveFile.Lifes && 99 <= saveFile.Coins )
				saveFile.Coins = 100;
			if ( _saveOnSpecifics && !saveFile.GeneralObjects.Contains( name ) )
				saveFile.GeneralObjects.Add( name );
			SaveController.WriteSave( saveFile );
			_collider.enabled = _animator.enabled = _spriteRenderer.enabled = false;
		}
		public void Receive( MessageData message )
		{
			if ( MessageFormat.Event == message.Format && message.ToggleValue.HasValue && !message.ToggleValue.Value )
				_collider.enabled = _animator.enabled = _spriteRenderer.enabled = true;
		}
	};
};
