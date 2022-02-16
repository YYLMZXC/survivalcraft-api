using System;
using Engine;
using Engine.Serialization;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game
{
	public class ComponentCreature : Component
	{
		private SubsystemPlayerStats m_subsystemPlayerStats;

		private string[] m_killVerbs;

		public ComponentBody ComponentBody { get; private set; }

		public ComponentHealth ComponentHealth { get; private set; }

		public ComponentSpawn ComponentSpawn { get; private set; }

		public ComponentCreatureModel ComponentCreatureModel { get; private set; }

		public ComponentCreatureSounds ComponentCreatureSounds { get; private set; }

		public ComponentLocomotion ComponentLocomotion { get; private set; }

		public PlayerStats PlayerStats
		{
			get
			{
				ComponentPlayer componentPlayer = this as ComponentPlayer;
				if (componentPlayer != null)
				{
					return m_subsystemPlayerStats.GetPlayerStats(componentPlayer.PlayerData.PlayerIndex);
				}
				return null;
			}
		}

		public bool ConstantSpawn { get; set; }

		public CreatureCategory Category { get; private set; }

		public string DisplayName { get; private set; }

		public ReadOnlyList<string> KillVerbs => new ReadOnlyList<string>(m_killVerbs);

		public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
		{
			ComponentBody = base.Entity.FindComponent<ComponentBody>(throwOnError: true);
			ComponentHealth = base.Entity.FindComponent<ComponentHealth>(throwOnError: true);
			ComponentSpawn = base.Entity.FindComponent<ComponentSpawn>(throwOnError: true);
			ComponentCreatureSounds = base.Entity.FindComponent<ComponentCreatureSounds>(throwOnError: true);
			ComponentCreatureModel = base.Entity.FindComponent<ComponentCreatureModel>(throwOnError: true);
			ComponentLocomotion = base.Entity.FindComponent<ComponentLocomotion>(throwOnError: true);
			m_subsystemPlayerStats = base.Project.FindSubsystem<SubsystemPlayerStats>(throwOnError: true);
			ConstantSpawn = valuesDictionary.GetValue<bool>("ConstantSpawn");
			Category = valuesDictionary.GetValue<CreatureCategory>("Category");
			DisplayName = valuesDictionary.GetValue<string>("DisplayName");
			m_killVerbs = HumanReadableConverter.ValuesListFromString<string>(',', valuesDictionary.GetValue<string>("KillVerbs"));
			if (m_killVerbs.Length == 0)
			{
				throw new InvalidOperationException("Must have at least one KillVerb");
			}
			if (!MathUtils.IsPowerOf2((long)Category))
			{
				throw new InvalidOperationException("A single category must be assigned for creature.");
			}
		}

		public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
		{
			valuesDictionary.SetValue("ConstantSpawn", ConstantSpawn);
		}
	}
}
