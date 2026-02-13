using UnityEngine;
namespace GwambaPrimeAdventure.Character
{
	public static class CharacterExporter
	{
		public static Vector2 GwambaLocalization() => GwambaMarker.Instance ? GwambaMarker.Instance.transform.position : Vector2.zero;
		public static bool EqualGwamba( params GameObject[] others ) => GwambaMarker.Instance ? GwambaMarker.Instance.EqualObject( others ) : false;
	};
};
