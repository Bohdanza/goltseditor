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
    /// <summary>
    /// I have no fucking idea if it works
    /// </summary>
    public class ObjectHitbox
    {
        [JsonProperty]
        public List<Tuple<double, double>> HitboxPoints { get; protected set; }
        [JsonProperty]
        public double MinX { get; private set; }
        [JsonProperty]
        public double MinY { get; private set; }
        [JsonProperty]
        public double MaxX { get; private set; }
        [JsonProperty]
        public double MaxY { get; private set; }

        [JsonConstructor]
        public ObjectHitbox() { }

        public ObjectHitbox(string path)
        {
            HitboxPoints = new List<Tuple<double, double>>();
            int[] lst = new int[0];

            using (StreamReader sr = new StreamReader(path))
            {
                lst = Array.ConvertAll(sr.ReadToEnd().Split(' '), int.Parse);
            }

            MinX = 10000000;
            MinY = 10000000;
            MaxX = -10000000;
            MaxY = -10000000;

            for (int i = 0; i < lst.Length; i += 2)
            {
                HitboxPoints.Add(new Tuple<double, double>(lst[i], lst[i + 1]));

                MinX = Math.Min(MinX, lst[i]);
                MinY = Math.Min(MinY, lst[i+1]);
                MaxX = Math.Max(MaxX, lst[i]);
                MaxY = Math.Max(MaxY, lst[i+1]);
            }
        }

        public ObjectHitbox(List<Tuple<double, double>> points)
        {
            MinX = 10000000;
            MinY = 10000000;
            MaxX = -10000000;
            MaxY = -10000000;

            HitboxPoints = points;

            for (int i = 0; i < HitboxPoints.Count; i++)
            {
                MinX = Math.Min(MinX, HitboxPoints[i].Item1);
                MinY = Math.Min(MinY, HitboxPoints[i].Item2);
                MaxX = Math.Max(MaxX, HitboxPoints[i].Item1);
                MaxY = Math.Max(MaxY, HitboxPoints[i].Item2);
            }
        }

        /// <summary>
        /// Checks if this hitbox intersects with another
        /// </summary>
        /// <param name="hitbox">Hitbox provided</param>
        /// <param name="x1">Global X offset of THIS hitbox</param>
        /// <param name="y1">Global Y offset of THIS hitbox</param>
        /// <param name="x2">Global X offset of PROVIDED hitbox</param>
        /// <param name="y2">Global Y offset of PROVIDED hitbox</param>
        /// <returns></returns>
        public bool CollidesWith(ObjectHitbox hitbox, double x1, double y1, double x2, double y2)
        {
            for (int i = 0; i < HitboxPoints.Count; i++)
            {
                Tuple<double, double> v1 = HitboxPoints[i];
                Tuple<double, double> v2 = HitboxPoints[(i + 1) % HitboxPoints.Count];

                Tuple<double, double> minmax1 = MinMaxPos(Game1.GetDirection(v2, v1) + Math.PI*0.5, this, x1, y1);
                Tuple<double, double> minmax2 = MinMaxPos(Game1.GetDirection(v2, v1) + Math.PI * 0.5, hitbox, x2, y2);

                if (!((minmax2.Item1 < minmax1.Item1 && minmax1.Item1 < minmax2.Item2 && minmax2.Item2 < minmax1.Item2)
                    || (minmax1.Item1 < minmax2.Item1 && minmax2.Item1 < minmax1.Item2 && minmax1.Item2 < minmax2.Item2)
                    || (minmax1.Item1 < minmax2.Item1 && minmax2.Item2 < minmax1.Item2)
                    || (minmax2.Item1 < minmax1.Item1 && minmax1.Item2 < minmax2.Item2)))
                    return false;
            }

            for (int i = 0; i < hitbox.HitboxPoints.Count; i++)
            {
                Tuple<double, double> v1 = hitbox.HitboxPoints[i];
                Tuple<double, double> v2 = hitbox.HitboxPoints[(i + 1) % HitboxPoints.Count];

                Tuple<double, double> minmax1 = MinMaxPos(Game1.GetDirection(v2, v1) + Math.PI * 0.5, this, x1, y1);
                Tuple<double, double> minmax2 = MinMaxPos(Game1.GetDirection(v2, v1) + Math.PI * 0.5, hitbox, x2, y2);

                if (!((minmax2.Item1 < minmax1.Item1 && minmax1.Item1 < minmax2.Item2 && minmax2.Item2 < minmax1.Item2)
                    || (minmax1.Item1 < minmax2.Item1 && minmax2.Item1 < minmax1.Item2 && minmax1.Item2 < minmax2.Item2)
                    || (minmax1.Item1 < minmax2.Item1 && minmax2.Item2 < minmax1.Item2)
                    || (minmax2.Item1 < minmax1.Item1 && minmax1.Item2 < minmax2.Item2)))
                    return false;
            }

            return true;
        }
        
        private Tuple<double, double> MinMaxPos(double direction, ObjectHitbox objectHitbox, double x, double y)
        {
            double maxdist = -1e9d, mindist = 1e9d;

            for (int i = 0; i < objectHitbox.HitboxPoints.Count; i++)
            {
                Tuple<double, double> htb = new Tuple<double, double>(objectHitbox.HitboxPoints[i].Item1 + x,
                    objectHitbox.HitboxPoints[i].Item2 + y);

                double dist = Math.Cos(direction -
                    Game1.GetDirection(new Tuple<double, double>(0, 0), htb)) *
                    Game1.GetDistance(new Tuple<double, double>(0, 0), htb);

                maxdist = Math.Max(maxdist, dist);
                mindist = Math.Min(mindist, dist);
            }
            
            return new Tuple<double, double>(mindist, maxdist);
        }

        public void Draw(int x, int y, SpriteBatch spriteBatch, float depth, Color color)
        {
            for (int i = 0; i < HitboxPoints.Count; i++)
            {
                Tuple<double, double> t2 = HitboxPoints[(i + 1) % HitboxPoints.Count];

                double rot = Game1.GetDirection(t2, HitboxPoints[i]);
                double scale = Game1.GetDistance(HitboxPoints[i], t2);

                spriteBatch.Draw(Game1.OnePixel,
                    new Vector2(x + (int)HitboxPoints[i].Item1, y + (int)HitboxPoints[i].Item2),
                    null, color, (float)rot, new Vector2(0, 0), new Vector2((float)scale, 2), SpriteEffects.None, depth);
            }
        }

        public void AddPoint(double x, double y)
        {
            HitboxPoints.Add(new Tuple<double, double>(x, y));

            MinX = Math.Min(MinX, x);
            MinY = Math.Min(MinY, y);
            MaxX = Math.Max(MaxX, x);
            MaxY = Math.Max(MaxY, y);
        }
    }
}