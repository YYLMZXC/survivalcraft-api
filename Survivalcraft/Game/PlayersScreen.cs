using System.Xml.Linq;
using Engine;
using Engine.Media;

namespace Game
{
	public class PlayersScreen : Screen
	{
		private StackPanelWidget m_playersPanel;

		private ButtonWidget m_addPlayerButton;

		private ButtonWidget m_screenLayoutButton;

		private SubsystemPlayers m_subsystemPlayers;

		private CharacterSkinsCache m_characterSkinsCache = new CharacterSkinsCache();

		public PlayersScreen()
		{
			XElement node = ContentManager.Get<XElement>("Screens/PlayersScreen");
			LoadContents(this, node);
			m_playersPanel = Children.Find<StackPanelWidget>("PlayersPanel");
			m_addPlayerButton = Children.Find<ButtonWidget>("AddPlayerButton");
			m_screenLayoutButton = Children.Find<ButtonWidget>("ScreenLayoutButton");
		}

		public override void Enter(object[] parameters)
		{
			m_subsystemPlayers = (SubsystemPlayers)parameters[0];
			m_subsystemPlayers.PlayerAdded += PlayersChanged;
			m_subsystemPlayers.PlayerRemoved += PlayersChanged;
			UpdatePlayersPanel();
		}

		public override void Leave()
		{
			m_subsystemPlayers.PlayerAdded -= PlayersChanged;
			m_subsystemPlayers.PlayerRemoved -= PlayersChanged;
			m_subsystemPlayers = null;
			m_characterSkinsCache.Clear();
			m_playersPanel.Children.Clear();
		}

		public override void Update()
		{
			if (m_addPlayerButton.IsClicked)
			{
				SubsystemGameInfo subsystemGameInfo = m_subsystemPlayers.Project.FindSubsystem<SubsystemGameInfo>(throwOnError: true);
				if (subsystemGameInfo.WorldSettings.GameMode == GameMode.Cruel)
				{
					DialogsManager.ShowDialog(null, new MessageDialog("Unavailable", "Cannot add players in cruel mode.", "OK", null, null));
				}
				else if (subsystemGameInfo.WorldSettings.GameMode == GameMode.Adventure)
				{
					DialogsManager.ShowDialog(null, new MessageDialog("Unavailable", "Cannot add players in adventure mode.", "OK", null, null));
				}
				else if (m_subsystemPlayers.PlayersData.Count >= 4)
				{
					DialogsManager.ShowDialog(null, new MessageDialog("Unavailable", $"A maximum of {4} players are allowed.", "OK", null, null));
				}
				else
				{
					ScreensManager.SwitchScreen("Player", PlayerScreen.Mode.Add, m_subsystemPlayers.Project);
				}
			}
			if (m_screenLayoutButton.IsClicked)
			{
				ScreenLayout[] array = null;
				if (m_subsystemPlayers.PlayersData.Count == 1)
				{
					array = new ScreenLayout[1];
				}
				else if (m_subsystemPlayers.PlayersData.Count == 2)
				{
					array = new ScreenLayout[3]
					{
						ScreenLayout.DoubleVertical,
						ScreenLayout.DoubleHorizontal,
						ScreenLayout.DoubleOpposite
					};
				}
				else if (m_subsystemPlayers.PlayersData.Count == 3)
				{
					array = new ScreenLayout[4]
					{
						ScreenLayout.TripleVertical,
						ScreenLayout.TripleHorizontal,
						ScreenLayout.TripleEven,
						ScreenLayout.TripleOpposite
					};
				}
				else if (m_subsystemPlayers.PlayersData.Count == 4)
				{
					array = new ScreenLayout[2]
					{
						ScreenLayout.Quadruple,
						ScreenLayout.QuadrupleOpposite
					};
				}
				if (array != null)
				{
					DialogsManager.ShowDialog(null, new ListSelectionDialog("Select Screen Layout", array, 80f, delegate(object o)
					{
						string text = o.ToString();
						string name = "Textures/Atlas/ScreenLayout" + text;
						return new StackPanelWidget
						{
							Direction = LayoutDirection.Horizontal,
							VerticalAlignment = WidgetAlignment.Center,
							Children = 
							{
								(Widget)new RectangleWidget
								{
									Size = new Vector2(98f, 56f),
									Subtexture = ContentManager.Get<Subtexture>(name),
									FillColor = Color.White,
									OutlineColor = Color.Transparent,
									Margin = new Vector2(10f, 0f)
								},
								(Widget)new StackPanelWidget
								{
									Direction = LayoutDirection.Vertical,
									VerticalAlignment = WidgetAlignment.Center,
									Margin = new Vector2(10f, 0f),
									Children = 
									{
										(Widget)new LabelWidget
										{
											Text = StringsManager.GetString("ScreenLayout." + text + ".Name"),
											Font = ContentManager.Get<BitmapFont>("Fonts/Pericles32")
										},
										(Widget)new LabelWidget
										{
											Text = StringsManager.GetString("ScreenLayout." + text + ".Description"),
											Font = ContentManager.Get<BitmapFont>("Fonts/Pericles18"),
											Color = Color.Gray
										}
									}
								}
							}
						};
					}, delegate(object o)
					{
						if (o != null)
						{
							if (m_subsystemPlayers.PlayersData.Count == 1)
							{
								SettingsManager.ScreenLayout1 = (ScreenLayout)o;
							}
							if (m_subsystemPlayers.PlayersData.Count == 2)
							{
								SettingsManager.ScreenLayout2 = (ScreenLayout)o;
							}
							if (m_subsystemPlayers.PlayersData.Count == 3)
							{
								SettingsManager.ScreenLayout3 = (ScreenLayout)o;
							}
							if (m_subsystemPlayers.PlayersData.Count == 4)
							{
								SettingsManager.ScreenLayout4 = (ScreenLayout)o;
							}
						}
					}));
				}
			}
			if (base.Input.Back || base.Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked)
			{
				ScreensManager.SwitchScreen("Game");
			}
		}

		private void UpdatePlayersPanel()
		{
			m_playersPanel.Children.Clear();
			foreach (PlayerData playersDatum in m_subsystemPlayers.PlayersData)
			{
				m_playersPanel.Children.Add(new PlayerWidget(playersDatum, m_characterSkinsCache));
			}
		}

		private void PlayersChanged(PlayerData playerData)
		{
			UpdatePlayersPanel();
		}
	}
}
