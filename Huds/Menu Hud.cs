using UnityEngine;
using UnityEngine.UIElements;
namespace GwambaPrimeAdventure.Hud
{
	[DisallowMultipleComponent, Icon( WorldBuild.PROJECT_ICON ), RequireComponent( typeof( Transform ), typeof( UIDocument ) )]
	internal sealed class MenuHud : MonoBehaviour
	{
		private static
		  MenuHud _instance;
		internal GroupBox Buttons
		{
			get;
			private set;
		}
		internal GroupBox Saves
		{
			get;
			private set;
		}
		internal Button Play
		{
			get;
			private set;
		}
		internal Button Configurations
		{
			get;
			private set;
		}
		internal Button Quit
		{
			get;
			private set;
		}
		internal Button Back
		{
			get;
			private set;
		}
		internal TextField[] SaveName
		{
			get;
			private set;
		}
		internal Button[] RenameFile
		{
			get;
			private set;
		}
		internal Button[] Load
		{
			get;
			private set;
		}
		internal Button[] Delete
		{
			get;
			private set;
		}
		private void Awake()
		{
			if ( _instance )
			{
				Destroy( gameObject, WorldBuild.MINIMUM_TIME_SPACE_LIMIT );
				return;
			}
			_instance = this;
			VisualElement root = GetComponent<UIDocument>().rootVisualElement;
			Buttons = root.Q<GroupBox>( nameof( Buttons ) );
			Saves = root.Q<GroupBox>( nameof( Saves ) );
			Play = root.Q<Button>( nameof( Play ) );
			Configurations = root.Q<Button>( nameof( Configurations ) );
			Quit = root.Q<Button>( nameof( Quit ) );
			Back = root.Q<Button>( nameof( Back ) );
			SaveName = new TextField[ 4 ];
			for ( ushort i = 0; SaveName.Length > i++; )
				SaveName[ i ] = root.Q<TextField>( $"{nameof( SaveName )}{1 + i}" );
			RenameFile = new Button[ 4 ];
			for ( ushort i = 0; SaveName.Length > i++; )
				RenameFile[ i ] = root.Q<Button>( $"{nameof( RenameFile )}{1 + i}" );
			Load = new Button[ 4 ];
			for ( ushort i = 0; Load.Length > i++; )
				Load[ i ] = root.Q<Button>( $"{nameof( Load )}{1 + i}" );
			Delete = new Button[ 4 ];
			for ( ushort i = 0; SaveName.Length > i++; )
				Delete[ i ] = root.Q<Button>( $"{nameof( Delete )}{1 + i}" );
		}
	};
};
