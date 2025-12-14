using UnityEngine;
using System.Collections;
namespace GwambaPrimeAdventure
{
	[DisallowMultipleComponent, Icon(WorldBuild.PROJECT_ICON), RequireComponent(typeof(Transform), typeof(BoxCollider2D))]
	public sealed class OcclusionObject : MonoBehaviour, IConnector
	{
		[Header("Interactions")]
		[SerializeField, Tooltip("If this object will activate the children.")] private bool _initialActive;
		[SerializeField, Tooltip("If this object will turn off the collisions.")] private bool _offCollision;
		[SerializeField, Tooltip("If this object will occlude any other object that enter the collision.")] private bool _collisionOcclusion = true;
		public MessagePath Path => MessagePath.System;
		private void Awake()
		{
			GetComponent<BoxCollider2D>().enabled = !_offCollision;
			Sender.Include(this);
		}
		private void OnDestroy() => Sender.Exclude(this);
		private IEnumerator Start()
		{
			yield return new WaitWhile(() => SceneInitiator.IsInTrancision());
			StateController[] states = GetComponentsInChildren<StateController>(true);
			yield return new WaitUntil(() =>
			{
				for (ushort i = 0; states.Length > i; i++)
					if (states[i] && !states[i].enabled)
						return false;
				return true;
			});
			Execution(_initialActive);
		}
		internal void Execution(bool activate)
		{
			for (ushort i = 0; transform.childCount > i; i++)
				transform.GetChild(i).gameObject.SetActive(activate);
		}
		private void OnTriggerEnter2D(Collider2D other)
		{
			if (_collisionOcclusion && other.TryGetComponent<IOccludee>(out var occludee) && occludee.Occlude && !other.transform.parent)
				other.transform.SetParent(transform);
		}
		public void Receive(MessageData message)
		{
			if (this == message.AdditionalData as OcclusionObject && MessageFormat.State == message.Format && message.ToggleValue.HasValue)
				Execution(message.ToggleValue.Value);
		}
	};
};
