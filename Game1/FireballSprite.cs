using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Game1.Collision;

namespace Game1
{
    /// <summary>
    /// a class for defining the fireball sprite
    /// </summary>
    public class FireballSprite
    {
        #region Variables
        //define graphical elements
        private Texture2D[] texture;

        //define properties for movement
        private Vector2 position;
        private bool left;
        private float speed;

        //define properties for animation
        private float animationTimer = 0f;
        private int animationState = 0;

        //define properties for collision
        private CollisionCircle bounds;
        public CollisionCircle Bounds => bounds;
        #endregion

        /// <summary>
        /// constructs a fireball sprite with a position, direction, and speed based on collected gems
        /// </summary>
        /// <param name="pos">The starting position for the fireball</param>
        /// <param name="leftward">whether the fireball should start facing left</param>
        /// <param name="collectedGems">the number of collected gems, in order to scale fireball speed with game progression</param>
        public FireballSprite(Vector2 pos, bool leftward, int collectedGems) 
        {
            position = pos;
            speed = 100 + collectedGems * 30;
            left = leftward;
        }

        /// <summary>
        /// constructs the fireball sprites
        /// </summary>
        /// <param name="content">the content manager</param>
        public void LoadContent(ContentManager content)
        {
            texture = [content.Load<Texture2D>("Fireball1"), content.Load<Texture2D>("Fireball2")];
        }

        /// <summary>
        /// update the animation and position of the fireball
        /// </summary>
        /// <param name="gameTime">the gametime</param>
        public void Update(GameTime gameTime)
        {
            float s = (float)gameTime.ElapsedGameTime.TotalSeconds;
            animationTimer += s;

            if (animationTimer >= 0.2f) 
            {
                animationTimer -= 0.2f;
                animationState = (animationState + 1) % 2;
            }
            if (left) position -= new Vector2(speed * s, 0);
            else position += new Vector2(speed * s, 0);

            bounds = new CollisionCircle(position + new Vector2(16, 16), 16);
        }

        /// <summary>
        /// Draws the fireball sprite
        /// </summary>
        /// <param name="gameTime">the gametime</param>
        /// <param name="spriteBatch">the sprite batch</param>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            SpriteEffects effects = left ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            spriteBatch.Draw(texture[animationState], position, null, Color.White, 0f, Vector2.Zero, 2f, effects, 0);
        }
    }
}
