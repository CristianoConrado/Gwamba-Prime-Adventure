using UnityEngine;
using UnityEngine.UIElements;
namespace GwambaPrimeAdventure.Hud
{
	[DisallowMultipleComponent, Icon(WorldBuild.PROJECT_ICON), RequireComponent(typeof(Transform), typeof(UIDocument))]
	internal sealed class MenuHud : MonoBehaviour
	{
		private static MenuHud _instance;
		internal GroupBox Buttons { get; private set; }
		internal GroupBox Saves { get; private set; }
		internal Button Play { get; private set; }
		internal Button Configurations { get; private set; }
		internal Button Quit { get; private set; }
		internal Button Back { get; private set; }
		internal TextField[] SaveName { get; private set; }
		internal Button[] RenameFile { get; private set; }
		internal Button[] Load { get; private set; }
		internal Button[] Delete { get; private set; }
		private void Awake()
		{
			if (_instance)
			{
				Destroy(gameObject, WorldBuild.MINIMUM_TIME_SPACE_LIMIT);
				return;
			}
			_instance = this;
			VisualElement root = GetComponent<UIDocument>().rootVisualElement;
			(Buttons, Play, Configurations, Quit) = (root.Q<GroupBox>(nameof(Buttons)), root.Q<Button>(nameof(Play)), root.Q<Button>(nameof(Configurations)), root.Q<Button>(nameof(Quit)));
			(Saves, Back) = (root.Q<GroupBox>(nameof(Saves)), root.Q<Button>(nameof(Back)));
			SaveName = new TextField[4];
			for (ushort i = 0; i < SaveName.Length; i++)
				SaveName[i] = root.Q<TextField>($"{nameof(SaveName)}{i + 1}");
			RenameFile = new Button[4];
			for (ushort i = 0; i < RenameFile.Length; i++)
				RenameFile[i] = root.Q<Button>($"{nameof(RenameFile)}{i + 1}");
			Load = new Button[4];
			for (ushort i = 0; i < Load.Length; i++)
				Load[i] = root.Q<Button>($"{nameof(Load)}{i + 1}");
			Delete = new Button[4];
			for (ushort i = 0; i < Delete.Length; i++)
				Delete[i] = root.Q<Button>($"{nameof(Delete)}{i + 1}");
		}
	};
};
