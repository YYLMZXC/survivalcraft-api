using System.Xml.Linq;
using System;
using System.Collections.Generic;
namespace Game
{
    public class SettingsUiScreen : Screen
    {
        public ContainerWidget m_windowModeContainer;

        public ButtonWidget m_windowModeButton;

        public ButtonWidget m_languageButton;

        public ButtonWidget m_displayLogButton;

        public ButtonWidget m_uiSizeButton;

        public ButtonWidget m_upsideDownButton;

        public ButtonWidget m_hideMoveLookPadsButton;

        public ButtonWidget m_showGuiInScreenshotsButton;

        public ButtonWidget m_showLogoInScreenshotsButton;

        public ButtonWidget m_screenshotSizeButton;

        public ButtonWidget m_communityContentModeButton;

        public static string fName = "SettingsUiScreen";

        public SettingsUiScreen()
        {
            XElement node = ContentManager.Get<XElement>("Screens/SettingsUiScreen");
            LoadContents(this, node);
            m_windowModeContainer = Children.Find<ContainerWidget>("WindowModeContainer");
            m_languageButton = Children.Find<ButtonWidget>("LanguageButton");
            m_displayLogButton = Children.Find<ButtonWidget>("DisplayLogButton");
            m_windowModeButton = Children.Find<ButtonWidget>("WindowModeButton");
            m_uiSizeButton = Children.Find<ButtonWidget>("UiSizeButton");
            m_upsideDownButton = Children.Find<ButtonWidget>("UpsideDownButton");
            m_hideMoveLookPadsButton = Children.Find<ButtonWidget>("HideMoveLookPads");
            m_showGuiInScreenshotsButton = Children.Find<ButtonWidget>("ShowGuiInScreenshotsButton");
            m_showLogoInScreenshotsButton = Children.Find<ButtonWidget>("ShowLogoInScreenshotsButton");
            m_screenshotSizeButton = Children.Find<ButtonWidget>("ScreenshotSizeButton");
            m_communityContentModeButton = Children.Find<ButtonWidget>("CommunityContentModeButton");
        }

        public override void Enter(object[] parameters)
        {
            m_windowModeContainer.IsVisible = true;
        }

        public override void Update()
        {
            if (m_windowModeButton.IsClicked)
            {
                SettingsManager.WindowMode = (Engine.WindowMode)((int)(SettingsManager.WindowMode + 1) % EnumUtils.GetEnumValues(typeof(Engine.WindowMode)).Count);
            }
            if (m_languageButton.IsClicked)
            {
                DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(fName, 1), LanguageControl.Get(fName, 2), LanguageControl.Yes, LanguageControl.No, delegate (MessageDialogButton button)
                {
                    if (button == MessageDialogButton.Button1)
                    {
                        int next = LanguageControl.LanguageTypes.IndexOf(ModsManager.Configs["Language"]) + 1;
                        if (next == LanguageControl.LanguageTypes.Count) next = 0;
                        LanguageControl.Initialize(LanguageControl.LanguageTypes[next]);
                        foreach (var c in ModsManager.ModList)
                        {
                            c.LoadLauguage();
                        }
                        Dictionary<string, object> objs = new Dictionary<string, object>();
                        foreach (var c in ScreensManager.m_screens) {
                            Type type = c.Value.GetType();
                            object obj = Activator.CreateInstance(type);
                            objs.Add(c.Key,obj);
                        }
                        foreach (var c in objs)
                        {
                            ScreensManager.m_screens[c.Key] = c.Value as Screen;
                        }
                        CraftingRecipesManager.Initialize();
                        ScreensManager.SwitchScreen("MainMenu");
                    }
                }));
            }
            if (m_displayLogButton.IsClicked)
            {
                SettingsManager.DisplayLog = !SettingsManager.DisplayLog;
            }
            if (m_uiSizeButton.IsClicked)
            {
                SettingsManager.GuiSize = (GuiSize)((int)(SettingsManager.GuiSize + 1) % EnumUtils.GetEnumValues(typeof(GuiSize)).Count);
            }
            if (m_upsideDownButton.IsClicked)
            {
                SettingsManager.UpsideDownLayout = !SettingsManager.UpsideDownLayout;
            }
            if (m_hideMoveLookPadsButton.IsClicked)
            {
                SettingsManager.HideMoveLookPads = !SettingsManager.HideMoveLookPads;
            }
            if (m_showGuiInScreenshotsButton.IsClicked)
            {
                SettingsManager.ShowGuiInScreenshots = !SettingsManager.ShowGuiInScreenshots;
            }
            if (m_showLogoInScreenshotsButton.IsClicked)
            {
                SettingsManager.ShowLogoInScreenshots = !SettingsManager.ShowLogoInScreenshots;
            }
            if (m_screenshotSizeButton.IsClicked)
            {
                SettingsManager.ScreenshotSize = (ScreenshotSize)((int)(SettingsManager.ScreenshotSize + 1) % EnumUtils.GetEnumValues(typeof(ScreenshotSize)).Count);
            }
            if (m_communityContentModeButton.IsClicked)
            {
                SettingsManager.CommunityContentMode = (CommunityContentMode)((int)(SettingsManager.CommunityContentMode + 1) % EnumUtils.GetEnumValues(typeof(CommunityContentMode)).Count);
            }
            m_windowModeButton.Text = LanguageControl.Get("WindowMode", SettingsManager.WindowMode.ToString());
            m_uiSizeButton.Text = LanguageControl.Get("GuiSize", SettingsManager.GuiSize.ToString());
            m_languageButton.Text = LanguageControl.Get("Language","Name");
            m_displayLogButton.Text = (SettingsManager.DisplayLog ? LanguageControl.Yes : LanguageControl.No);
            m_upsideDownButton.Text = (SettingsManager.UpsideDownLayout ? LanguageControl.Yes : LanguageControl.No);
            m_hideMoveLookPadsButton.Text = (SettingsManager.HideMoveLookPads ? LanguageControl.Yes : LanguageControl.No);
            m_showGuiInScreenshotsButton.Text = (SettingsManager.ShowGuiInScreenshots ? LanguageControl.Yes : LanguageControl.No);
            m_showLogoInScreenshotsButton.Text = (SettingsManager.ShowLogoInScreenshots ? LanguageControl.Yes : LanguageControl.No);
            m_screenshotSizeButton.Text = LanguageControl.Get("ScreenshotSize", SettingsManager.ScreenshotSize.ToString());
            m_communityContentModeButton.Text = LanguageControl.Get("CommunityContentMode", SettingsManager.CommunityContentMode.ToString());
            if (Input.Back || Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked)
            {
                ScreensManager.SwitchScreen(ScreensManager.PreviousScreen);
            }
        }
    }
}
