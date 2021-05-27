using Engine;
using Engine.Content;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Game
{
    public class LoadingScreen : Screen
    {
        public List<Action> m_loadActions = new List<Action>();

        public int m_index;

        public bool m_loadingStarted;

        public bool m_loadingFinished;

        public bool m_pauseLoading;

        public bool m_loadingErrorsSuppressed;

        public StackPanelWidget panelWidget = new StackPanelWidget() { Direction=LayoutDirection.Vertical,HorizontalAlignment=WidgetAlignment.Center,VerticalAlignment=WidgetAlignment.Far};

        public LabelWidget labelWidget = new LabelWidget() { Text="API v1.34",Color=Color.Red,VerticalAlignment=WidgetAlignment.Far, HorizontalAlignment=WidgetAlignment.Center};

        public LabelWidget labelWidget2 = new LabelWidget() { Color=Color.Red, VerticalAlignment = WidgetAlignment.Far,HorizontalAlignment = WidgetAlignment.Near,Margin=new Vector2(300,0)};

        public LoadingScreen()
        {
            XElement node = ContentManager.Get<XElement>("Screens/LoadingScreen");
            LoadContents(this, node);
            panelWidget.Children.Add(labelWidget);
            panelWidget.Children.Add(labelWidget2);
            Children.Add(panelWidget);
            AddLoadAction(delegate
            {
                SetMsg("��ʼ��DatabaseManager");
                DatabaseManager.Initialize();
            });
            AddLoadAction(delegate
            {
                SetMsg("��ʼ��CommunityContentManager");
                CommunityContentManager.Initialize();
            });
            AddLoadAction(delegate
            {
                SetMsg("��ʼ��MotdManager");
                MotdManager.Initialize();
            });
            AddLoadAction(delegate
            {
                SetMsg("��ʼ��LightingManager");
                LightingManager.Initialize();
            });
            AddLoadAction(delegate
            {
                SetMsg("��ʼ��StringsManager");
                StringsManager.LoadStrings();
            });
            AddLoadAction(delegate
            {
                SetMsg("��ʼ��TextureAtlasManager");
                TextureAtlasManager.LoadAtlases();
            });

            AddLoadAction(delegate
            {
                SetMsg("��ʼ��WorldsManager");
                WorldsManager.Initialize();
            });
            AddLoadAction(delegate
            {
                SetMsg("��ʼ��BlocksTexturesManager");
                BlocksTexturesManager.Initialize();
            });
            AddLoadAction(delegate
            {
                SetMsg("��ʼ��CharacterSkinsManager");
                CharacterSkinsManager.Initialize();
            });
            AddLoadAction(delegate
            {
                SetMsg("��ʼ��FurniturePacksManager");
                FurniturePacksManager.Initialize();
            });
            AddLoadAction(delegate
            {
                SetMsg("��ʼ��BlocksManager");
                BlocksManager.Initialize();
            });
            AddLoadAction(delegate
            {
                SetMsg("��ʼ��CraftingRecipesManager");
                CraftingRecipesManager.Initialize();
            });
            AddLoadAction(delegate
            {
                SetMsg("��ʼ��MusicManager");
                MusicManager.CurrentMix = MusicManager.Mix.Menu;
            });
            foreach (ContentInfo item in ContentManager.List())
            {
                ContentInfo localContentInfo = item;
                AddLoadAction(delegate
                {
                    SetMsg("����ļ�" + localContentInfo.Name);
                    ContentManager.Get(localContentInfo.Name);
                });
            }

        }

        public void AddLoadAction(Action action)
        {
            m_loadActions.Add(action);
        }
        public void SetMsg(string text) {
            labelWidget2.Text = text;
        }
        public override void Leave()
        {
            ContentManager.Dispose("Textures/Gui/CandyRufusLogo");
            ContentManager.Dispose("Textures/Gui/EngineLogo");
        }

        public override void Update()
        {
            if (!m_loadingStarted)
            {
                m_loadingStarted = true;
            }
            else
            {
                if (m_loadingFinished)
                {
                    return;
                }
                double realTime = Time.RealTime;
                while (!m_pauseLoading && m_index < m_loadActions.Count)
                {
                    try
                    {
                        m_loadActions[m_index++]();
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Loading error. Reason: " + ex.Message);
                        if (!m_loadingErrorsSuppressed)
                        {
                            m_pauseLoading = true;
                            DialogsManager.ShowDialog(ScreensManager.RootWidget, new MessageDialog("Loading Error", ExceptionManager.MakeFullErrorMessage(ex), "ȷ��", "Suppress", delegate (MessageDialogButton b)
                            {
                                switch (b)
                                {
                                    case MessageDialogButton.Button1:
                                        m_pauseLoading = false;
                                        break;
                                    case MessageDialogButton.Button2:
                                        m_loadingErrorsSuppressed = true;
                                        break;
                                }
                            }));
                        }
                    }
                    if (Time.RealTime - realTime > 0.1)
                    {
                        break;
                    }
                }
                if (m_index >= m_loadActions.Count)
                {
                    m_loadingFinished = true;
                    AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
                    ScreensManager.SwitchScreen("MainMenu");
                }
            }
        }
    }
}
