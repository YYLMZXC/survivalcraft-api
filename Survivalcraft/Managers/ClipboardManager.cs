namespace Game
{
    public static class ClipboardManager
    {
#if android
		internal static Android.Content.ClipboardManager m_clipboardManager {get;} =
 (Android.Content.ClipboardManager)Engine.Window.Activity.GetSystemService("clipboard");
		public static string ClipboardString
		{
			get
			{
				return m_clipboardManager.Text;
			}
			set
			{
				m_clipboardManager.Text = value;
			}
		}
#else
        public static string ClipboardString
        {
            get
            {
                return System.Windows.Forms.Clipboard.GetText();
            }
            set
            {
                System.Windows.Forms.Clipboard.SetText(value);
            }
        }
#endif
    }
}