namespace GwambaPrimeAdventure
{
	public static class WorldBuild
	{
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
		public const ushort DEFAULT_HERTZ_PER_SECOND = 50;
		public const ushort MEDIUM_HERTZ_PER_SECOND = 60;
		public const ushort MAXIMUM_HERTZ_PER_SECOND = 100;
		public const ushort MAXIMUM_PIXEL_PERFECT_SCALE = 12;
		public const ushort PIXEL_PERFECT_WIDTH = 320;
		public const ushort PIXEL_PERFECT_HEIGHT = 180;
		public const ushort DESIRED_PIXEL_PERFECT_SCALE = 2;
		public const ushort UI_SCALE_WIDTH = 1920;
		public const ushort UI_SCALE_HEIGHt = 1080;
		public const ushort LEVELS_COUNT = 10;
		public const float FIELD_SPACE_LENGTH = 8F;
		public const float PIXELS_PER_UNIT = 16F;
		public const float SNAP_LENGTH = 1F / PIXELS_PER_UNIT;
		public const float SCALE_SNAP = 0.0625F;
		public const float ROTATE_SNAP = 7.5F;
		public const float MINIMUM_TIME_SPACE_LIMIT = 1E-4F;
		public const float WIDTH_HEIGHT_PROPORTION = PIXEL_PERFECT_HEIGHT / PIXEL_PERFECT_WIDTH;
		public const float HEIGHT_WIDTH_PROPORTION = PIXEL_PERFECT_WIDTH / PIXEL_PERFECT_HEIGHT;
		public const float DEFAULT_HERTZ = 1F / DEFAULT_HERTZ_PER_SECOND;
		public const float MEDIUM_HERTZ = 1F / MEDIUM_HERTZ_PER_SECOND;
		public const float MAXIMUM_HERTZ = 1F / MAXIMUM_HERTZ_PER_SECOND;
	};
};
