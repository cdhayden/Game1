using Game1.Collision;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Collision
{
    /// <summary>
    /// Struct to represent circular bounds
    /// </summary>
    public struct CollisionCircle
    {
        /// <summary>
        /// vector to represent the center of the circle
        /// </summary>
        public Vector2 Center;

        /// <summary>
        /// float to represent the radius of the circle
        /// </summary>
        public float Radius;

        /// <summary>
        /// Constructs a new BoundingCircle
        /// </summary>
        /// <param name="center">the center of the circle</param>
        /// <param name="radius">the radius of the circle</param>
        public CollisionCircle(Vector2 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        /// <summary>
        /// determines collision between this circle and another
        /// </summary>
        /// <param name="other">the other circle</param>
        /// <returns>true for collision, false otherwise</returns>
        public bool CollidesWith(CollisionCircle other)
        {
            return CollisionHelper.Collides(this, other);
        }

        /// <summary>
        /// determines collision between this circle and a rectangle
        /// </summary>
        /// <param name="other">the rectangle</param>
        /// <returns>true for collision, false otherwise</returns>
        public bool CollidesWith(CollisionRectangle other)
        {
            return CollisionHelper.Collides(this, other);
        }

        public CollisionCircle Grow(float amount) 
        { 
            return new CollisionCircle(Center, Radius + amount);
        }
    }
}
