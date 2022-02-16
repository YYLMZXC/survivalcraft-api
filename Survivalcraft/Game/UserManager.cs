using System;
using System.Collections.Generic;
using System.Linq;
using Engine;

namespace Game
{
	public static class UserManager
	{
		private static List<UserInfo> m_users;

		public static UserInfo ActiveUser
		{
			get
			{
				return GetUser(SettingsManager.UserId) ?? GetUsers().FirstOrDefault();
			}
			set
			{
				SettingsManager.UserId = ((value != null) ? value.UniqueId : string.Empty);
			}
		}

		static UserManager()
		{
			m_users = new List<UserInfo>();
			string text;
			try
			{
				string path = "data:/UserId.dat";
				if (!Storage.FileExists(path))
				{
					text = Guid.NewGuid().ToString();
					Storage.WriteAllText(path, text);
				}
				else
				{
					text = Storage.ReadAllText(path);
				}
			}
			catch (Exception)
			{
				text = Guid.NewGuid().ToString();
			}
			m_users.Add(new UserInfo(text.ToString(), "Windows User"));
		}

		public static IEnumerable<UserInfo> GetUsers()
		{
			return new ReadOnlyList<UserInfo>(m_users);
		}

		public static UserInfo GetUser(string uniqueId)
		{
			return GetUsers().FirstOrDefault((UserInfo u) => u.UniqueId == uniqueId);
		}
	}
}
