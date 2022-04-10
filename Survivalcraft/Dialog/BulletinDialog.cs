using Engine;
using System;
using System.Xml.Linq;

namespace Game
{
    public class BulletinDialog : Dialog
    {
        public LabelWidget m_titleLabel;

        public LabelWidget m_contentLabel;

        public LabelWidget m_buttonLabel;

        public ButtonWidget m_okButton;

        public ScrollPanelWidget m_scrollPanel;

        public float m_areaLength = 0;

        public Action Action;

        public BulletinDialog(string title, string content, Action action)
        {
            XElement node = ContentManager.Get<XElement>("Dialogs/BulletinDialog");
            LoadContents(this, node);
            m_okButton = Children.Find<ButtonWidget>("OkButton");
            m_titleLabel = Children.Find<LabelWidget>("Title");
            m_contentLabel = Children.Find<LabelWidget>("Content");
            m_buttonLabel = Children.Find<LabelWidget>("ButtonLabel");
            m_scrollPanel = Children.Find<ScrollPanelWidget>("ScrollPanel");
            m_buttonLabel.Text = LanguageControl.Ok;
            m_okButton.IsVisible = false;
            m_titleLabel.Text = title;
            m_contentLabel.Text = content;
            Action = action;
        }

        public override void Update()
        {
            float length = MathUtils.Max(m_scrollPanel.m_scrollAreaLength - m_scrollPanel.ActualSize.Y, 0f);
            if (m_scrollPanel.ScrollPosition >= length * 0.8f && m_scrollPanel.m_scrollAreaLength != 0)
            {
                m_okButton.IsVisible = true;
            }
            if (m_okButton.IsClicked)
            {
                Action.Invoke();
                DialogsManager.HideDialog(this);
            }
        }
    }
}
