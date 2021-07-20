using Engine;
using GameEntitySystem;
using System;
using System.Globalization;
using TemplatesDatabase;

namespace Game
{
    public class ComponentMiner : Component, IUpdateable
    {
        public SubsystemTerrain m_subsystemTerrain;

        public SubsystemBodies m_subsystemBodies;

        public SubsystemMovingBlocks m_subsystemMovingBlocks;

        public SubsystemGameInfo m_subsystemGameInfo;

        public SubsystemTime m_subsystemTime;

        public SubsystemAudio m_subsystemAudio;

        public SubsystemSoundMaterials m_subsystemSoundMaterials;

        public SubsystemBlockBehaviors m_subsystemBlockBehaviors;

        public Random m_random = new Random();

        public static Random s_random = new Random();

        public double m_digStartTime;

        public float m_digProgress;

        public double m_lastHitTime;

        public static string fName = "ComponentMiner";

        public int m_lastDigFrameIndex;

        public float m_lastPokingPhase;

        public ComponentCreature ComponentCreature
        {
            get;
            set;
        }

        public ComponentPlayer ComponentPlayer
        {
            get;
            set;
        }

        public IInventory Inventory
        {
            get;
            set;
        }

        public int ActiveBlockValue
        {
            get
            {
                if (Inventory == null)
                {
                    return 0;
                }
                return Inventory.GetSlotValue(Inventory.ActiveSlotIndex);
            }
        }

        public float AttackPower
        {
            get;
            set;
        }

        public float PokingPhase
        {
            get;
            set;
        }

        public CellFace? DigCellFace
        {
            get;
            set;
        }

        public float DigTime
        {
            get
            {
                if (!DigCellFace.HasValue)
                {
                    return 0f;
                }
                return (float)(m_subsystemTime.GameTime - m_digStartTime);
            }
        }

        public float DigProgress
        {
            get
            {
                if (!DigCellFace.HasValue)
                {
                    return 0f;
                }
                return m_digProgress;
            }
        }

        public UpdateOrder UpdateOrder => UpdateOrder.Default;

        public void Poke(bool forceRestart)
        {
            PokingPhase = forceRestart ? 0.0001f : MathUtils.Max(0.0001f, PokingPhase);
        }

        public bool Dig(TerrainRaycastResult raycastResult)
        {
            bool flag=false;
            ModsManager.HookAction("ComponentMinerDig",list=> {
                foreach (ModLoader modEntity in list)
                {
                    flag |= modEntity.ComponentMinerDig(this, raycastResult);
                }
            });
            return flag;
        }

        public bool Place(TerrainRaycastResult raycastResult)
        {
            if (Place(raycastResult, ActiveBlockValue))
            {
                if (Inventory != null)
                {
                    Inventory.RemoveSlotItems(Inventory.ActiveSlotIndex, 1);
                }
                return true;
            }
            return false;
        }

        public bool Place(TerrainRaycastResult raycastResult, int value)
        {
            int num = Terrain.ExtractContents(value);
            if (BlocksManager.Blocks[num].IsPlaceable)
            {
                Block block = BlocksManager.Blocks[num];
                BlockPlacementData placementData = block.GetPlacementValue(m_subsystemTerrain, this, value, raycastResult);
                if (placementData.Value != 0)
                {
                    Point3 point = CellFace.FaceToPoint3(placementData.CellFace.Face);
                    int num2 = placementData.CellFace.X + point.X;
                    int num3 = placementData.CellFace.Y + point.Y;
                    int num4 = placementData.CellFace.Z + point.Z;
                    if (num3 > 0 && num3 < 255 && (IsBlockPlacingAllowed(ComponentCreature.ComponentBody) || m_subsystemGameInfo.WorldSettings.GameMode <= GameMode.Harmless))
                    {
                        bool flag = false;
                        if (block.IsCollidable)
                        {
                            BoundingBox boundingBox = ComponentCreature.ComponentBody.BoundingBox;
                            boundingBox.Min += new Vector3(0.2f);
                            boundingBox.Max -= new Vector3(0.2f);
                            BoundingBox[] customCollisionBoxes = block.GetCustomCollisionBoxes(m_subsystemTerrain, placementData.Value);
                            for (int i = 0; i < customCollisionBoxes.Length; i++)
                            {
                                BoundingBox box = customCollisionBoxes[i];
                                box.Min += new Vector3(num2, num3, num4);
                                box.Max += new Vector3(num2, num3, num4);
                                if (boundingBox.Intersection(box))
                                {
                                    flag = true;
                                    break;
                                }
                            }
                        }
                        if (!flag)
                        {
                            SubsystemBlockBehavior[] blockBehaviors = m_subsystemBlockBehaviors.GetBlockBehaviors(Terrain.ExtractContents(placementData.Value));
                            for (int i = 0; i < blockBehaviors.Length; i++)
                            {
                                blockBehaviors[i].OnItemPlaced(num2, num3, num4, ref placementData, value);
                            }
                            m_subsystemTerrain.DestroyCell(0, num2, num3, num4, placementData.Value, noDrop: false, noParticleSystem: false);
                            m_subsystemAudio.PlaySound("Audio/BlockPlaced", 1f, 0f, new Vector3(placementData.CellFace.X, placementData.CellFace.Y, placementData.CellFace.Z), 5f, autoDelay: false);
                            Poke(forceRestart: false);
                            if (ComponentCreature.PlayerStats != null)
                            {
                                ComponentCreature.PlayerStats.BlocksPlaced++;
                            }
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool Use(Ray3 ray)
        {
            int num = Terrain.ExtractContents(ActiveBlockValue);
            Block block = BlocksManager.Blocks[num];
            if (!CanUseTool(ActiveBlockValue))
            {
                ComponentPlayer?.ComponentGui.DisplaySmallMessage(string.Format(LanguageControl.Get(fName, 1), block.PlayerLevelRequired, block.GetDisplayName(m_subsystemTerrain, ActiveBlockValue)), Color.White, blinking: true, playNotificationSound: true);
                Poke(forceRestart: false);
                return false;
            }
            SubsystemBlockBehavior[] blockBehaviors = m_subsystemBlockBehaviors.GetBlockBehaviors(num);
            for (int i = 0; i < blockBehaviors.Length; i++)
            {
                if (blockBehaviors[i].OnUse(ray, this))
                {
                    Poke(forceRestart: false);
                    return true;
                }
            }
            return false;
        }

        public bool Interact(TerrainRaycastResult raycastResult)
        {            
            int cellContents = m_subsystemTerrain.Terrain.GetCellContents(raycastResult.CellFace.X, raycastResult.CellFace.Y, raycastResult.CellFace.Z);
            SubsystemBlockBehavior[] blockBehaviors = m_subsystemBlockBehaviors.GetBlockBehaviors(cellContents);
            for (int i = 0; i < blockBehaviors.Length; i++)
            {
                if (blockBehaviors[i].OnInteract(raycastResult, this))
                {
                    if (ComponentCreature.PlayerStats != null)
                    {
                        ComponentCreature.PlayerStats.BlocksInteracted++;
                    }
                    Poke(forceRestart: false);
                    return true;
                }
            }
            return false;
        }

        public void Hit(ComponentBody componentBody, Vector3 hitPoint, Vector3 hitDirection)
        {
            float AttackPower = 0f;
            //预先生成粒子特效
            var particleSystem = new HitValueParticleSystem(hitPoint + 0.75f * hitDirection, 1f * hitDirection + ComponentCreature.ComponentBody.Velocity, Color.White, LanguageControl.Get(fName, 2));
            ModsManager.HookAction("ComponentMinerHit", list=> {
                foreach (ModLoader modLoader in list)
                {
                    modLoader.ComponentMinerHit(this, componentBody, hitPoint, hitDirection, particleSystem, ref AttackPower);
                }
            });
            Poke(forceRestart: false);
        }

        public bool Aim(Ray3 aim, AimState state)
        {
            int num = Terrain.ExtractContents(ActiveBlockValue);
            Block block = BlocksManager.Blocks[num];
            if (block.IsAimable_(ActiveBlockValue))
            {
                if (!CanUseTool(ActiveBlockValue))
                {
                    ComponentPlayer?.ComponentGui.DisplaySmallMessage(string.Format(LanguageControl.Get(fName, 1), block.PlayerLevelRequired_(ActiveBlockValue), block.GetDisplayName(m_subsystemTerrain, ActiveBlockValue)), Color.White, blinking: true, playNotificationSound: true);
                    Poke(forceRestart: false);
                    return true;
                }
                SubsystemBlockBehavior[] blockBehaviors = m_subsystemBlockBehaviors.GetBlockBehaviors(num);
                for (int i = 0; i < blockBehaviors.Length; i++)
                {
                    if (blockBehaviors[i].OnAim(aim, this, state))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public object Raycast(Ray3 ray, RaycastMode mode, bool raycastTerrain = true, bool raycastBodies = true, bool raycastMovingBlocks = true)
        {
            float reach = (m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Creative) ? SettingsManager.CreativeReach : 5f;
            Vector3 creaturePosition = ComponentCreature.ComponentCreatureModel.EyePosition;
            Vector3 start = ray.Position;
            var direction = Vector3.Normalize(ray.Direction);
            Vector3 end = ray.Position + direction * 15f;
            Point3 startCell = Terrain.ToCell(start);
            BodyRaycastResult? bodyRaycastResult = m_subsystemBodies.Raycast(start, end, 0.35f, (ComponentBody body, float distance) => (Vector3.DistanceSquared(start + distance * direction, creaturePosition) <= reach * reach && body.Entity != Entity && !body.IsChildOfBody(ComponentCreature.ComponentBody) && !ComponentCreature.ComponentBody.IsChildOfBody(body) && Vector3.Dot(Vector3.Normalize(body.BoundingBox.Center() - start), direction) > 0.7f) ? true : false);
            MovingBlocksRaycastResult? movingBlocksRaycastResult = m_subsystemMovingBlocks.Raycast(start, end, extendToFillCells: true);
            TerrainRaycastResult? terrainRaycastResult = m_subsystemTerrain.Raycast(start, end, useInteractionBoxes: true, skipAirBlocks: true, delegate (int value, float distance)
            {
                if (Vector3.DistanceSquared(start + distance * direction, creaturePosition) <= reach * reach)
                {
                    Block block = BlocksManager.Blocks[Terrain.ExtractContents(value)];
                    if (distance == 0f && block is CrossBlock && Vector3.Dot(direction, new Vector3(startCell) + new Vector3(0.5f) - start) < 0f)
                    {
                        return false;
                    }
                    if (mode == RaycastMode.Digging)
                    {
                        return !block.IsDiggingTransparent;
                    }
                    if (mode == RaycastMode.Interaction)
                    {
                        if (block.IsPlacementTransparent_(value))
                        {
                            return block.IsInteractive(m_subsystemTerrain, value);
                        }
                        return true;
                    }
                    if (mode == RaycastMode.Gathering)
                    {
                        return block.IsGatherable_(value);
                    }
                }
                return false;
            });
            float num = bodyRaycastResult.HasValue ? bodyRaycastResult.Value.Distance : float.PositiveInfinity;
            float num2 = movingBlocksRaycastResult.HasValue ? movingBlocksRaycastResult.Value.Distance : float.PositiveInfinity;
            float num3 = terrainRaycastResult.HasValue ? terrainRaycastResult.Value.Distance : float.PositiveInfinity;
            if (num < num2 && num < num3)
            {
                return bodyRaycastResult.Value;
            }
            if (num2 < num && num2 < num3)
            {
                return movingBlocksRaycastResult.Value;
            }
            if (num3 < num && num3 < num2)
            {
                return terrainRaycastResult.Value;
            }
            return new Ray3(start, direction);
        }

        public T? Raycast<T>(Ray3 ray, RaycastMode mode, bool raycastTerrain = true, bool raycastBodies = true, bool raycastMovingBlocks = true) where T : struct
        {
            object obj = Raycast(ray, mode, raycastTerrain, raycastBodies, raycastMovingBlocks);
            if (!(obj is T))
            {
                return null;
            }
            return (T)obj;
        }

        public void RemoveActiveTool(int removeCount)
        {
            if (Inventory != null)
            {
                Inventory.RemoveSlotItems(Inventory.ActiveSlotIndex, removeCount);
            }
        }

        public void DamageActiveTool(int damageCount)
        {
            if (m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Creative || Inventory == null)
            {
                return;
            }
            int num = BlocksManager.DamageItem(ActiveBlockValue, damageCount);
            if (num != 0)
            {
                int slotCount = Inventory.GetSlotCount(Inventory.ActiveSlotIndex);
                Inventory.RemoveSlotItems(Inventory.ActiveSlotIndex, slotCount);
                if (Inventory.GetSlotCount(Inventory.ActiveSlotIndex) == 0)
                {
                    Inventory.AddSlotItems(Inventory.ActiveSlotIndex, num, slotCount);
                }
            }else
            {
                Inventory.RemoveSlotItems(Inventory.ActiveSlotIndex, 1);
            }
        }

        public static void AttackBody(ComponentBody target, ComponentCreature attacker, Vector3 hitPoint, Vector3 hitDirection, float attackPower, bool isMeleeAttack)
        {
            HitValueParticleSystem hitValueParticleSystem = null;
            if (attacker != null) {
                hitValueParticleSystem = new HitValueParticleSystem(hitPoint + 0.75f * hitDirection, 1f * hitDirection + attacker.ComponentBody.Velocity, Color.White, string.Empty);
            }
            ModsManager.HookAction("AttackBody", list=> {
                foreach (ModLoader modEntity in list)
                {
                    modEntity.AttackBody(target, attacker, hitPoint, hitDirection, attackPower, isMeleeAttack, hitValueParticleSystem);
                }
            });
        }

        public void Update(float dt)
        {
            float num = (m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Creative) ? (0.5f / SettingsManager.CreativeDigTime) : 4f;
            m_lastPokingPhase = PokingPhase;
            if (DigCellFace.HasValue || PokingPhase > 0f)
            {
                PokingPhase += num * m_subsystemTime.GameTimeDelta;
                if (PokingPhase > 1f)
                {
                    PokingPhase = DigCellFace.HasValue ? MathUtils.Remainder(PokingPhase, 1f) : 0f;
                }
            }
            if (DigCellFace.HasValue && Time.FrameIndex - m_lastDigFrameIndex > 1)
            {
                DigCellFace = null;
            }
        }

        public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
        {
            m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
            m_subsystemBodies = Project.FindSubsystem<SubsystemBodies>(throwOnError: true);
            m_subsystemMovingBlocks = Project.FindSubsystem<SubsystemMovingBlocks>(throwOnError: true);
            m_subsystemGameInfo = Project.FindSubsystem<SubsystemGameInfo>(throwOnError: true);
            m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
            m_subsystemAudio = Project.FindSubsystem<SubsystemAudio>(throwOnError: true);
            m_subsystemSoundMaterials = Project.FindSubsystem<SubsystemSoundMaterials>(throwOnError: true);
            m_subsystemBlockBehaviors = Project.FindSubsystem<SubsystemBlockBehaviors>(throwOnError: true);
            ComponentCreature = Entity.FindComponent<ComponentCreature>(throwOnError: true);
            ComponentPlayer = Entity.FindComponent<ComponentPlayer>();
            Inventory = m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Creative && ComponentPlayer != null
                ? Entity.FindComponent<ComponentCreativeInventory>()
                : (IInventory)Entity.FindComponent<ComponentInventory>();
            AttackPower = valuesDictionary.GetValue<float>("AttackPower");
        }

        public static bool IsBlockPlacingAllowed(ComponentBody componentBody)
        {
            if (componentBody.StandingOnBody != null || componentBody.StandingOnValue.HasValue)
            {
                return true;
            }
            if (componentBody.ImmersionFactor > 0.01f)
            {
                return true;
            }
            if (componentBody.ParentBody != null && IsBlockPlacingAllowed(componentBody.ParentBody))
            {
                return true;
            }
            ComponentLocomotion componentLocomotion = componentBody.Entity.FindComponent<ComponentLocomotion>();
            if (componentLocomotion != null && componentLocomotion.LadderValue.HasValue)
            {
                return true;
            }
            return false;
        }

        public float CalculateDigTime(int digValue, int toolValue)
        {
            Block block = BlocksManager.Blocks[Terrain.ExtractContents(toolValue)];
            Block block2 = BlocksManager.Blocks[Terrain.ExtractContents(digValue)];
            float digResilience = block2.GetDigResilience(digValue);
            BlockDigMethod digBlockMethod = block2.GetBlockDigMethod(digValue);
            float ShovelPower = block.GetShovelPower(toolValue);
            float QuarryPower = block.GetQuarryPower(toolValue);
            float HackPower = block.GetHackPower(toolValue);

            if (ComponentPlayer != null && m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Creative)
            {
                if (digResilience < float.PositiveInfinity)
                {
                    return 0f;
                }
                return float.PositiveInfinity;
            }
            if (ComponentPlayer != null && m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Adventure)
            {
                float num = 0f;
                if (digBlockMethod == BlockDigMethod.Shovel && ShovelPower >= 2f)
                {
                    num = ShovelPower;
                }
                else if (digBlockMethod == BlockDigMethod.Quarry && QuarryPower >= 2f)
                {
                    num = QuarryPower;
                }
                else if (digBlockMethod == BlockDigMethod.Hack && HackPower >= 2f)
                {
                    num = HackPower;
                }
                num *= ComponentPlayer.ComponentLevel.StrengthFactor;
                if (!(num > 0f))
                {
                    return float.PositiveInfinity;
                }
                return MathUtils.Max(digResilience / num, 0f);
            }
            float num2 = 0f;
            if (digBlockMethod == BlockDigMethod.Shovel)
            {
                num2 = ShovelPower;
            }
            else if (digBlockMethod == BlockDigMethod.Quarry)
            {
                num2 = QuarryPower;
            }
            else if (digBlockMethod == BlockDigMethod.Hack)
            {
                num2 = HackPower;
            }
            if (ComponentPlayer != null)
            {
                num2 *= ComponentPlayer.ComponentLevel.StrengthFactor;
            }
            if (!(num2 > 0f))
            {
                return float.PositiveInfinity;
            }
            return MathUtils.Max(digResilience / num2, 0f);
        }

        public bool CanUseTool(int toolValue)
        {
            if (m_subsystemGameInfo.WorldSettings.GameMode != 0 && m_subsystemGameInfo.WorldSettings.AreAdventureSurvivalMechanicsEnabled)
            {
                Block block = BlocksManager.Blocks[Terrain.ExtractContents(toolValue)];
                if (ComponentPlayer != null && ComponentPlayer.PlayerData.Level < block.PlayerLevelRequired_(toolValue))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
