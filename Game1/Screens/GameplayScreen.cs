using Game1;
using Game1.Collision;
using Game1.StateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;

namespace Game1.Screens
{
    // This screen implements the actual game logic. It is just a
    // placeholder to get the idea across: you'll probably want to
    // put some more interesting gameplay in here!
    public abstract class GameplayScreen : GameScreen
    {
        protected Random random = new Random();

        //set up timers and time thresholds for spawning gems and fireballs
        protected float gemSpawnThreshold;
        protected float gemSpawnTimer = 0;

        //declare fireball spawn variables
        protected float fireballSpawnThreshold;
        protected float fireballSpawnTimer = 0;

        //declare blinking variables
        protected float blinkTimer = 0;
        protected float blinkThreshold = 0.6f;
        protected bool blinking = false;

        //declare system variables
        protected readonly ScreenManager _screenManager;
        protected ContentManager _content;
        protected GraphicsDeviceManager _graphics;
        protected SpriteBatch _spriteBatch;

        //declare playable dimensions
        protected ScreenDimensions screen;
        protected ScreenDimensions playableScreen;
        protected CollisionRectangle[] obstacles;

        //declare state variables
        protected KeyboardState pastKeyboard;
        protected KeyboardState currentKeyboard;

        //declare persistent graphical elements
        protected SpriteFont font;
        protected Texture2D background;
        protected Rectangle backgroundRectangle;
        protected PlayerSprite player;
        protected GemSprite[] collectedGems = new GemSprite[6];

        //initialize variable state values
        protected int collectedCount => collectedGems.Count(item => item.Collected);

        protected List<GemSprite> fieldGems;
        protected List<FireballSprite> fireballs;

        //actions variables
        protected float _pauseAlpha;
        protected readonly InputAction _pauseAction;
        protected Texture2D _blank;

        //audio variables
        protected Song _backgroundMusic;
        protected SoundEffect _gemPickupSound;
        protected SoundEffect _fireDeathSound;

        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            _pauseAction = new InputAction(
                new Buttons[0],
                new[] { Keys.Space }, true);
        }

        // Load graphics content for the game
        public override void Activate()
        {
            if (_content == null)
                _content = new ContentManager(ScreenManager.Game.Services, "Content");

            //_gameFont = _content.Load<SpriteFont>("gamefont");

            _gemPickupSound = _content.Load<SoundEffect>("Picked Coin Echo");
            _fireDeathSound = _content.Load<SoundEffect>("foom_0");
            _backgroundMusic = _content.Load<Song>("CHIPTUNE_The_Bards_Tale");
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0.75f;
            MediaPlayer.Play(_backgroundMusic);

            _blank = _content.Load<Texture2D>("blank"); //86 x 68

            fieldGems = new List<GemSprite>();
            fireballs = new List<FireballSprite>();

            //load screen dimensions for convenience
            screen.Left = ScreenManager.GraphicsDevice.Viewport.X;
            screen.Width = ScreenManager.GraphicsDevice.Viewport.Width;
            screen.Top = ScreenManager.GraphicsDevice.Viewport.Y;
            screen.Height = ScreenManager.GraphicsDevice.Viewport.Height;
            _spriteBatch = ScreenManager.SpriteBatch;

            //load persistent sprites
            font = _content.Load<SpriteFont>("Saira");

            int cumulativeGemWidth = 0;
            for (int i = 0; i < 6; i++)
            {
                GemSprite gem = new GemSprite(i, new Vector2((screen.Width / 2) - (38 * (collectedGems.Length / 2)) + cumulativeGemWidth + (16 * i), screen.Top + 16));
                cumulativeGemWidth += gem.TextureWidth;
                collectedGems[i] = gem;
                collectedGems[i].LoadContent(_content);
            }


            // A real game would probably have more content than this sample, so
            // it would take longer to load. We simulate that by delaying for a
            // while, giving you a chance to admire the beautiful loading screen.
            Thread.Sleep(1000);

            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime();
        }


        public override void Deactivate()
        {
            base.Deactivate();
        }

        public override void Unload()
        {
            _content.Unload();
        }

        // This method checks the GameScreen.IsActive property, so the game will
        // stop updating when the pause menu is active, or if you tab away to a different application.
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Gradually fade in or out depending on whether we are covered by the pause screen.
            if (coveredByOtherScreen)
            {
                _pauseAlpha = Math.Min(_pauseAlpha + 1f / 32, 1);
                MediaPlayer.Volume = MathHelper.Clamp(MediaPlayer.Volume - 0.01f, 0.25f, 1f);
            }
            else
            { 
                _pauseAlpha = Math.Max(_pauseAlpha - 1f / 32, 0);
                MediaPlayer.Volume = MathHelper.Clamp(MediaPlayer.Volume + 0.01f, 0.25f, 1f);
            }
            if (IsActive)
            {
                //initailize keyboard states and check for exit
                pastKeyboard = currentKeyboard;
                currentKeyboard = Keyboard.GetState();

                // check for win condition
                if (collectedCount == 6) WinLevel();


                float s = (float)gameTime.ElapsedGameTime.TotalSeconds;

                player.Update(gameTime, currentKeyboard);

                foreach (GemSprite gem in fieldGems) gem.Update(gameTime);


                blinkTimer += s;
                if (blinkTimer >= blinkThreshold) 
                {
                    blinkTimer -= blinkThreshold;
                    blinking = !blinking;
                }

                //if player is alive, update game elements
                if (player.Animation != PlayerAnimationState.Die)
                {

                    gemSpawnTimer += s;
                    fireballSpawnTimer += s;

                    //spawn gems at when time has passed threshold
                    if (gemSpawnTimer >= gemSpawnThreshold)
                    {
                        gemSpawnTimer -= gemSpawnThreshold;
                        fieldGems.Clear();
                        int count = random.Next(1, 6);
                        for (int i = 0; i < count; i++)
                        {
                            GemSprite gem = GenerateGemSprite();
                            gem.LoadContent(_content);
                            fieldGems.Add(gem);
                        }
                    }

                    //check for collisions between player and gems, collecting them if collided, and increasing fireball difficulty
                    foreach (GemSprite g in fieldGems)
                    {
                        if (g.Bounds.CollidesWith(player.Bounds))
                        {
                            _gemPickupSound.Play();
                            int ind = (int)g.GemColor;
                            collectedGems[ind].Collected = true;
                            g.Collected = true;
                            fireballSpawnThreshold = (float)Math.Max(fireballSpawnThreshold * 0.75, 0.33);
                        }
                    }
                    fieldGems.RemoveAll(g => g.Collected);

                    //update all fireballs
                    foreach (FireballSprite f in fireballs) f.Update(gameTime);

                    //spawn fireballs at when time has passed threshold, using the number of collected gems to increase difficulty
                    if (fireballSpawnTimer >= fireballSpawnThreshold)
                    {
                        fireballSpawnTimer -= fireballSpawnThreshold;
                        bool left = false;
                        int x = random.Next(2);
                        if (x == 0) left = true;
                        FireballSprite f;
                        if (left) f = new FireballSprite(new Vector2(screen.Right, (float)(random.NextDouble() * playableScreen.Height + playableScreen.Top)), left, collectedCount);
                        else f = new FireballSprite(new Vector2(screen.Left, (float)(random.NextDouble() * playableScreen.Height + playableScreen.Top)), left, collectedCount);
                        f.LoadContent(_content);
                        fireballs.Add(f);
                    }

                    //check if fireballs collide with player, killing player if so
                    foreach (FireballSprite f in fireballs)
                    {
                        if (f.Bounds.CollidesWith(player.Bounds))
                        {
                            Restart();
                            break;
                        }
                    }
                }
            }
        }

        // Unlike the Update method, this will only be called when the gameplay screen is active.
        public override void HandleInput(GameTime gameTime, InputState input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            // Look up inputs for the active player profile.
            int playerIndex = (int)ControllingPlayer.Value;

            var keyboardState = input.CurrentKeyboardStates[playerIndex];

            // The game pauses either if the user presses the pause button, or if
            // they unplug the active gamepad. This requires us to keep track of
            // whether a gamepad was ever plugged in, because we don't want to pause
            // on PC if they are playing with a keyboard and have no gamepad at all!

            PlayerIndex player;
            if (_pauseAction.Occurred(input, ControllingPlayer, out player))
            {
                ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
            }
            else
            {
                /*cannon actions
                    s.Right) && _cannonRotation < 1 - MathHelper.PiOver2)
                    _cannonRotation+= 0.01f;

                if(!_shotFired && _cannonShotAction.Occurred(input, ControllingPlayer, out player)) 
                {
                    _shotFired = true;
                    Vector2 direction = Vector2.Normalize(new Vector2((float)Math.Cos(_cannonRotation), (float)Math.Sin(_cannonRotation)));
                    _cannonBall = new CannonBallSprite(_cannonPosition + _cannonOrigin - new Vector2(26,26) + direction*120, direction * 600); //35 x 35
                    _cannonBall.LoadContent(_content);
                    
                }
                */
            }
        }

        public override void Draw(GameTime gameTime)
        {
            // This game has a blue background. Why? Because!
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 0, 0);

            _spriteBatch.Begin();

            _spriteBatch.Draw(background, backgroundRectangle, Color.White);
            _spriteBatch.Draw(_blank, new Rectangle(58, screen.Height - 64, (int)((screen.Width - 114)), 6), Color.DarkSlateGray * 0.3f);
            _spriteBatch.Draw(_blank, new Rectangle(58, screen.Height - 64, (int)((screen.Width - 114) * ((gemSpawnThreshold - gemSpawnTimer) / gemSpawnThreshold)), 6), Color.Gold);
            player.Draw(gameTime, _spriteBatch);
            foreach (GemSprite g in collectedGems) g.Draw(gameTime, _spriteBatch, g.Collected ? 1f : 0.25f);
            foreach (GemSprite g in fieldGems) g.Draw(gameTime, _spriteBatch);
            foreach (FireballSprite f in fireballs) f.Draw(gameTime, _spriteBatch);
            if (!blinking)
                _spriteBatch.DrawString(font, $"PRESS SPACE TO PAUSE", new Vector2(screen.Width / 2 - 94, screen.Bottom - 45), Color.Gold);

            //foreach (CollisionRectangle cr in obstacles) _spriteBatch.Draw(_blank, new Rectangle((int)cr.Left, (int)cr.Top, (int)cr.Width, (int)cr.Height), Color.Red * 0.5f);

            _spriteBatch.End();



            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || _pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, _pauseAlpha / 2);

                ScreenManager.FadeBackBufferToBlack(alpha, Color.Black);
            }



            /* real drawn stuff

            //draw different elements based on game state
            switch (gameState)
            {
                //for active state, draw background, player, gems, fireballs, and basic messages to user
                case GameState.Active:
                    _spriteBatch.Draw(background, new Rectangle(screen.Left, screen.Top + 30, screen.Width, screen.Height - 80), Color.White);
                    player.Draw(gameTime, _spriteBatch);
                    foreach (GemSprite g in collectedGems) if (g.Collected) g.Draw(gameTime, _spriteBatch);
                    foreach (GemSprite g in fieldGems) g.Draw(gameTime, _spriteBatch);
                    foreach (FireballSprite f in fireballs) f.Draw(gameTime, _spriteBatch);
                    _spriteBatch.DrawString(font, $"Time until next gems: {1 + (int)(gemSpawnThreshold - gemSpawnTimer)}", new Vector2(screen.Left + 30, screen.Top + 10), Color.Gold);
                    _spriteBatch.DrawString(font, $"PRESS SPACE FOR INSTRUCTIONS", new Vector2(screen.Width / 2 - 130, screen.Bottom - 50), Color.Gold);
                    break;

                //for paused state, draw instructions and any collected gems
                case GameState.Instruction:
                    
                    foreach (GemSprite g in collectedGems) if (g.Collected) g.Draw(gameTime, _spriteBatch);
                    break;

                //for won game, simply display a message
                case GameState.Won:
                    _spriteBatch.DrawString(font,
                       "          YOU WIN!\n\n" +
                       "PRESS \'ESC\' TO EXIT",
                       new Vector2(screen.Width / 2 - 80, screen.Height / 2 - 35), Color.Gold
                       );
                    break;

                default:
                    break;
            }

            */


            /*instructions text
             
            /spritebatch.Draw(_tint, new Rectangle((int)pos.X - 20, (int)pos.Y - 20, (int)len.X + 40, (int)len.Y + 40), outline);
            string instruction = "\'BACKSPACE\'  to  Pause";
            Vector2 len = _gameFont.MeasureString(instruction);
            Vector2 pos = new Vector2(viewport.X + 60, viewport.Y + viewport.Height - 55);
            spriteBatch.Draw(_tint, new Rectangle((int)pos.X - 10, (int)pos.Y - 10, (int)len.X + 20, (int)len.Y + 20), Color.Black * 0.3f);
            spriteBatch.DrawString(_gameFont, instruction, pos, Color.LightGray);

            instruction = "Arrow  Keys  to  Rotate";
            len = _gameFont.MeasureString(instruction);
            pos = new Vector2(viewport.X + viewport.Width - 280, viewport.Y + viewport.Height - 70);
            spriteBatch.Draw(_tint, new Rectangle((int)pos.X - 10, (int)pos.Y - 10, (int)len.X + 20, (int)len.Y + 50), Color.Black * 0.3f);
            spriteBatch.DrawString(_gameFont, instruction, new Vector2(viewport.X + viewport.Width - 280, viewport.Y + viewport.Height - 40), Color.LightGray);

            instruction = "       'SPACE'  to  Shoot";
            pos = new Vector2(viewport.X + viewport.Width - 280, viewport.Y + viewport.Height - 70);
            spriteBatch.DrawString(_gameFont, instruction, pos, Color.LightGray);
            */

        }

        /// <summary>
        /// Restart game, player, gems, and fireballs
        /// </summary>
        protected void Restart()
        {
            fireballs.Clear();
            fieldGems.Clear();
            fireballSpawnTimer = 0;
            gemSpawnTimer = 0;
            fireballSpawnThreshold = 2;
            foreach (GemSprite g in collectedGems) g.Collected = false;
            player.Die();
            _fireDeathSound.Play();
        }

        protected GemSprite GenerateGemSprite() 
        {
            GemSprite gem = new(Vector2.Zero);
            bool validPosition = false;
            while (!validPosition) 
            {
                gem = new GemSprite(new Vector2((float)(random.NextDouble() * playableScreen.Width + playableScreen.Left) - 16, (float)(random.NextDouble() * playableScreen.Height + playableScreen.Top)));
                CollisionCircle bounds = gem.Bounds.Grow(8);
                validPosition = true;
                if (bounds.CollidesWith(player.Bounds)) continue;
                foreach (CollisionRectangle obs in obstacles) 
                {
                    if (bounds.CollidesWith(obs)) 
                    {
                        validPosition = false;
                        break;
                    }
                }
            }
            return gem;
        }

        /// <summary>
        /// Performs the win action
        /// </summary>
        protected abstract void WinLevel();
    }
}
