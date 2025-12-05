using Game1.Collision;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Game1
{
    /// <summary>
    /// defines a class for a gem sprite
    /// </summary>
    public class GemSprite
    {
        #region Variables
        //declare graphical elements variables 
        private Texture2D texture;
        private Texture2D sparkleTexture;

        private Color color;

        //declare state variables
        private Vector2 position;
        public bool Collected { get; set; }
        public GemColor GemColor { get; private set; }

        //declare collision properties
        private CollisionCircle bounds;
        public CollisionCircle Bounds => bounds;

        private const int MAX_FRAMES = 11;
        private const int MAX_SPARKLE_FRAMES = 10;

        private int _frame;
        private float _frameTimer;
        private float _frameTimerThreshold = 0.05f;

        private float _pauseTime = 1f;

        private bool sparkling;
        private int _sparkleFrame;
        private int _sparkleWidth = 20;
        private int _sparkleHeight = 19;

        private string _textureFilename;
        private int _textureFrameCount;
        
        public int TextureWidth { get; private set; }
        public int TextureHeight { get; private set; }

        #endregion
        /// <summary>
        /// initailzies a gem with a color and position
        /// </summary>
        /// <param name="gemColor">The int value corresponding to the color</param>
        /// <param name="pos">the vector corresponding to the spawn position</param>
        public GemSprite(int gemColor, Vector2 pos) 
        {

            sparkling = false;
            _sparkleFrame = 0;
            _frame = MAX_FRAMES;
            _frameTimer = 0;
            position = pos;
            InitializeByColor((GemColor)gemColor);
        }

        /// <summary>
        /// Constructor for a gem with a random color
        /// </summary>
        /// <param name="pos">position to spawn the gem in</param>
        public GemSprite(Vector2 pos) : this(-1, pos) { }

        /// <summary>
        /// loads the texture for the gem
        /// </summary>
        /// <param name="content">the content manager</param>
        public void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>(_textureFilename);
            sparkleTexture = content.Load<Texture2D>("RealGems/sparkle");
        }

        /// <summary>
        /// updates the gem animation
        /// </summary>
        /// <param name="gameTime">the game time</param>
        /// <param name="spriteBatch">the sprite batch</param>
        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _frameTimer += dt;
            if (_frameTimer >= _frameTimerThreshold)
            {
                if (sparkling)
                {
                    _frameTimer -= _frameTimerThreshold;
                    if (++_sparkleFrame >= MAX_SPARKLE_FRAMES)
                    {
                        sparkling = false;
                        _sparkleFrame = 0;
                    }
                }


                if (_frame < MAX_FRAMES)
                {
                    _frameTimer -= _frameTimerThreshold;
                    _frame = (_frame + 1);
                    if (_frame > 5 && !sparkling)
                    {
                        sparkling = true;
                    }
                }
                else if(_frameTimer >= _pauseTime) 
                {
                    _frameTimer -= _pauseTime;
                    _frame = 0;
                }
            }
        }

        /// <summary>
        /// draws the gem
        /// </summary>
        /// <param name="gameTime">the game time</param>
        /// <param name="spriteBatch">the sprite batch</param>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, new Rectangle(TextureWidth * MathHelper.Clamp(_frame, 0, _textureFrameCount - 1), 0, TextureWidth, TextureHeight), Color.White);
            if(sparkling)
                spriteBatch.Draw(sparkleTexture, position + new Vector2(-10, -10), new Rectangle(_sparkleWidth * MathHelper.Clamp(_sparkleFrame, 0, MAX_SPARKLE_FRAMES - 1), 0, _sparkleWidth, _sparkleHeight), Color.White);
        }

        /// <summary>
        /// draws the gem
        /// </summary>
        /// <param name="gameTime">the game time</param>
        /// <param name="spriteBatch">the sprite batch</param>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, float fade)
        {
            spriteBatch.Draw(texture, position, new Rectangle(TextureWidth * MathHelper.Clamp(_frame, 0, _textureFrameCount - 1), 0, TextureWidth, TextureHeight), Color.White * fade);
            if (sparkling)
                spriteBatch.Draw(sparkleTexture, position + new Vector2(-10, -10), new Rectangle(_sparkleWidth * MathHelper.Clamp(_sparkleFrame, 0, MAX_SPARKLE_FRAMES - 1), 0, _sparkleWidth, _sparkleHeight), Color.White);
        }

        private void InitializeByColor(GemColor color) 
        {


            if ((int)color < 0 || (int)color > 5)
            {
                Random random = new Random();
                GemColor = (GemColor)random.Next(6);
            }
            else
            {
                GemColor = color;
            }

            switch (GemColor) 
            {
                case GemColor.Red:
                    _textureFilename = "RealGems/red_3";
                    TextureHeight = 28;
                    TextureWidth = 28;
                    _textureFrameCount = 11;
                    break;
                case GemColor.Yellow:
                    _textureFilename = "RealGems/yellow_4";
                    TextureHeight = 30;
                    TextureWidth = 20;
                    _textureFrameCount = 11;
                    break;
                case GemColor.Green:
                    _textureFilename = "RealGems/green_1";
                    TextureHeight = 30;
                    TextureWidth = 18;
                    _textureFrameCount = 10;
                    break;
                case GemColor.Blue:
                    _textureFilename = "RealGems/blue_5";
                    TextureHeight = 22;
                    TextureWidth = 19;
                    _textureFrameCount = 11;
                    break;
                case GemColor.Indigo:
                    _textureFilename = "RealGems/indigo_6";
                    TextureHeight = 26;
                    TextureWidth = 24;
                    _textureFrameCount = 10;
                    break;
                case GemColor.Purple:
                    _textureFilename = "RealGems/purple_9";
                    TextureHeight = 26;
                    TextureWidth = 27;
                    _textureFrameCount = 10;
                    break;

                default:
                    _textureFilename = "blank";
                    TextureHeight = 2;
                    TextureWidth = 2;
                    _textureFrameCount = 1;
                    break;
            }

            bounds = new CollisionCircle(new Vector2(position.X + TextureWidth / 2f, position.Y + TextureHeight / 2f), ((TextureHeight + TextureWidth) / 4f));
        }
    }
}
