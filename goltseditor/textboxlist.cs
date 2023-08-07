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
    public class TextboxList
    {
        private List<AutoTextboxTuple> Textboxes = new List<AutoTextboxTuple>();
        public int TotalHeight { get; private set; } = 0;
        public int DelayY { get; private set; } = 0;

        public TextboxList(int delayY=5) 
        {
            DelayY = delayY;
        }

        public void AddTextbox(Textbox textbox, Ref assosiatedObject, 
            Func<string, Object> converter, Func<Object, string> reversedConverter)
        {
            Textboxes.Add(new AutoTextboxTuple(textbox, assosiatedObject, converter, reversedConverter));
            TotalHeight += (int)textbox.CharDimensions.Y;
        }

        public void Update(ContentManager contentManager, int x, int y)
        {
            int ch = 0;
            foreach (var currentItem in Textboxes)
            {
                currentItem.Update(x, y + ch);
                ch += (int)currentItem.Textbox.CharDimensions.Y + DelayY;
            }
        }

        public void Draw(SpriteBatch spriteBatch, int x, int y, Color mainColor, Color fontColor, float depth)
        {
            int ch = 0;
            foreach (var currentItem in Textboxes)
            {
                currentItem.Textbox.Draw(spriteBatch, x, y + ch, mainColor, fontColor, depth);

                ch += (int)currentItem.Textbox.CharDimensions.Y + DelayY;
            }
        }
    }

    public class AutoTextboxTuple
    {
        public Textbox Textbox { get; private set; }
        public Ref Reference { get; private set; }
        public Func<string, Object> Converter { get; private set; }
        public Func<Object, string> ReversedConverter { get; private set; }

        public AutoTextboxTuple(Textbox textbox, Ref reference, 
            Func<string, Object> converter, Func<object, string> reversedConverter)
        {
            Textbox = textbox;
            Reference = reference;
            Converter = converter;
            ReversedConverter = reversedConverter;
        }

        public void Update(int x, int y)
        {
            Textbox.Contents = ReversedConverter(Reference.Value);

            Textbox.Update(x, y,
                (int)((Textbox.Contents.Length+1) * Textbox.CharDimensions.X), (int)Textbox.CharDimensions.Y);

            Reference.Value = Converter(Textbox.Contents);
        }
    }

    public class Ref
    {
        private readonly Func<Object> getter;
        private readonly Action<Object> setter;
        public Ref(Func<Object> getter, Action<Object> setter)
        {
            this.getter = getter;
            this.setter = setter;
        }
        public Object Value { get { return getter(); } set { setter(value); } }
    }
}