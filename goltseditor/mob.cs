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
    public abstract class Mob:PhysicalObject
    {
        public int HP { get; protected set; }
        public int MaxHP { get; protected set; }

        [JsonProperty]
        public string Action { get; protected set; }

        [JsonIgnore]
        protected string previousAction = "";
 
        public int Direction { get; protected set; }
        protected int previousDirection = -1;

        [JsonProperty]
        protected string standTextureName { get; init; }

        [JsonConstructor]
        public Mob() { }

        public Mob(ContentManager contentManager, double x, double y, double movementX, double movementY, int weight,
            bool gravityAffected, string textureName, string hitboxPath, 
            int hP, int maxHP, string action):
            base(contentManager, x, y, movementX, movementY, weight, gravityAffected, textureName, hitboxPath)
        {
            HP = hP;
            MaxHP = maxHP;

            Action = action;
            standTextureName = textureName;
        }

        public Mob(ContentManager contentManager, double x, double y, double movementX, double movementY, int weight,
            bool gravityAffected, string textureName, List<Tuple<double, double>> hitbox, 
            int hP, int maxHP, string action) :
            base(contentManager, x, y, movementX, movementY, weight, gravityAffected, textureName, hitbox)
        {
            HP = hP;
            MaxHP = maxHP;

            Action = action;
            standTextureName = textureName;
        }

        public override void Update(ContentManager contentManager, World world)
        {
            if (MovementX > 0)
                Direction = 0;
            else if (MovementX < 0)
                Direction = 1;

            if (previousAction != Action)
            {
                Texture = new DynamicTexture(contentManager, standTextureName + "_" + Action + "_".ToString());
            }

            base.Update(contentManager, world);

            previousAction = Action;
            previousDirection = Direction;
        }

        public override void Draw(int x, int y, SpriteBatch spriteBatch, float depth, float scale, Color color, SpriteEffects spriteEffects)
        {
            if (Direction == 1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            base.Draw(x, y, spriteBatch, depth, scale, color, spriteEffects);

        }
    }
}