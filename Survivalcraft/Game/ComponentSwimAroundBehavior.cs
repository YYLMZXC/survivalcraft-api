using Engine;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game
{
	public class ComponentSwimAroundBehavior : ComponentBehavior, IUpdateable
	{
		private SubsystemTerrain m_subsystemTerrain;

		private ComponentCreature m_componentCreature;

		private ComponentPathfinding m_componentPathfinding;

		private StateMachine m_stateMachine = new StateMachine();

		private float m_importanceLevel = 1f;

		private Random m_random = new Random();

		public UpdateOrder UpdateOrder => UpdateOrder.Default;

		public override float ImportanceLevel => m_importanceLevel;

		public void Update(float dt)
		{
			if (string.IsNullOrEmpty(m_stateMachine.CurrentState))
			{
				m_stateMachine.TransitionTo("Inactive");
			}
			if (m_random.Float(0f, 1f) < 0.05f * dt)
			{
				m_importanceLevel = m_random.Float(1f, 3f);
			}
			if (IsActive)
			{
				m_stateMachine.Update();
			}
			else
			{
				m_stateMachine.TransitionTo("Inactive");
			}
		}

		public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
		{
			m_subsystemTerrain = base.Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
			m_componentCreature = base.Entity.FindComponent<ComponentCreature>(throwOnError: true);
			m_componentPathfinding = base.Entity.FindComponent<ComponentPathfinding>(throwOnError: true);
			m_stateMachine.AddState("Inactive", null, delegate
			{
				if (IsActive)
				{
					m_stateMachine.TransitionTo("Swim");
				}
			}, null);
			m_stateMachine.AddState("Stuck", delegate
			{
				if (m_random.Float(0f, 1f) < 0.5f)
				{
					m_importanceLevel = 1f;
				}
				m_stateMachine.TransitionTo("Swim");
			}, null, null);
			m_stateMachine.AddState("Swim", delegate
			{
				m_componentPathfinding.Stop();
			}, delegate
			{
				_ = m_componentCreature.ComponentBody.Position;
				if (!m_componentPathfinding.Destination.HasValue)
				{
					Vector3? destination = FindDestination();
					if (destination.HasValue)
					{
						m_componentPathfinding.SetDestination(destination, m_random.Float(0.3f, 0.4f), 1f, 0, useRandomMovements: false, ignoreHeightDifference: true, raycastDestination: false, null);
					}
					else
					{
						m_importanceLevel = 1f;
					}
				}
				else if (m_componentPathfinding.IsStuck)
				{
					m_stateMachine.TransitionTo("Stuck");
				}
			}, null);
		}

		private Vector3? FindDestination()
		{
			Vector3 vector = 0.5f * (m_componentCreature.ComponentBody.BoundingBox.Min + m_componentCreature.ComponentBody.BoundingBox.Max);
			float num = 2f;
			Vector3? result = null;
			float num2 = m_random.Float(10f, 16f);
			for (int i = 0; i < 16; i++)
			{
				Vector2 vector2 = m_random.Vector2(1f, 1f);
				float y = 0.3f * m_random.Float(-0.9f, 1f);
				Vector3 vector3 = Vector3.Normalize(new Vector3(vector2.X, y, vector2.Y));
				Vector3 vector4 = vector + num2 * vector3;
				TerrainRaycastResult? terrainRaycastResult = m_subsystemTerrain.Raycast(vector, vector4, useInteractionBoxes: false, skipAirBlocks: false, delegate(int value, float d)
				{
					int num3 = Terrain.ExtractContents(value);
					return !(BlocksManager.Blocks[num3] is WaterBlock);
				});
				if (!terrainRaycastResult.HasValue)
				{
					if (num2 > num)
					{
						result = vector4;
						num = num2;
					}
				}
				else if (terrainRaycastResult.Value.Distance > num)
				{
					result = vector + vector3 * terrainRaycastResult.Value.Distance;
					num = terrainRaycastResult.Value.Distance;
				}
			}
			return result;
		}
	}
}
