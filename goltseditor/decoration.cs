using Microsoft.Xna.Framework.Content;

namespace goltseditor
{
    public class Decoration : WorldObject
    {
        public Decoration(ContentManager contentManager, double x, double y, double movementx, double movementy, double weight,
            bool gravityAffected, string textureName, float paralaxCoefficient)
            : base(contentManager, x, y, movementx, movementy, weight, gravityAffected, textureName, paralaxCoefficient)
        { }
    }
}