using Game1;
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
    public class GameplayScreen : GameScreen
    {
        private Random random = new Random();

        //set up timers and time thresholds for spawning gems and fireballs
        private float gemSpawnThreshold = 6;
        private float gemSpawnTimer = 0;

        private float fireballSpawnTimer = 0;
        private float fireballSpawnThreshold = 2;

        //declare system variables
        private readonly ScreenManager _screenManager;
        private ContentManager _content;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private ScreenDimensions screen;
        private ScreenDimensions playableScreen;

        //declare state variables
        private KeyboardState pastKeyboard;
        private KeyboardState currentKeyboard;
        private GameState gameState;

        //declare persistent graphical elements
        private SpriteFont font;
        private Texture2D background;
        private PlayerSprite player;
        private GemSprite[] collectedGems = new GemSprite[6];

        //initialize variable state values
        private int collectedCount => collectedGems.Count(item => item.Collected);

        private List<GemSprite> fieldGems;
        private List<FireballSprite> fireballs;

        //actions variables
        private float _pauseAlpha;
        private readonly InputAction _pauseAction;

        private Texture2D _tint;

        private Viewport _viewport;

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

            _tint = _content.Load<Texture2D>("blank"); //86 x 68

            fieldGems = new List<GemSprite>();
            fireballs = new List<FireballSprite>();

        //load screen dimensions for convenience
            screen.Left = ScreenManager.GraphicsDevice.Viewport.X;
            screen.Width = ScreenManager.GraphicsDevice.Viewport.Width;
            screen.Top = ScreenManager.GraphicsDevice.Viewport.Y;
            screen.Height = ScreenManager.GraphicsDevice.Viewport.Height;
            playableScreen.Left = screen.Left + 95;
            playableScreen.Width = screen.Width - 185;
            playableScreen.Top = screen.Top + 120;
            playableScreen.Height = screen.Height - 250;
            _spriteBatch = ScreenManager.SpriteBatch;

            //load persistent sprites
            font = _content.Load<SpriteFont>("Saira");
            background = _content.Load<Texture2D>("Sample_Map4");
            player = new PlayerSprite(new Vector2(screen.Right / 2, screen.Bottom / 2), playableScreen);
            player.LoadContent(_content);
            for (int i = 0; i < 6; i++)
            {
                collectedGems[i] = new GemSprite(i, new Vector2((screen.Width / 2) - (32 * (collectedGems.Length / 2)) + (32 * i), screen.Top + 16));
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
            MediaPlayer.Pause();
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

            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;

            // Gradually fade in or out depending on whether we are covered by the pause screen.
            if (coveredByOtherScreen)
                _pauseAlpha = Math.Min(_pauseAlpha + 1f / 32, 1);
            else
                _pauseAlpha = Math.Max(_pauseAlpha - 1f / 32, 0);

            if (IsActive)
            {
                //initailize keyboard states and check for exit
                pastKeyboard = currentKeyboard;
                currentKeyboard = Keyboard.GetState();

                // check for win condition
                if (collectedCount == 6) gameState = GameState.Won;

                /*check for pause condition
                if (pastKeyboard.IsKeyUp(Keys.Space) && currentKeyboard.IsKeyDown(Keys.Space))
                {
                    if (gameState == GameState.Active) gameState = GameState.Instruction;
                    else if (gameState == GameState.Instruction) gameState = GameState.Active;
                }
                */

                //update game elements if game is active
                if (gameState == GameState.Active)
                {
                    player.Update(gameTime, currentKeyboard);

                    //if player is alive, update game elements
                    if (player.Animation != PlayerAnimationState.Die)
                    {
                        float s = (float)gameTime.ElapsedGameTime.TotalSeconds;

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
                                GemSprite gem = new GemSprite(new Vector2((float)(random.NextDouble() * playableScreen.Width + playableScreen.Left) - 16, (float)(random.NextDouble() * playableScreen.Height + playableScreen.Top)));
                                gem.LoadContent(_content);
                                fieldGems.Add(gem);
                            }
                        }

                        //check for collisions between player and gems, collecting them if collided, and increasing fireball difficulty
                        foreach (GemSprite g in fieldGems)
                        {
                            if (g.Bounds.CollidesWith(player.Bounds))
                            {
                                GemColor color = g.GemColor;
                                int ind = (int)color;
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
                 * 
                if (keyboardState.IsKeyDown(Keys.Left) && _cannonRotation > -1 - MathHelper.PiOver2)
                    _cannonRotation-= 0.01f;

                if (keyboardState.IsKeyDown(Keys.Right) && _cannonRotation < 1 - MathHelper.PiOver2)
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

            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;

            // Our player and enemy are both actually just text strings.
            var spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();

            _spriteBatch.Draw(background, new Rectangle(screen.Left, screen.Top + 30, screen.Width, screen.Height - 80), Color.White);
            player.Draw(gameTime, _spriteBatch);
            foreach (GemSprite g in collectedGems) if (g.Collected) g.Draw(gameTime, _spriteBatch);
            foreach (GemSprite g in fieldGems) g.Draw(gameTime, _spriteBatch);
            foreach (FireballSprite f in fireballs) f.Draw(gameTime, _spriteBatch);
            _spriteBatch.DrawString(font, $"Time until next gems: {1 + (int)(gemSpawnThreshold - gemSpawnTimer)}", new Vector2(screen.Left + 30, screen.Top + 10), Color.Gold);
            _spriteBatch.DrawString(font, $"PRESS SPACE TO PAUSE", new Vector2(screen.Width / 2 - 110, screen.Bottom - 50), Color.Gold);

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
                    _spriteBatch.DrawString(font,
                        "Follow the following instructions to play\n\n" +
                        "    1. Use arrow keys to move\n" +
                        "    2. Collect all 6 gem colors to win\n" +
                        "    3. Gems already collected are shown at the top\n" +
                        "    4. The fireballs kill you! This restarts the game\n" +
                        "    5. Collecting an uncollected color of gem makes fireballs faster\n" +
                        "    6. Collecting ANY gem causes more fireballs to get made\n\n" +
                        "PRESS SPACE TO RESUME",
                        new Vector2(screen.Width / 2 - 220, screen.Height / 2 - 100), Color.Gold
                        );
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

            spriteBatch.End();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || _pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, _pauseAlpha / 2);

                ScreenManager.FadeBackBufferToBlack(alpha, Color.Black);
            }
        }

        /// <summary>
        /// Restart game, player, gems, and fireballs
        /// </summary>
        private void Restart()
        {
            fireballs.Clear();
            fieldGems.Clear();
            fireballSpawnTimer = 0;
            gemSpawnTimer = 0;
            fireballSpawnThreshold = 2;
            foreach (GemSprite g in collectedGems) g.Collected = false;
            player.Die();
        }
    }
}
