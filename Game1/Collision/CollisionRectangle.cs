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
    /// a bounding rectangle for detecting collision
    /// </summary>
    public struct CollisionRectangle
    {
        public float X;

        public float Y;

        public float Width;

        public float Height;

        public float Left => X;

        public float Right => X + Width;

        public float Top => Y;

        public float Bottom => Y + Height;

        public CollisionRectangle(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public CollisionRectangle(Vector2 Position, float width, float height)
        {
            X = Position.X;
            Y = Position.Y;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// determines collision between this rectangle and another
        /// </summary>
        /// <param name="other">the other rectangle</param>
        /// <returns>true for collision, false otherwise</returns>
        public bool CollidesWith(CollisionRectangle other)
        {
            return CollisionHelper.Collides(this, other);
        }

        /// <summary>
        /// determines collision between this rectangle and a circle
        /// </summary>
        /// <param name="other">the circle</param>
        /// <returns>true for collision, false otherwise</returns>
        public bool CollidesWith(CollisionCircle other)
        {
            return CollisionHelper.Collides(this, other);
        }
    }
}
