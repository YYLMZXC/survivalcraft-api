using Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Tiny7z.Archive;

namespace Game
{
    public class SurvivalCraftModEntity : ModEntity
    {
        public SurvivalCraftModEntity()
        {
            var readers = new List<IContentReader.IContentReader>();
            readers.Add(new IContentReader.BitmapFontReader());
            readers.Add(new IContentReader.DaeModelReader());
            readers.Add(new IContentReader.ImageReader());
            readers.Add(new IContentReader.JsonArrayReader());
            readers.Add(new IContentReader.JsonObjectReader());
            readers.Add(new IContentReader.JsonModelReader());
            readers.Add(new IContentReader.MtllibStructReader());
            readers.Add(new IContentReader.ObjModelReader());
            readers.Add(new IContentReader.ShaderReader());
            readers.Add(new IContentReader.SoundBufferReader());
            readers.Add(new IContentReader.StreamingSourceReader());
            readers.Add(new IContentReader.StringReader());
            readers.Add(new IContentReader.SubtextureReader());
            readers.Add(new IContentReader.Texture2DReader());
            readers.Add(new IContentReader.XmlReader());
            for (int i = 0; i < readers.Count; i++)
            {
                ContentManager.ReaderList.Add(readers[i].Type, readers[i]);
            }

            Stream stream = Storage.OpenFile("app:Content.7z", OpenFileMode.Read);
            MemoryStream memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            stream.Close();
            memoryStream.Position = 0L;
            ModArchive = new SevenZipArchive(memoryStream, FileAccess.Read);
            InitResources();
            LabelWidget.BitmapFont = ContentManager.Get<Engine.Media.BitmapFont>("Fonts/Pericles");
            LoadingScreen.Info("加载资源:" + modInfo?.Name);
        }
        public override void LoadBlocksData()
        {
            LoadingScreen.Info("加载方块数据:" + modInfo?.Name);
            BlocksManager.LoadBlocksData(ContentManager.Get<string>("BlocksData"));
            ContentManager.Dispose("BlocksData");
        }
        public override void LoadDll()
        {
            List<Type> BlockTypes = new List<Type>();
            Type[] types = typeof(BlocksManager).Assembly.GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                Type type = types[i];
                if (type.IsSubclassOf(typeof(ModLoader)) && !type.IsAbstract)
                {
                    var modLoader = Activator.CreateInstance(types[i]) as ModLoader;
                    modLoader.Entity = this;
                    modLoader.__ModInitialize();
                    Loader = modLoader;
                    ModsManager.ModLoaders.Add(modLoader);
                }
                if (type.IsSubclassOf(typeof(Block)) && !type.IsAbstract)
                {
                    BlockTypes.Add(type);
                }
            }
            for (int i = 0; i < BlockTypes.Count; i++)
            {
                Type type = BlockTypes[i];
                FieldInfo fieldInfo = type.GetRuntimeFields().FirstOrDefault(p => p.Name == "Index" && p.IsPublic && p.IsStatic);
                if (fieldInfo == null || fieldInfo.FieldType != typeof(int))
                {
                    ModsManager.AddException(new InvalidOperationException($"Block type \"{type.FullName}\" does not have static field Index of type int."));
                }
                else
                {
                    int staticIndex = (int)fieldInfo.GetValue(null);
                    var block = (Block)Activator.CreateInstance(type.GetTypeInfo().AsType());
                    block.BlockIndex = staticIndex;
                    Blocks.Add(block);
                }

            }
        }
        public override void LoadXdb(ref XElement xElement)
        {
            LoadingScreen.Info("加载数据库:" + modInfo?.Name);
            xElement = ContentManager.Get<XElement>("Database");
            ContentManager.Dispose("Database");
        }
        public override void LoadCr(ref XElement xElement)
        {
            LoadingScreen.Info("加载合成谱:" + modInfo?.Name);
            xElement = ContentManager.Get<XElement>("CraftingRecipes");
            ContentManager.Dispose("CraftingRecipes");
        }
        public override void LoadClo(ClothingBlock block, ref XElement xElement)
        {
            LoadingScreen.Info("加载衣物数据:" + modInfo?.Name);
            xElement = ContentManager.Get<XElement>("Clothes");
            ContentManager.Dispose("Clothes");
        }
        public override void SaveSettings(XElement xElement)
        {


        }
        public override void LoadSettings(XElement xElement)
        {



        }
        public override void OnBlocksInitalized()
        {
            BlocksManager.AddCategory("Terrain");
            BlocksManager.AddCategory("Plants");
            BlocksManager.AddCategory("Construction");
            BlocksManager.AddCategory("Items");
            BlocksManager.AddCategory("Tools");
            BlocksManager.AddCategory("Weapons");
            BlocksManager.AddCategory("Clothes");
            BlocksManager.AddCategory("Electrics");
            BlocksManager.AddCategory("Food");
            BlocksManager.AddCategory("Spawner Eggs");
            BlocksManager.AddCategory("Painted");
            BlocksManager.AddCategory("Dyed");
            BlocksManager.AddCategory("Fireworks");
        }
    }
}
