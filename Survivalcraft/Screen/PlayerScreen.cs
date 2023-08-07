using Engine.Graphics;
using Engine.Input;
using GameEntitySystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TemplatesDatabase;

namespace Game
{
	public class PlayerScreen : Screen
	{
		public enum Mode
		{
			Initial,
			Add,
			Edit
		}

		public PlayerData m_playerData;

		public Mode m_mode;

		public CharacterSkinsCache m_characterSkinsCache;

		public bool m_nameWasInvalid;

		public PlayerModelWidget m_playerModel;

		public ButtonWidget m_playerClassButton;

		public TextBoxWidget m_nameTextBox;

		public LabelWidget m_characterSkinLabel;

		public ButtonWidget m_characterSkinButton;

		public LabelWidget m_controlsLabel;

		public ButtonWidget m_controlsButton;

		public LabelWidget m_descriptionLabel;

		public ButtonWidget m_addButton;

		public ButtonWidget m_addAnotherButton;

		public ButtonWidget m_deleteButton;

		public ButtonWidget m_playButton;

		public WidgetInputDevice[] m_inputDevices = new WidgetInputDevice[5]
		{
			WidgetInputDevice.None,
			WidgetInputDevice.GamePad1,
			WidgetInputDevice.GamePad2,
			WidgetInputDevice.GamePad3,
			WidgetInputDevice.GamePad4
		};

		public PlayerScreen()
		{
			XElement node = ContentManager.Get<XElement>("Screens/PlayerScreen");
			LoadContents(this, node);
			m_playerModel = Children.Find<PlayerModelWidget>("Model");
			m_playerClassButton = Children.Find<ButtonWidget>("PlayerClassButton");
			m_nameTextBox = Children.Find<TextBoxWidget>("Name");
			m_characterSkinLabel = Children.Find<LabelWidget>("CharacterSkinLabel");
			m_characterSkinButton = Children.Find<ButtonWidget>("CharacterSkinButton");
			m_controlsLabel = Children.Find<LabelWidget>("ControlsLabel");
			m_controlsButton = Children.Find<ButtonWidget>("ControlsButton");
			m_descriptionLabel = Children.Find<LabelWidget>("DescriptionLabel");
			m_addButton = Children.Find<ButtonWidget>("AddButton");
			m_addAnotherButton = Children.Find<ButtonWidget>("AddAnotherButton");
			m_deleteButton = Children.Find<ButtonWidget>("DeleteButton");
			m_playButton = Children.Find<ButtonWidget>("PlayButton");
			m_characterSkinsCache = new CharacterSkinsCache();
			m_playerModel.CharacterSkinsCache = m_characterSkinsCache;
			m_nameTextBox.FocusLost += delegate
			{
				if (VerifyName())
				{
					m_playerData.Name = m_nameTextBox.Text.Trim();
				}
				else
				{
					m_nameWasInvalid = true;
				}
			};
		}

		public override void Enter(object[] parameters)
		{
			m_mode = (Mode)parameters[0];
			m_playerData = m_mode == Mode.Edit ? (PlayerData)parameters[1] : new PlayerData((Project)parameters[1]);
			if (m_mode == Mode.Initial)
			{
				m_playerClassButton.IsEnabled = true;
				m_addButton.IsVisible = false;
				m_deleteButton.IsVisible = false;
				m_playButton.IsVisible = true;
				m_addAnotherButton.IsVisible = m_playerData.SubsystemPlayers.PlayersData.Count < 3;
			}
			else if (m_mode == Mode.Add)
			{
				m_playerClassButton.IsEnabled = true;
				m_addButton.IsVisible = true;
				m_deleteButton.IsVisible = false;
				m_playButton.IsVisible = false;
				m_addAnotherButton.IsVisible = false;
			}
			else if (m_mode == Mode.Edit)
			{
				m_playerClassButton.IsEnabled = false;
				m_addButton.IsVisible = false;
				m_deleteButton.IsVisible = m_playerData.SubsystemPlayers.PlayersData.Count > 1;
				m_playButton.IsVisible = false;
				m_addAnotherButton.IsVisible = false;
			}
		}

		public override void Leave()
		{
			m_characterSkinsCache.Clear();
			m_playerData = null;
		}

		public override void Update()
		{
			m_characterSkinsCache.GetTexture(m_playerData.CharacterSkinName);
			m_playerModel.PlayerClass = m_playerData.PlayerClass;
			m_playerModel.CharacterSkinName = m_playerData.CharacterSkinName;
			m_playerClassButton.Text = LanguageControl.Get(GetType().Name, m_playerData.PlayerClass.ToString());
			if (!m_nameTextBox.HasFocus)
			{
				m_nameTextBox.Text = m_playerData.Name;
			}
			m_characterSkinLabel.Text = CharacterSkinsManager.GetDisplayName(m_playerData.CharacterSkinName);
			m_controlsLabel.Text = GetDeviceDisplayName(m_inputDevices.Where((WidgetInputDevice id) => (id & m_playerData.InputDevice) != 0).FirstOrDefault());
			ValuesDictionary valuesDictionary = DatabaseManager.FindValuesDictionaryForComponent(DatabaseManager.FindEntityValuesDictionary(m_playerData.GetEntityTemplateName(), throwIfNotFound: true), typeof(ComponentCreature));
			string dy = valuesDictionary.GetValue<string>("Description");
			if (dy.StartsWith("[") && dy.EndsWith("]"))
			{
				string[] lp = dy.Substring(1, dy.Length - 2).Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
				dy = LanguageControl.GetDatabase("Description", lp[1]);
			}
			m_descriptionLabel.Text = dy;
			if (m_playerClassButton.IsClicked)
			{
				m_playerData.PlayerClass = (m_playerData.PlayerClass == PlayerClass.Male) ? PlayerClass.Female : PlayerClass.Male;
				m_playerData.RandomizeCharacterSkin();
				if (m_playerData.IsDefaultName)
				{
					m_playerData.ResetName();
				}
			}
			if (m_characterSkinButton.IsClicked)
			{
				CharacterSkinsManager.UpdateCharacterSkinsList();
				IEnumerable<string> items = CharacterSkinsManager.CharacterSkinsNames.Where((string n) => CharacterSkinsManager.GetPlayerClass(n) == m_playerData.PlayerClass || !CharacterSkinsManager.GetPlayerClass(n).HasValue);
				var dialog = new ListSelectionDialog(LanguageControl.Get(GetType().Name, 1), items, 64f, delegate (object item)
				 {
					 XElement node = ContentManager.Get<XElement>("Widgets/CharacterSkinItem");
					 var obj = (ContainerWidget)LoadWidget(this, node, null);
					 Texture2D texture = m_characterSkinsCache.GetTexture((string)item);
					 obj.Children.Find<LabelWidget>("CharacterSkinItem.Text").Text = CharacterSkinsManager.GetDisplayName((string)item);
					 obj.Children.Find<LabelWidget>("CharacterSkinItem.Details").Text = $"{texture.Width}x{texture.Height}";
					 PlayerModelWidget playerModelWidget = obj.Children.Find<PlayerModelWidget>("CharacterSkinItem.Model");
					 playerModelWidget.PlayerClass = m_playerData.PlayerClass;
					 playerModelWidget.CharacterSkinTexture = texture;
					 return obj;
				 }, delegate (object item)
				 {
					 m_playerData.CharacterSkinName = (string)item;
					 if (m_playerData.IsDefaultName)
					 {
						 m_playerData.ResetName();
					 }
				 });
				DialogsManager.ShowDialog(null, dialog);
			}
			if (m_controlsButton.IsClicked)
			{
				DialogsManager.ShowDialog(null, new ListSelectionDialog(LanguageControl.Get(GetType().Name, 2), m_inputDevices, 56f, (object d) => GetDeviceDisplayName((WidgetInputDevice)d), delegate (object d)
				 {
					 var widgetInputDevice = (WidgetInputDevice)d;
					 m_playerData.InputDevice = widgetInputDevice;
					 foreach (PlayerData playersDatum in m_playerData.SubsystemPlayers.PlayersData)
					 {
						 if (playersDatum != m_playerData && (playersDatum.InputDevice & widgetInputDevice) != 0)
						 {
							 playersDatum.InputDevice &= ~widgetInputDevice;
						 }
					 }
				 }));
			}
			if (m_addButton.IsClicked && VerifyName())
			{
				m_playerData.SubsystemPlayers.AddPlayerData(m_playerData);
				ScreensManager.SwitchScreen("Players", m_playerData.SubsystemPlayers);
			}
			if (m_deleteButton.IsClicked)
			{
				DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Warning, LanguageControl.Get(GetType().Name, 3), LanguageControl.Ok, LanguageControl.Cancel, delegate (MessageDialogButton b)
				   {
					   if (b == MessageDialogButton.Button1)
					   {
						   m_playerData.SubsystemPlayers.RemovePlayerData(m_playerData);
						   ScreensManager.SwitchScreen("Players", m_playerData.SubsystemPlayers);
					   }
				   }));
			}
			if (m_playButton.IsClicked && VerifyName())
			{
				m_playerData.SubsystemPlayers.AddPlayerData(m_playerData);
				ScreensManager.SwitchScreen("Game");
			}
			if (m_addAnotherButton.IsClicked && VerifyName())
			{
				m_playerData.SubsystemPlayers.AddPlayerData(m_playerData);
				ScreensManager.SwitchScreen("Player", Mode.Initial, m_playerData.SubsystemPlayers.Project);
			}
			if ((Input.Back || Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked) && VerifyName())
			{
				if (m_mode == Mode.Initial)
				{
					GameManager.SaveProject(waitForCompletion: true, showErrorDialog: true);
					GameManager.DisposeProject();
					ScreensManager.SwitchScreen("MainMenu");
				}
				else if (m_mode == Mode.Add || m_mode == Mode.Edit)
				{
					ScreensManager.SwitchScreen("Players", m_playerData.SubsystemPlayers);
				}
			}
			m_nameWasInvalid = false;
		}

		public static string GetDeviceDisplayName(WidgetInputDevice device)
		{
			switch (device)
			{
				case WidgetInputDevice.Keyboard | WidgetInputDevice.Mouse:
					return LanguageControl.Get(typeof(PlayerScreen).Name, 4);
				case WidgetInputDevice.GamePad1:
					return LanguageControl.Get(typeof(PlayerScreen).Name, 5) + (GamePad.IsConnected(0) ? "" : LanguageControl.Get(typeof(PlayerScreen).Name, 9));
				case WidgetInputDevice.GamePad2:
					return LanguageControl.Get(typeof(PlayerScreen).Name, 6) + (GamePad.IsConnected(1) ? "" : LanguageControl.Get(typeof(PlayerScreen).Name, 9));
				case WidgetInputDevice.GamePad3:
					return LanguageControl.Get(typeof(PlayerScreen).Name, 7) + (GamePad.IsConnected(2) ? "" : LanguageControl.Get(typeof(PlayerScreen).Name, 9));
				case WidgetInputDevice.GamePad4:
					return LanguageControl.Get(typeof(PlayerScreen).Name, 8) + (GamePad.IsConnected(3) ? "" : LanguageControl.Get(typeof(PlayerScreen).Name, 9));
				default:
					return LanguageControl.Get(typeof(PlayerScreen).Name, 10);
			}
		}

		public bool VerifyName()
		{
			if (m_nameWasInvalid)
			{
				return false;
			}
			if (PlayerData.VerifyName(m_nameTextBox.Text.Trim()))
			{
				return true;
			}
			DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Error, LanguageControl.Get(GetType().Name, 12), LanguageControl.Ok, null, null));
			return false;
		}
	}
}
