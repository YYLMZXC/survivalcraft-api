using System.Collections.Generic;
using Engine;
using Engine.Audio;
using Engine.Content;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemAudio : Subsystem, IUpdateable
	{
		private class Congestion
		{
			public double LastUpdateTime;

			public double LastPlayedTime;

			public float LastPlayedVolume;

			public float Value;
		}

		private struct SoundInfo
		{
			public double Time;

			public string Name;

			public float Volume;

			public float Pitch;

			public float Pan;
		}

		private SubsystemTime m_subsystemTime;

		private SubsystemGameWidgets m_subsystemViews;

		private Random m_random = new Random();

		private List<Vector3> m_listenerPositions = new List<Vector3>();

		private Dictionary<string, Congestion> m_congestions = new Dictionary<string, Congestion>();

		private double m_nextSoundTime;

		private List<SoundInfo> m_queuedSounds = new List<SoundInfo>();

		private List<Sound> m_sounds = new List<Sound>();

		private Dictionary<Sound, bool> m_mutedSounds = new Dictionary<Sound, bool>();

		public ReadOnlyList<Vector3> ListenerPositions => new ReadOnlyList<Vector3>(m_listenerPositions);

		public UpdateOrder UpdateOrder => UpdateOrder.Default;

		public float CalculateListenerDistanceSquared(Vector3 p)
		{
			float num = float.MaxValue;
			for (int i = 0; i < m_listenerPositions.Count; i++)
			{
				float num2 = Vector3.DistanceSquared(m_listenerPositions[i], p);
				if (num2 < num)
				{
					num = num2;
				}
			}
			return num;
		}

		public float CalculateListenerDistance(Vector3 p)
		{
			return MathUtils.Sqrt(CalculateListenerDistanceSquared(p));
		}

		public void Mute()
		{
			foreach (Sound sound in m_sounds)
			{
				if (sound.State == SoundState.Playing)
				{
					m_mutedSounds[sound] = true;
					sound.Pause();
				}
			}
		}

		public void Unmute()
		{
			foreach (Sound key in m_mutedSounds.Keys)
			{
				key.Play();
			}
			m_mutedSounds.Clear();
		}

		public void PlaySound(string name, float volume, float pitch, float pan, float delay)
		{
			double num = m_subsystemTime.GameTime + (double)delay;
			m_nextSoundTime = MathUtils.Min(m_nextSoundTime, num);
			m_queuedSounds.Add(new SoundInfo
			{
				Time = num,
				Name = name,
				Volume = volume,
				Pitch = pitch,
				Pan = pan
			});
		}

		public void PlaySound(string name, float volume, float pitch, Vector3 position, float minDistance, float delay)
		{
			float num = CalculateVolume(CalculateListenerDistance(position), minDistance);
			PlaySound(name, volume * num, pitch, 0f, delay);
		}

		public void PlaySound(string name, float volume, float pitch, Vector3 position, float minDistance, bool autoDelay)
		{
			float num = CalculateVolume(CalculateListenerDistance(position), minDistance);
			PlaySound(name, volume * num, pitch, 0f, autoDelay ? CalculateDelay(position) : 0f);
		}

		public void PlayRandomSound(string directory, float volume, float pitch, float pan, float delay)
		{
			ReadOnlyList<ContentInfo> readOnlyList = ContentManager.List(directory);
			if (readOnlyList.Count > 0)
			{
				int index = m_random.Int(0, readOnlyList.Count - 1);
				PlaySound(readOnlyList[index].Name, volume, pitch, pan, delay);
			}
			else
			{
				Log.Warning("Sounds directory \"{0}\" not found or empty.", directory);
			}
		}

		public void PlayRandomSound(string directory, float volume, float pitch, Vector3 position, float minDistance, float delay)
		{
			float num = CalculateVolume(CalculateListenerDistance(position), minDistance);
			PlayRandomSound(directory, volume * num, pitch, 0f, delay);
		}

		public void PlayRandomSound(string directory, float volume, float pitch, Vector3 position, float minDistance, bool autoDelay)
		{
			float num = CalculateVolume(CalculateListenerDistance(position), minDistance);
			PlayRandomSound(directory, volume * num, pitch, 0f, autoDelay ? CalculateDelay(position) : 0f);
		}

		public Sound CreateSound(string name)
		{
			Sound sound = new Sound(ContentManager.Get<SoundBuffer>(name));
			m_sounds.Add(sound);
			return sound;
		}

		public float CalculateVolume(float distance, float minDistance, float rolloffFactor = 2f)
		{
			if (distance > minDistance)
			{
				return minDistance / (minDistance + MathUtils.Max(rolloffFactor * (distance - minDistance), 0f));
			}
			return 1f;
		}

		public float CalculateDelay(Vector3 position)
		{
			return CalculateDelay(CalculateListenerDistance(position));
		}

		public float CalculateDelay(float distance)
		{
			return MathUtils.Min(distance / 100f, 5f);
		}

		public void Update(float dt)
		{
			m_listenerPositions.Clear();
			foreach (GameWidget gameWidget in m_subsystemViews.GameWidgets)
			{
				m_listenerPositions.Add(gameWidget.ActiveCamera.ViewPosition);
			}
			if (!(m_subsystemTime.GameTime >= m_nextSoundTime))
			{
				return;
			}
			m_nextSoundTime = double.MaxValue;
			int num = 0;
			while (num < m_queuedSounds.Count)
			{
				SoundInfo soundInfo = m_queuedSounds[num];
				if (m_subsystemTime.GameTime >= soundInfo.Time)
				{
					if (m_subsystemTime.GameTimeFactor == 1f && !m_subsystemTime.FixedTimeStep.HasValue && soundInfo.Volume * SettingsManager.SoundsVolume > AudioManager.MinAudibleVolume && UpdateCongestion(soundInfo.Name, soundInfo.Volume))
					{
						AudioManager.PlaySound(soundInfo.Name, soundInfo.Volume, soundInfo.Pitch, soundInfo.Pan);
					}
					m_queuedSounds.RemoveAt(num);
				}
				else
				{
					m_nextSoundTime = MathUtils.Min(m_nextSoundTime, soundInfo.Time);
					num++;
				}
			}
		}

		public override void Load(ValuesDictionary valuesDictionary)
		{
			m_subsystemTime = base.Project.FindSubsystem<SubsystemTime>(throwOnError: true);
			m_subsystemViews = base.Project.FindSubsystem<SubsystemGameWidgets>(throwOnError: true);
		}

		public override void Dispose()
		{
			foreach (Sound sound in m_sounds)
			{
				sound.Dispose();
			}
		}

		private bool UpdateCongestion(string name, float volume)
		{
			if (!m_congestions.TryGetValue(name, out var value))
			{
				value = new Congestion();
				m_congestions.Add(name, value);
			}
			double realTime = Time.RealTime;
			double lastUpdateTime = value.LastUpdateTime;
			double lastPlayedTime = value.LastPlayedTime;
			float num = ((lastUpdateTime > 0.0) ? ((float)(realTime - lastUpdateTime)) : 0f);
			value.Value = MathUtils.Max(value.Value - 10f * num, 0f);
			value.LastUpdateTime = realTime;
			if (value.Value <= 6f && (lastPlayedTime == 0.0 || volume > value.LastPlayedVolume || realTime - lastPlayedTime >= 0.0))
			{
				value.LastPlayedTime = realTime;
				value.LastPlayedVolume = volume;
				value.Value += 1f;
				return true;
			}
			return false;
		}
	}
}
