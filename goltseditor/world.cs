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
using Newtonsoft.Json;

using Color = Microsoft.Xna.Framework.Color;
using System.Diagnostics.SymbolStore;

namespace goltseditor
{
    public class World
    {
        public Button btn { get; private set; } = null;
        private  List<Button> ObjectButtons { get; set; }
        public WorldObject CurrentlyCreatedObject { get; protected set; } = null;
        //1920*3
        public const int MaxLoadedSize = 5760;

        public string Path { get; private set; }
        public string Name { get; private set; }

        [JsonProperty]
        public int RoomIndex { get; private set; } = 0;

        public ObjectList objects { get; private set; }
        public List<WorldObject> AvaliableObjects { get; protected set; }

        //why not
        public WorldObject Camera { get; private set; }

        private bool HitboxesShown = true;

        public int SelectedAvaliableObject { get; protected set; } = 1;
        public int CurrentInterfaceStage { get; protected set; }

        private MouseState PreviousMouseState;
        
        private SpriteFont MainFont;
        private int AvaliableObjectsOffsetY = 0, AvObjectsXBound=1625, CurrentlySelectedNumber=0, LastObjectChange=0;
        private Textbox CreatedTextureName, SelectedDrawingDepth, SelectedParalaxCoefficient;

        //Later these init methods shall be made one for the good code style rejoice.
        //It should automatically check for saves and load or create new depending on found ones

        /// <summary>
        /// Use this to init new world TEMPORARILY
        /// </summary>
        /// <param name="contentManager"></param>
        /// <param name="path"></param>
        public World(ContentManager contentManager, string path)
        {
            Camera = new Camera(contentManager, 0, 0, 0, 0, 1);

            PreviousMouseState = Mouse.GetState();

            if (path[path.Length - 1] != '\\')
                path += "\\";

            Path = path;

            objects = new ObjectList(MaxLoadedSize);
            AvaliableObjects = new List<WorldObject>();

            AvaliableObjects.Add(new Hero(contentManager, 800, 300, 0, 0));
            AvaliableObjects.Add(new Obstacle(contentManager, 0, 0, "goltsov_",
                new List<Tuple<double, double>>
                {
                    new Tuple<double, double>(-200, 0),
                    new Tuple<double, double>(200, 0),
                    new Tuple<double, double>(200, -20),
                    new Tuple<double, double>(-200, -20),
                }));

            MainFont = contentManager.Load<SpriteFont>("mainfont");

            ObjectButtonsInit(contentManager);
            CreatedTextureName = new Textbox(MainFont);
            SelectedDrawingDepth = new Textbox(MainFont, true);
            SelectedParalaxCoefficient = new Textbox(MainFont, true);
        }

        /// <summary>
        /// Use this to load existing world TEMPORARILY
        /// </summary>
        /// <param name="contentManager"></param>
        /// <param name="path"></param>
        public World(string path, ContentManager contentManager)
        {
            Camera = new Camera(contentManager, 0, 0, 0, 0, 1);

            if (path[path.Length - 1] != '\\')
                path += "\\";

            Path = path;

            Load();

            MainFont = contentManager.Load<SpriteFont>("mainfont");

            ObjectButtonsInit(contentManager);
            CreatedTextureName = new Textbox(MainFont);
            SelectedDrawingDepth = new Textbox(MainFont, true);
            SelectedParalaxCoefficient = new Textbox(MainFont, true);
        }

        public void Update(ContentManager contentManager)
        {
            if (btn == null)
                btn = new Button(0, AvObjectsXBound, 1000, 280, 108,
                    contentManager.Load<Texture2D>("newreleased"), contentManager.Load<Texture2D>("newpressed"));

            for (int i = 0; i < AvaliableObjects.Count; i++)
                AvaliableObjects[i].Update(contentManager, this);

            for (int i = 0; i < objects.objects.Count; i++)
            {
                objects.objects[i].Update(contentManager, this);
            }

            MouseState ms = Mouse.GetState();
            KeyboardState ks = Keyboard.GetState();

            if (CurrentInterfaceStage == 0)
            {
                SelectedDrawingDepth.Update(0, 0,
                    (int)((SelectedDrawingDepth.Contents.Length + 1) * SelectedDrawingDepth.CharDimensions.X),
                    (int)SelectedDrawingDepth.CharDimensions.Y);

                SelectedParalaxCoefficient.Update(0, (int)SelectedDrawingDepth.CharDimensions.Y+5,
                    (int)((SelectedParalaxCoefficient.Contents.Length + 1) * SelectedParalaxCoefficient.CharDimensions.X), 
                    (int)SelectedParalaxCoefficient.CharDimensions.Y);

                if(CurrentlySelectedNumber<objects.objects.Count)
                {
                    WorldObject wo = objects.objects[CurrentlySelectedNumber];
                    float.TryParse(SelectedDrawingDepth.Contents, out wo.DrawingDepth);
                    float.TryParse(SelectedParalaxCoefficient.Contents, out wo.ParalaxCoefficient);
                }

                if (!SelectedDrawingDepth.Selected && !SelectedParalaxCoefficient.Selected &&
                    ms.LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed)
                {
                    if (ms.X <= AvObjectsXBound && AvaliableObjects.Count > SelectedAvaliableObject)
                    {
                        WorldObject wo = AvaliableObjects[SelectedAvaliableObject];

                        WorldObject obj = Game1.Clone(AvaliableObjects[SelectedAvaliableObject]);
                        obj.X = ms.X;

                        obj.Y = ms.Y;

                        objects.AddObject(obj);
                    }
                }

                if (ms.RightButton == ButtonState.Released && PreviousMouseState.RightButton == ButtonState.Pressed)
                {
                    int x1 = ms.X + (int)Camera.X;
                    int y1 = ms.Y + (int)Camera.Y;
                    
                    //TODO: object selection with mouse
                }
                
                LastObjectChange++;

                if (LastObjectChange >= 7)
                {
                    bool changed = false;

                    if (ks.IsKeyDown(Keys.Down))
                    {
                        SelectedAvaliableObject++;
                        changed = true;
                    }
                    if (ks.IsKeyDown(Keys.Up))
                    {
                        SelectedAvaliableObject--;
                        changed = true;
                    }

                    if (ks.IsKeyDown(Keys.C))
                    {
                        CurrentlySelectedNumber++;
                        changed = true;
                    }
                    if (ks.IsKeyDown(Keys.X))
                    {
                        CurrentlySelectedNumber--;
                        changed = true;
                    }

                    if (changed)
                    {
                        LastObjectChange = 0;

                        if (AvaliableObjects.Count != 0)
                            SelectedAvaliableObject = (SelectedAvaliableObject + AvaliableObjects.Count) % AvaliableObjects.Count;

                        if (objects.objects.Count != 0)
                        {
                            CurrentlySelectedNumber = (CurrentlySelectedNumber + objects.objects.Count) % objects.objects.Count;

                            SelectedDrawingDepth = new Textbox(MainFont, true);
                            SelectedParalaxCoefficient = new Textbox(MainFont, true);
                            SelectedDrawingDepth.Contents = objects.objects[CurrentlySelectedNumber].DrawingDepth.ToString();
                            SelectedParalaxCoefficient.Contents = objects.objects[CurrentlySelectedNumber].ParalaxCoefficient.ToString();
                        }
                    }
                }

                btn.Update();

                if (btn.pressed)
                    CurrentInterfaceStage = 1;
            }
            else if(CurrentInterfaceStage==1)
            {
                for (int i = 0; i < ObjectButtons.Count; i++)
                {
                    ObjectButtons[i].Update();
                    if (ObjectButtons[i].pressed)
                    {
                        CurrentInterfaceStage = 2;
                        CreatedObjectInit(contentManager, i);
                    }
                }
            }
            else if(CurrentInterfaceStage==2)
            {
                CurrentlyCreatedObject.Update(contentManager, this);

                CreatedTextureName.Update(10, 10,
                    (int)CreatedTextureName.CharDimensions.X * (CreatedTextureName.Contents.Length+1),
                    (int)CreatedTextureName.CharDimensions.Y);

                CurrentlyCreatedObject.ChangeBaseName(contentManager, CreatedTextureName.Contents);

                if (CurrentlyCreatedObject is PhysicalObject 
                    && ms.LeftButton==ButtonState.Released && PreviousMouseState.LeftButton==ButtonState.Pressed&&
                    !CreatedTextureName.Selected)
                {
                    ((PhysicalObject)CurrentlyCreatedObject).Hitbox.HitboxPoints.Add(new Tuple<double, double>(ms.X - 960, ms.Y - 540));
                }

                if(ks.IsKeyDown(Keys.Enter))
                {
                    AvaliableObjects.Add(CurrentlyCreatedObject);
                    CurrentInterfaceStage = 0;
                }
            }

            PreviousMouseState = ms;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (CurrentInterfaceStage == 0)
            {
                btn.Draw(spriteBatch);

                spriteBatch.Draw(Game1.OnePixel, new Vector2(0, 0), null, Game1.BackgroundColor, 0f, new Vector2(0, 0),
                    new Vector2(AvObjectsXBound, 1080), SpriteEffects.None, 0.10001f);

                spriteBatch.Draw(Game1.OnePixel, new Vector2(AvObjectsXBound, 0), null, Game1.BorderColor, 0f, new Vector2(0, 0),
                    new Vector2(2, 1080), SpriteEffects.None, 1f);

                if (AvaliableObjects.Count > 0)
                {
                    int yq = 10 + AvaliableObjectsOffsetY +
                        (int)(AvaliableObjects[SelectedAvaliableObject].Texture.GetCurrentFrame().Height * Game1.StandardScale);

                    foreach (var cobj in AvaliableObjects) 
                    {
                        cobj.Draw(AvObjectsXBound + 960 - AvObjectsXBound / 2, yq, spriteBatch,
                            0.1f, Game1.StandardScale, Color.White, SpriteEffects.None);

                        yq += 5 + (int)(cobj.Texture.GetCurrentFrame().Height * Game1.StandardScale);
                    }
                }

                SelectedDrawingDepth.Draw(spriteBatch, 0, 0, Color.White, Color.Black, 1.0f);
                SelectedParalaxCoefficient.Draw(spriteBatch, 0, (int)SelectedDrawingDepth.CharDimensions.Y+5, 
                    Color.White, Color.Black, 1.0f);

                foreach (var currentObject in objects.objects)
                {
                    currentObject.Draw((int)currentObject.X, (int)currentObject.Y, spriteBatch, 0.5f, Game1.StandardScale, Color.White, SpriteEffects.None);

                    if (HitboxesShown && currentObject is PhysicalObject)
                        ((PhysicalObject)currentObject).Hitbox.Draw((int)currentObject.X, (int)currentObject.Y,
                            spriteBatch, 0.5f, Color.White);
                }

                if (CurrentlySelectedNumber < objects.objects.Count)
                {
                    WorldObject wo = objects.objects[CurrentlySelectedNumber];

                    wo.Draw((int)wo.X, (int)wo.Y, spriteBatch, 0.6f, Game1.StandardScale, Color.Green, SpriteEffects.None);
                }

                var ms = PreviousMouseState;

                if (SelectedAvaliableObject < AvaliableObjects.Count&&ms.X<AvObjectsXBound)
                {
                    AvaliableObjects[SelectedAvaliableObject].Draw(ms.X, ms.Y, spriteBatch, 0.999f, Game1.StandardScale, Color.Red,
                        SpriteEffects.None);

                    if (AvaliableObjects[SelectedAvaliableObject] is PhysicalObject)
                        ((PhysicalObject)AvaliableObjects[SelectedAvaliableObject]).Hitbox.Draw(ms.X, ms.Y, spriteBatch, 1f, Color.White);
                }
            }
            else if(CurrentInterfaceStage==1)
            {
                for (int i = 0; i < ObjectButtons.Count; i++)
                    ObjectButtons[i].Draw(spriteBatch);
            }
            else if(CurrentInterfaceStage==2)
            {
                CreatedTextureName.Draw(spriteBatch, 10, 10, Color.White, Color.Black, 1f);

                CurrentlyCreatedObject.Draw(960, 540, spriteBatch, 0.9f, Game1.StandardScale, Color.White, SpriteEffects.None);

                if (CurrentlyCreatedObject is PhysicalObject)
                {
                    PhysicalObject ps = (PhysicalObject)CurrentlyCreatedObject;

                    ps.Hitbox.Draw(960, 540, spriteBatch, 1f, Color.White);

                    if (ps.Hitbox.HitboxPoints.Count > 0)
                    {
                        int ls = ps.Hitbox.HitboxPoints.Count - 1;
                        Tuple<double, double> lq = new Tuple<double, double>(ps.Hitbox.HitboxPoints[ls].Item1 + 960, ps.Hitbox.HitboxPoints[ls].Item2 + 540);
                        double rot = Game1.GetDirection(new Tuple<double, double>(PreviousMouseState.X, PreviousMouseState.Y),
                            lq);
                        double scale = Game1.GetDistance(lq, new Tuple<double, double>(PreviousMouseState.X, PreviousMouseState.Y));
                        
                        spriteBatch.Draw(Game1.OnePixel,
                            new Vector2(960 + (int)ps.Hitbox.HitboxPoints[ls].Item1, 540 + (int)ps.Hitbox.HitboxPoints[ls].Item2),
                            null, Color.Red, (float)rot, new Vector2(0, 0), new Vector2((float)scale, 2), SpriteEffects.None, 1f);

                        ls = 0;

                        lq = new Tuple<double, double>(ps.Hitbox.HitboxPoints[ls].Item1 + 960, ps.Hitbox.HitboxPoints[ls].Item2 + 540);
                        rot = Game1.GetDirection(new Tuple<double, double>(PreviousMouseState.X, PreviousMouseState.Y),
                            lq);
                        scale = Game1.GetDistance(lq, new Tuple<double, double>(PreviousMouseState.X, PreviousMouseState.Y));

                        spriteBatch.Draw(Game1.OnePixel,
                            new Vector2(960 + (int)ps.Hitbox.HitboxPoints[ls].Item1, 540 + (int)ps.Hitbox.HitboxPoints[ls].Item2),
                            null, Color.Red, (float)rot, new Vector2(0, 0), new Vector2((float)scale, 2), SpriteEffects.None, 1f);

                    }
                }
            }
        }

        private void ObjectButtonsInit(ContentManager contentManager)
        {
            ObjectButtons = new List<Button>();

            ObjectButtons.Add(new Button(0, 10, 10, 160, 160,
                 contentManager.Load<Texture2D>("physicalcreatereleased"), contentManager.Load<Texture2D>("physicalcreatepressed")));
        }   

        private void CreatedObjectInit(ContentManager contentManager, int index)
        {
            if (index == 0)
                CurrentlyCreatedObject = new Obstacle(contentManager, 0, 0, "", new List<Tuple<double, double>>());
        }

        public void Save()
        {
            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);

            using (StreamWriter sw = new StreamWriter(Path + "currentroom"))
                sw.WriteLine(RoomIndex);


            var jss = new JsonSerializerSettings();
            jss.TypeNameHandling = TypeNameHandling.All;
            string st = JsonConvert.SerializeObject(AvaliableObjects, jss);

            using (StreamWriter sw = new StreamWriter(Path + "avaliable"))
                sw.WriteLine(st);

            SaveRoom();
        }

        private void SaveRoom()
        {
            var jss = new JsonSerializerSettings();
            jss.TypeNameHandling = TypeNameHandling.Objects;
            string st = JsonConvert.SerializeObject(objects, jss);

            using (StreamWriter sw=new StreamWriter(Path+RoomIndex.ToString()+"ed"))
                sw.Write(st);

            st=st.Replace("goltseditor", "golts");

            using (StreamWriter sw = new StreamWriter(Path + RoomIndex.ToString()))
                sw.Write(st);
        }

        private void Load()
        {
            using (StreamReader sr = new StreamReader(Path + "currentroom"))
                RoomIndex = int.Parse(sr.ReadLine());

            var jss = new JsonSerializerSettings();
            jss.TypeNameHandling = TypeNameHandling.All;
            string str = "";

            using (StreamReader sw = new StreamReader(Path + "avaliable"))
                str = sw.ReadToEnd();

            var q = JsonConvert.DeserializeObject(str, jss);
            AvaliableObjects = (List<WorldObject>)q;

            LoadRoom(RoomIndex);
        }

        private void LoadRoom(int index)
        {
            var jss = new JsonSerializerSettings();
            jss.TypeNameHandling = TypeNameHandling.Objects;

            using (StreamReader sr = new StreamReader(Path + index.ToString()+"ed"))
            {
                objects = (ObjectList)JsonConvert.DeserializeObject(sr.ReadToEnd(), jss);
            }
        }
    }
}