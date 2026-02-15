using System.Collections.Generic;
using UnityEngine;
namespace GwambaPrimeAdventure
{
	public static class BuildMathemathics
	{
		private static Vector3
			_scaleTurner = Vector3.zero;
		private static float
			_outpuNormalized = 0F;
		public static Vector2 OrthographicToRealSize( float orthographicSize ) => orthographicSize * 2F * new Vector2( WorldBuild.HEIGHT_WIDTH_PROPORTION, 1F );
		public static Vector2 OrthographicToScreenSize( float orthographicSize ) => WorldBuild.PIXELS_PER_UNIT * OrthographicToRealSize( orthographicSize );
		public static Vector2Int[] PixelPerfectResolutions()
		{
			List<Vector2Int> resolutions = new();
			Vector2Int
				maximumResolution = new(),
				indeedResolution = new();
			for ( byte i = (byte) ( Screen.resolutions.Length - 1 ); 0 < i; i-- )
				if ( 0 == Screen.resolutions[ i ].width % ( WorldBuild.PIXEL_PERFECT_WIDTH * WorldBuild.DESIRED_PIXEL_PERFECT_SCALE )
					&& 0 == Screen.resolutions[ i ].height % ( WorldBuild.PIXEL_PERFECT_HEIGHT * WorldBuild.DESIRED_PIXEL_PERFECT_SCALE ) )
				{
					maximumResolution.Set( Screen.resolutions[ i ].width, Screen.resolutions[ i ].height );
					break;
				}
			for ( byte i = WorldBuild.DESIRED_PIXEL_PERFECT_SCALE; WorldBuild.MAXIMUM_PIXEL_PERFECT_SCALE > i; i += WorldBuild.DESIRED_PIXEL_PERFECT_SCALE )
			{
				indeedResolution.Set( WorldBuild.PIXEL_PERFECT_WIDTH * i, WorldBuild.PIXEL_PERFECT_HEIGHT * i );
				resolutions.Add( indeedResolution );
				if ( WorldBuild.PIXEL_PERFECT_WIDTH * i >= maximumResolution.x && WorldBuild.PIXEL_PERFECT_HEIGHT * i >= maximumResolution.y )
					break;
			}
			return resolutions.ToArray();
		}
		public static void TurnScaleX( this Transform transform, float changer )
		{
			_scaleTurner.x = Mathf.Abs( transform.localScale.x ) * changer.RangeNormalizeWithoutZero( WorldBuild.SCALE_SNAP, true );
			_scaleTurner.y = transform.localScale.y;
			_scaleTurner.z = transform.localScale.z;
			transform.localScale = _scaleTurner;
		}
		public static void TurnScaleX( this Transform transform, bool conditionChanger ) => TurnScaleX( transform, conditionChanger ? -1F : 1F );
		public static bool InsideBoxCast( this Vector2 pointInside, Vector2 originPoint, Vector2 sizePoint ) =>
			pointInside.x.InsideRange(originPoint.x, sizePoint.x) && pointInside.y.InsideRange(originPoint.y, sizePoint.y);
		public static bool OutsideBoxCast( this Vector2 pointOutside, Vector2 originPoint, Vector2 sizePoint ) => !InsideBoxCast( pointOutside, originPoint, sizePoint );
		public static bool InsideCircleCast( this Vector2 pointInside, Vector2 originPoint, float radius ) => Vector2.Distance( originPoint, pointInside ) <= radius;
		public static bool OutsideCircleCast( this Vector2 pointOutside, Vector2 originPoint, float radius ) => !InsideCircleCast( pointOutside, originPoint, radius );
		public static bool MoreOrEqual( this Vector2 firstVector, Vector2 otherVector ) => firstVector.x >= otherVector.x && firstVector.y >= otherVector.y;
		public static bool LessOrEqual( this Vector2 firstVector, Vector2 otherVector ) => firstVector.x <= otherVector.x && firstVector.y <= otherVector.y;
		public static float Negative( this float input ) => -Mathf.Abs( input );
		public static sbyte RangeNormalize( this float input, float maxDelimiter, float minDelimiter ) =>
			(sbyte) ( input.CompareTo( Mathf.Abs( maxDelimiter ) ) + input.CompareTo( minDelimiter.Negative() ) ).CompareTo( 0 );
		public static sbyte RangeNormalize( this float input, float rangeDelimiter ) => RangeNormalize( input, rangeDelimiter, rangeDelimiter );
		public static sbyte RangeNormalizeWithoutZero( this float input, float maxDelimiter, float minDelimiter, bool negativePriority = false ) =>
			(sbyte) ( ( _outpuNormalized = RangeNormalize( input, maxDelimiter, minDelimiter ) ) + ( 0F == _outpuNormalized ? ( negativePriority ? -1F : 1F ) : 0F ) );
		public static sbyte RangeNormalizeWithoutZero( this float input, float rangeDelimiter, bool negativePriority = false ) =>
			RangeNormalizeWithoutZero(input, rangeDelimiter, rangeDelimiter, negativePriority);
		public static bool InsideRange( this float valueInside, float originValue, float sizeValue ) =>
			originValue + sizeValue / 2F >= valueInside && originValue - sizeValue / 2F <= valueInside;
		public static bool OutsideRange( this float valueInside, float originValue, float sizeValue ) => !InsideRange( valueInside, originValue, sizeValue );
	};
};
