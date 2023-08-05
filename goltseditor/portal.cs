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
    public class Portal : PhysicalObject
    {
        [JsonProperty]
        public int ExitPointIndex { get; set; }
        [JsonProperty]
        public int RoomIndex { get; set; }

        [JsonConstructor]
        public Portal() : base() { }

        public Portal(ContentManager contentManager, double x, double y, double movementx, double movementy, int weight,
            bool gravityAffected, string textureName, List<Tuple<double, double>> hitbox, int roomIndex, int exitPointIndex)
            : base(contentManager, x, y, movementx, movementy, weight, gravityAffected,
            textureName, hitbox, 1)
        {
            RoomIndex = roomIndex;
            ExitPointIndex = exitPointIndex;
        }

        public override void Update(ContentManager contentManager, World world)
        {
        }
    }

    public class ExitPoint : WorldObject
    {
        [JsonProperty]
        public int Index { get; set; }

        [JsonConstructor]
        public ExitPoint() : base() { }

        public ExitPoint(ContentManager contentManager, double x, double y, double movementx, double movementy, int weight,
            bool gravityAffected, string textureName, int index)
            : base(contentManager, x, y, movementx, movementy, weight, gravityAffected, textureName)
        {
            Index = index;
        }
    }
}