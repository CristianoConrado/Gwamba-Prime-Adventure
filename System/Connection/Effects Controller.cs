using UnityEngine;
using UnityEngine.U2D;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace GwambaPrimeAdventure.Connection
{
	[DisallowMultipleComponent, RequireComponent(typeof(Transform), typeof(Light2DBase))]
	public sealed class EffectsController : StateController
	{
		private static EffectsController _instance;
		private readonly List<Light2DBase> _lightsStack = new();
		private Collider2D _surfaceCollider;
		private bool _canHitStop = true;
		[SerializeField, Tooltip("The sounds of the surfaces that will be played.")] private SurfaceSound[] _surfaceSounds;
		[SerializeField, Tooltip("The source where the sounds came from.")] private AudioSource _sourceObject;
		[SerializeField, Tooltip("If the sentient objects will turn to the left instead of the right.")] private bool _turnToLeft;
		public static bool TurnToLeft => _instance._turnToLeft;
		private new void Awake()
		{
			base.Awake();
			if (_instance)
			{
				Destroy(gameObject, WorldBuild.MINIMUM_TIME_SPACE_LIMIT);
				return;
			}
			_instance = this;
			_lightsStack.Add(GetComponent<Light2DBase>());
		}
		private new void OnDestroy()
		{
			base.OnDestroy();
			StopAllCoroutines();
			_lightsStack.Clear();
		}
		private void OnEnable() => AudioListener.pause = false;
		private void OnDisable() => AudioListener.pause = true;
		private void PrvateHitStop(float stopTime, float slowTime)
		{
			if (_canHitStop)
				StartCoroutine(HitStop());
			IEnumerator HitStop()
			{
				_canHitStop = false;
				Time.timeScale = slowTime;
				yield return new WaitTime(this, stopTime, true);
				Time.timeScale = 1F;
				_canHitStop = true;
			}
		}
		private void PrivateGlobalLight(Light2DBase globalLight, bool active)
		{
			if ((active && !_lightsStack.Contains(globalLight) || !active && _lightsStack.Contains(globalLight)) && globalLight && _lightsStack[0])
			{
				for (ushort i = 0; _lightsStack.Count > i; i++)
					if (_lightsStack[i])
						_lightsStack[i].enabled = false;
				if (active)
					_lightsStack.Add(globalLight);
				else
					_lightsStack.Remove(globalLight);
				_lightsStack[^1].enabled = true;
			}
		}
		private void PrivateSoundEffect(AudioClip clip, Vector2 originSound)
		{
			SettingsController.Load(out Settings settings);
			AudioSource source = Instantiate(_sourceObject, originSound, Quaternion.identity);
			source.clip = clip;
			source.volume = 1F;
			source.mute = !settings.EffectsVolumeToggle || !settings.GeneralVolumeToggle;
			source.Play();
			StartCoroutine(SoundPlay(source, clip.length));
			IEnumerator SoundPlay(AudioSource source, float playTime)
			{
				while (0F < playTime)
				{
					playTime -= Time.deltaTime;
					yield return new WaitUntil(() => isActiveAndEnabled);
				}
				Destroy(source.gameObject);
			}
		}
		private void PrivateSurfaceSound(Vector2 originPosition)
		{
			if ((_surfaceCollider = Physics2D.OverlapCircle(originPosition, WorldBuild.SNAP_LENGTH, WorldBuild.SCENE_LAYER)) && _surfaceCollider.TryGetComponent<Surface>(out var surface))
				for (ushort i = 0; _surfaceSounds.Length > i; i++)
					if (_surfaceSounds[i].Tiles.Contains(surface.CheckForTile(originPosition)))
					{
						PrivateSoundEffect(_surfaceSounds[i].Clip, originPosition);
						return;
					}
		}
		public static void HitStop(float stopTime, float slowTime) => _instance.PrvateHitStop(stopTime, slowTime);
		public static void OnGlobalLight(Light2DBase globalLight) => _instance.PrivateGlobalLight(globalLight, true);
		public static void OffGlobalLight(Light2DBase globalLight) => _instance.PrivateGlobalLight(globalLight, false);
		public static void SoundEffect(AudioClip clip, Vector2 originSound) => _instance.PrivateSoundEffect(clip, originSound);
		public static void SurfaceSound(Vector2 originPosition) => _instance.PrivateSurfaceSound(originPosition);
	};
};
