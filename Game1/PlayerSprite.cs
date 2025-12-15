using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Game1.Collision;
using System.IO;

namespace Game1
{
    /// <summary>
    /// stores an enum for the player's animation state
    /// </summary>
    public enum PlayerAnimationState
    { 
        Idle = 0,
        Run = 2,
        Die = 4
    }

    /// <summary>
    /// a class for defining the player sprite, including movement, animation, and collision
    /// </summary>
    public class PlayerSprite
    {
        #region Variables
        //define variables and constants for animation and animation speed
        private const float IDLE_DURATION = 0.2f;
        private const float RUN_DURATION = 0.1f;
        private float animationTimer;
        private int state;
        public PlayerAnimationState Animation { get; private set; }

        //defines collision properties
        private CollisionRectangle bounds;
        public CollisionRectangle Bounds => bounds;

        //defines properties for drawing the sprite
        private Texture2D texture;
        private Texture2D blank;


        private bool left;
        private Vector2 center = new Vector2(16, 16);

        //defines movement properties
        private Vector2 velocity;
        private ScreenDimensions screen;
        public Vector2 Position { get; private set; }
        private Vector2 start;

        private readonly CollisionRectangle[] obstacles;

        #endregion

        /// <summary>
        /// constructs a player sprite with a starting position and screen dimensions
        /// </summary>
        /// <param name="startPos">the spawn location of the sprite</param>
        /// <param name="dim">the dimensions in which the sprite can move</param>
        public PlayerSprite(Vector2 startPos, ScreenDimensions dim, CollisionRectangle[] obstacle) 
        {
            animationTimer = 0f;
            start = startPos;
            Position = start;
            velocity = Vector2.Zero;
            screen = dim;
            left = true;
            bounds = new CollisionRectangle(Position + new Vector2(-16, -16), 32, 38);
            obstacles = obstacle;
        }

        /// <summary>
        /// loads the content for the player sprite
        /// </summary>
        /// <param name="content">the content manager</param>
        public void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("Simpleton");
            blank = content.Load<Texture2D>("blank");
        }

        /// <summary>
        /// Updates the player sprite based on input and elapsed time
        /// </summary>
        /// <param name="gameTime">the gametime</param>
        /// <param name="keyboardState">The keyboard state</param>
        public void Update(GameTime gameTime, KeyboardState keyboardState)
        {
            float s = (float)gameTime.ElapsedGameTime.TotalSeconds;
            PlayerAnimationState pastAnimation = Animation;

            animationTimer += s;

            //as long as the player is not dead, allow movement and animation changes
            if (pastAnimation != PlayerAnimationState.Die)
            {
                //reset velocity
                velocity = Vector2.Zero;

                //update velocity based on keyboard input
                if (keyboardState.IsKeyDown(Keys.Left))
                {
                    velocity += new Vector2(-100, 0) * s;
                    left = true;
                }
                if (keyboardState.IsKeyDown(Keys.Right))
                {
                    velocity += new Vector2(100, 0) * s;
                    left = false;
                }
                if (keyboardState.IsKeyDown(Keys.Up))
                {
                    velocity += new Vector2(0, -100) * s;
                }
                if (keyboardState.IsKeyDown(Keys.Down))
                {
                    velocity += new Vector2(0, 100) * s;
                }

                //prevent movement off the playable screen
                if (Position.X < screen.Left && velocity.X < 0)
                    velocity = new Vector2(0, velocity.Y);
                if (Position.X > screen.Right && velocity.X > 0)
                    velocity = new Vector2(0, velocity.Y);
                if (Position.Y < screen.Top && velocity.Y < 0)
                    velocity = new Vector2(velocity.X, 0);
                if (Position.Y > screen.Bottom && velocity.Y > 0)
                    velocity = new Vector2(velocity.X, 0);

                //update position and collision based on velocity
                bool validX = true;
                bool validY = true;
                float shiftX = velocity.X;
                float shiftY = velocity.Y;
                CollisionRectangle newBoundsX = bounds.Shift(ShiftType.Horizontal, shiftX);
                CollisionRectangle newBoundsY = bounds.Shift(ShiftType.Vertical, shiftY);
                CollisionRectangle newBounds = newBoundsX.Shift(ShiftType.Vertical, shiftY);
                foreach (CollisionRectangle cr in obstacles) 
                {
                    if (validX && cr.CollidesWith(newBoundsX))
                    {
                        validX = false;
                    }
                    else if (validY && cr.CollidesWith(newBoundsY))
                    {
                        validY = false;
                    }
                    else if (cr.CollidesWith(newBounds)) 
                    { 
                        validX = false;
                        validY = false;
                        break;
                    }
                }

                if (validX && validY)
                {
                    Position += velocity;
                    bounds = newBounds;
                }
                else 
                {
                    if (validX) 
                    { 
                        Position += new Vector2(shiftX, 0);
                        bounds = newBoundsX;
                    }
                    if (validY)
                    {
                        Position += new Vector2(0, shiftY);
                        bounds = newBoundsY;
                    }
                }

                //update animation based on current state and elapsed time
                if (pastAnimation == PlayerAnimationState.Run)
                {
                    if (velocity.X == 0 && velocity.Y == 0)
                    {
                        Animation = PlayerAnimationState.Idle;
                        state = 0;
                        animationTimer = 0;
                    }
                    else if (animationTimer >= RUN_DURATION)
                    {
                        animationTimer -= RUN_DURATION;
                        state = (state + 1) % 10;
                    }
                }

                //update animation based on current state and elapsed time
                else if (pastAnimation == PlayerAnimationState.Idle)
                {
                    if (velocity.X != 0 || velocity.Y != 0)
                    {
                        Animation = PlayerAnimationState.Run;
                        state = 0;
                        animationTimer = 0;
                    }
                    else if (animationTimer >= IDLE_DURATION)
                    {
                        animationTimer -= IDLE_DURATION;
                        state = (state + 1) % 10;
                    }
                }
            }
            //if player is dead, check for updating animation and restarting game
            else if (animationTimer >= RUN_DURATION)
            {
                if (state < 10)
                {
                    animationTimer -= RUN_DURATION;
                    state++;
                }
                else
                    Restart();
            }
        }

        /// <summary>
        /// draws the player sprite
        /// </summary>
        /// <param name="gameTime">the game time</param>
        /// <param name="spriteBatch">the sprite batch</param>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (state < 10)
            {
                SpriteEffects effects = left ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                spriteBatch.Draw(texture, Position, new Rectangle(state * 32, (int)Animation * 32, 32, 32), Color.White, 0f, center, 2f, effects, 0);
            }
            //spriteBatch.Draw(blank, new Rectangle((int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height), Color.Red);
        } 

        /// <summary>
        /// Method for restarting the player
        /// </summary>
        public void Restart() 
        {
            Animation = PlayerAnimationState.Idle;
            Position = start;
            bounds = new CollisionRectangle(Position + new Vector2(-16, -16), 32, 38);
            left = true;
            state = 0;
            animationTimer = 0f;
        }

        /// <summary>
        /// method for killing the player / starting the player kill sequence
        /// </summary>
        public void Die() 
        {
            Animation = PlayerAnimationState.Die;
            state = 0;
            animationTimer = 0f;
        }
    }
}
