using Engine;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemNoise : Subsystem
	{
		private SubsystemBodies m_subsystemBodies;

		private DynamicArray<ComponentBody> m_componentBodies = new DynamicArray<ComponentBody>();

		public void MakeNoise(Vector3 position, float loudness, float range)
		{
			MakeNoiseInternal(null, position, loudness, range);
		}

		public void MakeNoise(ComponentBody sourceBody, float loudness, float range)
		{
			MakeNoiseInternal(sourceBody, sourceBody.Position, loudness, range);
		}

		public override void Load(ValuesDictionary valuesDictionary)
		{
			m_subsystemBodies = base.Project.FindSubsystem<SubsystemBodies>(throwOnError: true);
		}

		private void MakeNoiseInternal(ComponentBody sourceBody, Vector3 position, float loudness, float range)
		{
			float num = range * range;
			m_componentBodies.Clear();
			m_subsystemBodies.FindBodiesAroundPoint(new Vector2(position.X, position.Z), range, m_componentBodies);
			for (int i = 0; i < m_componentBodies.Count; i++)
			{
				ComponentBody componentBody = m_componentBodies.Array[i];
				if (componentBody == sourceBody || !(Vector3.DistanceSquared(componentBody.Position, position) < num))
				{
					continue;
				}
				foreach (INoiseListener item in componentBody.Entity.FindComponents<INoiseListener>())
				{
					item.HearNoise(sourceBody, position, loudness);
				}
			}
		}
	}
}
