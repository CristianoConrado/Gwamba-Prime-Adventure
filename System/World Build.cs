using UnityEngine;
using System.Collections.Generic;
namespace GwambaPrimeAdventure
{
	public static class WorldBuild
	{
		public const string PROJECT_ICON = "Assets/Huds/Designs/Gwamba Prime Series Icon.svg";
		public const float FIELD_SPACE_LENGTH = 8F;
		public const float PIXELS_PER_UNIT = 16F;
		public const float SNAP_LENGTH = 1F / PIXELS_PER_UNIT;
		public const float SCALE_SNAP = 0.0625F;
		public const float ROTATE_SNAP = 7.5F;
		public const float MINIMUM_TIME_SPACE_LIMIT = 1E-4F;
		public const float WIDTH_HEIGHT_PROPORTION = 0.5625F;
		public const float HEIGHT_WIDTH_PROPORTION = 1.777778F;
		public const int SYSTEM_LAYER = 3;
		public const int UI_LAYER = 5;
		public const int CHARACTER_LAYER = 6;
		public const int SCENE_LAYER = 7;
		public const int ITEM_LAYER = 8;
		public const int ENEMY_LAYER = 9;
		public const int BOSS_LAYER = 10;
		public const int SYSTEM_LAYER_MASK = 1 << SYSTEM_LAYER;
		public const int UI_LAYER_MASK = 1 << UI_LAYER;
		public const int CHARACTER_LAYER_MASK = 1 << CHARACTER_LAYER;
		public const int SCENE_LAYER_MASK = 1 << SCENE_LAYER;
		public const int ITEM_LAYER_MASK = 1 << ITEM_LAYER;
		public const int ENEMY_LAYER_MASK = 1 << ENEMY_LAYER;
		public const int BOSS_LAYER_MASK = 1 << BOSS_LAYER;
		public const ushort PIXEL_PERFECT_WIDTH = 320;
		public const ushort PIXEL_PERFECT_HEIGHT = (ushort)(PIXEL_PERFECT_WIDTH * WIDTH_HEIGHT_PROPORTION);
		public const ushort UI_SCALE_WIDTH = 1920;
		public const ushort UI_SCALE_HEIGHt = (ushort)(UI_SCALE_WIDTH * WIDTH_HEIGHT_PROPORTION);
		public const ushort LEVELS_COUNT = 10;
		public static Vector2 OrthographicToRealSize(float orthographicSize) => new(orthographicSize * 2F * HEIGHT_WIDTH_PROPORTION, orthographicSize * 2F);
		public static Vector2 OrthographicToScreenSize(float orthographicSize) => OrthographicToRealSize(orthographicSize) * PIXELS_PER_UNIT;
		public static Resolution[] PixelPerfectResolutions()
		{
			List<Resolution> resolutions = new();
			for (ushort i = 0; Screen.resolutions.Length > i; i++)
				if (Screen.resolutions[i].width % PIXEL_PERFECT_WIDTH == 0 && Screen.resolutions[i].height % PIXEL_PERFECT_HEIGHT == 0)
					resolutions.Add(Screen.resolutions[i]);
			return resolutions.ToArray();
		}
		public static void TurnScaleX(this Transform transform, float valueChanger)
		{
			transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * valueChanger, transform.localScale.y, transform.localScale.z);
		}
		public static void TurnScaleX(this Transform transform, bool conditionChanger) => TurnScaleX(transform, conditionChanger ? -1F : 1F);
		public static bool InsideRectangle(this Vector2 pointInside, Vector2 originPoint, Vector2 sizePoint)
		{
			return originPoint.x + sizePoint.x / 2F >= pointInside.x && originPoint.x - sizePoint.x / 2F <= pointInside.x &&
				originPoint.y + sizePoint.y / 2F >= pointInside.y && originPoint.y - sizePoint.y / 2F <= pointInside.y;
		}
		public static bool OutsideRectangle(this Vector2 pointOutside, Vector2 originPoint, Vector2 sizePoint)
		{
			return originPoint.x + sizePoint.x / 2F < pointOutside.x || originPoint.x - sizePoint.x / 2F > pointOutside.x ||
				originPoint.y + sizePoint.y / 2F < pointOutside.y || originPoint.y - sizePoint.y / 2F > pointOutside.y;
		}
		public static bool InsideCircle(this Vector2 pointInside, Vector2 originPoint, float radius) => Vector2.Distance(originPoint, pointInside) < radius;
		public static bool OutsideCircle(this Vector2 pointOutside, Vector2 originPoint, float radius) => Vector2.Distance(originPoint, pointOutside) > radius;
	};
};

