using System;

namespace Engine.Input
{
	public static class GamePad
	{
		private class State
		{
			public bool IsConnected;

			public Vector2[] Sticks = new Vector2[2];

			public float[] Triggers = new float[2];

			public bool[] Buttons = new bool[14];

			public bool[] LastButtons = new bool[14];

			public double[] ButtonsRepeat = new double[14];
		}

		private const double m_buttonFirstRepeatTime = 0.2;

		private const double m_buttonNextRepeatTime = 0.04;

		private static State[] m_states = new State[4]
		{
			new State(),
			new State(),
			new State(),
			new State()
		};
        internal static void Initialize()
        {
        }
        internal static void Dispose()
        {
        }
		internal static void BeforeFrame()
		{
		}

		public static bool IsConnected(int gamePadIndex)
		{
			if (gamePadIndex < 0 || gamePadIndex >= m_states.Length)
			{
				throw new ArgumentOutOfRangeException("gamePadIndex");
			}
			return m_states[gamePadIndex].IsConnected;
		}

		public static Vector2 GetStickPosition(int gamePadIndex, GamePadStick stick, float deadZone = 0f)
		{
			if (deadZone < 0f || deadZone >= 1f)
			{
				throw new ArgumentOutOfRangeException("deadZone");
			}
			if (IsConnected(gamePadIndex))
			{
				Vector2 result = m_states[gamePadIndex].Sticks[(int)stick];
				if (deadZone > 0f)
				{
					float num = result.Length();
					if (num > 0f)
					{
						float num2 = ApplyDeadZone(num, deadZone);
						result *= num2 / num;
					}
				}
				return result;
			}
			return Vector2.Zero;
		}

		public static float GetTriggerPosition(int gamePadIndex, GamePadTrigger trigger, float deadZone = 0f)
		{
			if (deadZone < 0f || deadZone >= 1f)
			{
				throw new ArgumentOutOfRangeException("deadZone");
			}
			if (IsConnected(gamePadIndex))
			{
				return ApplyDeadZone(m_states[gamePadIndex].Triggers[(int)trigger], deadZone);
			}
			return 0f;
		}

		public static bool IsButtonDown(int gamePadIndex, GamePadButton button)
		{
			if (IsConnected(gamePadIndex))
			{
				return m_states[gamePadIndex].Buttons[(int)button];
			}
			return false;
		}

		public static bool IsButtonDownOnce(int gamePadIndex, GamePadButton button)
		{
			if (IsConnected(gamePadIndex))
			{
				if (m_states[gamePadIndex].Buttons[(int)button])
				{
					return !m_states[gamePadIndex].LastButtons[(int)button];
				}
				return false;
			}
			return false;
		}

		public static bool IsButtonDownRepeat(int gamePadIndex, GamePadButton button)
		{
			if (IsConnected(gamePadIndex))
			{
				if (m_states[gamePadIndex].Buttons[(int)button] && !m_states[gamePadIndex].LastButtons[(int)button])
				{
					return true;
				}
				double num = m_states[gamePadIndex].ButtonsRepeat[(int)button];
				if (num != 0.0)
				{
					return Time.FrameStartTime >= num;
				}
				return false;
			}
			return false;
		}

		public static void Clear()
		{
			for (int i = 0; i < m_states.Length; i++)
			{
				for (int j = 0; j < m_states[i].Sticks.Length; j++)
				{
					m_states[i].Sticks[j] = Vector2.Zero;
				}
				for (int k = 0; k < m_states[i].Triggers.Length; k++)
				{
					m_states[i].Triggers[k] = 0f;
				}
				for (int l = 0; l < m_states[i].Buttons.Length; l++)
				{
					m_states[i].Buttons[l] = false;
					m_states[i].ButtonsRepeat[l] = 0.0;
				}
			}
		}

		internal static void AfterFrame()
		{
			for (int i = 0; i < m_states.Length; i++)
			{
				if (Keyboard.BackButtonQuitsApp && IsButtonDownOnce(i, GamePadButton.Back))
				{
					Window.Close();
				}
				State state = m_states[i];
				for (int j = 0; j < state.Buttons.Length; j++)
				{
					if (state.Buttons[j])
					{
						if (!state.LastButtons[j])
						{
							state.ButtonsRepeat[j] = Time.FrameStartTime + 0.2;
						}
						else if (Time.FrameStartTime >= state.ButtonsRepeat[j])
						{
							state.ButtonsRepeat[j] = MathUtils.Max(Time.FrameStartTime, state.ButtonsRepeat[j] + 0.04);
						}
					}
					else
					{
						state.ButtonsRepeat[j] = 0.0;
					}
					state.LastButtons[j] = state.Buttons[j];
				}
			}
		}

		private static float ApplyDeadZone(float value, float deadZone)
		{
			return MathUtils.Sign(value) * MathUtils.Max(MathUtils.Abs(value) - deadZone, 0f) / (1f - deadZone);
		}
	}
}
