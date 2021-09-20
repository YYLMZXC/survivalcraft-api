﻿using System.IO;

namespace Game.IContentReader
{
    public class ObjModelReader:IContentReader
    {
        public override string Type => "Game.ObjModel";
        public override string[] DefaultSuffix => new string[] { "obj" };
        public override object Get(ContentInfo[] contents)
        {
            return Game.ObjModelReader.Load(contents[0].Duplicate());
        }
    }
}
