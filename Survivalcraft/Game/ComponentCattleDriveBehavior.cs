using Engine;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game
{
	public class ComponentCattleDriveBehavior : ComponentBehavior, IUpdateable, INoiseListener
	{
		private SubsystemTime m_subsystemTime;

		private SubsystemCreatureSpawn m_subsystemCreatureSpawn;

		private ComponentCreature m_componentCreature;

		private ComponentPathfinding m_componentPathfinding;

		private ComponentHerdBehavior m_componentHerdBehavior;

		private StateMachine m_stateMachine = new StateMachine();

		private Random m_random = new Random();

		private float m_importanceLevel;

		private Vector3 m_driveVector;

		public UpdateOrder UpdateOrder => UpdateOrder.Default;

		public override float ImportanceLevel => m_importanceLevel;

		public void HearNoise(ComponentBody sourceBody, Vector3 sourcePosition, float loudness)
		{
			if (loudness >= 0.5f)
			{
				Vector3 v = m_componentCreature.ComponentBody.Position - sourcePosition;
				m_driveVector += Vector3.Normalize(v) * MathUtils.Max(8f - 0.25f * v.Length(), 1f);
				float num = 12f + m_random.Float(0f, 3f);
				if (m_driveVector.Length() > num)
				{
					m_driveVector = num * Vector3.Normalize(m_driveVector);
				}
			}
		}

		public void Update(float dt)
		{
			m_stateMachine.Update();
		}

		public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
		{
			m_subsystemTime = base.Project.FindSubsystem<SubsystemTime>(throwOnError: true);
			m_subsystemCreatureSpawn = base.Project.FindSubsystem<SubsystemCreatureSpawn>(throwOnError: true);
			m_componentCreature = base.Entity.FindComponent<ComponentCreature>(throwOnError: true);
			m_componentPathfinding = base.Entity.FindComponent<ComponentPathfinding>(throwOnError: true);
			m_componentHerdBehavior = base.Entity.FindComponent<ComponentHerdBehavior>(throwOnError: true);
			m_stateMachine.AddState("Inactive", delegate
			{
				m_importanceLevel = 0f;
				m_driveVector = Vector3.Zero;
			}, delegate
			{
				if (IsActive)
				{
					m_stateMachine.TransitionTo("Drive");
				}
				if (m_driveVector.Length() > 3f)
				{
					m_importanceLevel = 7f;
				}
				FadeDriveVector();
			}, null);
			m_stateMachine.AddState("Drive", delegate
			{
			}, delegate
			{
				if (!IsActive)
				{
					m_stateMachine.TransitionTo("Inactive");
				}
				if (m_driveVector.LengthSquared() < 1f || m_componentPathfinding.IsStuck)
				{
					m_importanceLevel = 0f;
				}
				if (m_random.Float(0f, 1f) < 0.1f * m_subsystemTime.GameTimeDelta)
				{
					m_componentCreature.ComponentCreatureSounds.PlayIdleSound(skipIfRecentlyPlayed: true);
				}
				if (m_random.Float(0f, 1f) < 3f * m_subsystemTime.GameTimeDelta)
				{
					Vector3 v = CalculateDriveDirectionAndSpeed();
					float speed = MathUtils.Saturate(0.2f * v.Length());
					m_componentPathfinding.SetDestination(m_componentCreature.ComponentBody.Position + 15f * Vector3.Normalize(v), speed, 5f, 0, useRandomMovements: false, ignoreHeightDifference: true, raycastDestination: false, null);
				}
				FadeDriveVector();
			}, null);
			m_stateMachine.TransitionTo("Inactive");
		}

		private void FadeDriveVector()
		{
			float num = m_driveVector.Length();
			if (num > 0.1f)
			{
				m_driveVector -= m_subsystemTime.GameTimeDelta * m_driveVector / num;
			}
		}

		private Vector3 CalculateDriveDirectionAndSpeed()
		{
			int num = 1;
			Vector3 position = m_componentCreature.ComponentBody.Position;
			Vector3 vector = position;
			Vector3 driveVector = m_driveVector;
			foreach (ComponentCreature creature in m_subsystemCreatureSpawn.Creatures)
			{
				if (creature == m_componentCreature || !(creature.ComponentHealth.Health > 0f))
				{
					continue;
				}
				ComponentCattleDriveBehavior componentCattleDriveBehavior = creature.Entity.FindComponent<ComponentCattleDriveBehavior>();
				if (componentCattleDriveBehavior != null && componentCattleDriveBehavior.m_componentHerdBehavior.HerdName == m_componentHerdBehavior.HerdName)
				{
					Vector3 position2 = creature.ComponentBody.Position;
					if (Vector3.DistanceSquared(position, position2) < 625f)
					{
						vector += position2;
						driveVector += componentCattleDriveBehavior.m_driveVector;
						num++;
					}
				}
			}
			vector /= (float)num;
			driveVector /= (float)num;
			Vector3 v = vector - position;
			float num2 = MathUtils.Max(1.5f * v.Length() - 3f, 0f);
			return 0.33f * m_driveVector + 0.66f * driveVector + num2 * Vector3.Normalize(v);
		}
	}
}
