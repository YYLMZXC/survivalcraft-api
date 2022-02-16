using System.Collections.Generic;
using Engine;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemPistonBlockBehavior : SubsystemBlockBehavior, IUpdateable
	{
		private class QueuedAction
		{
			public int StoppedFrame;

			public bool Stop;

			public int? Move;
		}

		private SubsystemTime m_subsystemTime;

		private SubsystemTerrain m_subsystemTerrain;

		private SubsystemAudio m_subsystemAudio;

		private SubsystemMovingBlocks m_subsystemMovingBlocks;

		private bool m_allowPistonHeadRemove;

		private Dictionary<Point3, QueuedAction> m_actions = new Dictionary<Point3, QueuedAction>();

		private List<KeyValuePair<Point3, QueuedAction>> m_tmpActions = new List<KeyValuePair<Point3, QueuedAction>>();

		private DynamicArray<MovingBlock> m_movingBlocks = new DynamicArray<MovingBlock>();

		private const string IdString = "Piston";

		public const int PistonMaxMovedBlocks = 8;

		public const int PistonMaxExtension = 8;

		public const int PistonMaxSpeedSetting = 3;

		public UpdateOrder UpdateOrder => m_subsystemMovingBlocks.UpdateOrder + 1;

		public override int[] HandledBlocks => new int[0];

		public void AdjustPiston(Point3 position, int length)
		{
			if (!m_actions.TryGetValue(position, out var value))
			{
				value = new QueuedAction();
				m_actions[position] = value;
			}
			value.Move = length;
		}

		public void Update(float dt)
		{
			if (m_subsystemTime.PeriodicGameTimeEvent(0.125, 0.0))
			{
				ProcessQueuedActions();
			}
			UpdateMovableBlocks();
		}

		public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer)
		{
			int value = inventory.GetSlotValue(slotIndex);
			int count = inventory.GetSlotCount(slotIndex);
			int data = Terrain.ExtractData(value);
			DialogsManager.ShowDialog(componentPlayer.GuiWidget, new EditPistonDialog(data, delegate(int newData)
			{
				int num = Terrain.ReplaceData(value, newData);
				if (num != value)
				{
					inventory.RemoveSlotItems(slotIndex, count);
					inventory.AddSlotItems(slotIndex, num, 1);
				}
			}));
			return true;
		}

		public override bool OnEditBlock(int x, int y, int z, int value, ComponentPlayer componentPlayer)
		{
			int contents = Terrain.ExtractContents(value);
			int data = Terrain.ExtractData(value);
			DialogsManager.ShowDialog(componentPlayer.GuiWidget, new EditPistonDialog(data, delegate(int newData)
			{
				if (newData != data && base.SubsystemTerrain.Terrain.GetCellContents(x, y, z) == contents)
				{
					int value2 = Terrain.ReplaceData(value, newData);
					base.SubsystemTerrain.ChangeCell(x, y, z, value2);
					SubsystemElectricity subsystemElectricity = base.Project.FindSubsystem<SubsystemElectricity>(throwOnError: true);
					ElectricElement electricElement = subsystemElectricity.GetElectricElement(x, y, z, 0);
					if (electricElement != null)
					{
						subsystemElectricity.QueueElectricElementForSimulation(electricElement, subsystemElectricity.CircuitStep + 1);
					}
				}
			}));
			return true;
		}

		public override void OnBlockRemoved(int value, int newValue, int x, int y, int z)
		{
			int num = Terrain.ExtractContents(value);
			int data = Terrain.ExtractData(value);
			switch (num)
			{
			case 237:
			{
				StopPiston(new Point3(x, y, z));
				int face2 = PistonBlock.GetFace(data);
				Point3 point2 = CellFace.FaceToPoint3(face2);
				int cellValue3 = m_subsystemTerrain.Terrain.GetCellValue(x + point2.X, y + point2.Y, z + point2.Z);
				int num4 = Terrain.ExtractContents(cellValue3);
				int data4 = Terrain.ExtractData(cellValue3);
				if (num4 == 238 && PistonHeadBlock.GetFace(data4) == face2)
				{
					m_subsystemTerrain.DestroyCell(0, x + point2.X, y + point2.Y, z + point2.Z, 0, noDrop: false, noParticleSystem: false);
				}
				break;
			}
			case 238:
				if (!m_allowPistonHeadRemove)
				{
					int face = PistonHeadBlock.GetFace(data);
					Point3 point = CellFace.FaceToPoint3(face);
					int cellValue = m_subsystemTerrain.Terrain.GetCellValue(x + point.X, y + point.Y, z + point.Z);
					int cellValue2 = m_subsystemTerrain.Terrain.GetCellValue(x - point.X, y - point.Y, z - point.Z);
					int num2 = Terrain.ExtractContents(cellValue);
					int num3 = Terrain.ExtractContents(cellValue2);
					int data2 = Terrain.ExtractData(cellValue);
					int data3 = Terrain.ExtractData(cellValue2);
					if (num2 == 238 && PistonHeadBlock.GetFace(data2) == face)
					{
						m_subsystemTerrain.DestroyCell(0, x + point.X, y + point.Y, z + point.Z, 0, noDrop: false, noParticleSystem: false);
					}
					if (num3 == 237 && PistonBlock.GetFace(data3) == face)
					{
						m_subsystemTerrain.DestroyCell(0, x - point.X, y - point.Y, z - point.Z, 0, noDrop: false, noParticleSystem: false);
					}
					else if (num3 == 238 && PistonHeadBlock.GetFace(data3) == face)
					{
						m_subsystemTerrain.DestroyCell(0, x - point.X, y - point.Y, z - point.Z, 0, noDrop: false, noParticleSystem: false);
					}
				}
				break;
			}
		}

		public override void OnChunkDiscarding(TerrainChunk chunk)
		{
			BoundingBox boundingBox = new BoundingBox(chunk.BoundingBox.Min - new Vector3(16f), chunk.BoundingBox.Max + new Vector3(16f));
			DynamicArray<IMovingBlockSet> dynamicArray = new DynamicArray<IMovingBlockSet>();
			m_subsystemMovingBlocks.FindMovingBlocks(boundingBox, extendToFillCells: false, dynamicArray);
			foreach (IMovingBlockSet item in dynamicArray)
			{
				if (item.Id == "Piston")
				{
					StopPiston((Point3)item.Tag);
				}
			}
		}

		public override void Load(ValuesDictionary valuesDictionary)
		{
			base.Load(valuesDictionary);
			m_subsystemTime = base.Project.FindSubsystem<SubsystemTime>(throwOnError: true);
			m_subsystemTerrain = base.Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
			m_subsystemAudio = base.Project.FindSubsystem<SubsystemAudio>(throwOnError: true);
			m_subsystemMovingBlocks = base.Project.FindSubsystem<SubsystemMovingBlocks>(throwOnError: true);
			m_subsystemMovingBlocks.Stopped += MovingBlocksStopped;
			m_subsystemMovingBlocks.CollidedWithTerrain += MovingBlocksCollidedWithTerrain;
		}

		private void ProcessQueuedActions()
		{
			m_tmpActions.Clear();
			m_tmpActions.AddRange(m_actions);
			foreach (KeyValuePair<Point3, QueuedAction> tmpAction in m_tmpActions)
			{
				Point3 key = tmpAction.Key;
				QueuedAction value = tmpAction.Value;
				if (Terrain.ExtractContents(m_subsystemTerrain.Terrain.GetCellValue(key.X, key.Y, key.Z)) != 237)
				{
					StopPiston(key);
					value.Move = null;
					value.Stop = false;
				}
				else if (value.Stop)
				{
					StopPiston(key);
					value.Stop = false;
					value.StoppedFrame = Time.FrameIndex;
				}
			}
			foreach (KeyValuePair<Point3, QueuedAction> tmpAction2 in m_tmpActions)
			{
				Point3 key2 = tmpAction2.Key;
				QueuedAction value2 = tmpAction2.Value;
				if (!value2.Move.HasValue || value2.Stop || Time.FrameIndex == value2.StoppedFrame || m_subsystemMovingBlocks.FindMovingBlocks("Piston", key2) != null)
				{
					continue;
				}
				bool flag = true;
				for (int i = -1; i <= 1; i++)
				{
					for (int j = -1; j <= 1; j++)
					{
						TerrainChunk chunkAtCell = m_subsystemTerrain.Terrain.GetChunkAtCell(key2.X + i * 16, key2.Z + j * 16);
						if (chunkAtCell == null || chunkAtCell.State <= TerrainChunkState.InvalidContents4)
						{
							flag = false;
						}
					}
				}
				if (flag && MovePiston(key2, value2.Move.Value))
				{
					value2.Move = null;
				}
			}
			foreach (KeyValuePair<Point3, QueuedAction> tmpAction3 in m_tmpActions)
			{
				Point3 key3 = tmpAction3.Key;
				QueuedAction value3 = tmpAction3.Value;
				if (!value3.Move.HasValue && !value3.Stop)
				{
					m_actions.Remove(key3);
				}
			}
		}

		private void UpdateMovableBlocks()
		{
			foreach (IMovingBlockSet movingBlockSet in m_subsystemMovingBlocks.MovingBlockSets)
			{
				if (!(movingBlockSet.Id == "Piston"))
				{
					continue;
				}
				Point3 point = (Point3)movingBlockSet.Tag;
				int cellValue = m_subsystemTerrain.Terrain.GetCellValue(point.X, point.Y, point.Z);
				if (Terrain.ExtractContents(cellValue) != 237)
				{
					continue;
				}
				int data = Terrain.ExtractData(cellValue);
				PistonMode mode = PistonBlock.GetMode(data);
				int face = PistonBlock.GetFace(data);
				Point3 point2 = CellFace.FaceToPoint3(face);
				int num = int.MaxValue;
				foreach (MovingBlock block in movingBlockSet.Blocks)
				{
					num = MathUtils.Min(num, block.Offset.X * point2.X + block.Offset.Y * point2.Y + block.Offset.Z * point2.Z);
				}
				float num2 = movingBlockSet.Position.X * (float)point2.X + movingBlockSet.Position.Y * (float)point2.Y + movingBlockSet.Position.Z * (float)point2.Z;
				float num3 = point.X * point2.X + point.Y * point2.Y + point.Z * point2.Z;
				if (num2 > num3)
				{
					if ((float)num + num2 - num3 > 1f)
					{
						movingBlockSet.SetBlock(point2 * (num - 1), Terrain.MakeBlockValue(238, 0, PistonHeadBlock.SetFace(PistonHeadBlock.SetIsShaft(PistonHeadBlock.SetMode(0, mode), isShaft: true), face)));
					}
				}
				else if (num2 < num3 && (float)num + num2 - num3 <= 0f)
				{
					movingBlockSet.SetBlock(point2 * num, 0);
				}
			}
		}

		private static void GetSpeedAndSmoothness(int pistonSpeed, out float speed, out Vector2 smoothness)
		{
			switch (pistonSpeed)
			{
			default:
				speed = 5f;
				smoothness = new Vector2(0f, 0.5f);
				break;
			case 1:
				speed = 4.5f;
				smoothness = new Vector2(0.6f, 0.6f);
				break;
			case 2:
				speed = 4f;
				smoothness = new Vector2(0.9f, 0.9f);
				break;
			case 3:
				speed = 3.5f;
				smoothness = new Vector2(1.2f, 1.2f);
				break;
			}
		}

		private bool MovePiston(Point3 position, int length)
		{
			Terrain terrain = m_subsystemTerrain.Terrain;
			int data = Terrain.ExtractData(terrain.GetCellValue(position.X, position.Y, position.Z));
			int face = PistonBlock.GetFace(data);
			PistonMode mode = PistonBlock.GetMode(data);
			int maxExtension = PistonBlock.GetMaxExtension(data);
			int pullCount = PistonBlock.GetPullCount(data);
			int speed = PistonBlock.GetSpeed(data);
			Point3 point = CellFace.FaceToPoint3(face);
			length = MathUtils.Clamp(length, 0, maxExtension + 1);
			int num = 0;
			m_movingBlocks.Clear();
			Point3 offset = point;
			MovingBlock item;
			while (m_movingBlocks.Count < 8)
			{
				int cellValue = terrain.GetCellValue(position.X + offset.X, position.Y + offset.Y, position.Z + offset.Z);
				int num2 = Terrain.ExtractContents(cellValue);
				int face2 = PistonHeadBlock.GetFace(Terrain.ExtractData(cellValue));
				if (num2 != 238 || face2 != face)
				{
					break;
				}
				DynamicArray<MovingBlock> movingBlocks = m_movingBlocks;
				item = new MovingBlock
				{
					Offset = offset,
					Value = cellValue
				};
				movingBlocks.Add(item);
				offset += point;
				num++;
			}
			if (length > num)
			{
				DynamicArray<MovingBlock> movingBlocks2 = m_movingBlocks;
				item = new MovingBlock
				{
					Offset = Point3.Zero,
					Value = Terrain.MakeBlockValue(238, 0, PistonHeadBlock.SetFace(PistonHeadBlock.SetMode(PistonHeadBlock.SetIsShaft(0, num > 0), mode), face))
				};
				movingBlocks2.Add(item);
				int num3 = 0;
				while (num3 < 8)
				{
					int cellValue2 = terrain.GetCellValue(position.X + offset.X, position.Y + offset.Y, position.Z + offset.Z);
					if (!IsBlockMovable(cellValue2, face, position.Y + offset.Y, out var isEnd))
					{
						break;
					}
					DynamicArray<MovingBlock> movingBlocks3 = m_movingBlocks;
					item = new MovingBlock
					{
						Offset = offset,
						Value = cellValue2
					};
					movingBlocks3.Add(item);
					num3++;
					offset += point;
					if (isEnd)
					{
						break;
					}
				}
				if (!IsBlockBlocking(terrain.GetCellValue(position.X + offset.X, position.Y + offset.Y, position.Z + offset.Z)))
				{
					GetSpeedAndSmoothness(speed, out var speed2, out var smoothness);
					Point3 p = position + (length - num) * point;
					if (m_subsystemMovingBlocks.AddMovingBlockSet(new Vector3(position) + 0.01f * new Vector3(point), new Vector3(p), speed2, 0f, 0f, smoothness, m_movingBlocks, "Piston", position, testCollision: true) != null)
					{
						m_allowPistonHeadRemove = true;
						try
						{
							foreach (MovingBlock movingBlock in m_movingBlocks)
							{
								if (movingBlock.Offset != Point3.Zero)
								{
									m_subsystemTerrain.ChangeCell(position.X + movingBlock.Offset.X, position.Y + movingBlock.Offset.Y, position.Z + movingBlock.Offset.Z, 0);
								}
							}
						}
						finally
						{
							m_allowPistonHeadRemove = false;
						}
						m_subsystemTerrain.ChangeCell(position.X, position.Y, position.Z, Terrain.MakeBlockValue(237, 0, PistonBlock.SetIsExtended(data, isExtended: true)));
						m_subsystemAudio.PlaySound("Audio/Piston", 1f, 0f, new Vector3(position), 2f, autoDelay: true);
					}
				}
				return false;
			}
			if (length < num)
			{
				if (mode != 0)
				{
					int num4 = 0;
					for (int i = 0; i < pullCount + 1; i++)
					{
						int cellValue3 = terrain.GetCellValue(position.X + offset.X, position.Y + offset.Y, position.Z + offset.Z);
						if (!IsBlockMovable(cellValue3, face, position.Y + offset.Y, out var isEnd2))
						{
							break;
						}
						DynamicArray<MovingBlock> movingBlocks4 = m_movingBlocks;
						item = new MovingBlock
						{
							Offset = offset,
							Value = cellValue3
						};
						movingBlocks4.Add(item);
						offset += point;
						num4++;
						if (isEnd2)
						{
							break;
						}
					}
					if (mode == PistonMode.StrictPulling && num4 < pullCount + 1)
					{
						return false;
					}
				}
				GetSpeedAndSmoothness(speed, out var speed3, out var smoothness2);
				float num5 = ((length == 0) ? 0.01f : 0f);
				Vector3 targetPosition = new Vector3(position) + (length - num) * new Vector3(point) + num5 * new Vector3(point);
				if (m_subsystemMovingBlocks.AddMovingBlockSet(new Vector3(position), targetPosition, speed3, 0f, 0f, smoothness2, m_movingBlocks, "Piston", position, testCollision: true) != null)
				{
					m_allowPistonHeadRemove = true;
					try
					{
						foreach (MovingBlock movingBlock2 in m_movingBlocks)
						{
							m_subsystemTerrain.ChangeCell(position.X + movingBlock2.Offset.X, position.Y + movingBlock2.Offset.Y, position.Z + movingBlock2.Offset.Z, 0);
						}
					}
					finally
					{
						m_allowPistonHeadRemove = false;
					}
					m_subsystemAudio.PlaySound("Audio/Piston", 1f, 0f, new Vector3(position), 2f, autoDelay: true);
				}
				return false;
			}
			return true;
		}

		private void StopPiston(Point3 position)
		{
			IMovingBlockSet movingBlockSet = m_subsystemMovingBlocks.FindMovingBlocks("Piston", position);
			if (movingBlockSet == null)
			{
				return;
			}
			int cellValue = m_subsystemTerrain.Terrain.GetCellValue(position.X, position.Y, position.Z);
			int num = Terrain.ExtractContents(cellValue);
			int data = Terrain.ExtractData(cellValue);
			bool flag = num == 237;
			bool isExtended = false;
			m_subsystemMovingBlocks.RemoveMovingBlockSet(movingBlockSet);
			foreach (MovingBlock block in movingBlockSet.Blocks)
			{
				int x = Terrain.ToCell(MathUtils.Round(movingBlockSet.Position.X)) + block.Offset.X;
				int y = Terrain.ToCell(MathUtils.Round(movingBlockSet.Position.Y)) + block.Offset.Y;
				int z = Terrain.ToCell(MathUtils.Round(movingBlockSet.Position.Z)) + block.Offset.Z;
				if (new Point3(x, y, z) == position)
				{
					continue;
				}
				int num2 = Terrain.ExtractContents(block.Value);
				if (flag || num2 != 238)
				{
					m_subsystemTerrain.DestroyCell(0, x, y, z, block.Value, noDrop: false, noParticleSystem: false);
					if (num2 == 238)
					{
						isExtended = true;
					}
				}
			}
			if (flag)
			{
				m_subsystemTerrain.ChangeCell(position.X, position.Y, position.Z, Terrain.MakeBlockValue(237, 0, PistonBlock.SetIsExtended(data, isExtended)));
			}
		}

		private void MovingBlocksCollidedWithTerrain(IMovingBlockSet movingBlockSet, Point3 p)
		{
			if (!(movingBlockSet.Id == "Piston"))
			{
				return;
			}
			Point3 point = (Point3)movingBlockSet.Tag;
			int cellValue = m_subsystemTerrain.Terrain.GetCellValue(point.X, point.Y, point.Z);
			if (Terrain.ExtractContents(cellValue) != 237)
			{
				return;
			}
			Point3 point2 = CellFace.FaceToPoint3(PistonBlock.GetFace(Terrain.ExtractData(cellValue)));
			int num = p.X * point2.X + p.Y * point2.Y + p.Z * point2.Z;
			int num2 = point.X * point2.X + point.Y * point2.Y + point.Z * point2.Z;
			if (num > num2)
			{
				if (IsBlockBlocking(base.SubsystemTerrain.Terrain.GetCellValue(p.X, p.Y, p.Z)))
				{
					movingBlockSet.Stop();
				}
				else
				{
					base.SubsystemTerrain.DestroyCell(0, p.X, p.Y, p.Z, 0, noDrop: false, noParticleSystem: false);
				}
			}
		}

		private void MovingBlocksStopped(IMovingBlockSet movingBlockSet)
		{
			if (!(movingBlockSet.Id == "Piston") || !(movingBlockSet.Tag is Point3))
			{
				return;
			}
			Point3 key = (Point3)movingBlockSet.Tag;
			if (Terrain.ExtractContents(m_subsystemTerrain.Terrain.GetCellValue(key.X, key.Y, key.Z)) == 237)
			{
				if (!m_actions.TryGetValue(key, out var value))
				{
					value = new QueuedAction();
					m_actions.Add(key, value);
				}
				value.Stop = true;
			}
		}

		private static bool IsBlockMovable(int value, int pistonFace, int y, out bool isEnd)
		{
			isEnd = false;
			int num = Terrain.ExtractContents(value);
			int data = Terrain.ExtractData(value);
			switch (num)
			{
			case 27:
			case 45:
			case 64:
			case 65:
			case 216:
				return false;
			case 227:
				return true;
			case 237:
				return !PistonBlock.GetIsExtended(data);
			case 238:
				return false;
			case 131:
			case 132:
			case 244:
				return false;
			case 127:
				return false;
			case 126:
				return false;
			case 1:
				return y > 1;
			default:
			{
				Block block = BlocksManager.Blocks[num];
				if (block is BottomSuckerBlock)
				{
					return false;
				}
				if (block is MountedElectricElementBlock)
				{
					isEnd = true;
					return ((MountedElectricElementBlock)block).GetFace(value) == pistonFace;
				}
				if (block is DoorBlock || block is TrapdoorBlock)
				{
					return false;
				}
				if (block is LadderBlock)
				{
					isEnd = true;
					return pistonFace == LadderBlock.GetFace(data);
				}
				if (block is AttachedSignBlock)
				{
					isEnd = true;
					return pistonFace == AttachedSignBlock.GetFace(data);
				}
				if (block.IsNonDuplicable)
				{
					return false;
				}
				if (block.IsCollidable)
				{
					return true;
				}
				return false;
			}
			}
		}

		private static bool IsBlockBlocking(int value)
		{
			int num = Terrain.ExtractContents(value);
			return BlocksManager.Blocks[num].IsCollidable;
		}
	}
}
