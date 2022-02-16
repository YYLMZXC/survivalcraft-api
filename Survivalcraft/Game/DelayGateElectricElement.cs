namespace Game
{
	public class DelayGateElectricElement : BaseDelayGateElectricElement
	{
		private int? m_delaySteps;

		private int m_lastDelayCalculationStep;

		private static int[] m_delaysByPredecessorsCount = new int[3] { 20, 80, 400 };

		public override int DelaySteps
		{
			get
			{
				if (base.SubsystemElectricity.CircuitStep - m_lastDelayCalculationStep > 50)
				{
					m_delaySteps = null;
				}
				if (!m_delaySteps.HasValue)
				{
					int count = 0;
					CountDelayPredecessors(this, ref count);
					m_delaySteps = m_delaysByPredecessorsCount[count];
					m_lastDelayCalculationStep = base.SubsystemElectricity.CircuitStep;
				}
				return m_delaySteps.Value;
			}
		}

		public DelayGateElectricElement(SubsystemElectricity subsystemElectricity, CellFace cellFace)
			: base(subsystemElectricity, cellFace)
		{
		}

		private static void CountDelayPredecessors(DelayGateElectricElement delayGate, ref int count)
		{
			if (count >= 2)
			{
				return;
			}
			foreach (ElectricConnection connection in delayGate.Connections)
			{
				if (connection.ConnectorType == ElectricConnectorType.Input)
				{
					DelayGateElectricElement delayGateElectricElement = connection.NeighborElectricElement as DelayGateElectricElement;
					if (delayGateElectricElement != null)
					{
						count++;
						CountDelayPredecessors(delayGateElectricElement, ref count);
						break;
					}
				}
			}
		}
	}
}
