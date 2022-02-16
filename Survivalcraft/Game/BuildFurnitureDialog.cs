using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Engine;

namespace Game
{
	public class BuildFurnitureDialog : Dialog
	{
		private FurnitureDesign m_design;

		private FurnitureDesign m_sourceDesign;

		private int m_axis;

		private Action<bool> m_handler;

		private bool m_isValid;

		private LabelWidget m_nameLabel;

		private LabelWidget m_statusLabel;

		private FurnitureDesignWidget m_designWidget2d;

		private FurnitureDesignWidget m_designWidget3d;

		private ButtonWidget m_axisButton;

		private ButtonWidget m_leftButton;

		private ButtonWidget m_rightButton;

		private ButtonWidget m_upButton;

		private ButtonWidget m_downButton;

		private ButtonWidget m_mirrorButton;

		private ButtonWidget m_turnRightButton;

		private ButtonWidget m_increaseResolutionButton;

		private ButtonWidget m_decreaseResolutionButton;

		private LabelWidget m_resolutionLabel;

		private ButtonWidget m_nameButton;

		private ButtonWidget m_buildButton;

		private ButtonWidget m_cancelButton;

		public BuildFurnitureDialog(FurnitureDesign design, FurnitureDesign sourceDesign, Action<bool> handler)
		{
			XElement node = ContentManager.Get<XElement>("Dialogs/BuildFurnitureDialog");
			LoadContents(this, node);
			m_nameLabel = Children.Find<LabelWidget>("BuildFurnitureDialog.Name");
			m_statusLabel = Children.Find<LabelWidget>("BuildFurnitureDialog.Status");
			m_designWidget2d = Children.Find<FurnitureDesignWidget>("BuildFurnitureDialog.Design2d");
			m_designWidget3d = Children.Find<FurnitureDesignWidget>("BuildFurnitureDialog.Design3d");
			m_nameButton = Children.Find<ButtonWidget>("BuildFurnitureDialog.NameButton");
			m_axisButton = Children.Find<ButtonWidget>("BuildFurnitureDialog.AxisButton");
			m_leftButton = Children.Find<ButtonWidget>("BuildFurnitureDialog.LeftButton");
			m_rightButton = Children.Find<ButtonWidget>("BuildFurnitureDialog.RightButton");
			m_upButton = Children.Find<ButtonWidget>("BuildFurnitureDialog.UpButton");
			m_downButton = Children.Find<ButtonWidget>("BuildFurnitureDialog.DownButton");
			m_mirrorButton = Children.Find<ButtonWidget>("BuildFurnitureDialog.MirrorButton");
			m_turnRightButton = Children.Find<ButtonWidget>("BuildFurnitureDialog.TurnRightButton");
			m_increaseResolutionButton = Children.Find<ButtonWidget>("BuildFurnitureDialog.IncreaseResolutionButton");
			m_decreaseResolutionButton = Children.Find<ButtonWidget>("BuildFurnitureDialog.DecreaseResolutionButton");
			m_resolutionLabel = Children.Find<LabelWidget>("BuildFurnitureDialog.ResolutionLabel");
			m_cancelButton = Children.Find<ButtonWidget>("BuildFurnitureDialog.CancelButton");
			m_buildButton = Children.Find<ButtonWidget>("BuildFurnitureDialog.BuildButton");
			m_handler = handler;
			m_design = design;
			m_sourceDesign = sourceDesign;
			m_axis = 1;
			int num = 0;
			num += m_design.Geometry.SubsetOpaqueByFace.Sum((BlockMesh b) => (b != null) ? (b.Indices.Count / 3) : 0);
			num += m_design.Geometry.SubsetAlphaTestByFace.Sum((BlockMesh b) => (b != null) ? (b.Indices.Count / 3) : 0);
			m_isValid = num <= 300;
			m_statusLabel.Text = string.Format("Complexity {0}/{1}{2}", new object[3]
			{
				num,
				300,
				m_isValid ? " (OK)" : " (too complex)"
			});
			m_designWidget2d.Design = m_design;
			m_designWidget3d.Design = m_design;
		}

		public override void Update()
		{
			m_nameLabel.Text = (string.IsNullOrEmpty(m_design.Name) ? m_design.GetDefaultName() : m_design.Name);
			m_designWidget2d.Mode = (FurnitureDesignWidget.ViewMode)m_axis;
			m_designWidget3d.Mode = FurnitureDesignWidget.ViewMode.Perspective;
			if (m_designWidget2d.Mode == FurnitureDesignWidget.ViewMode.Side)
			{
				m_axisButton.Text = "Side View";
			}
			if (m_designWidget2d.Mode == FurnitureDesignWidget.ViewMode.Top)
			{
				m_axisButton.Text = "Top View";
			}
			if (m_designWidget2d.Mode == FurnitureDesignWidget.ViewMode.Front)
			{
				m_axisButton.Text = "Front View";
			}
			m_leftButton.IsEnabled = IsShiftPossible(DirectionAxisToDelta(0, m_axis));
			m_rightButton.IsEnabled = IsShiftPossible(DirectionAxisToDelta(1, m_axis));
			m_upButton.IsEnabled = IsShiftPossible(DirectionAxisToDelta(2, m_axis));
			m_downButton.IsEnabled = IsShiftPossible(DirectionAxisToDelta(3, m_axis));
			m_decreaseResolutionButton.IsEnabled = IsDecreaseResolutionPossible();
			m_increaseResolutionButton.IsEnabled = IsIncreaseResolutionPossible();
			m_resolutionLabel.Text = $"{m_design.Resolution}";
			m_buildButton.IsEnabled = m_isValid;
			if (m_nameButton.IsClicked)
			{
				List<Tuple<string, Action>> list = new List<Tuple<string, Action>>();
				if (m_sourceDesign != null)
				{
					list.Add(new Tuple<string, Action>("Rename Original Furniture", delegate
					{
						Dismiss(result: false);
						DialogsManager.ShowDialog(base.ParentWidget, new TextBoxDialog("Rename Furniture", m_sourceDesign.Name, 20, delegate(string s)
						{
							try
							{
								if (s != null)
								{
									m_sourceDesign.Name = s;
								}
							}
							catch (Exception ex3)
							{
								DialogsManager.ShowDialog(base.ParentWidget, new MessageDialog("Error", ex3.Message, "OK", null, null));
							}
						}));
					}));
					list.Add(new Tuple<string, Action>("Rename Modified Furniture", delegate
					{
						DialogsManager.ShowDialog(base.ParentWidget, new TextBoxDialog("Name Furniture", m_design.Name, 20, delegate(string s)
						{
							try
							{
								if (s != null)
								{
									m_design.Name = s;
								}
							}
							catch (Exception ex2)
							{
								DialogsManager.ShowDialog(base.ParentWidget, new MessageDialog("Error", ex2.Message, "OK", null, null));
							}
						}));
					}));
				}
				else
				{
					list.Add(new Tuple<string, Action>("Name Modified Furniture", delegate
					{
						DialogsManager.ShowDialog(base.ParentWidget, new TextBoxDialog("Name Furniture", m_design.Name, 20, delegate(string s)
						{
							try
							{
								if (s != null)
								{
									m_design.Name = s;
								}
							}
							catch (Exception ex)
							{
								DialogsManager.ShowDialog(base.ParentWidget, new MessageDialog("Error", ex.Message, "OK", null, null));
							}
						}));
					}));
				}
				if (list.Count == 1)
				{
					list[0].Item2();
				}
				else
				{
					DialogsManager.ShowDialog(base.ParentWidget, new ListSelectionDialog("Furniture Naming", list, 64f, (object t) => ((Tuple<string, Action>)t).Item1, delegate(object t)
					{
						((Tuple<string, Action>)t).Item2();
					}));
				}
			}
			if (m_axisButton.IsClicked)
			{
				m_axis = (m_axis + 1) % 3;
			}
			if (m_leftButton.IsClicked)
			{
				Shift(DirectionAxisToDelta(0, m_axis));
			}
			if (m_rightButton.IsClicked)
			{
				Shift(DirectionAxisToDelta(1, m_axis));
			}
			if (m_upButton.IsClicked)
			{
				Shift(DirectionAxisToDelta(2, m_axis));
			}
			if (m_downButton.IsClicked)
			{
				Shift(DirectionAxisToDelta(3, m_axis));
			}
			if (m_mirrorButton.IsClicked)
			{
				m_design.Mirror(m_axis);
			}
			if (m_turnRightButton.IsClicked)
			{
				m_design.Rotate(m_axis, 1);
			}
			if (m_decreaseResolutionButton.IsClicked)
			{
				DecreaseResolution();
			}
			if (m_increaseResolutionButton.IsClicked)
			{
				IncreaseResolution();
			}
			if (m_buildButton.IsClicked && m_isValid)
			{
				Dismiss(result: true);
			}
			if (base.Input.Back || m_cancelButton.IsClicked)
			{
				Dismiss(result: false);
			}
		}

		private bool IsShiftPossible(Point3 delta)
		{
			int resolution = m_design.Resolution;
			Box box = m_design.Box;
			box.Location += delta;
			if (box.Left >= 0 && box.Top >= 0 && box.Near >= 0 && box.Right <= resolution && box.Bottom <= resolution)
			{
				return box.Far <= resolution;
			}
			return false;
		}

		private void Shift(Point3 delta)
		{
			if (IsShiftPossible(delta))
			{
				m_design.Shift(delta);
			}
		}

		private bool IsDecreaseResolutionPossible()
		{
			int resolution = m_design.Resolution;
			if (resolution > 2)
			{
				int num = MathUtils.Max(m_design.Box.Width, m_design.Box.Height, m_design.Box.Depth);
				return resolution > num;
			}
			return false;
		}

		private void DecreaseResolution()
		{
			if (IsDecreaseResolutionPossible())
			{
				int resolution = m_design.Resolution;
				Point3 zero = Point3.Zero;
				if (m_design.Box.Right >= resolution)
				{
					zero.X = -1;
				}
				if (m_design.Box.Bottom >= resolution)
				{
					zero.Y = -1;
				}
				if (m_design.Box.Far >= resolution)
				{
					zero.Z = -1;
				}
				m_design.Shift(zero);
				m_design.Resize(resolution - 1);
			}
		}

		private bool IsIncreaseResolutionPossible()
		{
			return m_design.Resolution < 16;
		}

		private void IncreaseResolution()
		{
			if (IsIncreaseResolutionPossible())
			{
				m_design.Resize(m_design.Resolution + 1);
			}
		}

		private static Point3 DirectionAxisToDelta(int direction, int axis)
		{
			if (direction == 0)
			{
				switch (axis)
				{
				case 0:
					return new Point3(0, 0, 1);
				case 1:
					return new Point3(1, 0, 0);
				case 2:
					return new Point3(1, 0, 0);
				}
			}
			if (direction == 1)
			{
				switch (axis)
				{
				case 0:
					return new Point3(0, 0, -1);
				case 1:
					return new Point3(-1, 0, 0);
				case 2:
					return new Point3(-1, 0, 0);
				}
			}
			if (direction == 2)
			{
				switch (axis)
				{
				case 0:
					return new Point3(0, 1, 0);
				case 1:
					return new Point3(0, 0, 1);
				case 2:
					return new Point3(0, 1, 0);
				}
			}
			if (direction == 3)
			{
				switch (axis)
				{
				case 0:
					return new Point3(0, -1, 0);
				case 1:
					return new Point3(0, 0, -1);
				case 2:
					return new Point3(0, -1, 0);
				}
			}
			return Point3.Zero;
		}

		private void Dismiss(bool result)
		{
			DialogsManager.HideDialog(this);
			m_handler(result);
		}
	}
}
