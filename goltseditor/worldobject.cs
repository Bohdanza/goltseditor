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
    public abstract class WorldObject
    {
        [JsonIgnore]
        public TextboxList Parameters { get; protected set; }

        [JsonProperty]
        public double StandartFallingSpeed { get; protected set; } = 3;
        [JsonProperty]
        public double CurrentFallingSpeed { get; protected set; }

        [JsonProperty]
        public bool GravityAffected { get; protected set; }
        [JsonProperty]
        public double Weight { get; protected set; }

        [JsonProperty]
        public double MovementX { get; protected set; }
        [JsonProperty]
        public double MovementY { get; protected set; }
        [JsonProperty]
        public double X { get;set; }
        [JsonProperty]
        public double Y { get; set; }

        [JsonProperty]
        public DynamicTexture Texture { get; protected set; }
        [JsonProperty]
        public float DrawingDepth=0.0f;
        [JsonProperty]
        public float ParalaxCoefficient = 1.0f;

        [Newtonsoft.Json.JsonConstructor]
        public WorldObject() 
        {
            InitParams();
        }

        public WorldObject(ContentManager contentManager, 
            double x, double y, double movementx, double movementy, double weight, bool gravityAffected, 
            string textureName, float paralaxCoefficient=1f)
        {
            X = x;
            Y = y;

            MovementX = movementx;
            MovementY = movementy;

            Weight = weight;
            GravityAffected = gravityAffected;

            CurrentFallingSpeed = StandartFallingSpeed;

            ParalaxCoefficient = paralaxCoefficient;

            Texture = new DynamicTexture(contentManager, textureName);

            InitParams();
        }

        public virtual void Update(ContentManager contentManager, World world)
        {
            Texture.Update(contentManager);
        }

        /// <summary>
        /// Draws object sprite centered horizontally and with bottom on y
        /// </summary>
        /// <param name="x">Sprite drawing location center</param>
        /// <param name="y">Sprite drawing location bottom</param>
        /// <param name="spriteBatch"></param>
        /// <param name="color"></param>
        public virtual void Draw(int x, int y, SpriteBatch spriteBatch, float depth, float scale, 
            Color color, SpriteEffects spriteEffects)
        {
            Texture2D spriteToDraw = Texture.GetCurrentFrame();
            spriteBatch.Draw(spriteToDraw, new Vector2(x - spriteToDraw.Width*scale / 2, y - spriteToDraw.Height*scale),
                null, color, 0f, new Vector2(0, 0), scale, spriteEffects, depth + DrawingDepth);
        }

        protected virtual void InitParams()
        {
            Parameters = new TextboxList();

            Parameters.AddTextbox(new Textbox(Game1.MonospaceFont),
                new Ref(() => Texture.BaseName, x => { ChangeBaseName((string)x); }),
                x => { return x; },
                x => { return (string)x; });

            Parameters.AddTextbox(new Textbox(Game1.MonospaceFont, true),
                new Ref(() => DrawingDepth, x => { DrawingDepth = (float)x; }),
                x => { return float.Parse(x); },
                x => { return ((float)x).ToString(); });

            Parameters.AddTextbox(new Textbox(Game1.MonospaceFont, true),
                new Ref(() => StandartFallingSpeed, x => { StandartFallingSpeed = (double)x; }),
                x => { return double.Parse(x); }, x => { return ((double)x).ToString(); });
        }

        /// <summary>
        /// Adds x and y to MovementX and MovementY respectively taking weight into account
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public virtual void ChangeMovement(double x, double y)
        {
            if (Weight != 0)
            {
                MovementX += x / Weight;
                MovementY += y / Weight;
            }
        }

        /// <summary>
        /// Use this instead of adressing texture directly cos mobs have it differently
        /// </summary>
        /// <param name="newName"></param>
        public virtual void ChangeBaseName(string newName)
        {
            if (Texture.BaseName != newName)
                Texture.ChangeBaseName(newName);
        }
    }
}