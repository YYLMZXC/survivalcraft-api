using System.Linq;
using Engine;
using Engine.Serialization;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemMagnetBlockBehavior : SubsystemBlockBehavior
	{
		private SubsystemPlayers m_subsystemPlayers;

		private DynamicArray<Vector3> m_magnets = new DynamicArray<Vector3>();

		public const int MaxMagnets = 8;

		public override int[] HandledBlocks => new int[1] { 167 };

		public int MagnetsCount => m_magnets.Count;

		public Vector3 FindNearestCompassTarget(Vector3 compassPosition)
		{
			if (m_magnets.Count > 0)
			{
				float num = float.MaxValue;
				Vector3 vector = Vector3.Zero;
				for (int i = 0; i < m_magnets.Count && i < 8; i++)
				{
					Vector3 vector2 = m_magnets.Array[i];
					float num2 = Vector3.DistanceSquared(compassPosition, vector2);
					if (num2 < num)
					{
						num = num2;
						vector = vector2;
					}
				}
				return vector + new Vector3(0.5f);
			}
			float num3 = float.MaxValue;
			Vector3 vector3 = Vector3.Zero;
			foreach (PlayerData playersDatum in m_subsystemPlayers.PlayersData)
			{
				Vector3 spawnPosition = playersDatum.SpawnPosition;
				float num4 = Vector3.DistanceSquared(compassPosition, spawnPosition);
				if (num4 < num3)
				{
					num3 = num4;
					vector3 = spawnPosition;
				}
			}
			return vector3 + new Vector3(0.5f);
		}

		public override void Load(ValuesDictionary valuesDictionary)
		{
			base.Load(valuesDictionary);
			m_subsystemPlayers = base.Project.FindSubsystem<SubsystemPlayers>(throwOnError: true);
			string value = valuesDictionary.GetValue<string>("Magnets");
			m_magnets = new DynamicArray<Vector3>(HumanReadableConverter.ValuesListFromString<Vector3>(';', value));
		}

		public override void Save(ValuesDictionary valuesDictionary)
		{
			base.Save(valuesDictionary);
			string value = HumanReadableConverter.ValuesListToString(';', m_magnets.ToArray());
			valuesDictionary.SetValue("Magnets", value);
		}

		public override void OnBlockAdded(int value, int oldValue, int x, int y, int z)
		{
			m_magnets.Add(new Vector3(x, y, z));
		}

		public override void OnBlockRemoved(int value, int newValue, int x, int y, int z)
		{
			m_magnets.Remove(new Vector3(x, y, z));
		}

		public override void OnNeighborBlockChanged(int x, int y, int z, int neighborX, int neighborY, int neighborZ)
		{
			int cellContents = base.SubsystemTerrain.Terrain.GetCellContents(x, y - 1, z);
			if (BlocksManager.Blocks[cellContents].IsTransparent)
			{
				base.SubsystemTerrain.DestroyCell(0, x, y, z, 0, noDrop: false, noParticleSystem: false);
			}
		}
	}
}
