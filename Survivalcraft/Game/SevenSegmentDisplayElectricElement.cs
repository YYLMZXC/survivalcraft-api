using Engine;

namespace Game
{
	public class SevenSegmentDisplayElectricElement : MountedElectricElement
	{
		private SubsystemGlow m_subsystemGlow;

		private float m_voltage = float.PositiveInfinity;

		private GlowPoint[] m_glowPoints = new GlowPoint[7];

		private Color m_color;

		private Vector2[] m_centers = new Vector2[7]
		{
			new Vector2(0f, 6f),
			new Vector2(-4f, 3f),
			new Vector2(-4f, -3f),
			new Vector2(0f, -6f),
			new Vector2(4f, -3f),
			new Vector2(4f, 3f),
			new Vector2(0f, 0f)
		};

		private Vector2[] m_sizes = new Vector2[7]
		{
			new Vector2(3.2f, 1f),
			new Vector2(1f, 2.3f),
			new Vector2(1f, 2.3f),
			new Vector2(3.2f, 1f),
			new Vector2(1f, 2.3f),
			new Vector2(1f, 2.3f),
			new Vector2(3.2f, 1f)
		};

		private int[] m_patterns = new int[16]
		{
			63, 6, 91, 79, 102, 109, 125, 7, 127, 111,
			119, 124, 57, 94, 121, 113
		};

		public SevenSegmentDisplayElectricElement(SubsystemElectricity subsystemElectricity, CellFace cellFace)
			: base(subsystemElectricity, cellFace)
		{
			m_subsystemGlow = subsystemElectricity.Project.FindSubsystem<SubsystemGlow>(throwOnError: true);
		}

		public override void OnAdded()
		{
			CellFace cellFace = base.CellFaces[0];
			int data = Terrain.ExtractData(base.SubsystemElectricity.SubsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z));
			int mountingFace = SevenSegmentDisplayBlock.GetMountingFace(data);
			m_color = LedBlock.LedColors[SevenSegmentDisplayBlock.GetColor(data)];
			for (int i = 0; i < 7; i++)
			{
				Vector3 vector = new Vector3((float)cellFace.X + 0.5f, (float)cellFace.Y + 0.5f, (float)cellFace.Z + 0.5f);
				Vector3 vector2 = CellFace.FaceToVector3(mountingFace);
				Vector3 vector3 = ((mountingFace < 4) ? Vector3.UnitY : Vector3.UnitX);
				Vector3 vector4 = Vector3.Cross(vector2, vector3);
				m_glowPoints[i] = m_subsystemGlow.AddGlowPoint();
				m_glowPoints[i].Position = vector - 0.4375f * CellFace.FaceToVector3(mountingFace) + m_centers[i].X * 0.0625f * vector4 + m_centers[i].Y * 0.0625f * vector3;
				m_glowPoints[i].Forward = vector2;
				m_glowPoints[i].Right = vector4 * m_sizes[i].X * 0.0625f;
				m_glowPoints[i].Up = vector3 * m_sizes[i].Y * 0.0625f;
				m_glowPoints[i].Color = Color.Transparent;
				m_glowPoints[i].Size = 1.35f;
				m_glowPoints[i].FarSize = 1.35f;
				m_glowPoints[i].FarDistance = 1f;
				m_glowPoints[i].Type = ((m_sizes[i].X > m_sizes[i].Y) ? GlowPointType.HorizontalRectangle : GlowPointType.VerticalRectangle);
			}
		}

		public override void OnRemoved()
		{
			for (int i = 0; i < 7; i++)
			{
				m_subsystemGlow.RemoveGlowPoint(m_glowPoints[i]);
			}
		}

		public override bool Simulate()
		{
			float voltage = m_voltage;
			m_voltage = 0f;
			foreach (ElectricConnection connection in base.Connections)
			{
				if (connection.ConnectorType != ElectricConnectorType.Output && connection.NeighborConnectorType != 0)
				{
					m_voltage = MathUtils.Max(m_voltage, connection.NeighborElectricElement.GetOutputVoltage(connection.NeighborConnectorFace));
				}
			}
			if (m_voltage != voltage)
			{
				int num = (int)MathUtils.Round(m_voltage * 15f);
				for (int i = 0; i < 7; i++)
				{
					if ((m_patterns[num] & (1 << i)) != 0)
					{
						m_glowPoints[i].Color = m_color;
					}
					else
					{
						m_glowPoints[i].Color = Color.Transparent;
					}
				}
			}
			return false;
		}
	}
}
