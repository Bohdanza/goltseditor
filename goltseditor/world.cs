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
using System.Drawing;

using Color = Microsoft.Xna.Framework.Color;

namespace goltseditor
{
    public class World
    {
        private int TimeSinceTextureChange = 11, currentTextureNumber=0;

        public List<string> textureNames = new List<string>
        {
            "goltsov_",
        };

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

        private int AvaliableObjectsOffsetY = 0, AvObjectsXBound=1625;

        //Later these init methods shall be made one for the good code style rejoice.
        //It should automatically check for saves and load or create new depending on found ones

        /// <summary>
        /// Use this to init new world TEMPORARILY
        /// </summary>
        /// <param name="contentManager"></param>
        /// <param name="path"></param>
        public World(ContentManager contentManager, string path)
        {
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

            ObjectButtonsInit(contentManager);
        }

        /// <summary>
        /// Use this to load existing world TEMPORARILY
        /// </summary>
        /// <param name="contentManager"></param>
        /// <param name="path"></param>
        public World(string path, ContentManager contentManager)
        {
            if (path[path.Length - 1] != '\\')
                path += "\\";

            Path = path;

            Load();
            ObjectButtonsInit(contentManager);
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
                if (ms.LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed)
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

                if (ks.IsKeyDown(Keys.Down))
                    SelectedAvaliableObject++;
                if (ks.IsKeyDown(Keys.Up))
                    SelectedAvaliableObject--;

                SelectedAvaliableObject = Math.Max(SelectedAvaliableObject, 0);
                SelectedAvaliableObject = Math.Min(SelectedAvaliableObject, AvaliableObjects.Count - 1);

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

                TimeSinceTextureChange++;

                if(TimeSinceTextureChange>10)
                {
                    if(ks.IsKeyDown(Keys.Left))
                    {
                        TimeSinceTextureChange = 0;
                        currentTextureNumber--;
                        currentTextureNumber += textureNames.Count;
                        currentTextureNumber %= textureNames.Count;
                    }

                    if (ks.IsKeyDown(Keys.Right))
                    {
                        TimeSinceTextureChange = 0;
                        currentTextureNumber++;
                        currentTextureNumber %= textureNames.Count;
                    }
                }

                if(CurrentlyCreatedObject is PhysicalObject 
                    && ms.LeftButton==ButtonState.Released && PreviousMouseState.LeftButton==ButtonState.Pressed)
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
                    new Vector2(AvObjectsXBound, 1080), SpriteEffects.None, 0.49f);

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

                foreach (var currentObject in objects.objects)
                {
                    currentObject.Draw((int)currentObject.X, (int)currentObject.Y, spriteBatch, 0.5f, Game1.StandardScale, Color.White, SpriteEffects.None);

                    if (HitboxesShown && currentObject is PhysicalObject)
                        ((PhysicalObject)currentObject).Hitbox.Draw((int)currentObject.X, (int)currentObject.Y,
                            spriteBatch, 0f, Color.White);
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