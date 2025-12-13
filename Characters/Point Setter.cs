using UnityEngine;
using System.Collections;
using NaughtyAttributes;
using GwambaPrimeAdventure.Connection;
namespace GwambaPrimeAdventure.Character
{
	[DisallowMultipleComponent, Icon("Assets/Huds/Designs/Gwamba Prime Series Logo.png"), RequireComponent(typeof(Transform), typeof(BoxCollider2D))]
	internal sealed class PointSetter : StateController, IConnector
	{
		private static PointSetter _instance;
		private readonly Sender _sender = Sender.Create();
		[SerializeField, BoxGroup("Hubby World Interaction"), Tooltip("The name of the hubby world scene."), Space(WorldBuild.FIELD_SPACE_LENGTH * 2F)] private SceneField _hubbyWorldScene;
		[SerializeField, BoxGroup("Hubby World Interaction"), Tooltip("Which point setter is setted when scene is the hubby world.")] private ushort _selfIndex;
		public MessagePath Path => MessagePath.Character;
		private new void Awake()
		{
			base.Awake();
			_sender.SetFormat(MessageFormat.Event);
			_sender.SetAdditionalData((Vector2)transform.position);
			_sender.SetToggle(false);
			Sender.Include(this);
		}
		private new void OnDestroy()
		{
			base.OnDestroy();
			Sender.Exclude(this);
		}
		private IEnumerator Start()
		{
			yield return new WaitWhile(() => SceneInitiator.IsInTrancision());
			SaveController.Load(out SaveFile saveFile);
			if (gameObject.scene.name == _hubbyWorldScene && !string.IsNullOrEmpty(saveFile.LastLevelEntered))
				if (saveFile.LastLevelEntered.Contains($"{_selfIndex}"))
					_sender.Send(MessagePath.Character);
		}
		private void OnTriggerEnter2D(Collider2D other)
		{
			if (GwambaStateMarker.EqualObject(other.gameObject) && this != _instance)
				_instance = this;
		}
		public void Receive(MessageData message)
		{
			if (MessageFormat.Event == message.Format && message.ToggleValue.HasValue && message.ToggleValue.Value && this == _instance)
				_sender.Send(MessagePath.Character);
		}
	};
};
