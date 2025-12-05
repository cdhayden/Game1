using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Game1.StateManagement;

namespace Game1.Screens
{
    // A popup message box screen, used to display "are you sure?" confirmation messages.
    public class MessageBoxScreen : GameScreen
    {
        private string _usageText;
        private bool _includeUsage;
        private readonly string _message;
        private Texture2D _parchmentTexture;
        private readonly InputAction _menuSelect;
        private readonly InputAction _menuCancel;

        public event EventHandler<PlayerIndexEventArgs> Accepted;
        public event EventHandler<PlayerIndexEventArgs> Cancelled;


        // Constructor lets the caller specify whether to include the standard
        // "A=ok, B=cancel" usage text prompt.
        public MessageBoxScreen(string message, bool includeUsageText = true)
        {
            _usageText = "Space, Enter = ok" +
                                     "\nEscape = cancel";

            _includeUsage = includeUsageText;
            _message = message;

            IsPopup = true;

            TransitionOnTime = TimeSpan.FromSeconds(0.2);
            TransitionOffTime = TimeSpan.FromSeconds(0.2);

            _menuSelect = new InputAction(
                new[] { Buttons.A, Buttons.Start },
                new[] { Keys.Enter, Keys.Space }, true);
            _menuCancel = new InputAction(
                new[] { Buttons.B, Buttons.Back },
                new[] { Keys.Escape }, true);
        }

        // Loads graphics content for this screen. This uses the shared ContentManager
        // provided by the Game class, so the content will remain loaded forever.
        // Whenever a subsequent MessageBoxScreen tries to load this same content,
        // it will just get back another reference to the already loaded data.
        public override void Activate()
        {
            var content = ScreenManager.Game.Content;
                _parchmentTexture = content.Load<Texture2D>("blank_parchment_01");
        }

        public override void HandleInput(GameTime gameTime, InputState input)
        {
            PlayerIndex playerIndex;

            // We pass in our ControllingPlayer, which may either be null (to
            // accept input from any player) or a specific index. If we pass a null
            // controlling player, the InputState helper returns to us which player
            // actually provided the input. We pass that through to our Accepted and
            // Cancelled events, so they can tell which player triggered them.
            if (_menuSelect.Occurred(input, ControllingPlayer, out playerIndex))
            {
                Accepted?.Invoke(this, new PlayerIndexEventArgs(playerIndex));
                ExitScreen();
            }
            else if (_menuCancel.Occurred(input, ControllingPlayer, out playerIndex))
            {
                Cancelled?.Invoke(this, new PlayerIndexEventArgs(playerIndex));
                ExitScreen();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            var spriteBatch = ScreenManager.SpriteBatch;
            var font = ScreenManager.Font;

            // Darken down any other screens that were drawn beneath the popup.
            ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 2 / 3, Color.Black);

            // Center the message text in the viewport.
            var viewport = ScreenManager.GraphicsDevice.Viewport;
            var viewportSize = new Vector2(viewport.Width, viewport.Height);
            string[] parts = _usageText.Split("\n");
            float xSize = 0;
            var msgSize = font.MeasureString(_message);
            foreach (string s in parts) xSize += font.MeasureString(s).Y;
            var textSize = new Vector2(Math.Max(msgSize.X, xSize) + 32, msgSize.Y * 3);
            var textPosition = (viewportSize - textSize) / 2;

            // The background includes a border somewhat larger than the text itself.
            const int hPad = 64;
            const int vPad = 32;

            var backgroundRectangle = new Rectangle((int)textPosition.X - hPad,
                (int)textPosition.Y - vPad, (int)textSize.X + hPad * 2, (int)textSize.Y + vPad * 2);

            var parchmentColor = Color.White * TransitionAlpha;    // Fade the popup alpha during transitions
            var textColor = Color.Black * TransitionAlpha;    // Fade the popup alpha during transitions


            spriteBatch.Begin();

            spriteBatch.Draw(_parchmentTexture, backgroundRectangle, parchmentColor);
            if (_includeUsage) 
            {
                spriteBatch.DrawString(font, _message, new Vector2(backgroundRectangle.X + 15 + (backgroundRectangle.Width - textSize.X) / 2, backgroundRectangle.Y + 32), textColor);
                                
                var optionSize = font.MeasureString(parts[0]);
                var pos1 = new Vector2(backgroundRectangle.X + 75, backgroundRectangle.Y + backgroundRectangle.Height - optionSize.Y - 40);
                spriteBatch.DrawString(font, parts[0], pos1, textColor);
                optionSize = font.MeasureString(parts[1]);
                var pos2 = new Vector2(backgroundRectangle.X + backgroundRectangle.Width - optionSize.X - 75, backgroundRectangle.Y + backgroundRectangle.Height - optionSize.Y - 40);
                spriteBatch.DrawString(font, parts[1], pos2, textColor);
            } 
            else spriteBatch.DrawString(font, _message, new Vector2(backgroundRectangle.X + (backgroundRectangle.Width + textSize.X) / 2, backgroundRectangle.Y + (backgroundRectangle.Height + textSize.Y) / 2), textColor);

            spriteBatch.End();
        }
    }
}
