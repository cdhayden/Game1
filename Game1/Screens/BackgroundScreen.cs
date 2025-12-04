using Game1.StateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System;

namespace Game1.Screens
{
    // The background screen sits behind all the other menu screens.
    // It draws a background image that remains fixed in place regardless
    // of whatever transitions the screens on top of it may be doing.
    public class BackgroundScreen : GameScreen
    {
        private ContentManager _content;
        private Texture2D _backgroundTexture;
        private Texture2D _tint;

        public BackgroundScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
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

            _backgroundTexture = _content.Load<Texture2D>("title_image");
            _tint = _content.Load<Texture2D>("blank");
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
            var fullscreen = new Rectangle(20, 60, viewport.Width - 40, viewport.Height - 120);

            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 0, 0);

            spriteBatch.Begin();

            spriteBatch.Draw(_backgroundTexture, fullscreen,
                new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha));
            spriteBatch.Draw(_tint, viewport.Bounds, Color.Black * 0.3f);

            spriteBatch.End();
        }
    }
}
