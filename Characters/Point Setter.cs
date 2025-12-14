using UnityEngine;
using System.Collections;
using NaughtyAttributes;
using GwambaPrimeAdventure.Connection;
namespace GwambaPrimeAdventure.Character
{
	[DisallowMultipleComponent, RequireComponent(typeof(Transform), typeof(BoxCollider2D))]
	internal sealed class PointSetter : StateController, ILoader
	{
		private static PointSetter _instance;
		[SerializeField, BoxGroup("Hubby World Interaction"), Tooltip("The name of the hubby world scene."), Space(WorldBuild.FIELD_SPACE_LENGTH * 2F)] private SceneField _hubbyWorldScene;
		[SerializeField, BoxGroup("Hubby World Interaction"), Tooltip("Which point setter is setted when scene is the hubby world.")] private ushort _selfIndex;
		internal static Vector2 CheckedPoint => _instance ? _instance.transform.position : Vector2.zero;
		public IEnumerator Load()
		{
			SaveController.Load(out SaveFile saveFile);
			if (gameObject.scene.name == _hubbyWorldScene && !string.IsNullOrEmpty(saveFile.LastLevelEntered))
				if (saveFile.LastLevelEntered.Contains($"{_selfIndex}"))
					_instance = this;
			yield return null;
		}
		private void OnTriggerEnter2D(Collider2D other)
		{
			if (GwambaStateMarker.EqualObject(other.gameObject) && this != _instance)
				_instance = this;
		}
	};
};
