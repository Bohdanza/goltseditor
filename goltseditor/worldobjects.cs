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
using System.Text.Json.Serialization;
using System.Diagnostics.CodeAnalysis;

namespace goltseditor
{
    public class ObjectList
    {
        public const int GridCellSize = 120;

        [JsonProperty]
        public List<WorldObject> objects { get; private set; }
        [Newtonsoft.Json.JsonIgnore]
        public List<PhysicalObject>[,] ObjectGrid { get; private set; }
        [JsonProperty]
        public int GridSize { get; private set; }

        [Newtonsoft.Json.JsonConstructor]
        public ObjectList(List<WorldObject> objects, int GridSize) 
        {
            this.GridSize = GridSize;
            this.objects = objects;
            ObjectGrid = new List<PhysicalObject>[GridSize, GridSize];

            for (int i = 0; i < GridSize; i++)
                for (int j = 0; j < GridSize; j++)
                    ObjectGrid[i, j] = new List<PhysicalObject>();

            foreach(var currentObject in this.objects)
            {
                if(currentObject is PhysicalObject)
                    AddToGrid((PhysicalObject)currentObject);
            }
        }

        /// <summary>
        /// Init normally
        /// </summary>
        /// <param name="worldSize">Size of the LOADED world. ]
        /// </param>
        public ObjectList(int worldSize)
        {
            GridSize = worldSize / GridCellSize;

            objects = new List<WorldObject>();
            ObjectGrid=new List<PhysicalObject>[GridSize, GridSize];

            for (int i = 0; i < GridSize; i++)
                for (int j = 0; j < GridSize; j++)
                    ObjectGrid[i, j] = new List<PhysicalObject>();
        }

        public void AddObject(WorldObject worldObject)
        {
            objects.Add(worldObject);
            
            if(worldObject is PhysicalObject)
                AddToGrid((PhysicalObject)worldObject);
        }

        public void DeleteObject(WorldObject worldObject)
        {
            objects.Remove(worldObject);

            if (worldObject is PhysicalObject)
                DeleteFromGrid((PhysicalObject)worldObject);
        }

        private void AddToGrid(PhysicalObject po)
        {
            double xBegin = Math.Max(0, po.X + po.Hitbox.MinX - GridCellSize);
            double xEnd = Math.Min(GridSize * GridCellSize, po.X + po.Hitbox.MaxX + GridCellSize);
            double yBegin = Math.Max(0, po.Y + po.Hitbox.MinY - GridCellSize);
            double yEnd = Math.Min(GridSize * GridCellSize, po.Y + po.Hitbox.MaxY + GridCellSize);

            for (double i = xBegin; i < xEnd; i += GridCellSize)
                for (double j = yBegin; j < yEnd; j += GridCellSize)
                    ObjectGrid[(int)(i / GridCellSize), (int)(j / GridCellSize)].Add(po);
        }

        private void DeleteFromGrid(PhysicalObject po)
        {
            double xBegin = Math.Max(0, po.X + po.Hitbox.MinX - GridCellSize);
            double xEnd = Math.Min(GridSize * GridCellSize, po.X + po.Hitbox.MaxX + GridCellSize);
            double yBegin = Math.Max(0, po.Y + po.Hitbox.MinY - GridCellSize);
            double yEnd = Math.Min(GridSize * GridCellSize, po.Y + po.Hitbox.MaxY + GridCellSize);

            for (double i = xBegin; i < xEnd; i += GridCellSize)
                for (double j = yBegin; j < yEnd; j += GridCellSize)
                    ObjectGrid[(int)(i / GridCellSize), (int)(j / GridCellSize)].Remove(po);
        }

        public void UpdateObjectPosition(PhysicalObject physicalObject, double previousX, double previousY)
        {
            if ((int)(physicalObject.X / GridCellSize) != (int)(previousX / GridCellSize)|| 
                (int)(physicalObject.Y / GridCellSize) != (int)(previousY / GridCellSize))
            {
                double xBegin = Math.Max(0, previousX + physicalObject.Hitbox.MinX-GridCellSize);
                double xEnd = Math.Min(GridSize * GridCellSize, previousX + physicalObject.Hitbox.MaxX+GridCellSize);
                double yBegin = Math.Max(0, previousY + physicalObject.Hitbox.MinY - GridCellSize);
                double yEnd = Math.Min(GridSize * GridCellSize, previousY + physicalObject.Hitbox.MaxY+GridCellSize);

                for (double i = xBegin; i < xEnd; i += GridCellSize)
                    for (double j = yBegin; j < yEnd; j += GridCellSize)
                        ObjectGrid[(int)(i / GridCellSize), (int)(j / GridCellSize)].Remove(physicalObject);

                xBegin = Math.Max(0, physicalObject.X + physicalObject.Hitbox.MinX - GridCellSize);
                xEnd = Math.Min(GridSize * GridCellSize, physicalObject.X + physicalObject.Hitbox.MaxX+ GridCellSize);
                yBegin = Math.Max(0, physicalObject.Y + physicalObject.Hitbox.MinY - GridCellSize);
                yEnd = Math.Min(GridSize * GridCellSize, physicalObject.Y + physicalObject.Hitbox.MaxY + GridCellSize);

                for (double i = xBegin; i < xEnd; i += GridCellSize)
                    for (double j = yBegin; j < yEnd; j += GridCellSize)
                        ObjectGrid[(int)(i / GridCellSize), (int)(j / GridCellSize)].Add(physicalObject);
            }
        }

        /// <summary>
        /// Returns objects that physicalObject may collide with (including this object)
        /// </summary>
        /// <param name="physicalObject"></param>
        /// <returns></returns>
        public HashSet<PhysicalObject> GetNearbyObjects(PhysicalObject physicalObject)
        {
            HashSet<PhysicalObject> objects = new HashSet<PhysicalObject>();

            double xBegin = Math.Max(0, physicalObject.X + physicalObject.Hitbox.MinX - GridCellSize);
            double xEnd = Math.Min(GridSize * GridCellSize, physicalObject.X + physicalObject.Hitbox.MaxX + GridCellSize);
            double yBegin = Math.Max(0, physicalObject.Y + physicalObject.Hitbox.MinY - GridCellSize);
            double yEnd = Math.Min(GridSize * GridCellSize, physicalObject.Y + physicalObject.Hitbox.MaxY + GridCellSize);

            for (double i = xBegin; i < xEnd; i += GridCellSize)
                for (double j = yBegin; j < yEnd; j += GridCellSize)
                    objects.UnionWith(ObjectGrid[(int)(i / GridCellSize), (int)(j / GridCellSize)]);

            return objects;
        }
    }
}