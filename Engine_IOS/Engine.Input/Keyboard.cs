using OpenTK;
using System;
using UIKit;

namespace Engine.Input
{
	public static class Keyboard
	{

		private static bool[] m_keysDownArray = new bool[Enum.GetValues(typeof(Key)).Length];

		private static bool[] m_keysDownOnceArray = new bool[Enum.GetValues(typeof(Key)).Length];

		private static double[] m_keysDownRepeatArray = new double[Enum.GetValues(typeof(Key)).Length];

		private static Key? m_lastKey;

		public static string LastString;

		private static char? m_lastChar;

		public static Key? LastKey => m_lastKey;

		public static char? LastChar => m_lastChar;

		public static bool IsKeyboardVisible
		{
			get;
			private set;
		}

		public static bool BackButtonQuitsApp
		{
			get;
			set;
		}

		public static event Action<Key> KeyDown;

		public static event Action<Key> KeyUp;

		public static event Action<char> CharacterEntered;

		public static bool IsKeyDown(Key key)
		{
			return m_keysDownArray[(int)key];
		}

		public static bool IsKeyDownOnce(Key key)
		{
			return m_keysDownOnceArray[(int)key];
		}

		public static bool IsKeyDownRepeat(Key key)
		{
			double num = m_keysDownRepeatArray[(int)key];
			if (!(num < 0.0))
			{
				if (num != 0.0)
				{
					return Time.FrameStartTime >= num;
				}
				return false;
			}
			return true;
		}

		public static void ShowKeyboard(string title, string description, string defaultText, bool passwordMode, Action<string> enter, Action cancel)
		{
			if (title == null)
			{
				throw new ArgumentNullException("title");
			}
			if (description == null)
			{
				throw new ArgumentNullException("description");
			}
			if (defaultText == null)
			{
				throw new ArgumentNullException("defaultText");
			}
			if (!IsKeyboardVisible)
			{
				Clear();
				Touch.Clear();
				Mouse.Clear();
				IsKeyboardVisible = true;
				try
				{
					ShowKeyboardInternal(title, description, defaultText, passwordMode, delegate(string text)
					{
						Dispatcher.Dispatch(delegate
						{
							IsKeyboardVisible = false;
							if (enter != null)
							{
								enter(text ?? string.Empty);
							}
						});
					}, delegate
					{
						Dispatcher.Dispatch(delegate
						{
							IsKeyboardVisible = false;
							if (cancel != null)
							{
								cancel();
							}
						});
					});
				}
				catch
				{
					IsKeyboardVisible = false;
					throw;
				}
			}
		}

		public static void Clear()
		{
			m_lastKey = null;
			m_lastChar = null;
			for (int i = 0; i < m_keysDownArray.Length; i++)
			{
				m_keysDownArray[i] = false;
				m_keysDownOnceArray[i] = false;
				m_keysDownRepeatArray[i] = 0.0;
			}
		}

		internal static void BeforeFrame()
		{
		}

		internal static void AfterFrame()
		{
			if (BackButtonQuitsApp && IsKeyDownOnce(Key.Back))
			{
				Window.Close();
			}
			m_lastKey = null;
			m_lastChar = null;
			for (int i = 0; i < m_keysDownOnceArray.Length; i++)
			{
				m_keysDownOnceArray[i] = false;
			}
			for (int j = 0; j < m_keysDownRepeatArray.Length; j++)
			{
				if (m_keysDownArray[j])
				{
					if (m_keysDownRepeatArray[j] < 0.0)
					{
						m_keysDownRepeatArray[j] = Time.FrameStartTime + 0.2;
					}
					else if (Time.FrameStartTime >= m_keysDownRepeatArray[j])
					{
						m_keysDownRepeatArray[j] = MathUtils.Max(Time.FrameStartTime, m_keysDownRepeatArray[j] + 0.033);
					}
				}
				else
				{
					m_keysDownRepeatArray[j] = 0.0;
				}
			}
		}

		private static bool ProcessCharacterEntered(char ch)
		{
			if (!Window.IsActive || IsKeyboardVisible)
			{
				return false;
			}
			m_lastChar = ch;
			CharacterEntered?.Invoke(ch);
			return true;
		}

		internal static void Initialize()
		{
		}

		internal static void Dispose()
		{
		}

		private static void ShowKeyboardInternal(string title, string description, string defaultText, bool passwordMode, Action<string> enter, Action cancel)
		{
            UIAlertController alertController = UIAlertController.Create(title, defaultText, UIAlertControllerStyle.Alert);

            alertController.AddTextField((UITextField obj) =>
            {
                obj.Placeholder = description;
            });

            alertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, (UIAlertAction obj) =>
            {
                var userInput = alertController.TextFields[0].Text;
                enter(userInput);
            }));

            alertController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, (UIAlertAction obj) => {
                cancel();
            }));
            Window.uIViewController.PresentViewController(alertController, true, null);
        }

		private static void KeyPressHandler(object sender, KeyPressEventArgs e)
		{
			KeyboardInput.Chars.Add(e.KeyChar);
			ProcessCharacterEntered(e.KeyChar);
			LastString += e.KeyChar;
		}


	}
}
