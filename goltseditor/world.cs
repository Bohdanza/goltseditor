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

namespace goltseditor
{
    public class World
    {
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
        }

        /// <summary>
        /// Use this to load existing world TEMPORARILY
        /// </summary>
        /// <param name="contentManager"></param>
        /// <param name="path"></param>
        public World(string path)
        {
            if (path[path.Length - 1] != '\\')
                path += "\\";

            Path = path;

            Load();

            AvaliableObjects = new List<WorldObject>();
        }

        public void Update(ContentManager contentManager)
        {
            for (int i = 0; i < AvaliableObjects.Count; i++)
                AvaliableObjects[i].Update(contentManager, this);

            for(int i=0; i<objects.objects.Count; i++)
            {
                objects.objects[i].Update(contentManager, this);
            }

            MouseState ms = Mouse.GetState();
            KeyboardState ks = Keyboard.GetState();

            if (ms.LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed)
            {
                if (CurrentInterfaceStage == 0)
                {
                    if (ms.X <= AvObjectsXBound&&AvaliableObjects.Count>SelectedAvaliableObject)
                    {
                        WorldObject wo = AvaliableObjects[SelectedAvaliableObject];

                        WorldObject obj = Game1.Clone(AvaliableObjects[SelectedAvaliableObject]);
                        obj.X = ms.X;
                        obj.Y = ms.Y;

                        objects.AddObject(obj);
                    }
                }
            }

            if (ks.IsKeyDown(Keys.Down))
                SelectedAvaliableObject++;
            if (ks.IsKeyDown(Keys.Up))
                SelectedAvaliableObject--;

            SelectedAvaliableObject = Math.Max(SelectedAvaliableObject, 0);
            SelectedAvaliableObject = Math.Min(SelectedAvaliableObject, AvaliableObjects.Count);

            PreviousMouseState = ms;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (CurrentInterfaceStage == 0)
            {
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

                if (SelectedAvaliableObject < AvaliableObjects.Count)
                {
                    AvaliableObjects[SelectedAvaliableObject].Draw(ms.X, ms.Y, spriteBatch, 0.999f, Game1.StandardScale, Color.Red,
                        SpriteEffects.None);

                    if (AvaliableObjects[SelectedAvaliableObject] is PhysicalObject)
                        ((PhysicalObject)AvaliableObjects[SelectedAvaliableObject]).Hitbox.Draw(ms.X, ms.Y, spriteBatch, 1f, Color.White);
                }
            }
        }

        public void Save()
        {
            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);

            using (StreamWriter sw = new StreamWriter(Path + "currentroom"))
                sw.WriteLine(RoomIndex);

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