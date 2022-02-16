using System.Xml.Linq;
using Engine;
using Engine.Media;

namespace Game
{
	public class BevelledButtonWidget : ButtonWidget
	{
		private BevelledRectangleWidget m_rectangleWidget;

		private RectangleWidget m_imageWidget;

		private LabelWidget m_labelWidget;

		private ClickableWidget m_clickableWidget;

		public override bool IsClicked => m_clickableWidget.IsClicked;

		public override bool IsChecked
		{
			get
			{
				return m_clickableWidget.IsChecked;
			}
			set
			{
				m_clickableWidget.IsChecked = value;
			}
		}

		public override bool IsAutoCheckingEnabled
		{
			get
			{
				return m_clickableWidget.IsAutoCheckingEnabled;
			}
			set
			{
				m_clickableWidget.IsAutoCheckingEnabled = value;
			}
		}

		public override string Text
		{
			get
			{
				return m_labelWidget.Text;
			}
			set
			{
				m_labelWidget.Text = value;
			}
		}

		public override BitmapFont Font
		{
			get
			{
				return m_labelWidget.Font;
			}
			set
			{
				m_labelWidget.Font = value;
			}
		}

		public Subtexture Subtexture
		{
			get
			{
				return m_imageWidget.Subtexture;
			}
			set
			{
				m_imageWidget.Subtexture = value;
			}
		}

		public override Color Color { get; set; }

		public Color BevelColor
		{
			get
			{
				return m_rectangleWidget.BevelColor;
			}
			set
			{
				m_rectangleWidget.BevelColor = value;
			}
		}

		public Color CenterColor
		{
			get
			{
				return m_rectangleWidget.CenterColor;
			}
			set
			{
				m_rectangleWidget.CenterColor = value;
			}
		}

		public float AmbientLight
		{
			get
			{
				return m_rectangleWidget.AmbientLight;
			}
			set
			{
				m_rectangleWidget.AmbientLight = value;
			}
		}

		public float DirectionalLight
		{
			get
			{
				return m_rectangleWidget.DirectionalLight;
			}
			set
			{
				m_rectangleWidget.DirectionalLight = value;
			}
		}

		public float BevelSize { get; set; }

		public BevelledButtonWidget()
		{
			Color = Color.White;
			BevelSize = 2f;
			XElement node = ContentManager.Get<XElement>("Widgets/BevelledButtonContents");
			LoadChildren(this, node);
			m_rectangleWidget = Children.Find<BevelledRectangleWidget>("BevelledButton.Rectangle");
			m_imageWidget = Children.Find<RectangleWidget>("BevelledButton.Image");
			m_labelWidget = Children.Find<LabelWidget>("BevelledButton.Label");
			m_clickableWidget = Children.Find<ClickableWidget>("BevelledButton.Clickable");
			LoadProperties(this, node);
		}

		public override void MeasureOverride(Vector2 parentAvailableSize)
		{
			bool isEnabledGlobal = base.IsEnabledGlobal;
			m_labelWidget.Color = (isEnabledGlobal ? Color : new Color(112, 112, 112));
			m_imageWidget.FillColor = (isEnabledGlobal ? Color : new Color(112, 112, 112));
			if (m_clickableWidget.IsPressed || IsChecked)
			{
				m_rectangleWidget.BevelSize = -0.5f * BevelSize;
			}
			else
			{
				m_rectangleWidget.BevelSize = BevelSize;
			}
			base.MeasureOverride(parentAvailableSize);
		}
	}
}
