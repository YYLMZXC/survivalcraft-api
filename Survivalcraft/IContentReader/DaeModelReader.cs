﻿using System.IO;
using Engine.Graphics;

namespace Game.IContentReader
{
    public class DaeModelReader:IContentReader
    {
        public override string Type => "Engine.Graphics.Model";
        public override string[] DefaultSuffix => new string[] { "dae" };
        public override object Get(ContentInfo[] contents)
        {
            return Model.Load(contents[0].Duplicate(), true);
        }
    }
}
