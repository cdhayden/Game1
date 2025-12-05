using Game1.StateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

namespace Game1.Screens
{
    // The main menu screen is the first thing displayed when the game starts up.
    public class MainMenuScreen : GameScreen
    {
        private InputAction _menuLeft;
        private InputAction _menuRight;
        private InputAction _menuSelect;
        private InputAction _menuCancel;

        private SoundEffect _menuMoveSound;
        private SoundEffect _menuSelectSound;

        private ContentManager _content;

        protected Song _mainMusic;

        private int _selectedEntry;

        private List<MenuEntry> _menuEntries = new();
        private string _menuTitle;

        public MainMenuScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0);

            _menuTitle = "Stealin' Stuff";

            _menuLeft = new InputAction(
                new[] { Buttons.DPadLeft, Buttons.LeftThumbstickLeft },
                new[] { Keys.Left }, true);
            _menuRight = new InputAction(
                new[] { Buttons.DPadRight, Buttons.LeftThumbstickRight },
                new[] { Keys.Right }, true);
            _menuSelect = new InputAction(
                new[] { Buttons.A, Buttons.Start },
                new[] { Keys.Enter, Keys.Space }, true);
            _menuCancel = new InputAction(
                new[] { Buttons.B, Buttons.Back },
                new[] { Keys.Escape }, true);

            var playGameMenuEntry = new MenuEntry("Play Game");
            var instructionsMenuEntry = new MenuEntry("Instructions");
            var quitMenuEntry = new MenuEntry("Exit");

            playGameMenuEntry.Selected += PlayGameMenuEntrySelected;
            instructionsMenuEntry.Selected += InstructionsMenuEntrySelected;
            quitMenuEntry.Selected += OnCancel;

            _menuEntries.Add(playGameMenuEntry);
            _menuEntries.Add(instructionsMenuEntry);
            _menuEntries.Add(quitMenuEntry);
        }

        public override void Activate()
        {
            base.Activate();

            if (_content == null)
                _content = new ContentManager(ScreenManager.Game.Services, "Content");

            _menuSelectSound = _content.Load<SoundEffect>("Pickup5");
            _menuMoveSound = _content.Load<SoundEffect>("Blip1");

            _mainMusic = _content.Load<Song>("Kings_Feast");

            MediaPlayer.Volume = 1; 
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(_mainMusic);
            
        }

        // Responds to user input, changing the selected entry and accepting or cancelling the menu.
        public override void HandleInput(GameTime gameTime, InputState input)
        {
            // For input tests we pass in our ControllingPlayer, which may
            // either be null (to accept input from any player) or a specific index.
            // If we pass a null controlling player, the InputState helper returns to
            // us which player actually provided the input. We pass that through to
            // OnSelectEntry and OnCancel, so they can tell which player triggered them.
            PlayerIndex playerIndex;

            if (_menuLeft.Occurred(input, ControllingPlayer, out playerIndex))
            {
                _selectedEntry--;
                _menuMoveSound.Play();

                if (_selectedEntry < 0)
                    _selectedEntry = _menuEntries.Count - 1;
            }

            if (_menuRight.Occurred(input, ControllingPlayer, out playerIndex))
            {
                _selectedEntry++;
                _menuMoveSound.Play();

                if (_selectedEntry >= _menuEntries.Count)
                    _selectedEntry = 0;
            }

            if (_menuSelect.Occurred(input, ControllingPlayer, out playerIndex))
            {
                _menuEntries[_selectedEntry].OnSelectEntry(playerIndex);

                _menuSelectSound.Play();
            }
            else if (_menuCancel.Occurred(input, ControllingPlayer, out playerIndex))
                OnCancel(playerIndex);
        }

        // Allows the screen the chance to position the menu entries. By default,
        // all menu entries are lined up in a vertical list, centered on the screen.
        protected virtual void UpdateMenuEntryLocations()
        {
            // Make the menu slide into place during transitions, using a
            // power curve to make things look more interesting (this makes
            // the movement slow down as it nears the end).
            float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

            // start at Y = 175; each X value is generated per entry
            var position = new Vector2(0f, 0f);

            var font = ScreenManager.Font;

            // update each menu entry's location in turn
            for (int i = 0; i < _menuEntries.Count; i++)
            {
                MenuEntry menuEntry = _menuEntries[i];

                // each entry is to be centered horizontally
                position.X = (ScreenManager.GraphicsDevice.Viewport.Width / (_menuEntries.Count + 1) * (i+1)) - font.MeasureString(menuEntry.Text).X / 2;

                /*
                if (ScreenState == ScreenState.TransitionOn)
                    position.X -= transitionOffset * 256;
                else
                    position.X += transitionOffset * 512;
                */

                position.Y = ScreenManager.GraphicsDevice.Viewport.Height - font.MeasureString(menuEntry.Text).Y - 20;

                // set the entry's position
                menuEntry.Position = position;

                // move down for the next entry the size of this entry
            }
        }

        private void PlayGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            LoadingScreen.Load(ScreenManager, true, e.PlayerIndex, false, new GameplayScreen());
        }

        private void InstructionsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            LoadingScreen.Load(ScreenManager, false, e.PlayerIndex, false, new InstructionsScreen());
        }

        public override void Draw(GameTime gameTime)
        {
            // make sure our entries are in the right place before we draw them
            UpdateMenuEntryLocations();

            var graphics = ScreenManager.GraphicsDevice;
            var spriteBatch = ScreenManager.SpriteBatch;
            var font = ScreenManager.Font;

            spriteBatch.Begin();

            for (int i = 0; i < _menuEntries.Count; i++)
            {
                var menuEntry = _menuEntries[i];
                bool isSelected = IsActive && i == _selectedEntry;
                menuEntry.Draw(isSelected, gameTime, ScreenManager);
            }

            // Make the menu slide into place during transitions, using a
            // power curve to make things look more interesting (this makes
            // the movement slow down as it nears the end).
            float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

            // Draw the menu title centered on the screen
            var titlePosition = new Vector2(graphics.Viewport.Width / 2, 40);
            var titleOrigin = font.MeasureString(_menuTitle) / 2;
            //var titleColor = new Color(192, 192, 192) * TransitionAlpha;
            var titleColor = Color.Gold * TransitionAlpha;
            const float titleScale = 1.25f;

            titlePosition.Y -= transitionOffset * 100;

            spriteBatch.DrawString(font, _menuTitle, titlePosition, titleColor,
                0, titleOrigin, titleScale, SpriteEffects.None, 0);

            spriteBatch.End();
        }

        // Helper overload makes it easy to use OnCancel as a MenuEntry event handler.
        protected void OnCancel(object sender, PlayerIndexEventArgs e)
        {
            OnCancel(e.PlayerIndex);
        }

        protected void OnCancel(PlayerIndex playerIndex)
        {
            const string message = "Are you sure you want to quit stealing?";
            var confirmExitMessageBox = new MessageBoxScreen(message);

            confirmExitMessageBox.Accepted += ConfirmExitMessageBoxAccepted;

            ScreenManager.AddScreen(confirmExitMessageBox, playerIndex);
        }

        private void ConfirmExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.Game.Exit();
        }
    }
}
