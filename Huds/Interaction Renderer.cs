using GwambaPrimeAdventure.Character;
using UnityEngine;
using UnityEngine.UIElements;
namespace GwambaPrimeAdventure.Hud
{
	[DisallowMultipleComponent, RequireComponent( typeof( Collider2D ), typeof( IInteractable ) )]
	internal sealed class InteractionRenderer : StateController, IConnector
	{
		private
			Animator _animator;
		private
			UIDocument _document;
		private readonly int
			IsOn = Animator.StringToHash( nameof( IsOn ) );
		private bool
			_isActive = true,
			_isOnCollision = false;
		[SerializeField, Tooltip( "The UI document of the interaction." ), Header( "Interaction Components" )]
		private
			UIDocument _documentObject;
		[SerializeField, Tooltip( "The offset of the document of interaction." )]
		private
			Vector2 _imageOffset;
		public MessagePath Path =>
			MessagePath.Hud;
		private new void Awake()
		{
			base.Awake();
			_animator = GetComponent<Animator>();
			_document = Instantiate( _documentObject, transform );
			_document.transform.localPosition = _imageOffset;
			_document.enabled = false;
			Sender.Include( this );
		}
		private new void OnDestroy()
		{
			base.OnDestroy();
			Sender.Exclude( this );
		}
		private void OnEnable()
		{
			if ( _animator )
				_animator.SetFloat( IsOn, 1F );
		}
		private void OnDisable()
		{
			if ( _animator )
				_animator.SetFloat( IsOn, 0F );
		}
		private void OnTriggerEnter2D( Collider2D collision )
		{
			if ( ( _isOnCollision = CharacterExporter.EqualGwamba( collision.gameObject ) ) && _isActive )
				_document.enabled = true;
		}
		private void OnTriggerExit2D( Collider2D collision )
		{
			if ( !( _isOnCollision = !CharacterExporter.EqualGwamba( collision.gameObject ) ) )
				_document.enabled = false;
		}
		public void Receive( MessageData message )
		{
			if ( gameObject == message.AdditionalData as GameObject && MessageFormat.State == message.Format && message.ToggleValue.HasValue )
                _document.enabled = ( _isActive = message.ToggleValue.Value ) && _isOnCollision;
		}
	};
};
