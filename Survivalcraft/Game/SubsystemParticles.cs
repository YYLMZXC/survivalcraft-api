using System;
using System.Collections.Generic;
using Engine.Graphics;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemParticles : Subsystem, IDrawable, IUpdateable
	{
		private SubsystemTime m_subsystemTime;

		private Dictionary<ParticleSystemBase, bool> m_particleSystems = new Dictionary<ParticleSystemBase, bool>();

		public PrimitivesRenderer3D PrimitivesRenderer = new PrimitivesRenderer3D();

		public bool ParticleSystemsDraw = true;

		public bool ParticleSystemsSimulate = true;

		private int[] m_drawOrders = new int[1] { 300 };

		private List<ParticleSystemBase> m_endedParticleSystems = new List<ParticleSystemBase>();

		public UpdateOrder UpdateOrder => UpdateOrder.Default;

		public int[] DrawOrders => m_drawOrders;

		public void AddParticleSystem(ParticleSystemBase particleSystem)
		{
			if (particleSystem.SubsystemParticles == null)
			{
				m_particleSystems.Add(particleSystem, value: true);
				particleSystem.SubsystemParticles = this;
				particleSystem.OnAdded();
				return;
			}
			throw new InvalidOperationException("Particle system is already added.");
		}

		public void RemoveParticleSystem(ParticleSystemBase particleSystem)
		{
			if (particleSystem.SubsystemParticles == this)
			{
				particleSystem.OnRemoved();
				m_particleSystems.Remove(particleSystem);
				particleSystem.SubsystemParticles = null;
				return;
			}
			throw new InvalidOperationException("Particle system is not added.");
		}

		public bool ContainsParticleSystem(ParticleSystemBase particleSystem)
		{
			return particleSystem.SubsystemParticles == this;
		}

		public override void Load(ValuesDictionary valuesDictionary)
		{
			m_subsystemTime = base.Project.FindSubsystem<SubsystemTime>(throwOnError: true);
		}

		public void Update(float dt)
		{
			if (!ParticleSystemsSimulate)
			{
				return;
			}
			m_endedParticleSystems.Clear();
			foreach (ParticleSystemBase key in m_particleSystems.Keys)
			{
				if (key.Simulate(m_subsystemTime.GameTimeDelta))
				{
					m_endedParticleSystems.Add(key);
				}
			}
			foreach (ParticleSystemBase endedParticleSystem in m_endedParticleSystems)
			{
				RemoveParticleSystem(endedParticleSystem);
			}
		}

		public void Draw(Camera camera, int drawOrder)
		{
			if (!ParticleSystemsDraw)
			{
				return;
			}
			foreach (ParticleSystemBase key in m_particleSystems.Keys)
			{
				key.Draw(camera);
			}
			PrimitivesRenderer.Flush(camera.ViewProjectionMatrix);
		}
	}
}
