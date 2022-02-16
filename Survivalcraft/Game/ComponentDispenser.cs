using Engine;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game
{
	public class ComponentDispenser : ComponentInventoryBase
	{
		private SubsystemTerrain m_subsystemTerrain;

		private SubsystemAudio m_subsystemAudio;

		private SubsystemPickables m_subsystemPickables;

		private SubsystemProjectiles m_subsystemProjectiles;

		private ComponentBlockEntity m_componentBlockEntity;

		private Random m_random = new Random();

		public void Dispense()
		{
			Point3 coordinates = m_componentBlockEntity.Coordinates;
			int data = Terrain.ExtractData(m_subsystemTerrain.Terrain.GetCellValue(coordinates.X, coordinates.Y, coordinates.Z));
			int direction = DispenserBlock.GetDirection(data);
			DispenserBlock.Mode mode = DispenserBlock.GetMode(data);
			for (int i = 0; i < SlotsCount; i++)
			{
				int slotValue = GetSlotValue(i);
				int slotCount = GetSlotCount(i);
				if (slotValue != 0 && slotCount > 0)
				{
					int num = RemoveSlotItems(i, 1);
					for (int j = 0; j < num; j++)
					{
						DispenseItem(coordinates, direction, slotValue, mode);
					}
					break;
				}
			}
		}

		public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
		{
			base.Load(valuesDictionary, idToEntityMap);
			m_subsystemTerrain = base.Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
			m_subsystemAudio = base.Project.FindSubsystem<SubsystemAudio>(throwOnError: true);
			m_subsystemPickables = base.Project.FindSubsystem<SubsystemPickables>(throwOnError: true);
			m_subsystemProjectiles = base.Project.FindSubsystem<SubsystemProjectiles>(throwOnError: true);
			m_componentBlockEntity = base.Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		}

		private void DispenseItem(Point3 point, int face, int value, DispenserBlock.Mode mode)
		{
			Vector3 vector = CellFace.FaceToVector3(face);
			Vector3 position = new Vector3((float)point.X + 0.5f, (float)point.Y + 0.5f, (float)point.Z + 0.5f) + 0.6f * vector;
			if (mode == DispenserBlock.Mode.Dispense)
			{
				float num = 1.8f;
				m_subsystemPickables.AddPickable(value, 1, position, num * (vector + m_random.Vector3(0.2f)), null);
				m_subsystemAudio.PlaySound("Audio/DispenserDispense", 1f, 0f, new Vector3(position.X, position.Y, position.Z), 3f, autoDelay: true);
				return;
			}
			float num2 = m_random.Float(39f, 41f);
			if (m_subsystemProjectiles.FireProjectile(value, position, num2 * (vector + m_random.Vector3(0.025f) + new Vector3(0f, 0.05f, 0f)), Vector3.Zero, null) != null)
			{
				m_subsystemAudio.PlaySound("Audio/DispenserShoot", 1f, 0f, new Vector3(position.X, position.Y, position.Z), 4f, autoDelay: true);
			}
			else
			{
				DispenseItem(point, face, value, DispenserBlock.Mode.Dispense);
			}
		}
	}
}
