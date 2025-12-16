using Game1.StateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Screens
{
    public class WinScreen : GameScreen
    {
        private ContentManager _content;
        private Texture2D _backgroundTexture;
        private Texture2D _tint;

        private InputAction _return;

        public WinScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0);
            TransitionOffTime = TimeSpan.FromSeconds(0);

            _return = new InputAction(
                new[] { Buttons.A, Buttons.Start },
                new[] { Keys.Escape, Keys.Space }, true);
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
                LoadingScreen.Load(ScreenManager, false, ControllingPlayer, false, new BackgroundScreen(), new MainMenuScreen());
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

            spriteBatch.DrawString(font, "YOU WIN!", new Vector2(viewport.Width / 2 - font.MeasureString("YOU WIN!").X / 2, viewport.Height / 2 - 20), Color.Gold);
            spriteBatch.DrawString(font, "Press space to return to the main menu", new Vector2(viewport.Width / 2 - font.MeasureString("Press space to return to the main menu").X / 2, viewport.Height / 2 + 50), Color.Gold);

            spriteBatch.End();
        }
    }
}
