﻿using System.IO;
using System.Collections.Generic;
using Engine.Graphics;
using Engine;
using System.Text;
using SimpleJson;
using System.Reflection;
using System;
using System.Xml.Linq;
using System.Linq;
using GameEntitySystem;
namespace Game
{
    public abstract class ModLoader
    {
        /// <summary>
        /// 当ModLoader类被实例化时执行
        /// </summary>
        public virtual void __ModInitialize() { }
        /// <summary>
        /// 当人物进行挖掘时执行
        /// </summary>
        /// <param name="miner"></param>
        /// <param name="raycastResult"></param>
        /// <returns></returns>
        public virtual bool ComponentMinerDig(ComponentMiner miner, TerrainRaycastResult raycastResult)
        {

            return false;
        }
        /// <summary>
        /// 当人物使用手持物品时执行
        /// </summary>
        /// <param name="miner"></param>
        /// <param name="ray"></param>
        /// <returns></returns>
        public virtual bool ComponentMinerUse(ComponentMiner miner, Ray3 ray)
        {

            return false;
        }
        /// <summary>
        /// 当人物放置方块时执行
        /// </summary>
        /// <param name="miner"></param>
        /// <param name="raycastResult"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual bool ComponentMinerPlace(ComponentMiner miner, TerrainRaycastResult raycastResult, int value)
        {
            return false;
        }
        /// <summary>
        /// 当生物收到攻击时执行
        /// </summary>
        /// <param name="target"></param>
        /// <param name="attacker"></param>
        /// <param name="hitPoint"></param>
        /// <param name="hitDirection"></param>
        /// <param name="attackPower"></param>
        /// <param name="isMeleeAttack"></param>
        public virtual void AttackBody(ComponentBody target, ComponentCreature attacker, Vector3 hitPoint, Vector3 hitDirection, float attackPower, bool isMeleeAttack) { }
        /// <summary>
        /// 计算护甲免伤时执行
        /// </summary>
        /// <param name="componentClothing"></param>
        /// <param name="attackPower">未计算免伤前的伤害</param>
        /// <returns>免伤后的伤害，当多个mod都有免伤计算时，取最小值</returns>
        public virtual float ApplyArmorProtection(ComponentClothing componentClothing, float attackPower)
        {
            return attackPower;
        }
        /// <summary>
        /// 生物生成的初始化方法
        /// </summary>
        /// <param name="spawn"></param>
        /// <param name="creatureTypes"></param>
        public virtual void InitializeCreatureTypes(SubsystemCreatureSpawn spawn, List<SubsystemCreatureSpawn.CreatureType> creatureTypes) { }
        /// <summary>
        /// 生物被生成时执行
        /// </summary>
        /// <param name="spawn"></param>
        /// <param name="entity"></param>
        /// <param name="spawnEntityData"></param>
        public virtual void SpawnEntity(SubsystemSpawn spawn, Entity entity, SpawnEntityData spawnEntityData) { }
        /// <summary>
        /// 当区块加载时，区块的生物数据被读取时执行
        /// </summary>
        /// <param name="spawn"></param>
        /// <param name="data"></param>
        /// <param name="creaturesData"></param>
        public virtual void LoadSpawnsData(SubsystemSpawn spawn, string data, List<SpawnEntityData> creaturesData) { }
        /// <summary>
        /// 当区块卸载时，区块的生物数据被保存时执行
        /// </summary>
        /// <param name="spawn"></param>
        /// <param name="spawnsData"></param>
        /// <returns></returns>
        public virtual string SaveSpawnsData(SubsystemSpawn spawn, List<SpawnEntityData> spawnsData) { return ""; }
        /// <summary>
        /// 掉落物被添加时执行
        /// </summary>
        /// <param name="subsystemPickables"></param>
        /// <param name="pickable"></param>
        public virtual void PickableAdded(SubsystemPickables subsystemPickables, Pickable pickable) { }
        /// <summary>
        /// 掉落物被删除时执行
        /// </summary>
        /// <param name="subsystemPickables"></param>
        /// <param name="pickable"></param>
        public virtual void PickableRemoved(SubsystemPickables subsystemPickables, Pickable pickable) { }
        /// <summary>
        /// 投掷物被添加时执行
        /// </summary>
        /// <param name="subsystemProjectiles"></param>
        /// <param name="projectile"></param>
        public virtual void ProjectileAdded(SubsystemProjectiles subsystemProjectiles, Projectile projectile) { }
        /// <summary>
        /// 投掷物被删除时执行
        /// </summary>
        /// <param name="subsystemProjectiles"></param>
        /// <param name="projectile"></param>
        public virtual void ProjectileRemoved(SubsystemProjectiles subsystemProjectiles, Projectile projectile) { }
        /// <summary>
        /// 背包界面被打开时执行
        /// </summary>
        /// <param name="componentGui"></param>
        /// <param name="clothingWidget"></param>
        public virtual void OnClothingWidgetOpen(ComponentGui componentGui, ClothingWidget clothingWidget) { }
        /// <summary>
        /// 人物死亡时执行
        /// </summary>
        /// <param name="playerData"></param>
        public virtual void OnPlayerDead(PlayerData playerData) {


        }
        /// <summary>
        /// 改变相机模式时执行
        /// </summary>
        /// <param name="m_componentPlayer"></param>
        /// <param name="componentGui"></param>
        public virtual void OnCameraChange(ComponentPlayer m_componentPlayer, ComponentGui componentGui) {


        }
        /// <summary>
        /// 地形生成步骤1时执行
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="chunk"></param>
        public virtual void GenerateChunkContentsPass1(ITerrainContentsGenerator generator, TerrainChunk chunk)
        {

        }
        /// <summary>
        /// 地形生成步骤2时执行
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="chunk"></param>
        public virtual void GenerateChunkContentsPass2(ITerrainContentsGenerator generator, TerrainChunk chunk)
        {

        }
        /// <summary>
        /// 地形生成步骤3时执行
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="chunk"></param>
        public virtual void GenerateChunkContentsPass3(ITerrainContentsGenerator generator, TerrainChunk chunk)
        {

        }
        /// <summary>
        /// 地形生成步骤4时执行
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="chunk"></param>
        public virtual void GenerateChunkContentsPass4(ITerrainContentsGenerator generator, TerrainChunk chunk)
        {

        }
        /// <summary>
        /// 更新TerrainChunk状态时执行
        /// </summary>
        /// <param name="terrainUpdater"></param>
        /// <param name="chunk"></param>
        /// <param name="skylightValue"></param>
        public virtual void UpdateChunkSingleStep(TerrainUpdater terrainUpdater, TerrainChunk chunk, int skylightValue) {

        }
        /// <summary>
        /// 人物攻击生物时执行
        /// </summary>
        /// <param name="miner"></param>
        /// <param name="componentBody"></param>
        /// <param name="hitPoint"></param>
        /// <param name="hitDirection"></param>
        public virtual void ComponentMinerHit(ComponentMiner miner, ComponentBody componentBody, Vector3 hitPoint, Vector3 hitDirection) { }
        /// <summary>
        /// 生物变形重生为另一个生物时执行
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="componentSpawn"></param>
        public virtual void ComponentSpawn_Despawned(Entity entity, ComponentSpawn componentSpawn) { }
        /// <summary>
        /// 绘制额外模型数据的方法，如人物头顶的名字
        /// </summary>
        /// <param name="modelsRenderer"></param>
        /// <param name="componentModel"></param>
        /// <param name="camera"></param>
        /// <param name="alphaThreshold"></param>
        public virtual void OnModelRendererDrawExtra(SubsystemModelsRenderer modelsRenderer, ComponentModel componentModel, Camera camera, float? alphaThreshold) { }
        /// <summary>
        /// BlocksManager初始化完成执行的方法
        /// </summary>
        public virtual void OnBlocksManagerInitalized() { 
        }
        /// <summary>
        /// Xdb被加载时执行的方法
        /// </summary>
        /// <param name="xElement"></param>
        public virtual void OnXdbLoad(XElement xElement) { }
        /// <summary>
        /// ScreensManager初始化完成的方法
        /// </summary>
        /// <param name="loadingScreen"></param>
        public virtual void OnScreensManagerInitalized(LoadingScreen loadingScreen) { }
        /// <summary>
        /// 游戏设置数据保存的方法
        /// </summary>
        /// <param name="xElement"></param>
        public virtual void SaveSettings(XElement xElement)
        {

        }
        /// <summary>
        /// 游戏设置数据加载的方法
        /// </summary>
        /// <param name="xElement"></param>
        public virtual void LoadSettings(XElement xElement)
        {

        }
        /// <summary>
        /// Project.xml加载时执行的方法
        /// </summary>
        /// <param name="xElement"></param>
        public virtual void ProjectLoad(XElement xElement) { }
        /// <summary>
        /// Project.xml保存时执行的方法
        /// </summary>
        /// <param name="xElement"></param>
        public virtual void ProjectSave(XElement xElement) { }
        /// <summary>
        /// ComponentGui帧更新方法
        /// </summary>
        /// <param name="componentGui"></param>
        public virtual void GuiUpdate(ComponentGui componentGui) { 
        
        }
        /// <summary>
        /// ComponentGui被添加时的方法
        /// </summary>
        /// <param name="componentGui"></param>
        /// <param name="entity"></param>
        public virtual void OnGuiEntityAdd(ComponentGui componentGui,Entity entity)
        {

        }
        /// <summary>
        /// ComponentGui被删除时的方法
        /// </summary>
        /// <param name="componentGui"></param>
        /// <param name="entity"></param>
        public virtual void OnGuiEntityRemove(ComponentGui componentGui, Entity entity)
        {

        }

        /// <summary>
        /// 渲染CubeBlock图形数据的方法
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="color">点输出颜色</param>
        /// <param name="light">传入的亮度</param>
        /// <param name="face">哪个面</param>
        /// <param name="corner"></param>
        /// <param name="vertex"></param>
        public virtual bool SetupCornerVertex(float x, float y, float z,ref Color color, int light, int face, int corner, ref TerrainVertex vertex) {
            return false;
        }
        /// <summary>
        /// 渲染CrossBlock图形数据的方法
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="color"></param>
        /// <param name="corner"></param>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public virtual bool SetupLitCornerVertex(float x, float y, float z,ref Color color, int corner, ref TerrainVertex vertex) {
            return false;

        }
        /// <summary>
        /// 渲染MeshBlock图形数据的方法
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="color"></param>
        /// <param name="tx"></param>
        /// <param name="ty"></param>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public virtual bool SetupVertex(float x, float y, float z,ref Color color, float tx, float ty, ref TerrainVertex vertex) {
            return false;

        }
        /// <summary>
        /// 生物最大组件数，多个Mod时取最大
        /// </summary>
        /// <returns></returns>
        public virtual int GetMaxInstancesCount() {
            return 7;
        }
    }
}