using System;
using Engine;
using Engine.Graphics;
using Engine.Media;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game
{
	public class ComponentScreenOverlays : Component, IDrawable, IUpdateable
	{
		private SubsystemTime m_subsystemTime;

		private SubsystemTerrain m_subsystemTerrain;

		private SubsystemSky m_subsystemSky;

		private ComponentGui m_componentGui;

		private ComponentPlayer m_componentPlayer;

		private PrimitivesRenderer2D m_primitivesRenderer2D = new PrimitivesRenderer2D();

		private PrimitivesRenderer3D m_primitivesRenderer3D = new PrimitivesRenderer3D();

		private Random m_random = new Random(0);

		private Vector2[] m_iceVertices;

		private Point2 m_cellsCount;

		private float? m_light;

		private double? m_waterSurfaceCrossTime;

		private bool m_isUnderWater;

		private static int[] m_drawOrders = new int[1] { 1101 };

		public float BlackoutFactor { get; set; }

		public float RedoutFactor { get; set; }

		public float GreenoutFactor { get; set; }

		public string FloatingMessage { get; set; }

		public float FloatingMessageFactor { get; set; }

		public string Message { get; set; }

		public float MessageFactor { get; set; }

		public float IceFactor { get; set; }

		public UpdateOrder UpdateOrder => UpdateOrder.Reset;

		public int[] DrawOrders => m_drawOrders;

		public void Update(float dt)
		{
			bool flag = m_subsystemSky.ViewUnderWaterDepth > 0f;
			if (flag != m_isUnderWater)
			{
				m_isUnderWater = flag;
				m_waterSurfaceCrossTime = m_subsystemTime.GameTime;
			}
			BlackoutFactor = 0f;
			RedoutFactor = 0f;
			GreenoutFactor = 0f;
			IceFactor = 0f;
			FloatingMessage = null;
			FloatingMessageFactor = 0f;
			Message = null;
			MessageFactor = 0f;
		}

		public void Draw(Camera camera, int drawOrder)
		{
			if (m_componentPlayer.GameWidget != camera.GameWidget)
			{
				return;
			}
			if (m_waterSurfaceCrossTime.HasValue)
			{
				float num = (float)(m_subsystemTime.GameTime - m_waterSurfaceCrossTime.Value);
				float num2 = 0.66f * MathUtils.Sqr(MathUtils.Saturate(1f - 0.75f * num));
				if (num2 > 0.01f)
				{
					Matrix matrix = default(Matrix);
					matrix.Translation = Vector3.Zero;
					matrix.Forward = camera.ViewDirection;
					matrix.Right = Vector3.Normalize(Vector3.Cross(camera.ViewUp, matrix.Forward));
					matrix.Up = Vector3.Normalize(Vector3.Cross(matrix.Right, matrix.Forward));
					Vector3 vector = matrix.ToYawPitchRoll();
					Vector2 zero = Vector2.Zero;
					zero.X -= 2f * vector.X / (float)Math.PI + 0.05f * MathUtils.Sin(5f * num);
					zero.Y += 2f * vector.Y / (float)Math.PI + (m_isUnderWater ? (0.75f * num) : (-0.75f * num));
					Texture2D texture = ContentManager.Get<Texture2D>("Textures/SplashOverlay");
					DrawTexturedOverlay(camera, texture, new Color(156, 206, 210), num2, num2, zero);
				}
			}
			if (IceFactor > 0f)
			{
				DrawIceOverlay(camera, IceFactor);
			}
			if (RedoutFactor > 0.01f)
			{
				DrawOverlay(camera, new Color(255, 64, 0), MathUtils.Saturate(2f * (RedoutFactor - 0.5f)), RedoutFactor);
			}
			if (BlackoutFactor > 0.01f)
			{
				DrawOverlay(camera, Color.Black, MathUtils.Saturate(2f * (BlackoutFactor - 0.5f)), BlackoutFactor);
			}
			if (GreenoutFactor > 0.01f)
			{
				DrawOverlay(camera, new Color(166, 175, 103), GreenoutFactor, MathUtils.Saturate(2f * GreenoutFactor));
			}
			if (!string.IsNullOrEmpty(FloatingMessage) && FloatingMessageFactor > 0.01f)
			{
				DrawFloatingMessage(camera, FloatingMessage, FloatingMessageFactor);
			}
			if (!string.IsNullOrEmpty(Message) && MessageFactor > 0.01f)
			{
				DrawMessage(camera, Message, MessageFactor);
			}
		}

		public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
		{
			m_subsystemTime = base.Project.FindSubsystem<SubsystemTime>(throwOnError: true);
			m_subsystemTerrain = base.Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
			m_subsystemSky = base.Project.FindSubsystem<SubsystemSky>(throwOnError: true);
			m_componentGui = base.Entity.FindComponent<ComponentGui>(throwOnError: true);
			m_componentPlayer = base.Entity.FindComponent<ComponentPlayer>(throwOnError: true);
		}

		private void DrawOverlay(Camera camera, Color color, float innerFactor, float outerFactor)
		{
			Vector2 viewportSize = camera.ViewportSize;
			Vector2 vector = new Vector2(0f, 0f);
			Vector2 vector2 = new Vector2(viewportSize.X, 0f);
			Vector2 vector3 = new Vector2(viewportSize.X, viewportSize.Y);
			Vector2 vector4 = new Vector2(0f, viewportSize.Y);
			Vector2 p = new Vector2(viewportSize.X / 2f, viewportSize.Y / 2f);
			Color color2 = color * outerFactor;
			Color color3 = color * innerFactor;
			FlatBatch2D flatBatch2D = m_primitivesRenderer2D.FlatBatch(0, DepthStencilState.None, null, BlendState.AlphaBlend);
			int count = flatBatch2D.TriangleVertices.Count;
			flatBatch2D.QueueTriangle(vector, vector2, p, 0f, color2, color2, color3);
			flatBatch2D.QueueTriangle(vector2, vector3, p, 0f, color2, color2, color3);
			flatBatch2D.QueueTriangle(vector3, vector4, p, 0f, color2, color2, color3);
			flatBatch2D.QueueTriangle(vector4, vector, p, 0f, color2, color2, color3);
			flatBatch2D.TransformTriangles(camera.ViewportMatrix, count);
			flatBatch2D.Flush();
		}

		private void DrawTexturedOverlay(Camera camera, Texture2D texture, Color color, float innerFactor, float outerFactor, Vector2 offset)
		{
			Vector2 viewportSize = camera.ViewportSize;
			float num = viewportSize.X / viewportSize.Y;
			Vector2 vector = new Vector2(0f, 0f);
			Vector2 vector2 = new Vector2(viewportSize.X, 0f);
			Vector2 vector3 = new Vector2(viewportSize.X, viewportSize.Y);
			Vector2 vector4 = new Vector2(0f, viewportSize.Y);
			Vector2 p = new Vector2(viewportSize.X / 2f, viewportSize.Y / 2f);
			offset.X = MathUtils.Remainder(offset.X, 1f);
			offset.Y = MathUtils.Remainder(offset.Y, 1f);
			Vector2 vector5 = new Vector2(0f, 0f) + offset;
			Vector2 vector6 = new Vector2(num, 0f) + offset;
			Vector2 vector7 = new Vector2(num, 1f) + offset;
			Vector2 vector8 = new Vector2(0f, 1f) + offset;
			Vector2 texCoord = new Vector2(num / 2f, 0.5f) + offset;
			Color color2 = color * outerFactor;
			Color color3 = color * innerFactor;
			TexturedBatch2D texturedBatch2D = m_primitivesRenderer2D.TexturedBatch(texture, useAlphaTest: false, 0, DepthStencilState.None, null, BlendState.Additive, SamplerState.PointWrap);
			int count = texturedBatch2D.TriangleVertices.Count;
			texturedBatch2D.QueueTriangle(vector, vector2, p, 0f, vector5, vector6, texCoord, color2, color2, color3);
			texturedBatch2D.QueueTriangle(vector2, vector3, p, 0f, vector6, vector7, texCoord, color2, color2, color3);
			texturedBatch2D.QueueTriangle(vector3, vector4, p, 0f, vector7, vector8, texCoord, color2, color2, color3);
			texturedBatch2D.QueueTriangle(vector4, vector, p, 0f, vector8, vector5, texCoord, color2, color2, color3);
			texturedBatch2D.TransformTriangles(camera.ViewportMatrix, count);
			texturedBatch2D.Flush();
		}

		private void DrawIceOverlay(Camera camera, float factor)
		{
			Vector2 viewportSize = camera.ViewportSize;
			float num = (camera.Eye.HasValue ? 1.3f : 1f);
			float num2 = (camera.Eye.HasValue ? MathUtils.Pow(factor, 0.4f) : factor);
			Vector2 vector = (camera.Eye.HasValue ? viewportSize : new Vector2(1f));
			float num3 = vector.Length();
			Point2 point = new Point2((int)MathUtils.Round(12f * viewportSize.X / viewportSize.Y), (int)MathUtils.Round(12f));
			if (m_iceVertices == null || m_cellsCount != point)
			{
				m_cellsCount = point;
				m_random.Seed(0);
				m_iceVertices = new Vector2[(point.X + 1) * (point.Y + 1)];
				for (int i = 0; i <= point.X; i++)
				{
					for (int j = 0; j <= point.Y; j++)
					{
						float num4 = i;
						float num5 = j;
						if (i != 0 && i != point.X)
						{
							num4 += m_random.Float(-0.4f, 0.4f);
						}
						if (j != 0 && j != point.Y)
						{
							num5 += m_random.Float(-0.4f, 0.4f);
						}
						float x = num4 / (float)point.X;
						float y = num5 / (float)point.Y;
						m_iceVertices[i + j * (point.X + 1)] = new Vector2(x, y);
					}
				}
			}
			Vector3 vector2 = Vector3.UnitX / camera.ProjectionMatrix.M11 * 2f * 0.2f * num;
			Vector3 vector3 = Vector3.UnitY / camera.ProjectionMatrix.M22 * 2f * 0.2f * num;
			Vector3 vector4 = -0.2f * Vector3.UnitZ - 0.5f * (vector2 + vector3);
			if (!m_light.HasValue || Time.PeriodicEvent(0.05000000074505806, 0.0))
			{
				m_light = LightingManager.CalculateSmoothLight(m_subsystemTerrain, camera.ViewPosition) ?? m_light ?? 1f;
			}
			Color color = Color.MultiplyColorOnly(Color.White, m_light.Value);
			m_random.Seed(0);
			Texture2D texture = ContentManager.Get<Texture2D>("Textures/IceOverlay");
			TexturedBatch3D texturedBatch3D = m_primitivesRenderer3D.TexturedBatch(texture, useAlphaTest: false, 0, DepthStencilState.None, RasterizerState.CullNoneScissor, BlendState.AlphaBlend, SamplerState.PointWrap);
			Vector2 vector5 = new Vector2(viewportSize.X / viewportSize.Y, 1f);
			Vector2 vector6 = new Vector2(point.X - 1, point.Y - 1);
			for (int k = 0; k < point.X; k++)
			{
				for (int l = 0; l < point.Y; l++)
				{
					float num6 = (new Vector2((float)(2 * k) / vector6.X - 1f, (float)(2 * l) / vector6.Y - 1f) * vector).Length() / num3;
					if (1f - num6 + m_random.Float(0f, 0.05f) < num2)
					{
						Vector2 vector7 = m_iceVertices[k + l * (point.X + 1)];
						Vector2 vector8 = m_iceVertices[k + 1 + l * (point.X + 1)];
						Vector2 vector9 = m_iceVertices[k + 1 + (l + 1) * (point.X + 1)];
						Vector2 vector10 = m_iceVertices[k + (l + 1) * (point.X + 1)];
						Vector3 vector11 = vector4 + vector7.X * vector2 + vector7.Y * vector3;
						Vector3 p = vector4 + vector8.X * vector2 + vector8.Y * vector3;
						Vector3 vector12 = vector4 + vector9.X * vector2 + vector9.Y * vector3;
						Vector3 p2 = vector4 + vector10.X * vector2 + vector10.Y * vector3;
						Vector2 vector13 = vector7 * vector5;
						Vector2 texCoord = vector8 * vector5;
						Vector2 vector14 = vector9 * vector5;
						Vector2 texCoord2 = vector10 * vector5;
						texturedBatch3D.QueueTriangle(vector11, p, vector12, vector13, texCoord, vector14, color);
						texturedBatch3D.QueueTriangle(vector12, p2, vector11, vector14, texCoord2, vector13, color);
					}
				}
			}
			texturedBatch3D.Flush(camera.ProjectionMatrix);
		}

		private void DrawFloatingMessage(Camera camera, string message, float factor)
		{
			BitmapFont font = ContentManager.Get<BitmapFont>("Fonts/Pericles32");
			if (!camera.Eye.HasValue)
			{
				Vector2 position = camera.ViewportSize / 2f;
				position.X += 0.07f * camera.ViewportSize.X * (float)MathUtils.Sin(1.7300000190734863 * Time.FrameStartTime);
				position.Y += 0.07f * camera.ViewportSize.Y * (float)MathUtils.Cos(1.1200000047683716 * Time.FrameStartTime);
				FontBatch2D fontBatch2D = m_primitivesRenderer2D.FontBatch(font, 1, DepthStencilState.None, null, BlendState.AlphaBlend);
				int count = fontBatch2D.TriangleVertices.Count;
				fontBatch2D.QueueText(message, position, 0f, Color.White * factor, TextAnchor.Center, Vector2.One * camera.GameWidget.GlobalScale, Vector2.Zero);
				fontBatch2D.TransformTriangles(camera.ViewportMatrix, count);
				fontBatch2D.Flush();
			}
			else
			{
				Vector3 position2 = -4f * Vector3.UnitZ;
				position2.X += 0.28f * (float)MathUtils.Sin(1.7300000190734863 * Time.FrameStartTime);
				position2.Y += 0.28f * (float)MathUtils.Cos(1.1200000047683716 * Time.FrameStartTime);
				FontBatch3D fontBatch3D = m_primitivesRenderer3D.FontBatch(font, 1, DepthStencilState.None, RasterizerState.CullNoneScissor, BlendState.AlphaBlend);
				fontBatch3D.QueueText(message, position2, 0.008f * Vector3.UnitX, -0.008f * Vector3.UnitY, Color.White * factor, TextAnchor.Center, Vector2.Zero);
				fontBatch3D.Flush(camera.ProjectionMatrix);
			}
		}

		private void DrawMessage(Camera camera, string message, float factor)
		{
			BitmapFont font = ContentManager.Get<BitmapFont>("Fonts/Pericles18");
			if (!camera.Eye.HasValue)
			{
				Vector2 position = new Vector2(camera.ViewportSize.X / 2f, camera.ViewportSize.Y - 25f);
				FontBatch2D fontBatch2D = m_primitivesRenderer2D.FontBatch(font, 0, DepthStencilState.None, null, BlendState.AlphaBlend);
				int count = fontBatch2D.TriangleVertices.Count;
				fontBatch2D.QueueText(message, position, 0f, Color.Gray * factor, TextAnchor.HorizontalCenter | TextAnchor.Bottom, Vector2.One * camera.GameWidget.GlobalScale, Vector2.Zero);
				fontBatch2D.TransformTriangles(camera.ViewportMatrix, count);
				fontBatch2D.Flush();
			}
			else
			{
				Vector3 position2 = -4f * Vector3.UnitZ + -0.24f / camera.ProjectionMatrix.M22 * 2f * Vector3.UnitY;
				FontBatch3D fontBatch3D = m_primitivesRenderer3D.FontBatch(font, 0, DepthStencilState.None, RasterizerState.CullNoneScissor, BlendState.AlphaBlend);
				fontBatch3D.QueueText(message, position2, 0.0104f * Vector3.UnitX, -0.0104f * Vector3.UnitY, Color.White * factor, TextAnchor.Center, Vector2.Zero);
				fontBatch3D.Flush(camera.ProjectionMatrix);
			}
		}
	}
}
