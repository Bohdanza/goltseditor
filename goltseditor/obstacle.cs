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
using System.Net.NetworkInformation;

namespace goltseditor
{
    public class Obstacle:PhysicalObject
    {
        [JsonConstructor]
        public Obstacle() { }

        public Obstacle(ContentManager contentManager, double x, double y, 
            string textureName, List<Tuple<double, double>> hitbox):
            base(contentManager, x, y, 0, 0, 10000, false, textureName, hitbox)
        {}
    }
}