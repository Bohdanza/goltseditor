using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace goltseditor
{
    public class ObjectCreator
    {
        private List<string> texureNames { get; set; }
        public List<Texture2D> Textures { get; protected set; }

        public ObjectCreator()
        {
            
        }
    }
}