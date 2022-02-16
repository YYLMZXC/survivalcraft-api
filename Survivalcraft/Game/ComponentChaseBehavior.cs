using Engine;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game
{
	public class ComponentChaseBehavior : ComponentBehavior, IUpdateable
	{
		private SubsystemGameInfo m_subsystemGameInfo;

		private SubsystemPlayers m_subsystemPlayers;

		private SubsystemSky m_subsystemSky;

		private SubsystemBodies m_subsystemBodies;

		private SubsystemTime m_subsystemTime;

		private SubsystemNoise m_subsystemNoise;

		private ComponentCreature m_componentCreature;

		private ComponentPathfinding m_componentPathfinding;

		private ComponentMiner m_componentMiner;

		private ComponentRandomFeedBehavior m_componentFeedBehavior;

		private ComponentCreatureModel m_componentCreatureModel;

		private DynamicArray<ComponentBody> m_componentBodies = new DynamicArray<ComponentBody>();

		private Random m_random = new Random();

		private StateMachine m_stateMachine = new StateMachine();

		private float m_dayChaseRange;

		private float m_nightChaseRange;

		private float m_dayChaseTime;

		private float m_nightChaseTime;

		private float m_chaseNonPlayerProbability;

		private float m_chaseWhenAttackedProbability;

		private float m_chaseOnTouchProbability;

		private CreatureCategory m_autoChaseMask;

		private float m_importanceLevel;

		private float m_targetUnsuitableTime;

		private float m_targetInRangeTime;

		private double m_nextUpdateTime;

		private ComponentCreature m_target;

		private float m_dt;

		private float m_range;

		private float m_chaseTime;

		private bool m_isPersistent;

		private float m_autoChaseSuppressionTime;

		public ComponentCreature Target => m_target;

		public UpdateOrder UpdateOrder => UpdateOrder.Default;

		public override float ImportanceLevel => m_importanceLevel;

		public void Attack(ComponentCreature componentCreature, float maxRange, float maxChaseTime, bool isPersistent)
		{
			m_target = componentCreature;
			m_nextUpdateTime = 0.0;
			m_range = maxRange;
			m_chaseTime = maxChaseTime;
			m_isPersistent = isPersistent;
			m_importanceLevel = 200f;
		}

		public void Update(float dt)
		{
			m_autoChaseSuppressionTime -= dt;
			if (IsActive && m_target != null)
			{
				m_chaseTime -= dt;
				m_componentCreature.ComponentCreatureModel.LookAtOrder = m_target.ComponentCreatureModel.EyePosition;
				if (IsTargetInAttackRange(m_target.ComponentBody))
				{
					m_componentCreatureModel.AttackOrder = true;
				}
				if (m_componentCreatureModel.IsAttackHitMoment)
				{
					Vector3 hitPoint;
					ComponentBody hitBody = GetHitBody(m_target.ComponentBody, out hitPoint);
					if (hitBody != null)
					{
						float x = (m_isPersistent ? m_random.Float(8f, 10f) : 2f);
						m_chaseTime = MathUtils.Max(m_chaseTime, x);
						m_componentMiner.Hit(hitBody, hitPoint, m_componentCreature.ComponentBody.Matrix.Forward);
						m_componentCreature.ComponentCreatureSounds.PlayAttackSound();
					}
				}
			}
			if (m_subsystemTime.GameTime >= m_nextUpdateTime)
			{
				m_dt = m_random.Float(0.25f, 0.35f) + MathUtils.Min((float)(m_subsystemTime.GameTime - m_nextUpdateTime), 0.1f);
				m_nextUpdateTime = m_subsystemTime.GameTime + (double)m_dt;
				m_stateMachine.Update();
			}
		}

		public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
		{
			m_subsystemGameInfo = base.Project.FindSubsystem<SubsystemGameInfo>(throwOnError: true);
			m_subsystemPlayers = base.Project.FindSubsystem<SubsystemPlayers>(throwOnError: true);
			m_subsystemSky = base.Project.FindSubsystem<SubsystemSky>(throwOnError: true);
			m_subsystemBodies = base.Project.FindSubsystem<SubsystemBodies>(throwOnError: true);
			m_subsystemTime = base.Project.FindSubsystem<SubsystemTime>(throwOnError: true);
			m_subsystemNoise = base.Project.FindSubsystem<SubsystemNoise>(throwOnError: true);
			m_componentCreature = base.Entity.FindComponent<ComponentCreature>(throwOnError: true);
			m_componentPathfinding = base.Entity.FindComponent<ComponentPathfinding>(throwOnError: true);
			m_componentMiner = base.Entity.FindComponent<ComponentMiner>(throwOnError: true);
			m_componentFeedBehavior = base.Entity.FindComponent<ComponentRandomFeedBehavior>();
			m_componentCreatureModel = base.Entity.FindComponent<ComponentCreatureModel>(throwOnError: true);
			m_dayChaseRange = valuesDictionary.GetValue<float>("DayChaseRange");
			m_nightChaseRange = valuesDictionary.GetValue<float>("NightChaseRange");
			m_dayChaseTime = valuesDictionary.GetValue<float>("DayChaseTime");
			m_nightChaseTime = valuesDictionary.GetValue<float>("NightChaseTime");
			m_autoChaseMask = valuesDictionary.GetValue<CreatureCategory>("AutoChaseMask");
			m_chaseNonPlayerProbability = valuesDictionary.GetValue<float>("ChaseNonPlayerProbability");
			m_chaseWhenAttackedProbability = valuesDictionary.GetValue<float>("ChaseWhenAttackedProbability");
			m_chaseOnTouchProbability = valuesDictionary.GetValue<float>("ChaseOnTouchProbability");
			m_componentCreature.ComponentHealth.Attacked += delegate(ComponentCreature attacker)
			{
				if (m_random.Float(0f, 1f) < m_chaseWhenAttackedProbability)
				{
					if (m_chaseWhenAttackedProbability >= 1f)
					{
						Attack(attacker, 30f, 60f, isPersistent: true);
					}
					else
					{
						Attack(attacker, 7f, 7f, isPersistent: false);
					}
				}
			};
			m_componentCreature.ComponentBody.CollidedWithBody += delegate(ComponentBody body)
			{
				if (m_target == null && m_autoChaseSuppressionTime <= 0f && m_random.Float(0f, 1f) < m_chaseOnTouchProbability)
				{
					ComponentCreature componentCreature2 = body.Entity.FindComponent<ComponentCreature>();
					if (componentCreature2 != null)
					{
						bool flag2 = m_subsystemPlayers.IsPlayer(body.Entity);
						bool flag3 = (componentCreature2.Category & m_autoChaseMask) != 0;
						if ((flag2 && m_subsystemGameInfo.WorldSettings.GameMode > GameMode.Harmless) || (!flag2 && flag3))
						{
							Attack(componentCreature2, 7f, 7f, isPersistent: false);
						}
					}
				}
				if (m_target != null && body == m_target.ComponentBody && body.StandingOnBody == m_componentCreature.ComponentBody)
				{
					m_componentCreature.ComponentLocomotion.JumpOrder = 1f;
				}
			};
			m_stateMachine.AddState("LookingForTarget", delegate
			{
				m_importanceLevel = 0f;
				m_target = null;
			}, delegate
			{
				if (IsActive)
				{
					m_stateMachine.TransitionTo("Chasing");
				}
				else if (m_autoChaseSuppressionTime <= 0f && (m_target == null || ScoreTarget(m_target) <= 0f) && m_componentCreature.ComponentHealth.Health > 0.4f)
				{
					m_range = ((m_subsystemSky.SkyLightIntensity < 0.2f) ? m_nightChaseRange : m_dayChaseRange);
					ComponentCreature componentCreature = FindTarget();
					if (componentCreature != null)
					{
						m_targetInRangeTime += m_dt;
					}
					else
					{
						m_targetInRangeTime = 0f;
					}
					if (m_targetInRangeTime > 3f)
					{
						bool flag = m_subsystemSky.SkyLightIntensity >= 0.1f;
						float maxRange = (flag ? (m_dayChaseRange + 6f) : (m_nightChaseRange + 6f));
						float maxChaseTime = (flag ? (m_dayChaseTime * m_random.Float(0.75f, 1f)) : (m_nightChaseTime * m_random.Float(0.75f, 1f)));
						Attack(componentCreature, maxRange, maxChaseTime, (!flag) ? true : false);
					}
				}
			}, null);
			m_stateMachine.AddState("RandomMoving", delegate
			{
				m_componentPathfinding.SetDestination(m_componentCreature.ComponentBody.Position + new Vector3(6f * m_random.Float(-1f, 1f), 0f, 6f * m_random.Float(-1f, 1f)), 1f, 1f, 0, useRandomMovements: false, ignoreHeightDifference: true, raycastDestination: false, null);
			}, delegate
			{
				if (m_componentPathfinding.IsStuck || !m_componentPathfinding.Destination.HasValue)
				{
					m_stateMachine.TransitionTo("Chasing");
				}
				if (!IsActive)
				{
					m_stateMachine.TransitionTo("LookingForTarget");
				}
			}, delegate
			{
				m_componentPathfinding.Stop();
			});
			m_stateMachine.AddState("Chasing", delegate
			{
				m_subsystemNoise.MakeNoise(m_componentCreature.ComponentBody, 0.25f, 6f);
				m_componentCreature.ComponentCreatureSounds.PlayIdleSound(skipIfRecentlyPlayed: false);
				m_nextUpdateTime = 0.0;
			}, delegate
			{
				if (!IsActive)
				{
					m_stateMachine.TransitionTo("LookingForTarget");
				}
				else if (m_chaseTime <= 0f)
				{
					m_autoChaseSuppressionTime = m_random.Float(10f, 60f);
					m_importanceLevel = 0f;
				}
				else if (m_target == null)
				{
					m_importanceLevel = 0f;
				}
				else if (m_target.ComponentHealth.Health <= 0f)
				{
					if (m_componentFeedBehavior != null)
					{
						m_subsystemTime.QueueGameTimeDelayedExecution(m_subsystemTime.GameTime + (double)m_random.Float(1f, 3f), delegate
						{
							if (m_target != null)
							{
								m_componentFeedBehavior.Feed(m_target.ComponentBody.Position);
							}
						});
					}
					m_importanceLevel = 0f;
				}
				else if (!m_isPersistent && m_componentPathfinding.IsStuck)
				{
					m_importanceLevel = 0f;
				}
				else if (m_isPersistent && m_componentPathfinding.IsStuck)
				{
					m_stateMachine.TransitionTo("RandomMoving");
				}
				else
				{
					if (ScoreTarget(m_target) <= 0f)
					{
						m_targetUnsuitableTime += m_dt;
					}
					else
					{
						m_targetUnsuitableTime = 0f;
					}
					if (m_targetUnsuitableTime > 3f)
					{
						m_importanceLevel = 0f;
					}
					else
					{
						int maxPathfindingPositions = 0;
						if (m_isPersistent)
						{
							maxPathfindingPositions = (m_subsystemTime.FixedTimeStep.HasValue ? 1500 : 500);
						}
						BoundingBox boundingBox = m_componentCreature.ComponentBody.BoundingBox;
						BoundingBox boundingBox2 = m_target.ComponentBody.BoundingBox;
						Vector3 v = 0.5f * (boundingBox.Min + boundingBox.Max);
						Vector3 vector = 0.5f * (boundingBox2.Min + boundingBox2.Max);
						float num = Vector3.Distance(v, vector);
						float num2 = ((num < 4f) ? 0.2f : 0f);
						m_componentPathfinding.SetDestination(vector + num2 * num * m_target.ComponentBody.Velocity, 1f, 1.5f, maxPathfindingPositions, useRandomMovements: true, ignoreHeightDifference: false, raycastDestination: true, m_target.ComponentBody);
						if (m_random.Float(0f, 1f) < 0.33f * m_dt)
						{
							m_componentCreature.ComponentCreatureSounds.PlayAttackSound();
						}
					}
				}
			}, null);
			m_stateMachine.TransitionTo("LookingForTarget");
		}

		private ComponentCreature FindTarget()
		{
			Vector3 position = m_componentCreature.ComponentBody.Position;
			ComponentCreature result = null;
			float num = 0f;
			m_componentBodies.Clear();
			m_subsystemBodies.FindBodiesAroundPoint(new Vector2(position.X, position.Z), m_range, m_componentBodies);
			for (int i = 0; i < m_componentBodies.Count; i++)
			{
				ComponentCreature componentCreature = m_componentBodies.Array[i].Entity.FindComponent<ComponentCreature>();
				if (componentCreature != null)
				{
					float num2 = ScoreTarget(componentCreature);
					if (num2 > num)
					{
						num = num2;
						result = componentCreature;
					}
				}
			}
			return result;
		}

		private float ScoreTarget(ComponentCreature componentCreature)
		{
			bool flag = componentCreature.Entity.FindComponent<ComponentPlayer>() != null;
			bool flag2 = m_componentCreature.Category != CreatureCategory.WaterPredator && m_componentCreature.Category != CreatureCategory.WaterOther;
			bool flag3 = componentCreature == Target || m_subsystemGameInfo.WorldSettings.GameMode > GameMode.Harmless;
			bool flag4 = (componentCreature.Category & m_autoChaseMask) != 0;
			bool flag5 = componentCreature == Target || (flag4 && MathUtils.Remainder(0.004999999888241291 * m_subsystemTime.GameTime + (double)((float)(GetHashCode() % 1000) / 1000f) + (double)((float)(componentCreature.GetHashCode() % 1000) / 1000f), 1.0) < (double)m_chaseNonPlayerProbability);
			if (componentCreature != m_componentCreature && ((!flag && flag5) || (flag && flag3)) && componentCreature.Entity.IsAddedToProject && componentCreature.ComponentHealth.Health > 0f && (flag2 || IsTargetInWater(componentCreature.ComponentBody)))
			{
				float num = Vector3.Distance(m_componentCreature.ComponentBody.Position, componentCreature.ComponentBody.Position);
				if (num < m_range)
				{
					return m_range - num;
				}
			}
			return 0f;
		}

		private bool IsTargetInWater(ComponentBody target)
		{
			if (target.ImmersionDepth > 0f)
			{
				return true;
			}
			if (target.ParentBody != null && IsTargetInWater(target.ParentBody))
			{
				return true;
			}
			if (target.StandingOnBody != null && target.StandingOnBody.Position.Y < target.Position.Y && IsTargetInWater(target.StandingOnBody))
			{
				return true;
			}
			return false;
		}

		private bool IsTargetInAttackRange(ComponentBody target)
		{
			if (IsBodyInAttackRange(target))
			{
				return true;
			}
			BoundingBox boundingBox = m_componentCreature.ComponentBody.BoundingBox;
			BoundingBox boundingBox2 = target.BoundingBox;
			Vector3 vector = 0.5f * (boundingBox.Min + boundingBox.Max);
			Vector3 vector2 = 0.5f * (boundingBox2.Min + boundingBox2.Max) - vector;
			float num = vector2.Length();
			Vector3 v = vector2 / num;
			float num2 = 0.5f * (boundingBox.Max.X - boundingBox.Min.X + boundingBox2.Max.X - boundingBox2.Min.X);
			float num3 = 0.5f * (boundingBox.Max.Y - boundingBox.Min.Y + boundingBox2.Max.Y - boundingBox2.Min.Y);
			if (MathUtils.Abs(vector2.Y) < num3 * 0.99f)
			{
				if (num < num2 + 0.99f && Vector3.Dot(v, m_componentCreature.ComponentBody.Matrix.Forward) > 0.25f)
				{
					return true;
				}
			}
			else if (num < num3 + 0.3f && MathUtils.Abs(Vector3.Dot(v, Vector3.UnitY)) > 0.8f)
			{
				return true;
			}
			if (target.ParentBody != null && IsTargetInAttackRange(target.ParentBody))
			{
				return true;
			}
			if (target.StandingOnBody != null && target.StandingOnBody.Position.Y < target.Position.Y && IsTargetInAttackRange(target.StandingOnBody))
			{
				return true;
			}
			return false;
		}

		private bool IsBodyInAttackRange(ComponentBody target)
		{
			BoundingBox boundingBox = m_componentCreature.ComponentBody.BoundingBox;
			BoundingBox boundingBox2 = target.BoundingBox;
			Vector3 vector = 0.5f * (boundingBox.Min + boundingBox.Max);
			Vector3 vector2 = 0.5f * (boundingBox2.Min + boundingBox2.Max) - vector;
			float num = vector2.Length();
			Vector3 v = vector2 / num;
			float num2 = 0.5f * (boundingBox.Max.X - boundingBox.Min.X + boundingBox2.Max.X - boundingBox2.Min.X);
			float num3 = 0.5f * (boundingBox.Max.Y - boundingBox.Min.Y + boundingBox2.Max.Y - boundingBox2.Min.Y);
			if (MathUtils.Abs(vector2.Y) < num3 * 0.99f)
			{
				if (num < num2 + 0.99f && Vector3.Dot(v, m_componentCreature.ComponentBody.Matrix.Forward) > 0.25f)
				{
					return true;
				}
			}
			else if (num < num3 + 0.3f && MathUtils.Abs(Vector3.Dot(v, Vector3.UnitY)) > 0.8f)
			{
				return true;
			}
			return false;
		}

		private ComponentBody GetHitBody(ComponentBody target, out Vector3 hitPoint)
		{
			Vector3 vector = m_componentCreature.ComponentBody.BoundingBox.Center();
			Vector3 vector2 = target.BoundingBox.Center();
			Ray3 ray = new Ray3(vector, Vector3.Normalize(vector2 - vector));
			BodyRaycastResult? bodyRaycastResult = m_componentMiner.Raycast<BodyRaycastResult>(ray, RaycastMode.Interaction);
			if (bodyRaycastResult.HasValue && bodyRaycastResult.Value.Distance < 1.75f && (bodyRaycastResult.Value.ComponentBody == target || bodyRaycastResult.Value.ComponentBody.IsChildOfBody(target) || target.IsChildOfBody(bodyRaycastResult.Value.ComponentBody) || target.StandingOnBody == bodyRaycastResult.Value.ComponentBody))
			{
				hitPoint = bodyRaycastResult.Value.HitPoint();
				return bodyRaycastResult.Value.ComponentBody;
			}
			hitPoint = default(Vector3);
			return null;
		}
	}
}
