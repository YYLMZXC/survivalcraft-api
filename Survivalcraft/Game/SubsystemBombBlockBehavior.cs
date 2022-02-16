using System.Collections.Generic;
using System.Linq;
using Engine;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemBombBlockBehavior : SubsystemBlockBehavior, IUpdateable
	{
		private SubsystemGameInfo m_subsystemGameInfo;

		private SubsystemTime m_subsystemTime;

		private SubsystemBlockBehaviors m_subsystemBlockBehaviors;

		private SubsystemExplosions m_subsystemExplosions;

		private SubsystemProjectiles m_subsystemProjectiles;

		private Dictionary<Projectile, bool> m_projectiles = new Dictionary<Projectile, bool>();

		public override int[] HandledBlocks => new int[0];

		public UpdateOrder UpdateOrder => UpdateOrder.Default;

		public override void Load(ValuesDictionary valuesDictionary)
		{
			base.Load(valuesDictionary);
			m_subsystemGameInfo = base.Project.FindSubsystem<SubsystemGameInfo>(throwOnError: true);
			m_subsystemTime = base.Project.FindSubsystem<SubsystemTime>(throwOnError: true);
			m_subsystemExplosions = base.Project.FindSubsystem<SubsystemExplosions>(throwOnError: true);
			m_subsystemProjectiles = base.Project.FindSubsystem<SubsystemProjectiles>(throwOnError: true);
			m_subsystemBlockBehaviors = base.Project.FindSubsystem<SubsystemBlockBehaviors>(throwOnError: true);
			foreach (Projectile projectile in m_subsystemProjectiles.Projectiles)
			{
				ScanProjectile(projectile);
			}
			m_subsystemProjectiles.ProjectileAdded += delegate(Projectile projectile)
			{
				ScanProjectile(projectile);
			};
			m_subsystemProjectiles.ProjectileRemoved += delegate(Projectile projectile)
			{
				m_projectiles.Remove(projectile);
			};
		}

		private void ScanProjectile(Projectile projectile)
		{
			if (!m_projectiles.ContainsKey(projectile))
			{
				int num = Terrain.ExtractContents(projectile.Value);
				if (m_subsystemBlockBehaviors.GetBlockBehaviors(num).Contains(this))
				{
					m_projectiles.Add(projectile, value: true);
					projectile.ProjectileStoppedAction = ProjectileStoppedAction.DoNothing;
					Color color = ((num == 228) ? new Color(255, 140, 192) : Color.White);
					m_subsystemProjectiles.AddTrail(projectile, new Vector3(0f, 0.25f, 0.1f), new SmokeTrailParticleSystem(20, 0.33f, float.MaxValue, color));
				}
			}
		}

		public void Update(float dt)
		{
			if (!m_subsystemTime.PeriodicGameTimeEvent(0.1, 0.0))
			{
				return;
			}
			foreach (Projectile key in m_projectiles.Keys)
			{
				if (m_subsystemGameInfo.TotalElapsedGameTime - key.CreationTime > 5.0)
				{
					m_subsystemExplosions.TryExplodeBlock(Terrain.ToCell(key.Position.X), Terrain.ToCell(key.Position.Y), Terrain.ToCell(key.Position.Z), key.Value);
					key.ToRemove = true;
				}
			}
		}
	}
}
