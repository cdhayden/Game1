using Game1.Collision;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1
{
    /// <summary>
    /// Defines an enum used for storing colors of gems for easy conversion to ints
    /// </summary>
    public enum GemColor 
    { 
        Red,
        Orange,
        Yellow,
        Green,
        Blue,
        Purple,
    }

    /// <summary>
    /// defines a class for a gem sprite
    /// </summary>
    public class GemSprite
    {
        #region Variables
        //declare graphical elements variables
        private Texture2D texture;
        private Color color;

        //declare state variables
        private Vector2 position;
        public bool Collected { get; set; }
        public GemColor GemColor { get; private set; }

        //declare collision properties
        private CollisionCircle bounds;
        public CollisionCircle Bounds => bounds;
        #endregion
        /// <summary>
        /// initailzies a gem with a color and position
        /// </summary>
        /// <param name="gemColor">The int value corresponding to the color</param>
        /// <param name="pos">the vector corresponding to the spawn position</param>
        public GemSprite(int gemColor, Vector2 pos) 
        {
            bounds = new CollisionCircle(pos + new Vector2(16, 16), 16);
            position = pos;
            if (gemColor >= 0 && gemColor < 6) 
            {
                GemColor = (GemColor)gemColor;
                color = ConvertGemColor(GemColor);
            }
            else 
            {
                Random random = new Random();
                GemColor = (GemColor)random.Next(6);
                color = ConvertGemColor(GemColor);
            }
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
            texture = content.Load<Texture2D>("gems_db16");
        }

        /// <summary>
        /// draws the gem
        /// </summary>
        /// <param name="gameTime">the game time</param>
        /// <param name="spriteBatch">the sprite batch</param>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, new Rectangle(32, 64, 32, 32), color);
        }

        /// <summary>
        /// helper method for converting a gemcolor to a color
        /// </summary>
        /// <param name="color">the gemcolor to convert</param>
        /// <returns></returns>
        public static Color ConvertGemColor(GemColor color) 
        {
            switch (color) 
            { 
                case GemColor.Red:
                    return Color.Red;

                case GemColor.Orange:
                    return Color.Orange;

                case GemColor.Yellow:
                    return Color.Yellow;

                case GemColor.Green:
                    return Color.Green;

                case GemColor.Blue:
                    return Color.DeepSkyBlue;

                case GemColor.Purple:
                    return Color.MediumPurple;

                default:
                    return Color.White;
            }
        }
    }
}
