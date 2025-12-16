using UnityEngine;
using UnityEngine.UIElements;
using GwambaPrimeAdventure.Character;
namespace GwambaPrimeAdventure.Hud
{
	[DisallowMultipleComponent, RequireComponent(typeof(Transform), typeof(Collider2D), typeof(IInteractable))]
	internal sealed class InteractionRenderer : StateController, IConnector
	{
		private Animator _animator;
		private UIDocument _document;
		private readonly int IsOn = Animator.StringToHash(nameof(IsOn));
		private bool
			_isActive = true,
			_isOnCollision = false;
		[Header("Interaction Components")]
		[SerializeField, Tooltip("The UI document of the interaction.")] private UIDocument _documentObject;
		[SerializeField, Tooltip("The offset of the document of interaction.")] private Vector2 _imageOffset;
		public MessagePath Path => MessagePath.Hud;
		private new void Awake()
		{
			base.Awake();
			(_animator, _document) = (GetComponent<Animator>(), Instantiate(_documentObject, transform));
			(_document.transform.localPosition, _document.enabled) = (_imageOffset, false);
			Sender.Include(this);
		}
		private new void OnDestroy()
		{
			base.OnDestroy();
			Sender.Exclude(this);
		}
		private void OnEnable()
		{
			if (_animator)
				_animator.SetFloat(IsOn, 1F);
		}
		private void OnDisable()
		{
			if (_animator)
				_animator.SetFloat(IsOn, 0F);
		}
		private void OnTriggerEnter2D(Collider2D collision)
		{
			if ((_isOnCollision = GwambaStateMarker.EqualObject(collision.gameObject)) && _isActive)
				_document.enabled = true;
		}
		private void OnTriggerExit2D(Collider2D collision)
		{
			if (!(_isOnCollision = !GwambaStateMarker.EqualObject(collision.gameObject)))
				_document.enabled = false;
		}
		public void Receive(MessageData message)
		{
			if (gameObject == message.AdditionalData as GameObject && MessageFormat.State == message.Format && message.ToggleValue.HasValue)
				if (message.ToggleValue.Value)
				{
					_isActive = true;
					if (_isOnCollision)
						_document.enabled = true;
				}
				else
					_isActive = _document.enabled = false;
		}
	};
};
