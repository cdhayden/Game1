using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Game1.Collision
{
    public static class CollisionHelper
    {
        /// <summary>
        /// Detects collisiong between two bounding circles
        /// </summary>
        /// <param name="a">one circle</param>
        /// <param name="b">the other circle</param>
        /// <returns>true for collision, false otherwise</returns>
        public static bool Collides(CollisionCircle a, CollisionCircle b)
        {
            return Math.Pow(a.Radius + b.Radius, 2) >=
                Math.Pow(a.Center.X - b.Center.X, 2) +
                Math.Pow(a.Center.Y - b.Center.Y, 2);
        }

        /// <summary>
        /// Detects collision between two rectangles
        /// </summary>
        /// <param name="a">one rectangle</param>
        /// <param name="b">the other rectangle</param>
        /// <returns>true on collision, false otherwise</returns>
        public static bool Collides(CollisionRectangle a, CollisionRectangle b)
        {
            return !(a.Left > b.Right || a.Right < b.Left ||
                     a.Top > b.Bottom || a.Bottom < b.Top);
        }

        /// <summary>
        /// Detects a collision between a rectangle and circle
        /// </summary>
        /// <param name="c">the circle</param>
        /// <param name="r">the rectangle</param>
        /// <returns>true on collision, false otherwise</returns>
        public static bool Collides(CollisionCircle c, CollisionRectangle r)
        {
            float nearestX = MathHelper.Clamp(c.Center.X, r.Left, r.Right);
            float nearestY = MathHelper.Clamp(c.Center.Y, r.Top, r.Bottom);
            return Math.Pow(c.Radius, 2) >=
                Math.Pow(c.Center.X - nearestX, 2) +
                Math.Pow(c.Center.Y - nearestY, 2);
        }

        /// <summary>
        /// Detects a collision between a rectangle and circle
        /// </summary>
        /// <param name="c">the circle</param>
        /// <param name="r">the rectangle</param>
        /// <returns>true on collision, false otherwise</returns>
        public static bool Collides(CollisionRectangle r, CollisionCircle c)
        {
            float nearestX = MathHelper.Clamp(c.Center.X, r.Left, r.Right);
            float nearestY = MathHelper.Clamp(c.Center.Y, r.Top, r.Bottom);
            return Math.Pow(c.Radius, 2) >=
                Math.Pow(c.Center.X - nearestX, 2) +
                Math.Pow(c.Center.Y - nearestY, 2);
        }
    }
}
