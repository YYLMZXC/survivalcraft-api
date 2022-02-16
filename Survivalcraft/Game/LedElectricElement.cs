using Engine;

namespace Game
{
	public class LedElectricElement : MountedElectricElement
	{
		private SubsystemGlow m_subsystemGlow;

		private float m_voltage;

		private GlowPoint m_glowPoint;

		private Color m_color;

		public LedElectricElement(SubsystemElectricity subsystemElectricity, CellFace cellFace)
			: base(subsystemElectricity, cellFace)
		{
			m_subsystemGlow = subsystemElectricity.Project.FindSubsystem<SubsystemGlow>(throwOnError: true);
		}

		public override void OnAdded()
		{
			m_glowPoint = m_subsystemGlow.AddGlowPoint();
			CellFace cellFace = base.CellFaces[0];
			int data = Terrain.ExtractData(base.SubsystemElectricity.SubsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z));
			int mountingFace = LedBlock.GetMountingFace(data);
			m_color = LedBlock.LedColors[LedBlock.GetColor(data)];
			Vector3 vector = new Vector3((float)cellFace.X + 0.5f, (float)cellFace.Y + 0.5f, (float)cellFace.Z + 0.5f);
			m_glowPoint.Position = vector - 0.4375f * CellFace.FaceToVector3(mountingFace);
			m_glowPoint.Forward = CellFace.FaceToVector3(mountingFace);
			m_glowPoint.Up = ((mountingFace < 4) ? Vector3.UnitY : Vector3.UnitX);
			m_glowPoint.Right = Vector3.Cross(m_glowPoint.Forward, m_glowPoint.Up);
			m_glowPoint.Color = Color.Transparent;
			m_glowPoint.Size = 0.0324f;
			m_glowPoint.FarSize = 0.0324f;
			m_glowPoint.FarDistance = 0f;
			m_glowPoint.Type = GlowPointType.Square;
		}

		public override void OnRemoved()
		{
			m_subsystemGlow.RemoveGlowPoint(m_glowPoint);
		}

		public override bool Simulate()
		{
			float voltage = m_voltage;
			m_voltage = CalculateVoltage();
			if (ElectricElement.IsSignalHigh(m_voltage) != ElectricElement.IsSignalHigh(voltage))
			{
				if (ElectricElement.IsSignalHigh(m_voltage))
				{
					m_glowPoint.Color = m_color;
				}
				else
				{
					m_glowPoint.Color = Color.Transparent;
				}
			}
			return false;
		}

		private float CalculateVoltage()
		{
			return (CalculateHighInputsCount() > 0) ? 1 : 0;
		}
	}
}
