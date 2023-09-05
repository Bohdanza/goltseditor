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
    public class BlackenedObstacle : PhysicalObject
    {
        [JsonProperty]
        public int TextureWidth { get; protected set; }
        [JsonProperty]
        public int TextureHeight { get; protected set; }
        [JsonProperty]
        public Color TextureColor { get; protected set; }

        [JsonConstructor]
        public BlackenedObstacle() : base()
        { }

        public BlackenedObstacle(ContentManager contentManager,
            double x, double y, double movementx, double movementy, double weight, bool gravityAffected,
            string textureName, List<Tuple<double, double>> hitbox, int textureHeight, int textureWidth, int collisionLayer = 0)
            : base(contentManager, x, y, movementx, movementy, weight, gravityAffected, textureName, hitbox, collisionLayer)
        {
            TextureWidth = textureWidth;
            TextureHeight = textureHeight;

            TextureColor = Color.Black;
        }

        protected override void InitParams()
        {
            base.InitParams();

            Parameters.AddTextbox(new Textbox(Game1.MonospaceFont, true, false),
                new Ref(() => TextureWidth, x => { TextureWidth = (int)x; }),
                x => { return int.Parse(x); }, x => { return ((int)x).ToString(); });

            Parameters.AddTextbox(new Textbox(Game1.MonospaceFont, true, false),
                new Ref(() => TextureHeight, x => { TextureHeight = (int)x; }),
                x => { return int.Parse(x); }, x => { return ((int)x).ToString(); });
        }

        public override void Draw(int x, int y, SpriteBatch spriteBatch, float depth, float scale,
            Color color, SpriteEffects spriteEffects)
        {
            int x1 = x;
            int y1 = y;

            Texture2D spriteToDraw = Game1.OnePixel;
            spriteBatch.Draw(spriteToDraw, new Vector2(x1 - spriteToDraw.Width * scale / 2, y1 - spriteToDraw.Height * scale),
                null, TextureColor, 0f, new Vector2(0, 0), new Vector2(TextureWidth * scale, TextureHeight * scale),
                spriteEffects, depth + DrawingDepth);
        }
    }
}