using Engine;
using Engine.Graphics;
using Engine.Media;

namespace Game
{
	public class FurnitureDesignWidget : Widget
	{
		public enum ViewMode
		{
			Side,
			Top,
			Front,
			Perspective
		}

		private PrimitivesRenderer2D m_primitivesRenderer2d = new PrimitivesRenderer2D();

		private PrimitivesRenderer3D m_primitivesRenderer3d = new PrimitivesRenderer3D();

		private Vector2? m_dragStartPoint;

		private Vector3 m_direction;

		private Vector2 m_rotationSpeed;

		private static bool DrawDebugFurniture;

		public Vector2 Size { get; set; }

		public ViewMode Mode { get; set; }

		public FurnitureDesign Design { get; set; }

		public FurnitureDesignWidget()
		{
			base.ClampToBounds = true;
			Size = new Vector2(float.PositiveInfinity);
			Mode = ViewMode.Perspective;
			m_direction = Vector3.Normalize(new Vector3(1f, -0.5f, -1f));
			m_rotationSpeed = new Vector2(2f, 0.5f);
		}

		public override void Draw(DrawContext dc)
		{
			if (Design == null)
			{
				return;
			}
			Matrix matrix4;
			if (Mode == ViewMode.Perspective)
			{
				Viewport viewport = Display.Viewport;
				Vector3 vector = new Vector3(0.5f, 0.5f, 0.5f);
				Matrix matrix = Matrix.CreateLookAt(2.65f * m_direction + vector, vector, Vector3.UnitY);
				Matrix matrix2 = Matrix.CreatePerspectiveFieldOfView(1.2f, base.ActualSize.X / base.ActualSize.Y, 0.4f, 4f);
				Matrix matrix3 = MatrixUtils.CreateScaleTranslation(base.ActualSize.X, 0f - base.ActualSize.Y, base.ActualSize.X / 2f, base.ActualSize.Y / 2f) * base.GlobalTransform * MatrixUtils.CreateScaleTranslation(2f / (float)viewport.Width, -2f / (float)viewport.Height, -1f, 1f);
				matrix4 = matrix * matrix2 * matrix3;
				FlatBatch3D flatBatch3D = m_primitivesRenderer3d.FlatBatch(1, DepthStencilState.DepthRead);
				for (int i = 0; i <= Design.Resolution; i++)
				{
					float num = (float)i / (float)Design.Resolution;
					Color color = ((i % 2 == 0) ? new Color(56, 56, 56, 56) : new Color(28, 28, 28, 28));
					color *= base.GlobalColorTransform;
					flatBatch3D.QueueLine(new Vector3(num, 0f, 0f), new Vector3(num, 0f, 1f), color);
					flatBatch3D.QueueLine(new Vector3(0f, 0f, num), new Vector3(1f, 0f, num), color);
					flatBatch3D.QueueLine(new Vector3(0f, num, 0f), new Vector3(0f, num, 1f), color);
					flatBatch3D.QueueLine(new Vector3(0f, 0f, num), new Vector3(0f, 1f, num), color);
					flatBatch3D.QueueLine(new Vector3(0f, num, 1f), new Vector3(1f, num, 1f), color);
					flatBatch3D.QueueLine(new Vector3(num, 0f, 1f), new Vector3(num, 1f, 1f), color);
				}
				Color color2 = new Color(64, 64, 64, 255) * base.GlobalColorTransform;
				FontBatch3D fontBatch3D = m_primitivesRenderer3d.FontBatch(ContentManager.Get<BitmapFont>("Fonts/Pericles32"), 1);
				fontBatch3D.QueueText("Front", new Vector3(0.5f, 0f, 0f), 0.004f * new Vector3(-1f, 0f, 0f), 0.004f * new Vector3(0f, 0f, -1f), color2, TextAnchor.HorizontalCenter);
				fontBatch3D.QueueText("Side", new Vector3(1f, 0f, 0.5f), 0.004f * new Vector3(0f, 0f, -1f), 0.004f * new Vector3(1f, 0f, 0f), color2, TextAnchor.HorizontalCenter);
				if (DrawDebugFurniture)
				{
					DebugDraw();
				}
			}
			else
			{
				Vector3 position;
				Vector3 up;
				if (Mode == ViewMode.Side)
				{
					position = new Vector3(1f, 0f, 0f);
					up = new Vector3(0f, 1f, 0f);
				}
				else if (Mode == ViewMode.Top)
				{
					position = new Vector3(0f, 1f, 0f);
					up = new Vector3(0f, 0f, 1f);
				}
				else
				{
					position = new Vector3(0f, 0f, -1f);
					up = new Vector3(0f, 1f, 0f);
				}
				Viewport viewport2 = Display.Viewport;
				float num2 = MathUtils.Min(base.ActualSize.X, base.ActualSize.Y);
				Matrix matrix5 = Matrix.CreateLookAt(position, new Vector3(0f, 0f, 0f), up);
				Matrix matrix6 = Matrix.CreateOrthographic(2f, 2f, -10f, 10f);
				Matrix matrix7 = MatrixUtils.CreateScaleTranslation(num2, 0f - num2, base.ActualSize.X / 2f, base.ActualSize.Y / 2f) * base.GlobalTransform * MatrixUtils.CreateScaleTranslation(2f / (float)viewport2.Width, -2f / (float)viewport2.Height, -1f, 1f);
				matrix4 = Matrix.CreateTranslation(-0.5f, -0.5f, -0.5f) * matrix5 * matrix6 * matrix7;
				FlatBatch2D flatBatch2D = m_primitivesRenderer2d.FlatBatch();
				Matrix m = base.GlobalTransform;
				for (int j = 1; j < Design.Resolution; j++)
				{
					float num3 = (float)j / (float)Design.Resolution;
					Vector2 v = new Vector2(base.ActualSize.X * num3, 0f);
					Vector2 v2 = new Vector2(base.ActualSize.X * num3, base.ActualSize.Y);
					Vector2 v3 = new Vector2(0f, base.ActualSize.Y * num3);
					Vector2 v4 = new Vector2(base.ActualSize.X, base.ActualSize.Y * num3);
					Vector2.Transform(ref v, ref m, out v);
					Vector2.Transform(ref v2, ref m, out v2);
					Vector2.Transform(ref v3, ref m, out v3);
					Vector2.Transform(ref v4, ref m, out v4);
					Color color3 = ((j % 2 == 0) ? new Color(0, 0, 0, 56) : new Color(0, 0, 0, 28));
					Color color4 = ((j % 2 == 0) ? new Color(56, 56, 56, 56) : new Color(28, 28, 28, 28));
					color3 *= base.GlobalColorTransform;
					color4 *= base.GlobalColorTransform;
					flatBatch2D.QueueLine(v, v2, 0f, (j % 2 == 0) ? color3 : (color3 * 0.75f));
					flatBatch2D.QueueLine(v + new Vector2(1f, 0f), v2 + new Vector2(1f, 0f), 0f, color4);
					flatBatch2D.QueueLine(v3, v4, 0f, color3);
					flatBatch2D.QueueLine(v3 + new Vector2(0f, 1f), v4 + new Vector2(0f, 1f), 0f, color4);
				}
			}
			Matrix matrix8 = Matrix.Identity;
			FurnitureGeometry geometry = Design.Geometry;
			for (int k = 0; k < 6; k++)
			{
				Color globalColorTransform = base.GlobalColorTransform;
				if (Mode == ViewMode.Perspective)
				{
					float num4 = LightingManager.LightIntensityByLightValueAndFace[15 + 16 * CellFace.OppositeFace(k)];
					globalColorTransform *= new Color(num4, num4, num4);
				}
				if (geometry.SubsetOpaqueByFace[k] != null)
				{
					BlocksManager.DrawMeshBlock(m_primitivesRenderer3d, geometry.SubsetOpaqueByFace[k], globalColorTransform, 1f, ref matrix8, null);
				}
				if (geometry.SubsetAlphaTestByFace[k] != null)
				{
					BlocksManager.DrawMeshBlock(m_primitivesRenderer3d, geometry.SubsetAlphaTestByFace[k], globalColorTransform, 1f, ref matrix8, null);
				}
			}
			m_primitivesRenderer3d.Flush(matrix4);
			m_primitivesRenderer2d.Flush();
		}

		public override void Update()
		{
			if (Mode != ViewMode.Perspective)
			{
				return;
			}
			if (base.Input.Tap.HasValue && HitTestGlobal(base.Input.Tap.Value) == this)
			{
				m_dragStartPoint = base.Input.Tap;
			}
			if (base.Input.Press.HasValue)
			{
				if (m_dragStartPoint.HasValue)
				{
					Vector2 vector = ScreenToWidget(base.Input.Press.Value) - ScreenToWidget(m_dragStartPoint.Value);
					Vector2 vector2 = default(Vector2);
					vector2.Y = -0.01f * vector.X;
					vector2.X = 0.01f * vector.Y;
					if (Time.FrameDuration > 0f)
					{
						m_rotationSpeed = vector2 / Time.FrameDuration;
					}
					Rotate(vector2);
					m_dragStartPoint = base.Input.Press;
				}
			}
			else
			{
				m_dragStartPoint = null;
				Rotate(m_rotationSpeed * Time.FrameDuration);
				m_rotationSpeed *= MathUtils.Pow(0.1f, Time.FrameDuration);
			}
		}

		public override void MeasureOverride(Vector2 parentAvailableSize)
		{
			base.IsDrawRequired = Design != null;
			base.DesiredSize = Size;
		}

		private void Rotate(Vector2 angles)
		{
			float num = MathUtils.DegToRad(1f);
			Vector3 axis = Vector3.Normalize(Vector3.Cross(m_direction, Vector3.UnitY));
			m_direction = Vector3.TransformNormal(m_direction, Matrix.CreateRotationY(angles.Y));
			float num2 = MathUtils.Acos(Vector3.Dot(m_direction, Vector3.UnitY));
			float num3 = MathUtils.Acos(Vector3.Dot(m_direction, -Vector3.UnitY));
			angles.X = MathUtils.Min(angles.X, num2 - num);
			angles.X = MathUtils.Max(angles.X, 0f - (num3 - num));
			m_direction = Vector3.TransformNormal(m_direction, Matrix.CreateFromAxisAngle(axis, angles.X));
			m_direction = Vector3.Normalize(m_direction);
		}

		private void DebugDraw()
		{
		}
	}
}
