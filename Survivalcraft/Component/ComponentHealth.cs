using Engine;
using GameEntitySystem;
using System;
using TemplatesDatabase;

namespace Game
{
	public class ComponentHealth : Component, IUpdateable
	{
		public SubsystemTime m_subsystemTime;

		public SubsystemTimeOfDay m_subsystemTimeOfDay;

		public SubsystemTerrain m_subsystemTerrain;

		public SubsystemParticles m_subsystemParticles;

		public SubsystemGameInfo m_subsystemGameInfo;

		public SubsystemPickables m_subsystemPickables;

		public ComponentCreature m_componentCreature;

		public ComponentPlayer m_componentPlayer;

		public ComponentOnFire m_componentOnFire;

		public float m_lastHealth;

		public bool m_wasStanding;

		public float m_redScreenFactor;

		public Random m_random = new Random();

		public string CauseOfDeath
		{
			get;
			set;
		}

		public bool IsInvulnerable
		{
			get;
			set;
		}

		public float Health
		{
			get;
			set;
		}

		public float HealthChange
		{
			get;
			set;
		}

		public BreathingMode BreathingMode
		{
			get;
			set;
		}

		public float Air
		{
			get;
			set;
		}

		public float AirCapacity
		{
			get;
			set;
		}

		public bool CanStrand
		{
			get;
			set;
		}
		public float m_attackResilience;
		public float m_fallResilience;
		public float m_fireResilience;

		/// <summary>
		/// 生命值
		/// </summary>
		public float AttackResilience
		{
			get;
			set;
		}
		/// <summary>
		/// 掉落抗性
		/// </summary>
		public float FallResilience
		{
			get;
			set;
		}

		public float FireResilience
		{
			get;
			set;
		}

		public double? DeathTime
		{
			get;
			set;
		}

		public float CorpseDuration
		{
			get;
			set;
		}

		/// <summary>
		/// 生命值加成系数
		/// </summary>
		public float AttackResilienceFactor { get; set; }
		/// <summary>
		/// 掉落抗性加成系数
		/// </summary>
		public float FallResilienceFactor { get; set; }
		/// <summary>
		/// 火焰伤害抗性系数
		/// </summary>
		public float FireResilienceFactor { get; set; }
		/// <summary>
		/// 生命恢复速度系数
		/// </summary>
		public float HealFactor { get; set; }

		public UpdateOrder UpdateOrder => UpdateOrder.Default;

		public virtual Action<ComponentCreature> Attacked { get; set; }

		public virtual Action<ComponentCreature> Injured { get; set; }

		public virtual void Heal(float amount)
		{
			if (amount > 0f)
			{
				Health = MathUtils.Min(Health + (amount * HealFactor), 1f);
			}
		}

		public virtual void Injure(float amount, ComponentCreature attacker, bool ignoreInvulnerability, string cause)
		{
			bool pass = false;
			ModsManager.HookAction("OnCreatureInjure", loader =>
			{
				loader.OnCreatureInjure(this, amount, attacker, ignoreInvulnerability, cause, out bool Skip);
				pass |= Skip;
				return false;
			});
			if (pass) return;
			if (!(amount > 0f) || (!ignoreInvulnerability && IsInvulnerable))
			{
				return;
			}
			if (Health > 0f)
			{
				if (m_componentCreature.PlayerStats != null)
				{
					if (attacker != null)
					{
						m_componentCreature.PlayerStats.HitsReceived++;
					}
					m_componentCreature.PlayerStats.TotalHealthLost += MathUtils.Min(amount, Health);
				}
				Health = MathUtils.Max(Health - amount, 0f);
				if (Health <= 0f)
				{
					CauseOfDeath = cause;
					if (m_componentCreature.PlayerStats != null)
					{
						m_componentCreature.PlayerStats.AddDeathRecord(new PlayerStats.DeathRecord
						{
							Day = m_subsystemTimeOfDay.Day,
							Location = m_componentCreature.ComponentBody.Position,
							Cause = cause
						});
					}
					if (attacker != null)
					{
						ComponentPlayer componentPlayer = attacker.Entity.FindComponent<ComponentPlayer>();
						if (componentPlayer != null)
						{
							if (m_componentPlayer != null)
							{
								componentPlayer.PlayerStats.PlayerKills++;
							}
							else if (m_componentCreature.Category == CreatureCategory.LandPredator || m_componentCreature.Category == CreatureCategory.LandOther)
							{
								componentPlayer.PlayerStats.LandCreatureKills++;
							}
							else if (m_componentCreature.Category == CreatureCategory.WaterPredator || m_componentCreature.Category == CreatureCategory.WaterOther)
							{
								componentPlayer.PlayerStats.WaterCreatureKills++;
							}
							else
							{
								componentPlayer.PlayerStats.AirCreatureKills++;
							}
							int num = (int)MathUtils.Ceiling(m_componentCreature.ComponentHealth.AttackResilience / 12f);
							for (int i = 0; i < num; i++)
							{
								Vector2 vector = m_random.Vector2(2.5f, 3.5f);
								m_subsystemPickables.AddPickable(248, 1, m_componentCreature.ComponentBody.Position, new Vector3(vector.X, 6f, vector.Y), null);
							}
						}
					}
				}
			}
			if (attacker != null) Attacked?.Invoke(attacker);
			Injured?.Invoke(attacker);
		}

		public void Update(float dt)
		{
			//更新属性加成
			AttackResilience = m_attackResilience * AttackResilienceFactor;
			FallResilience = m_fallResilience * FallResilienceFactor;
			FireResilienceFactor = m_fireResilience * FireResilienceFactor;
			Vector3 position = m_componentCreature.ComponentBody.Position;
			if (Health > 0f && Health < 1f)
			{
				float num = 0f;
				if (m_componentPlayer != null)
				{
					if (m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Harmless)
					{
						num = 0.0166666675f;
					}
					else if (m_componentPlayer.ComponentSleep.SleepFactor == 1f && m_componentPlayer.ComponentVitalStats.Food > 0f)
					{
						num = 0.00166666671f;
					}
					else if (m_componentPlayer.ComponentVitalStats.Food > 0.5f)
					{
						num = 0.00111111114f;
					}
				}
				else
				{
					num = 0.00111111114f;
				}
				Heal(m_subsystemGameInfo.TotalElapsedGameTimeDelta * num);
			}
			if (BreathingMode == BreathingMode.Air)
			{
				int cellContents = m_subsystemTerrain.Terrain.GetCellContents(Terrain.ToCell(position.X), Terrain.ToCell(m_componentCreature.ComponentCreatureModel.EyePosition.Y), Terrain.ToCell(position.Z));
				Air = BlocksManager.Blocks[cellContents] is FluidBlock || position.Y > 259f ? MathUtils.Saturate(Air - (dt / AirCapacity)) : 1f;
			}
			else if (BreathingMode == BreathingMode.Water)
			{
				Air = m_componentCreature.ComponentBody.ImmersionFactor > 0.25f ? 1f : MathUtils.Saturate(Air - (dt / AirCapacity));
			}
			if (m_componentCreature.ComponentBody.ImmersionFactor > 0f && m_componentCreature.ComponentBody.ImmersionFluidBlock is MagmaBlock)
			{
				Injure(2f * m_componentCreature.ComponentBody.ImmersionFactor * dt, null, ignoreInvulnerability: false, LanguageControl.Get(GetType().Name, 1));
				float num2 = 1.1f + (0.1f * (float)MathUtils.Sin(12.0 * m_subsystemTime.GameTime));
				m_redScreenFactor = MathUtils.Max(m_redScreenFactor, num2 * 1.5f * m_componentCreature.ComponentBody.ImmersionFactor);
			}
			float num3 = MathUtils.Abs(m_componentCreature.ComponentBody.CollisionVelocityChange.Y);
			if (!m_wasStanding && num3 > FallResilience)
			{
				float num4 = MathUtils.Sqr(MathUtils.Max(num3 - FallResilience, 0f)) / 15f;
				if (m_componentPlayer != null)
				{
					num4 /= m_componentPlayer.ComponentLevel.ResilienceFactor;
				}
				Injure(num4, null, ignoreInvulnerability: false, LanguageControl.Get(GetType().Name, 2));
			}
			m_wasStanding = m_componentCreature.ComponentBody.StandingOnValue.HasValue || m_componentCreature.ComponentBody.StandingOnBody != null;
			if ((position.Y < 0f || position.Y > 296f) && m_subsystemTime.PeriodicGameTimeEvent(2.0, 0.0))
			{
				Injure(0.1f, null, ignoreInvulnerability: true, LanguageControl.Get(GetType().Name, 3));
				m_componentPlayer?.ComponentGui.DisplaySmallMessage(LanguageControl.Get(GetType().Name, 4), Color.White, blinking: true, playNotificationSound: false);
			}
			bool num5 = m_subsystemTime.PeriodicGameTimeEvent(1.0, 0.0);
			if (num5 && Air == 0f)
			{
				float num6 = 0.12f;
				if (m_componentPlayer != null)
				{
					num6 /= m_componentPlayer.ComponentLevel.ResilienceFactor;
				}
				Injure(num6, null, ignoreInvulnerability: false, LanguageControl.Get(GetType().Name, 7));
			}
			if (num5 && (m_componentOnFire.IsOnFire || m_componentOnFire.TouchesFire))
			{
				float num7 = 1f / FireResilience;
				if (m_componentPlayer != null)
				{
					num7 /= m_componentPlayer.ComponentLevel.ResilienceFactor;
				}
				Injure(num7, m_componentOnFire.Attacker, ignoreInvulnerability: false, LanguageControl.Get(GetType().Name, 5));
			}
			if (num5 && CanStrand && m_componentCreature.ComponentBody.ImmersionFactor < 0.25f && (m_componentCreature.ComponentBody.StandingOnValue != 0 || m_componentCreature.ComponentBody.StandingOnBody != null))
			{
				Injure(0.05f, null, ignoreInvulnerability: false, LanguageControl.Get(GetType().Name, 6));
			}
			HealthChange = Health - m_lastHealth;
			m_lastHealth = Health;
			if (m_redScreenFactor > 0.01f)
			{
				m_redScreenFactor *= MathUtils.Pow(0.2f, dt);
			}
			else
			{
				m_redScreenFactor = 0f;
			}
			if (HealthChange < 0f)
			{
				m_componentCreature.ComponentCreatureSounds.PlayPainSound();
				m_redScreenFactor += -4f * HealthChange;
				m_componentPlayer?.ComponentGui.HealthBarWidget.Flash(MathUtils.Clamp((int)((0f - HealthChange) * 30f), 0, 10));
			}
			if (m_componentPlayer != null)
			{
				m_componentPlayer.ComponentScreenOverlays.RedoutFactor = MathUtils.Max(m_componentPlayer.ComponentScreenOverlays.RedoutFactor, m_redScreenFactor);
			}
			if (m_componentPlayer != null)
			{
				m_componentPlayer.ComponentGui.HealthBarWidget.Value = Health;
			}
			if (Health == 0f && HealthChange < 0f)
			{
				bool pass = false;
				ModsManager.HookAction("DeadBeforeDrops", loader =>
				{
					loader.DeadBeforeDrops(this, out bool Skip);
					pass |= Skip;
					return false;
				});
				if (!pass)
				{
					Vector3 position2 = m_componentCreature.ComponentBody.Position + new Vector3(0f, m_componentCreature.ComponentBody.StanceBoxSize.Y / 2f, 0f);
					float x = m_componentCreature.ComponentBody.StanceBoxSize.X;
					m_subsystemParticles.AddParticleSystem(new KillParticleSystem(m_subsystemTerrain, position2, x));
					Vector3 position3 = (m_componentCreature.ComponentBody.BoundingBox.Min + m_componentCreature.ComponentBody.BoundingBox.Max) / 2f;
					foreach (IInventory item in Entity.FindComponents<IInventory>())
					{
						item.DropAllItems(position3);
					}
					DeathTime = m_subsystemGameInfo.TotalElapsedGameTime;
				}
			}
			if (Health <= 0f && CorpseDuration > 0f && m_subsystemGameInfo.TotalElapsedGameTime - DeathTime > CorpseDuration)
			{
				m_componentCreature.ComponentSpawn.Despawn();
			}
		}

		public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
		{
			m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
			m_subsystemTimeOfDay = Project.FindSubsystem<SubsystemTimeOfDay>(throwOnError: true);
			m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
			m_subsystemParticles = Project.FindSubsystem<SubsystemParticles>(throwOnError: true);
			m_subsystemGameInfo = Project.FindSubsystem<SubsystemGameInfo>(throwOnError: true);
			m_subsystemPickables = Project.FindSubsystem<SubsystemPickables>(throwOnError: true);
			m_componentCreature = Entity.FindComponent<ComponentCreature>(throwOnError: true);
			m_componentPlayer = Entity.FindComponent<ComponentPlayer>();
			m_componentOnFire = Entity.FindComponent<ComponentOnFire>(throwOnError: true);
			AttackResilience = valuesDictionary.GetValue<float>("AttackResilience");
			FallResilience = valuesDictionary.GetValue<float>("FallResilience");
			FireResilience = valuesDictionary.GetValue<float>("FireResilience");
			m_attackResilience = AttackResilience;
			m_fallResilience = FallResilience;
			m_fireResilience = FireResilience;
			CorpseDuration = valuesDictionary.GetValue<float>("CorpseDuration");
			BreathingMode = valuesDictionary.GetValue<BreathingMode>("BreathingMode");
			CanStrand = valuesDictionary.GetValue<bool>("CanStrand");
			Health = valuesDictionary.GetValue<float>("Health");
			Air = valuesDictionary.GetValue<float>("Air");
			AirCapacity = valuesDictionary.GetValue<float>("AirCapacity");
			double value = valuesDictionary.GetValue<double>("DeathTime");
			AttackResilienceFactor = 1f;
			FallResilienceFactor = 1f;
			FireResilienceFactor = 1f;
			HealFactor = 1f;
			DeathTime = (value >= 0.0) ? new double?(value) : null;
			CauseOfDeath = valuesDictionary.GetValue<string>("CauseOfDeath");
			if (m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Creative && Entity.FindComponent<ComponentPlayer>() != null)
			{
				IsInvulnerable = true;
			}
		}

		public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
		{
			valuesDictionary.SetValue("Health", Health);
			valuesDictionary.SetValue("Air", Air);
			if (DeathTime.HasValue)
			{
				valuesDictionary.SetValue("DeathTime", DeathTime);
			}
			if (!string.IsNullOrEmpty(CauseOfDeath))
			{
				valuesDictionary.SetValue("CauseOfDeath", CauseOfDeath);
			}
		}


	}
}
