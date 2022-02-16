using System;
using Engine;
using Engine.Graphics;

namespace Game
{
	public class SeaUrchinBlock : BottomSuckerBlock
	{
		public new const int Index = 226;

		private BlockMesh[] m_blockMeshes = new BlockMesh[24];

		private BlockMesh m_standaloneBlockMesh;

		private BoundingBox[][] m_collisionBoxes = new BoundingBox[24][];

		private static Color[] m_colors = new Color[4]
		{
			new Color(20, 20, 20),
			new Color(50, 20, 20),
			new Color(80, 30, 30),
			new Color(20, 20, 40)
		};

		private static Vector2[] m_offsets = new Vector2[4]
		{
			0.15f * new Vector2(-0.8f, -1f),
			0.15f * new Vector2(1f, -0.75f),
			0.15f * new Vector2(-0.65f, 1f),
			0.15f * new Vector2(0.9f, 0.7f)
		};

		public override void Initialize()
		{
			Model model = ContentManager.Get<Model>("Models/SeaUrchin");
			Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Urchin").ParentBone);
			Matrix boneAbsoluteTransform2 = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Bottom").ParentBone);
			for (int i = 0; i < 6; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					Vector2 zero = Vector2.Zero;
					if (i < 4)
					{
						zero.Y = (float)i * (float)Math.PI / 2f;
					}
					else if (i == 4)
					{
						zero.X = -(float)Math.PI / 2f;
					}
					else
					{
						zero.X = (float)Math.PI / 2f;
					}
					Matrix matrix = Matrix.CreateRotationX((float)Math.PI / 2f) * Matrix.CreateRotationZ(0.3f + 2f * (float)j) * Matrix.CreateTranslation(m_offsets[j].X, m_offsets[j].Y, -0.49f) * Matrix.CreateRotationX(zero.X) * Matrix.CreateRotationY(zero.Y) * Matrix.CreateTranslation(0.5f, 0.5f, 0.5f);
					int num = 4 * i + j;
					m_blockMeshes[num] = new BlockMesh();
					m_blockMeshes[num].AppendModelMeshPart(model.FindMesh("Urchin").MeshParts[0], boneAbsoluteTransform * matrix, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
					m_collisionBoxes[num] = new BoundingBox[1] { m_blockMeshes[num].CalculateBoundingBox() };
				}
			}
			m_standaloneBlockMesh = new BlockMesh();
			m_standaloneBlockMesh.AppendModelMeshPart(model.FindMesh("Urchin").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0f, -0.1f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
			m_standaloneBlockMesh.AppendModelMeshPart(model.FindMesh("Bottom").MeshParts[0], boneAbsoluteTransform2 * Matrix.CreateTranslation(0f, -0.1f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
			base.Initialize();
		}

		public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value)
		{
			int data = Terrain.ExtractData(value);
			int face = BottomSuckerBlock.GetFace(data);
			int subvariant = BottomSuckerBlock.GetSubvariant(data);
			return m_collisionBoxes[4 * face + subvariant];
		}

		public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
		{
			int data = Terrain.ExtractData(value);
			int face = BottomSuckerBlock.GetFace(data);
			int subvariant = BottomSuckerBlock.GetSubvariant(data);
			Color color = m_colors[subvariant];
			generator.GenerateMeshVertices(this, x, y, z, m_blockMeshes[4 * face + subvariant], color, null, geometry.SubsetOpaque);
			base.GenerateTerrainVertices(generator, geometry, value, x, y, z);
		}

		public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
			BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, color * new Color(40, 40, 40), 3f * size, ref matrix, environmentData);
		}

		public override BlockDebrisParticleSystem CreateDebrisParticleSystem(SubsystemTerrain subsystemTerrain, Vector3 position, int value, float strength)
		{
			return new BlockDebrisParticleSystem(subsystemTerrain, position, 0.75f * strength, DestructionDebrisScale, new Color(64, 64, 64), DefaultTextureSlot);
		}
	}
}
