using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Game1
{
    /// <summary>
    /// defines a struct for storing screen dimensions conveniently
    /// </summary>
    public struct ScreenDimensions 
    {
        public int Left;
        public int Right => Left + Width;
        public int Top;
        public int Bottom => Top + Height;

        public int Height;
        public int Width;
    }

    /// <summary>
    /// defines an enum for storing the game state
    /// </summary>
    public enum GameState 
    { 
        Active,
        Instruction,
        Won
    }

    public class Game1 : Game
    {
        #region Variables
        //initialize random
        private Random random = new Random();

        //set up timers and time thresholds for spawning gems and fireballs
        private float gemSpawnThreshold = 6;
        private float gemSpawnTimer = 0;

        private float fireballSpawnTimer = 0;
        private float fireballSpawnThreshold = 2;

        //declare system variables
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

        private List<GemSprite> fieldGems = new List<GemSprite>();
        private List<FireballSprite> fireballs = new List<FireballSprite>();

        #endregion

        /// <summary>
        /// constructor for the game
        /// </summary>
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        /// <summary>
        /// initailzies the game and its game state
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            gameState = GameState.Active; 
            base.Initialize();
        }

        /// <summary>
        /// loads graphical content for the game
        /// </summary>
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            //load screen dimensions for convenience
            screen.Left = GraphicsDevice.Viewport.X;
            screen.Width = GraphicsDevice.Viewport.Width;
            screen.Top = GraphicsDevice.Viewport.Y;
            screen.Height = GraphicsDevice.Viewport.Height;
            playableScreen.Left = screen.Left + 95;
            playableScreen.Width = screen.Width - 185;
            playableScreen.Top = screen.Top + 120;
            playableScreen.Height = screen.Height - 250;

            //load persistent sprites
            font = Content.Load<SpriteFont>("Saira");
            background = Content.Load<Texture2D>("Sample_Map4");
            player = new PlayerSprite(new Vector2(screen.Right / 2, screen.Bottom / 2), playableScreen);
            player.LoadContent(Content);
            for (int i = 0; i < 6; i++)
            {
                collectedGems[i] = new GemSprite(i, new Vector2((screen.Width / 2) - (32 * (collectedGems.Length / 2)) + (32 * i), screen.Top + 16));
                collectedGems[i].LoadContent(Content);
            }
        }

        /// <summary>
        /// Updates the game state and all game elements
        /// </summary>
        /// <param name="gameTime">the gametime</param>
        protected override void Update(GameTime gameTime)
        {
            //initailize keyboard states and check for exit
            pastKeyboard = currentKeyboard;
            currentKeyboard = Keyboard.GetState();
            if (currentKeyboard.IsKeyDown(Keys.Escape))
                Exit();

            // check for win condition
            if (collectedCount == 6) gameState = GameState.Won;

            //check for pause condition
            if (pastKeyboard.IsKeyUp(Keys.Space) && currentKeyboard.IsKeyDown(Keys.Space)) 
            {
                if (gameState == GameState.Active) gameState = GameState.Instruction;
                else if (gameState == GameState.Instruction) gameState = GameState.Active;
            }

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
                            gem.LoadContent(Content);
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
                        f.LoadContent(Content);
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
                base.Update(gameTime);
        }

        /// <summary>
        /// Draw the game elements
        /// </summary>
        /// <param name="gameTime">the gametime</param>
        protected override void Draw(GameTime gameTime)
        {
            //set background to black
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();

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
                    _spriteBatch.DrawString(font, $"Time until next gems: {1+(int)(gemSpawnThreshold - gemSpawnTimer)}", new Vector2(screen.Left + 30, screen.Top + 10), Color.Gold);
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
                        new Vector2(screen.Width/2 - 220, screen.Height / 2 - 100), Color.Gold
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
            
            _spriteBatch.End();
            base.Draw(gameTime);
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
