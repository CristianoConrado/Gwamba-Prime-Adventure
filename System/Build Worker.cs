using UnityEngine;
using System.Collections.Generic;
namespace GwambaPrimeAdventure
{
    public static class BuildWorker
    {
		private static Vector3 _scaleTurner = Vector3.zero;
		public static Vector2 OrthographicToRealSize( float orthographicSize ) => new Vector2( orthographicSize * 2F * WorldBuild.HEIGHT_WIDTH_PROPORTION, orthographicSize * 2F );
		public static Vector2 OrthographicToScreenSize( float orthographicSize ) => OrthographicToRealSize( orthographicSize ) * WorldBuild.PIXELS_PER_UNIT;
		public static Resolution[] PixelPerfectResolutions()
		{
			List<Resolution> resolutions = new List<Resolution>();
			for ( ushort i = 0; Screen.resolutions.Length > i; i++ )
				if ( Screen.resolutions[ i ].width % WorldBuild.PIXEL_PERFECT_WIDTH == 0 && Screen.resolutions[ i ].height % WorldBuild.PIXEL_PERFECT_HEIGHT == 0 )
					resolutions.Add( Screen.resolutions[ i ] );
			return resolutions.ToArray();
		}
		public static void TurnScaleX( this Transform transform, float valueChanger )
		{
			_scaleTurner.Set( Mathf.Abs( transform.localScale.x ) * valueChanger, transform.localScale.y, transform.localScale.z );
			transform.localScale = _scaleTurner;
		}
		public static void TurnScaleX( this Transform transform, bool conditionChanger ) => TurnScaleX( transform, conditionChanger ? -1F : 1F );
		public static bool InsideRectangle( this Vector2 pointInside, Vector2 originPoint, Vector2 sizePoint )
		{
			return originPoint.x + sizePoint.x / 2F >= pointInside.x && originPoint.x - sizePoint.x / 2F <= pointInside.x &&
				originPoint.y + sizePoint.y / 2F >= pointInside.y && originPoint.y - sizePoint.y / 2F <= pointInside.y;
		}
		public static bool OutsideRectangle( this Vector2 pointOutside, Vector2 originPoint, Vector2 sizePoint )
		{
			return originPoint.x + sizePoint.x / 2F < pointOutside.x || originPoint.x - sizePoint.x / 2F > pointOutside.x ||
				originPoint.y + sizePoint.y / 2F < pointOutside.y || originPoint.y - sizePoint.y / 2F > pointOutside.y;
		}
		public static bool InsideCircle( this Vector2 pointInside, Vector2 originPoint, float radius ) => Vector2.Distance( originPoint, pointInside ) < radius;
		public static bool OutsideCircle( this Vector2 pointOutside, Vector2 originPoint, float radius ) => Vector2.Distance( originPoint, pointOutside ) > radius;
	};
};
