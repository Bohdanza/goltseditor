﻿using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

//I took it from another project, so style is not good
namespace goltseditor
{
    public class Button
    {
        public int x, y, width, height;
        public Texture2D NormalTexture { get; private set; }
        public Texture2D PressedTexture { get; private set; }
        public bool pressed { get; private set; }

        public int type { get; private set; }

        private SpriteFont font;
        private string text = "";
        public Color textColor = Color.Black;

        private MouseState oldState;

        /// <summary>
        /// Creates a button
        /// </summary>
        /// <param name="type">Type of button to create, eg. normal button, checkbox-like etc.</param>
        /// <param name="x">Left top corner X coord on screen</param>
        /// <param name="y">Left top corner Y coord on screen</param>
        /// <param name="width">Button hitbox width</param>
        /// <param name="height">Button hitbox height</param>
        /// <param name="normal_texture">Normal texture</param>
        /// <param name="pressed_texture">Texture when pressed</param>
        /// <param name="spriteFont">Font</param>
        /// <param name="text">Text</param>
        public Button(int type, int x, int y, int width, int height, Texture2D normal_texture, Texture2D pressed_texture, SpriteFont spriteFont, string text, Color color)
        {
            this.type = type;

            this.x = x;
            this.y = y;

            this.width = width;
            this.height = height;

            NormalTexture = normal_texture;
            PressedTexture = pressed_texture;

            this.font = spriteFont;
            this.text = text;

            this.textColor = color;
        }

        public Button(int type, int x, int y, int width, int height, Texture2D normal_texture, Texture2D pressed_texture)
        {
            this.type = type;

            this.x = x;
            this.y = y;

            this.width = width;
            this.height = height;

            NormalTexture = normal_texture;
            PressedTexture = pressed_texture;

            this.font = null;
            this.text = null;
        }

        public void Update()
        {
            var mouseState = Mouse.GetState();

            if (this.type == 0)
            {
                this.pressed = false;
            }

            if (this.type == 0)
            {
                if (mouseState.LeftButton == ButtonState.Released && this.oldState.LeftButton == ButtonState.Pressed
                    && mouseState.X >= this.x && mouseState.Y >= this.y && mouseState.X <= this.x + this.width
                    && mouseState.Y <= this.y + this.height)
                {
                    this.pressed = true;
                }
            }

            if (this.type == 1)
            {
                if (mouseState.LeftButton == ButtonState.Released && this.oldState.LeftButton == ButtonState.Pressed
                    && mouseState.X >= this.x && mouseState.Y >= this.y && mouseState.X <= this.x + this.width
                    && mouseState.Y <= this.y + this.height)
                {
                    if (this.pressed == true)
                    {
                        this.pressed = false;
                    }
                    else
                    {
                        this.pressed = true;
                    }
                }
            }

            this.oldState = mouseState;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (this.type == 0)
            {
                if (this.oldState.LeftButton == ButtonState.Pressed && this.oldState.X >= this.x && this.oldState.Y >= this.y && this.oldState.X <= this.x + this.width && this.oldState.Y <= this.y + this.height)
                {
                    spriteBatch.Draw(PressedTexture, new Vector2(this.x, this.y), null, Color.White, 0f, new Vector2(0,0),
                        Game1.StandardScale, SpriteEffects.None, 1f);
                }
                else
                {
                    spriteBatch.Draw(NormalTexture, new Vector2(this.x, this.y), null, Color.White,0f, new Vector2(0,0),
                        Game1.StandardScale, SpriteEffects.None, 1f);
                }
            }

            if (this.type == 1)
            {
                if (this.pressed)
                {
                    spriteBatch.Draw(PressedTexture, new Vector2(this.x, this.y), null, Color.White, 0f, new Vector2(0, 0),
                        Game1.StandardScale, SpriteEffects.None, 1f);
                }
                else
                {
                    spriteBatch.Draw(NormalTexture, new Vector2(this.x, this.y), null, Color.White, 0f, new Vector2(0, 0),
                        Game1.StandardScale, SpriteEffects.None, 1f);
                }
            }

            if (this.text != null && this.font != null)
            {
                Vector2 tmp_size = this.font.MeasureString(this.text);

                spriteBatch.DrawString(this.font, this.text, new Vector2(this.x + (NormalTexture.Width - (int)tmp_size.X) / 2, this.y - (NormalTexture.Height - (int)tmp_size.Y) / 2), this.textColor);
            }
        }
    }
}