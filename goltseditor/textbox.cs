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
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace goltseditor
{
    public class Textbox
    {
        public SpriteFont Font { get; private set; }
        public string Contents { get; private set; }
        public int CurrentPosition { get; private set; }
        public KeyboardState PreviousKeyboardState { get; private set; }
        public MouseState PreviousMouseState { get; private set; }

        public bool Selected { get; private set; } = false;
        private int Ctick = 0;
        public Vector2 CharDimensions { get; private set; }

        private Dictionary<Keys, string> SpecialKeys = new Dictionary<Keys, string>
        {
            {Keys.LeftShift, ""}, {Keys.RightShift, ""}, {Keys.LeftControl, ""}, {Keys.RightControl, ""},
            {Keys.Space, " "}, {Keys.D0, "0"}, {Keys.D1, "1"}, {Keys.D2, "2"},
            {Keys.D3, "3"}, {Keys.D4, "4" }, {Keys.D5, "5"}, {Keys.D6, "6"},
            {Keys.D7, "7"}, {Keys.D8, "8"}, {Keys.D9, "9"}, {Keys.Down, ""},
            {Keys.Up, ""}, {Keys.Tab, "\t"}, {Keys.Multiply, "*"}, {Keys.OemMinus, "-"}, 
        };
        private Dictionary<Keys, string> SpecialShiftKeys = new Dictionary<Keys, string>
        {
            {Keys.LeftShift, ""}, {Keys.RightShift, ""}, {Keys.LeftControl, ""}, {Keys.RightControl, ""},
            {Keys.OemMinus, "_"}
        };

        public Textbox(SpriteFont spriteFont)
        {
            Font = spriteFont;
            CurrentPosition = 0;
            Contents = "";

            PreviousKeyboardState = Keyboard.GetState();
            PreviousMouseState = Mouse.GetState();

            CharDimensions = Font.MeasureString("A");
        }

        public void Update(int x, int y, int width, int height)
        {
            var ms = Mouse.GetState();
            var ks = Keyboard.GetState();

            if(ms.LeftButton==ButtonState.Released&&PreviousMouseState.LeftButton==ButtonState.Pressed)
            {
                if (ms.X >= x && ms.X < x + width && ms.Y >= y && ms.Y < y + height)
                {
                    Selected = true;
                    CurrentPosition = (int)((ms.X - x) / CharDimensions.X);
                }
                else
                {
                    Selected = false;
                    Ctick = 30;
                }
            }

            if(Selected)
            {
                Keys[] cpressed = ks.GetPressedKeys();
                Keys[] ppressed = PreviousKeyboardState.GetPressedKeys();
            
                foreach(var currentKey in ppressed)
                    if(!ks.IsKeyDown(currentKey))
                    {
                        string gval = "";

                        if (currentKey == Keys.Back)
                        {
                            if(Contents.Length>0&&CurrentPosition>=0&&CurrentPosition<Contents.Length)
                                Contents = Contents.Remove(CurrentPosition, 1);
                            
                            CurrentPosition--;
                        }

                        if (ks.IsKeyDown(Keys.LeftShift) || ks.IsKeyDown(Keys.RightShift))
                        {
                            if (SpecialShiftKeys.TryGetValue(currentKey, out gval))
                                Contents = Contents.Insert(CurrentPosition, gval);
                            else
                                Contents = Contents.Insert(CurrentPosition, currentKey.ToString());
                        }
                        else
                        {
                            string cstr = currentKey.ToString();
                            StringBuilder cst = new StringBuilder(cstr);

                            if (cst.Length == 1 && cst[0] >= 'A' && cst[0] <= 'Z')
                            {
                                cst[0] += (char)32;
                                cstr = cst.ToString();
                            }

                            if (SpecialKeys.TryGetValue(currentKey, out gval))
                                Contents = Contents.Insert(CurrentPosition, gval);
                            else
                                Contents = Contents.Insert(CurrentPosition, cstr);
                        }

                        CurrentPosition++;
                    }

                Ctick++;
                Ctick %= 30;
            }

            PreviousKeyboardState = ks;
            PreviousMouseState = ms;
        }

        public void Draw(SpriteBatch spriteBatch, int x, int y, Color mainColor, Color cursorColor, float depth)
        {
            spriteBatch.DrawString(Font, Contents, new Vector2(x, y), mainColor, 0f, new Vector2(0,0), 1f, SpriteEffects.None, depth);

            if (Ctick < 15)
                spriteBatch.Draw(Game1.OnePixel, new Vector2(x + CurrentPosition * CharDimensions.X, y), null,
                    cursorColor, 0f, new Vector2(0, 0), CharDimensions, SpriteEffects.None, depth - 0.0001f);
        }
    }
}