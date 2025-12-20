using UnityEngine;
namespace GwambaPrimeAdventure.Character
{
    public static class CharacterExporter
    {
		public static Vector2 GwambaLocalization() => GwambaState<GwambaMarker>.Instance ? GwambaState<GwambaMarker>.Instance.transform.position : Vector2.zero;
		public static bool EqualGwamba( params GameObject[] others ) => GwambaState<GwambaMarker>.Instance ? GwambaState<GwambaMarker>.Instance.EqualObject( others ) : false;
	};
};