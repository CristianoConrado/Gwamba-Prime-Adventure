using UnityEngine;
using System.Collections.Generic;
namespace GwambaPrimeAdventure
{
    public static class BuildWorker
    {
		private static Vector3 _scaleTurner = Vector3.zero;
		private static float _outpuNormalized = 0F;
		public static Vector2 OrthographicToRealSize( float orthographicSize ) => orthographicSize * 2F * new Vector2( WorldBuild.HEIGHT_WIDTH_PROPORTION, 1F );
		public static Vector2 OrthographicToScreenSize( float orthographicSize ) => WorldBuild.PIXELS_PER_UNIT * OrthographicToRealSize( orthographicSize );
		public static Resolution[] PixelPerfectResolutions()
		{
			List<Resolution> resolutions = new List<Resolution>();
			for ( ushort i = 0; Screen.resolutions.Length > i; i++ )
				if ( 0 == Screen.resolutions[ i ].width % WorldBuild.PIXEL_PERFECT_WIDTH && 0 == Screen.resolutions[ i ].height % WorldBuild.PIXEL_PERFECT_HEIGHT )
					resolutions.Add( Screen.resolutions[ i ] );
			return resolutions.ToArray();
		}
		public static void TurnScaleX( this Transform transform, float changer )
		{
			_scaleTurner.Set( Mathf.Abs( transform.localScale.x ) * changer.RangeNormalizeWithoutZero( WorldBuild.SCALE_SNAP, true ), transform.localScale.y, transform.localScale.z );
			transform.localScale = _scaleTurner;
		}
		public static void TurnScaleX( this Transform transform, bool conditionChanger ) => TurnScaleX( transform, conditionChanger ? -1F : 1F );
		public static bool InsideRectangle( this Vector2 pointInside, Vector2 originPoint, Vector2 sizePoint ) =>
			originPoint.x + sizePoint.x / 2F >= pointInside.x && originPoint.x - sizePoint.x / 2F <= pointInside.x &&
			originPoint.y + sizePoint.y / 2F >= pointInside.y && originPoint.y - sizePoint.y / 2F <= pointInside.y;
		public static bool OutsideRectangle( this Vector2 pointOutside, Vector2 originPoint, Vector2 sizePoint ) => !InsideRectangle( pointOutside, originPoint, sizePoint );
		public static bool InsideCircle( this Vector2 pointInside, Vector2 originPoint, float radius ) => Vector2.Distance( originPoint, pointInside ) <= radius;
		public static bool OutsideCircle( this Vector2 pointOutside, Vector2 originPoint, float radius ) => !InsideCircle( pointOutside, originPoint, radius );
		public static short RangeNormalize( this float input, float maxDelimiter, float minDelimiter ) =>
			(short) ( input.CompareTo( Mathf.Abs( maxDelimiter ) ) + input.CompareTo( -Mathf.Abs( minDelimiter ) ) ).CompareTo( 0 );
		public static short RangeNormalize( this float input, float rangeDelimiter ) => RangeNormalize( input, rangeDelimiter, rangeDelimiter );
		public static short RangeNormalizeWithoutZero( this float input, float maxDelimiter, float minDelimiter, bool negativePriority = false ) =>
			(short) ( ( _outpuNormalized = RangeNormalize( input, maxDelimiter, minDelimiter ) ) + ( 0F == _outpuNormalized ? ( negativePriority ? -1F : 1F ) : 0F ) );
		public static short RangeNormalizeWithoutZero( this float input, float rangeDelimiter, bool negativePriority = false ) =>
			RangeNormalizeWithoutZero(input, rangeDelimiter, rangeDelimiter, negativePriority);
	};
};
