using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using GwambaPrimeAdventure.Connection;
namespace GwambaPrimeAdventure.Story
{
	[DisallowMultipleComponent, Icon(WorldBuild.PROJECT_ICON), RequireComponent(typeof(Transform), typeof(Collider2D))]
	internal sealed class InteractiveDialog : MonoBehaviour, IInteractable, IConnector
	{
		private DialogHud _dialogHud;
		private StoryTeller _storyTeller;
		private Animator _animator;
		private WaitForSeconds _dialogWait;
		private readonly Sender _sender = Sender.Create();
		private readonly int IsOn = Animator.StringToHash(nameof(IsOn));
		private string _text = "";
		private ushort _speachIndex = 0;
		private float _dialogTime = 0F;
		private bool _nextSlide = false;
		[Header("Interaction Objects")]
		[SerializeField, Tooltip("The object that handles the hud of the dialog.")] private DialogHud _dialogHudObject;
		[SerializeField, Tooltip("The collection of the object that contais the dialog.")] private DialogObject _dialogObject;
		public MessagePath Path => MessagePath.Story;
		private void Awake()
		{
			_storyTeller = GetComponent<StoryTeller>();
			_animator = GetComponent<Animator>();
			_sender.SetFormat(MessageFormat.State);
			_sender.SetAdditionalData(gameObject);
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
		private IEnumerator TextDigitation()
		{
			if (_nextSlide)
			{
				_nextSlide = false;
				yield return StartCoroutine(_storyTeller.NextSlide());
				_dialogHud.RootElement.style.display = DisplayStyle.Flex;
			}
			_dialogWait = new WaitForSeconds(_dialogTime);
			_dialogHud.CharacterIcon.style.backgroundImage = new StyleBackground(_dialogObject.Speachs[_speachIndex].Model);
			_dialogHud.CharacterName.text = _dialogObject.Speachs[_speachIndex].CharacterName;
			_text = _dialogObject.Speachs[_speachIndex].SpeachText;
			_dialogHud.CharacterSpeach.text = "";
			foreach (char letter in _text.ToCharArray())
			{
				_dialogHud.CharacterSpeach.text += letter;
				yield return _dialogWait;
			}
		}
		private void AdvanceSpeach()
		{
			if (_dialogHud.CharacterSpeach.text.Length == _text.Length && _dialogHud.CharacterSpeach.text == _text)
			{
				SettingsController.Load(out Settings settings);
				_dialogTime = settings.SpeachDelay;
				if (_dialogObject.Speachs.Length - 1 > _speachIndex)
				{
					if (_storyTeller && _dialogObject.Speachs[_speachIndex].NextSlide)
					{
						_nextSlide = true;
						_dialogHud.RootElement.style.display = DisplayStyle.None;
					}
					_speachIndex += 1;
					StartCoroutine(TextDigitation());
				}
				else
				{
					_text = null;
					_speachIndex = 0;
					_dialogHud.CharacterIcon.style.backgroundImage = null;
					_dialogHud.CharacterName.text = "";
					_dialogHud.CharacterSpeach.text = "";
					_dialogHud.AdvanceSpeach.clicked -= AdvanceSpeach;
					Destroy(_dialogHud.gameObject);
					StateController.SetState(true);
					if (_storyTeller)
						_storyTeller.CloseScene();
					SaveController.Load(out SaveFile saveFile);
					if (_dialogObject.SaveOnEspecific && !saveFile.GeneralObjects.Contains(name))
					{
						saveFile.GeneralObjects.Add(name);
						SaveController.WriteSave(saveFile);
					}
					if (_dialogObject.Transition && TryGetComponent<Transitioner>(out var transitioner))
						transitioner.Transicion(_dialogObject.SceneToTransition);
					else if (_dialogObject.ActivateAnimation)
						_animator.SetTrigger(_dialogObject.Animation);
					else if (_dialogObject.EndDestroy)
						Destroy(gameObject, _dialogObject.TimeToDestroy);
					else if (_dialogObject.DesactiveInteraction)
					{
						_sender.SetAdditionalData(null);
						Destroy(this);
					}
					_sender.SetToggle(true);
					_sender.Send(MessagePath.Hud);
				}
			}
			else
				_dialogTime = 0F;
		}
		public void Interaction()
		{
			SettingsController.Load(out Settings settings);
			if (settings.DialogToggle && _dialogObject && _dialogHudObject)
			{
				_sender.SetToggle(false);
				_sender.Send(MessagePath.Hud);
				StateController.SetState(false);
				_dialogHud = Instantiate(_dialogHudObject, transform);
				_dialogTime = settings.SpeachDelay;
				_dialogHud.AdvanceSpeach.clicked += AdvanceSpeach;
				StartCoroutine(TextDigitation());
				if (_storyTeller)
					_storyTeller.ShowScene();
			}
		}
		public void Receive(MessageData message)
		{
			if (MessageFormat.Event == message.Format && gameObject == message.AdditionalData as GameObject)
				Interaction();
		}
	};
};
