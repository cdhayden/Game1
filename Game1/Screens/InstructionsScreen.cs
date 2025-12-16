using Game1.StateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Screens
{
    public class InstructionsScreen : GameScreen
    {

        private ContentManager _content;
        private Texture2D _backgroundTexture;
        private Texture2D _tint;

        private InputAction _return;

        public InstructionsScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0);
            TransitionOffTime = TimeSpan.FromSeconds(0);

            _return = new InputAction(
                new[] { Buttons.A, Buttons.Start },
                new[] { Keys.Escape }, true);
        }

        public override void HandleInput(GameTime gameTime, InputState input)
        {
            PlayerIndex playerIndex;

            // We pass in our ControllingPlayer, which may either be null (to
            // accept input from any player) or a specific index. If we pass a null
            // controlling player, the InputState helper returns to us which player
            // actually provided the input. We pass that through to our Accepted and
            // Cancelled events, so they can tell which player triggered them.
            if (_return.Occurred(input, ControllingPlayer, out playerIndex))
            {
                LoadingScreen.Load(ScreenManager, false, playerIndex, false, new BackgroundScreen(), new MainMenuScreen());
            }
        }

        /// <summary>
        /// Loads graphics content for this screen. The background texture is quite
        /// big, so we use our own local ContentManager to load it. This allows us
        /// to unload before going from the menus into the game itself, whereas if we
        /// used the shared ContentManager provided by the Game class, the content
        /// would remain loaded forever.
        /// </summary>
        public override void Activate()
        {
            if (_content == null)
                _content = new ContentManager(ScreenManager.Game.Services, "Content");

            //_backgroundTexture = _content.Load<Texture2D>("title_image");
            //_tint = _content.Load<Texture2D>("blank");
        }

        public override void Unload()
        {
            _content.Unload();
        }

        // Unlike most screens, this should not transition off even if
        // it has been covered by another screen: it is supposed to be
        // covered, after all! This overload forces the coveredByOtherScreen
        // parameter to false in order to stop the base Update method wanting to transition off.
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);
        }

        public override void Draw(GameTime gameTime)
        {
            var spriteBatch = ScreenManager.SpriteBatch;
            var viewport = ScreenManager.GraphicsDevice.Viewport;

            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 0, 0);

            var font = ScreenManager.Font;

            spriteBatch.Begin();

            spriteBatch.DrawString(font,
                        "Follow these instructions to play\n\n" +
                        "    1. Use arrow keys to move\n" +
                        "    2. Collect all 6 gem colors to win\n" +
                        "    3. Gems already collected are shown at the top\n" +
                        "    4. The fireballs kill you! This restarts the level\n" +
                        "    5. Collecting an uncollected color of gem makes fireballs faster\n" +
                        "    6. Collecting a collected gem color speeds up the gem spawn clock\n\n" +
                        "PRESS ESCAPE TO RETURN TO MAIN MENU",
                        new Vector2(viewport.Width / 2 - 220, viewport.Height / 2 - 100), Color.Gold
                        );

            spriteBatch.End();
        }
    }
}
