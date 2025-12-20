using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;
using GwambaPrimeAdventure.Connection;
namespace GwambaPrimeAdventure.Hud
{
	[DisallowMultipleComponent, Icon( WorldBuild.PROJECT_ICON ), RequireComponent( typeof( Transform ), typeof( Transitioner ) )]
	internal sealed class DeathScreenController : MonoBehaviour, IConnector
	{
		private static DeathScreenController _instance;
		private DeathScreenHud _deathScreenHud;
		private readonly Sender _sender = Sender.Create();
		[Header( "Interaction Object" )]
		[SerializeField, Tooltip( "The object that handles the hud of the death screen." )] private DeathScreenHud _deathScreenHudObject;
		[SerializeField, Tooltip( "The scene of the level selector." )] private SceneField _levelSelectorScene;
		[SerializeField, Tooltip( "The scene of the menu." )] private SceneField _menuScene;
		[SerializeField, Tooltip( "The scene of the boss of actual scene." )] private SceneField _bossScene;
		public MessagePath Path => MessagePath.Hud;
		private void Awake()
		{
			if ( _instance )
			{
				Destroy( gameObject, WorldBuild.MINIMUM_TIME_SPACE_LIMIT );
				return;
			}
			(_instance, _deathScreenHud) = (this, Instantiate( _deathScreenHudObject, transform ));
			SceneManager.sceneLoaded += SceneLoaded;
			Sender.Include( this );
		}
		private void OnDestroy()
		{
			if ( !_instance || this != _instance )
				return;
			_deathScreenHud.Continue.clicked -= Continue;
			_deathScreenHud.OutLevel.clicked -= OutLevel;
			_deathScreenHud.GameOver.clicked -= GameOver;
			SceneManager.sceneLoaded -= SceneLoaded;
			Sender.Exclude( this );
		}
		private IEnumerator Start()
		{
			if ( !_instance || this != _instance )
				yield break;
			yield return new WaitWhile( () => SceneInitiator.IsInTrancision() );
			_deathScreenHud.Continue.clicked += Continue;
			_deathScreenHud.OutLevel.clicked += OutLevel;
			_deathScreenHud.GameOver.clicked += GameOver;
			DontDestroyOnLoad( gameObject );
		}
		private void SceneLoaded( Scene scene, LoadSceneMode loadMode )
		{
			if ( scene.name == _levelSelectorScene || scene.name == _menuScene )
				Destroy( gameObject );
			else
			{
				_deathScreenHud.Text.text = "You have died";
				_deathScreenHud.OutLevel.style.display = _deathScreenHud.Continue.style.display = _deathScreenHud.Text.style.display = DisplayStyle.Flex;
				_deathScreenHud.RootElement.style.display = _deathScreenHud.Curtain.style.display = _deathScreenHud.GameOver.style.display = DisplayStyle.None;
			}
		}
		private void Continue()
		{
			if ( _bossScene is not null && SceneManager.GetActiveScene().name == _bossScene )
				GetComponent<Transitioner>().Transicion( _bossScene );
			else
				StartCoroutine( Curtain() );
			IEnumerator Curtain()
			{
				_deathScreenHud.GameOver.style.display = _deathScreenHud.OutLevel.style.display = _deathScreenHud.Continue.style.display = _deathScreenHud.Text.style.display = DisplayStyle.None;
				_deathScreenHud.Curtain.style.display = DisplayStyle.Flex;
				for ( float i = 0F; 1F > _deathScreenHud.Curtain.style.opacity.value; i += 5E-2F )
					yield return _deathScreenHud.Curtain.style.opacity = i;
				_sender.SetToggle( false );
				_sender.SetFormat( MessageFormat.Event );
				_sender.Send( MessagePath.System );
				_sender.Send( MessagePath.Character );
				_sender.Send( MessagePath.Item );
				for ( float i = 1F; 0F < _deathScreenHud.Curtain.style.opacity.value; i -= 5E-2F )
					yield return _deathScreenHud.Curtain.style.opacity = i;
				_sender.SetToggle( true );
				_sender.Send( MessagePath.System );
				_sender.Send( MessagePath.Character );
				_sender.SetFormat( MessageFormat.None );
				_sender.Send( MessagePath.Enemy );
				ConfigurationController.SetActive( true );
				_deathScreenHud.RootElement.style.display = DisplayStyle.None;
				_deathScreenHud.Text.text = "You have died";
				_deathScreenHud.OutLevel.style.display = _deathScreenHud.Continue.style.display = _deathScreenHud.Text.style.display = DisplayStyle.Flex;
				_deathScreenHud.Curtain.style.display = _deathScreenHud.GameOver.style.display = DisplayStyle.None;
			}
		}
		private void OutLevel() => GetComponent<Transitioner>().Transicion( _levelSelectorScene );
		private void GameOver()
		{
			SaveController.RefreshData();
			GetComponent<Transitioner>().Transicion();
		}
		public void Receive( MessageData message )
		{
			if ( MessageFormat.Event == message.Format && message.ToggleValue.HasValue && !message.ToggleValue.Value )
			{
				SaveController.Load( out SaveFile saveFile );
				if ( 0 >= saveFile.Lifes )
				{
					_deathScreenHud.Text.text = "Game Over";
					_deathScreenHud.OutLevel.style.display = _deathScreenHud.Continue.style.display = DisplayStyle.None;
					_deathScreenHud.GameOver.style.display = DisplayStyle.Flex;
				}
				_deathScreenHud.RootElement.style.display = DisplayStyle.Flex;
			}
		}
	};
};
