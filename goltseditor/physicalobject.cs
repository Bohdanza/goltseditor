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
    //It's like WorldObject but it has hitbox and stuff
    public abstract class PhysicalObject:WorldObject
    {
        //Strange things happen when it's less than 0.001
        public const double HitPresicion = 0.1;

        [JsonProperty]
        public ObjectHitbox Hitbox { get; protected set; }

        [JsonProperty]
        public int CollisionLayer { get; protected set; }

        [JsonIgnore]
        protected bool CollidedY = false, CollidedX = false;
        
        [Newtonsoft.Json.JsonConstructor]
        public PhysicalObject() : base() { }

        public PhysicalObject(ContentManager contentManager, 
            double x, double y, double movementx, double movementy, double weight, bool gravityAffected,
            string textureName,  string hitboxPath, int collisionLayer = 0):
            base(contentManager, x, y, movementx, movementy, weight, gravityAffected, textureName)
        {
            Hitbox = new ObjectHitbox(hitboxPath);
            CollisionLayer = collisionLayer;
        }

        public PhysicalObject(ContentManager contentManager, 
            double x, double y, double movementx, double movementy, double weight, bool gravityAffected,
            string textureName, List<Tuple<double, double>> hitbox, int collisionLayer = 0) :
            base(contentManager, x, y, movementx, movementy, weight, gravityAffected, textureName)
        {
            Hitbox = new ObjectHitbox(hitbox);
            CollisionLayer = collisionLayer;
        }

        public override void Update(ContentManager contentManager, World world)
        {
            double px = X;
            double py = Y;

            world.objects.UpdateObjectPosition(this, px, py);

            Texture.Update(contentManager);
        }
        
        private bool Obstructed(HashSet<PhysicalObject> relatedObjects)
        {
            foreach (var currentObject in relatedObjects)
                if (currentObject != this && Hitbox.CollidesWith(currentObject.Hitbox, X, Y, currentObject.X, currentObject.Y))
                {
                    return true;
                }

            return false;
        }
    }
}