using UnityEngine;
using UnityEngine.Tilemaps;
namespace GwambaPrimeAdventure
{
	[CreateAssetMenu( fileName = "Surface Sound", menuName = "Surface/Sound", order = 0 )]
	public class SurfaceSound : ScriptableObject
	{
		[field: SerializeField, Tooltip( "The tiles that contain the sound clip specified." ), Header( "Surfaces" ), Space( WorldBuild.FIELD_SPACE_LENGTH * 2F )] public Tile[] Tiles { get; private set; }
		[field: SerializeField, Tooltip( "The sound clip that the tiles contains to be played." )] public AudioClip Clip { get; private set; }
	};
};
