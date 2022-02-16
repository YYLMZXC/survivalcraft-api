using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Engine.Serialization;

namespace Game
{
	public class ArrowLineWidget : Widget
	{
		private string m_pointsString;

		private float m_width;

		private float m_arrowWidth;

		private bool m_absoluteCoordinates;

		private List<Vector2> m_vertices = new List<Vector2>();

		private bool m_parsingPending;

		private Vector2 m_startOffset;

		public float Width
		{
			get
			{
				return m_width;
			}
			set
			{
				m_width = value;
				m_parsingPending = true;
			}
		}

		public float ArrowWidth
		{
			get
			{
				return m_arrowWidth;
			}
			set
			{
				m_arrowWidth = value;
				m_parsingPending = true;
			}
		}

		public Color Color { get; set; }

		public string PointsString
		{
			get
			{
				return m_pointsString;
			}
			set
			{
				m_pointsString = value;
				m_parsingPending = true;
			}
		}

		public bool AbsoluteCoordinates
		{
			get
			{
				return m_absoluteCoordinates;
			}
			set
			{
				m_absoluteCoordinates = value;
				m_parsingPending = true;
			}
		}

		public ArrowLineWidget()
		{
			Width = 6f;
			ArrowWidth = 0f;
			Color = Color.White;
			IsHitTestVisible = false;
			PointsString = "0, 0; 50, 0";
		}

		public override void Draw(DrawContext dc)
		{
			if (m_parsingPending)
			{
				ParsePoints();
			}
			Color color = Color * base.GlobalColorTransform;
			FlatBatch2D flatBatch2D = dc.PrimitivesRenderer2D.FlatBatch(1, DepthStencilState.None);
			int count = flatBatch2D.TriangleVertices.Count;
			for (int i = 0; i < m_vertices.Count; i += 3)
			{
				Vector2 p = m_startOffset + m_vertices[i];
				Vector2 p2 = m_startOffset + m_vertices[i + 1];
				Vector2 p3 = m_startOffset + m_vertices[i + 2];
				flatBatch2D.QueueTriangle(p, p2, p3, 0f, color);
			}
			flatBatch2D.TransformTriangles(base.GlobalTransform, count);
		}

		public override void MeasureOverride(Vector2 parentAvailableSize)
		{
			if (m_parsingPending)
			{
				ParsePoints();
			}
			base.IsDrawRequired = Color.A > 0 && Width > 0f;
		}

		private void ParsePoints()
		{
			m_parsingPending = false;
			List<Vector2> list = new List<Vector2>();
			string[] array = m_pointsString.Split(';');
			foreach (string data in array)
			{
				list.Add(HumanReadableConverter.ConvertFromString<Vector2>(data));
			}
			m_vertices.Clear();
			for (int j = 0; j < list.Count; j++)
			{
				if (j >= 1)
				{
					Vector2 vector = list[j - 1];
					Vector2 vector2 = list[j];
					Vector2 vector3 = Vector2.Normalize(vector2 - vector);
					Vector2 vector4 = vector3;
					Vector2 v = vector3;
					if (j >= 2)
					{
						vector4 = Vector2.Normalize(vector - list[j - 2]);
					}
					if (j <= list.Count - 2)
					{
						v = Vector2.Normalize(list[j + 1] - vector2);
					}
					Vector2 vector5 = Vector2.Perpendicular(vector4);
					Vector2 vector6 = Vector2.Perpendicular(vector3);
					float num = (float)Math.PI - Vector2.Angle(vector4, vector3);
					float num2 = 0.5f * Width / MathUtils.Tan(num / 2f);
					Vector2 vector7 = 0.5f * vector5 * Width - vector4 * num2;
					float num3 = (float)Math.PI - Vector2.Angle(vector3, v);
					float num4 = 0.5f * Width / MathUtils.Tan(num3 / 2f);
					Vector2 vector8 = 0.5f * vector6 * Width - vector3 * num4;
					m_vertices.Add(vector + vector7);
					m_vertices.Add(vector - vector7);
					m_vertices.Add(vector2 - vector8);
					m_vertices.Add(vector2 - vector8);
					m_vertices.Add(vector2 + vector8);
					m_vertices.Add(vector + vector7);
					if (j == list.Count - 1)
					{
						m_vertices.Add(vector2 - 0.5f * ArrowWidth * vector6);
						m_vertices.Add(vector2 + 0.5f * ArrowWidth * vector6);
						m_vertices.Add(vector2 + 0.5f * ArrowWidth * vector3);
					}
				}
			}
			if (m_vertices.Count > 0)
			{
				float? num5 = null;
				float? num6 = null;
				float? num7 = null;
				float? num8 = null;
				for (int k = 0; k < m_vertices.Count; k++)
				{
					if (!num5.HasValue || m_vertices[k].X < num5)
					{
						num5 = m_vertices[k].X;
					}
					if (!num6.HasValue || m_vertices[k].Y < num6)
					{
						num6 = m_vertices[k].Y;
					}
					if (!num7.HasValue || m_vertices[k].X > num7)
					{
						num7 = m_vertices[k].X;
					}
					if (!num8.HasValue || m_vertices[k].Y > num8)
					{
						num8 = m_vertices[k].Y;
					}
				}
				if (AbsoluteCoordinates)
				{
					base.DesiredSize = new Vector2(num7.Value, num8.Value);
					m_startOffset = Vector2.Zero;
				}
				else
				{
					base.DesiredSize = new Vector2(num7.Value - num5.Value, num8.Value - num6.Value);
					m_startOffset = -new Vector2(num5.Value, num6.Value);
				}
			}
			else
			{
				base.DesiredSize = Vector2.Zero;
				m_startOffset = Vector2.Zero;
			}
		}
	}
}
