using System;
using System.Linq;
using System.Xml.Linq;
using Engine;

namespace Game
{
	public class MessageWidget : CanvasWidget
	{
		private LabelWidget m_labelWidget;

		private string m_message;

		private double m_messageStartTime;

		private float m_duration;

		private Color m_color;

		private bool m_blinking;

		public MessageWidget()
		{
			XElement node = ContentManager.Get<XElement>("Widgets/MessageWidget");
			LoadContents(this, node);
			m_labelWidget = Children.Find<LabelWidget>("Label");
		}

		public void DisplayMessage(string text, Color color, bool blinking)
		{
			m_message = text;
			m_messageStartTime = Time.RealTime;
			m_duration = (blinking ? 6f : (4f + MathUtils.Min(1f * (float)m_message.Count((char c) => c == '\n'), 4f)));
			m_color = color;
			m_blinking = blinking;
		}

		public override void Update()
		{
			double realTime = Time.RealTime;
			if (!string.IsNullOrEmpty(m_message))
			{
				float num;
				if (m_blinking)
				{
					num = MathUtils.Saturate(1f * (float)(m_messageStartTime + (double)m_duration - realTime));
					if (realTime - m_messageStartTime < 0.417)
					{
						num *= MathUtils.Lerp(0.25f, 1f, 0.5f * (1f - MathUtils.Cos((float)Math.PI * 12f * (float)(realTime - m_messageStartTime))));
					}
				}
				else
				{
					num = MathUtils.Saturate(MathUtils.Min(3f * (float)(realTime - m_messageStartTime), 1f * (float)(m_messageStartTime + (double)m_duration - realTime)));
				}
				m_labelWidget.Color = m_color * num;
				m_labelWidget.IsVisible = true;
				m_labelWidget.Text = m_message;
				if (realTime - m_messageStartTime > (double)m_duration)
				{
					m_message = null;
				}
			}
			else
			{
				m_labelWidget.IsVisible = false;
			}
		}
	}
}
