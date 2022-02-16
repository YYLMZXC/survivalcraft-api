using System;
using Engine;

namespace Game
{
	public class TerrainChunk : IDisposable
	{
		private struct BrushPaint
		{
			public Point3 Position;

			public TerrainBrush Brush;
		}

		public const int SizeBits = 4;

		public const int Size = 16;

		public const int HeightBits = 8;

		public const int Height = 256;

		public const int SizeMinusOne = 15;

		public const int HeightMinusOne = 255;

		public const int SliceHeight = 16;

		public const int SlicesCount = 16;

		public const int SlicesCountMinusOne = 15;

		public Terrain Terrain;

		public Point2 Coords;

		public Point2 Origin;

		public BoundingBox BoundingBox;

		public Vector2 Center;

		public TerrainChunkState State;

		public TerrainChunkState ThreadState;

		public bool WasDowngraded;

		public TerrainChunkState? DowngradedState;

		public bool WasUpgraded;

		public TerrainChunkState? UpgradedState;

		public int ModificationCounter;

		public float[] FogEnds = new float[4];

		public bool AreBehaviorsNotified;

		public bool IsLoaded;

		public volatile bool NewGeometryData;

		public TerrainChunkGeometry Geometry = new TerrainChunkGeometry();

		private int[] Cells;

		private int[] Shafts;

		private static ArrayCache<int> m_cellsCache = new ArrayCache<int>(new int[1] { 65536 }, 0.66f, 60f, 0.33f, 5f);

		private static ArrayCache<int> m_shaftsCache = new ArrayCache<int>(new int[1] { 256 }, 0.66f, 60f, 0.33f, 5f);

		private DynamicArray<BrushPaint> m_brushPaints = new DynamicArray<BrushPaint>();

		public TerrainChunk(Terrain terrain, int x, int z)
		{
			Terrain = terrain;
			Coords = new Point2(x, z);
			Origin = new Point2(x * 16, z * 16);
			BoundingBox = new BoundingBox(new Vector3(Origin.X, 0f, Origin.Y), new Vector3(Origin.X + 16, 256f, Origin.Y + 16));
			Center = new Vector2((float)Origin.X + 8f, (float)Origin.Y + 8f);
			Cells = m_cellsCache.Rent(65536, clearArray: true);
			Shafts = m_shaftsCache.Rent(256, clearArray: true);
		}

		public void Dispose()
		{
			if (Geometry == null)
			{
				throw new InvalidOperationException();
			}
			Geometry.Dispose();
			Geometry = null;
			m_cellsCache.Return(Cells);
			m_shaftsCache.Return(Shafts);
		}

		public static bool IsCellValid(int x, int y, int z)
		{
			if (x >= 0 && x < 16 && y >= 0 && y < 256 && z >= 0)
			{
				return z < 16;
			}
			return false;
		}

		public static bool IsShaftValid(int x, int z)
		{
			if (x >= 0 && x < 16 && z >= 0)
			{
				return z < 16;
			}
			return false;
		}

		public static int CalculateCellIndex(int x, int y, int z)
		{
			return y + x * 256 + z * 256 * 16;
		}

		public int CalculateTopmostCellHeight(int x, int z)
		{
			int num = CalculateCellIndex(x, 255, z);
			int num2 = 255;
			while (num2 >= 0)
			{
				if (Terrain.ExtractContents(GetCellValueFast(num)) != 0)
				{
					return num2;
				}
				num2--;
				num--;
			}
			return 0;
		}

		public int GetCellValueFast(int index)
		{
			return Cells[index];
		}

		public int GetCellValueFast(int x, int y, int z)
		{
			return Cells[y + x * 256 + z * 256 * 16];
		}

		public void SetCellValueFast(int x, int y, int z, int value)
		{
			Cells[y + x * 256 + z * 256 * 16] = value;
		}

		public void SetCellValueFast(int index, int value)
		{
			Cells[index] = value;
		}

		public int GetCellContentsFast(int x, int y, int z)
		{
			return Terrain.ExtractContents(GetCellValueFast(x, y, z));
		}

		public int GetCellLightFast(int x, int y, int z)
		{
			return Terrain.ExtractLight(GetCellValueFast(x, y, z));
		}

		public int GetShaftValueFast(int x, int z)
		{
			return Shafts[x + z * 16];
		}

		public void SetShaftValueFast(int x, int z, int value)
		{
			Shafts[x + z * 16] = value;
		}

		public int GetTemperatureFast(int x, int z)
		{
			return Terrain.ExtractTemperature(GetShaftValueFast(x, z));
		}

		public void SetTemperatureFast(int x, int z, int temperature)
		{
			SetShaftValueFast(x, z, Terrain.ReplaceTemperature(GetShaftValueFast(x, z), temperature));
		}

		public int GetHumidityFast(int x, int z)
		{
			return Terrain.ExtractHumidity(GetShaftValueFast(x, z));
		}

		public void SetHumidityFast(int x, int z, int humidity)
		{
			SetShaftValueFast(x, z, Terrain.ReplaceHumidity(GetShaftValueFast(x, z), humidity));
		}

		public int GetTopHeightFast(int x, int z)
		{
			return Terrain.ExtractTopHeight(GetShaftValueFast(x, z));
		}

		public void SetTopHeightFast(int x, int z, int topHeight)
		{
			SetShaftValueFast(x, z, Terrain.ReplaceTopHeight(GetShaftValueFast(x, z), topHeight));
		}

		public int GetBottomHeightFast(int x, int z)
		{
			return Terrain.ExtractBottomHeight(GetShaftValueFast(x, z));
		}

		public void SetBottomHeightFast(int x, int z, int bottomHeight)
		{
			SetShaftValueFast(x, z, Terrain.ReplaceBottomHeight(GetShaftValueFast(x, z), bottomHeight));
		}

		public int GetSunlightHeightFast(int x, int z)
		{
			return Terrain.ExtractSunlightHeight(GetShaftValueFast(x, z));
		}

		public void SetSunlightHeightFast(int x, int z, int sunlightHeight)
		{
			SetShaftValueFast(x, z, Terrain.ReplaceSunlightHeight(GetShaftValueFast(x, z), sunlightHeight));
		}

		public void AddBrushPaint(int x, int y, int z, TerrainBrush brush)
		{
			m_brushPaints.Add(new BrushPaint
			{
				Position = new Point3(x, y, z),
				Brush = brush
			});
		}

		public void ApplyBrushPaints(TerrainChunk chunk)
		{
			foreach (BrushPaint brushPaint in m_brushPaints)
			{
				brushPaint.Brush.PaintFast(chunk, brushPaint.Position.X, brushPaint.Position.Y, brushPaint.Position.Z);
			}
		}
	}
}
