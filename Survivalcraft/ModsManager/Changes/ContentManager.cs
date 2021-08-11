using Engine;
using System;
using System.IO;
using System.Collections.Generic;
using Engine.Serialization;
using Engine.Media;
using Engine.Graphics;
using System.Xml.Linq;

namespace Game
{
    public class ContentInfo { 
        public ModEntity Entity;
        public string Filename;
        public string ContentPath;
        public object obj;
        public ContentInfo(ModEntity entity, string AbsolutePath_)
        {
            Entity = entity;
            Filename = Path.GetFileName(AbsolutePath_);
            int pos = AbsolutePath_.LastIndexOf('.');
            ContentPath = AbsolutePath_.Substring(0, pos);
        }
        public bool Get(Type type, string name,out object obj)
        {
            obj = null;
            if (Entity.GetAssetsFile(name, out Stream stream))
            {
                obj = ContentManager.StreamConvertType(type, stream);
                return true;
            }
            return false;
        }
        public bool Get(string name, out Stream stream)
        {
            stream = null;
            if (Entity.GetAssetsFile(name, out stream))
            {
                return true;
            }
            return false;
        }
    }
    public static class ContentManager
    {
        internal static Dictionary<string, ContentInfo> Resources = new Dictionary<string, ContentInfo>();

        public static void Initialize()
        {
            Resources.Clear();
        }
        /// <summary>
        /// ��ȡ��Դ
        /// </summary>
        /// <typeparam name="T">Ҫת��������</typeparam>
        /// <param name="name">��Դ����</param>
        /// <param name="useCache">�Ƿ�ʹ�û���</param>
        /// <returns></returns>
        public static T Get<T>(string name, bool useCache = false) where T : class
        {
            return Get(typeof(T), name,useCache) as T;
        }

        public static object Get(Type type, string name,bool useCache=false)
        {
            object obj = null;
            if (name.Contains(":"))
            { //�������ռ�Ľ���
                string[] spl = name.Split(new char[] { ':' }, StringSplitOptions.None);
                string ModSpace = spl[0];
                name = spl[1];
            }
            string fixname = string.Empty;
            switch (type.FullName)
            {
                case "Engine.Media.BitmapFont":
                    {
                        if (Resources.TryGetValue(name + ".png", out ContentInfo contentInfo1))
                        {
                            if (contentInfo1.Get(name + ".png", out Stream stream))
                            {
                                if (contentInfo1.obj != null && useCache) return contentInfo1.obj;
                                using (stream)
                                {
                                    if (contentInfo1.Get(name + ".lst", out Stream stream2))
                                    {
                                        using (stream)
                                        {
                                            return contentInfo1.obj = BitmapFont.Initialize(stream, stream2);
                                        }
                                    }
                                }
                            }
                        }
                        throw new Exception("Not found Resources:" + name + " for " + type.FullName);
                    }
                case "Engine.Graphics.Shader": 
                    {
                        if (Resources.TryGetValue(name + ".psh", out ContentInfo contentInfo1))
                        {
                            if (contentInfo1.Get(name + ".psh", out Stream stream))
                            {
                                if (contentInfo1.obj != null && useCache) return contentInfo1.obj;
                                using (stream)
                                {
                                    if (contentInfo1.Get(name + ".vsh", out Stream stream2))
                                    {
                                        using (stream)
                                        {
                                            return contentInfo1.obj = new Shader(new VertexShaderCode() { Code = new StreamReader(stream).ReadToEnd() }, new PixelShaderCode() { Code = new StreamReader(stream2).ReadToEnd() }, new ShaderMacro[] { new ShaderMacro(name) });
                                        }
                                    }
                                }
                            }
                        }
                        throw new Exception("Not found Resources:" + name + " for " + type.FullName);
                    }
                case "Engine.Audio.SoundBuffer":
                    {
                        if (Resources.TryGetValue(name + ".ogg", out ContentInfo contentInfo1))
                        {
                            if (contentInfo1.obj != null && useCache) return contentInfo1.obj;
                            if (contentInfo1.Get(name + ".ogg", out Stream stream))
                            {
                                return contentInfo1.obj = Engine.Audio.SoundBuffer.Load(stream, SoundFileFormat.Ogg);
                            }
                        }
                        if (Resources.TryGetValue(name + ".wav", out ContentInfo contentInfo2))
                        {
                            if (contentInfo2.obj != null && useCache) return contentInfo2.obj;
                            if (contentInfo2.Get(name + ".wav", out Stream stream))
                            {
                                return contentInfo2.obj = Engine.Audio.SoundBuffer.Load(stream, SoundFileFormat.Wav);
                            }
                        }
                        throw new Exception("Not found Resources:" + name + " for " + type.FullName);
                    }
                case "Engine.Graphics.Texture2D": fixname=name+".png";break;
                case "System.String": fixname = name + ".txt"; break;
                case "Engine.Media.Image": fixname = name + ".png"; break;
                case "System.Xml.Linq.XElement": fixname = name + ".xml"; break;
                case "Engine.Graphics.Model": fixname = name + ".dae"; break;
                case "Engine.Media.StreamingSource":fixname = name + ".ogg";break;
                case "Engine.Graphics.VertexShaderCode": fixname = name + ".vsh"; break;
                case "Engine.Graphics.PixelShaderCode": fixname = name + ".psh"; break;
                case "Game.ObjModel": fixname = name + ".obj"; break;
                case "Game.JsonModel": fixname = name + ".json"; break;
                case "SimpleJson.JsonObject": fixname = name + ".json"; break;
                case "Game.Subtexture": if (name.StartsWith("Textures/Atlas/")) return TextureAtlasManager.GetSubtexture(name); else return new Subtexture(Get<Texture2D>(name),Vector2.Zero,Vector2.One);
                default: { break; }
            }
            if (Resources.TryGetValue(fixname, out ContentInfo contentInfo3))
            {
                if(contentInfo3.obj != null && useCache) return contentInfo3.obj;
                if (contentInfo3.Get(fixname, out Stream stream))
                {
                    using (stream)
                    {//���ļ�ת��
                        obj = StreamConvertType(type, stream);
                    }
                }
            }
            if (obj == null) throw new Exception("Not found Resources:" + name + " for " + type.FullName);
            contentInfo3.obj = obj;
            return obj;
        }
        public static object StreamConvertType(Type type,Stream stream)
        {
            switch (type.FullName)
            {
                case "SimpleJson.JsonObject": return SimpleJson.SimpleJson.DeserializeObject(new StreamReader(stream).ReadToEnd());
                case "Engine.Graphics.VertexShaderCode": return new VertexShaderCode() { Code=new StreamReader(stream).ReadToEnd()};
                case "Engine.Graphics.PixelShaderCode": return new PixelShaderCode() { Code = new StreamReader(stream).ReadToEnd() };
                case "Engine.Media.StreamingSource":return Ogg.Stream(stream);
                case "Engine.Audio.SoundBuffer":return SoundData.Load(stream);
                case "Engine.Graphics.Texture2D": return Texture2D.Load(stream);
                case "System.String":return new StreamReader(stream).ReadToEnd();
                case "Engine.Media.Image": return Image.Load(stream);
                case "Game.ObjModel": return ObjModelReader.Load(stream);
                case "Game.JsonModel": return JsonModelReader.Load(stream);
                case "System.Xml.Linq.XElement": return XElement.Load(stream);
                case "Engine.Graphics.Model": return Model.Load(stream,true);
            }
            return null;
        }


        public static void Add(ModEntity entity,string name)
        {
            if (Resources.TryGetValue(name, out ContentInfo contentInfo))
            {
                contentInfo = new ContentInfo(entity, name);
            }
            else
                Resources.Add(name, new ContentInfo(entity, name));
        }

        public static void Dispose(string name)
        {
            foreach (ContentInfo contentInfo in Resources.Values)
            {
                if (contentInfo.ContentPath == name) {
                    IDisposable disposable = contentInfo.obj as IDisposable;
                    if (disposable != null) disposable.Dispose();
                    break;
                }
            }
        }

        public static bool IsContent(object content)
        {
            foreach (ContentInfo contentInfo in Resources.Values)
            {
                if (contentInfo.obj == content) return true;
            }
            return false;
        }

        public static ReadOnlyList<ContentInfo> List()
        {
            return new ReadOnlyList<ContentInfo>(Resources.Values.ToDynamicArray());
        }

        public static ReadOnlyList<ContentInfo> List(string directory)
        {
            List<ContentInfo> contents = new List<ContentInfo>();
            foreach (var content in Resources.Values) {
                if(content.ContentPath.StartsWith(directory))contents.Add(content);            
            }
            return new ReadOnlyList<ContentInfo>(contents);
        }
    }
}
