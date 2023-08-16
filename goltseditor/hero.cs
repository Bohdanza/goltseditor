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
    public class Hero : Mob
    {
        [JsonProperty]
        private bool inJump = false;
        [JsonProperty]
        private long timeSinceJump = 0;//actual time in GAME TICKS since last jump started
        [JsonProperty]
        private int maxJumpingTime = 12; //maximum time after leaving the land during which a jump can be performed

        [JsonConstructor]
        public Hero() { }

        public Hero(ContentManager contentManager, double x, double y, double movementX, double movementY)
            : base(contentManager, x, y, movementX, movementY, 5, true, "hero", @"boxes\hero", 5, 5, "id")
        {
            StandartFallingSpeed = 0.6;
        }

        public override void Update(ContentManager contentManager, World world)
        {
            base.Update(contentManager, world);
        }
    }
}