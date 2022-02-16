using System;
using Engine;

namespace Game
{
	public class DragHostWidget : ContainerWidget
	{
		private Widget m_dragWidget;

		private object m_dragData;

		private Action m_dragEndedHandler;

		private Vector2 m_dragPosition;

		public bool IsDragInProgress => m_dragWidget != null;

		public DragHostWidget()
		{
			IsHitTestVisible = false;
		}

		public void BeginDrag(Widget dragWidget, object dragData, Action dragEndedHandler)
		{
			if (m_dragWidget == null)
			{
				m_dragWidget = dragWidget;
				m_dragData = dragData;
				m_dragEndedHandler = dragEndedHandler;
				Children.Add(m_dragWidget);
				UpdateDragPosition();
			}
		}

		public void EndDrag()
		{
			if (m_dragWidget != null)
			{
				Children.Remove(m_dragWidget);
				m_dragWidget = null;
				m_dragData = null;
				if (m_dragEndedHandler != null)
				{
					m_dragEndedHandler();
					m_dragEndedHandler = null;
				}
			}
		}

		public override void Update()
		{
			if (m_dragWidget == null)
			{
				return;
			}
			UpdateDragPosition();
			IDragTargetWidget dragTargetWidget = HitTestGlobal(m_dragPosition, (Widget w) => w is IDragTargetWidget) as IDragTargetWidget;
			if (base.Input.Drag.HasValue)
			{
				dragTargetWidget?.DragOver(m_dragWidget, m_dragData);
				return;
			}
			try
			{
				dragTargetWidget?.DragDrop(m_dragWidget, m_dragData);
			}
			finally
			{
				EndDrag();
			}
		}

		public override void ArrangeOverride()
		{
			foreach (Widget child in Children)
			{
				Vector2 parentDesiredSize = child.ParentDesiredSize;
				parentDesiredSize.X = MathUtils.Min(parentDesiredSize.X, base.ActualSize.X);
				parentDesiredSize.Y = MathUtils.Min(parentDesiredSize.Y, base.ActualSize.Y);
				child.Arrange(ScreenToWidget(m_dragPosition) - 0.5f * parentDesiredSize, parentDesiredSize);
			}
		}

		private void UpdateDragPosition()
		{
			if (base.Input.Drag.HasValue)
			{
				m_dragPosition = base.Input.Drag.Value;
				m_dragPosition.X = MathUtils.Clamp(m_dragPosition.X, base.GlobalBounds.Min.X, base.GlobalBounds.Max.X - 1f);
				m_dragPosition.Y = MathUtils.Clamp(m_dragPosition.Y, base.GlobalBounds.Min.Y, base.GlobalBounds.Max.Y - 1f);
			}
		}
	}
}
