using Game1;
using Game1.Screens;
using Game1.StateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game1
{

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
        private readonly ScreenManager _screenManager;
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

        //audio variables
        private Song _backgroundMusic;
        #endregion

        /// <summary>
        /// constructor for the game
        /// </summary>
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            var screenFactory = new ScreenFactory();
            Services.AddService(typeof(IScreenFactory), screenFactory);

            _screenManager = new ScreenManager(this);
            Components.Add(_screenManager);

            AddInitialScreens();
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

            /* Uncomment to allow exit from anywhere
            if (currentKeyboard.IsKeyDown(Keys.Escape))
                Exit();
            */

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
            base.Draw(gameTime);
        }

        private void AddInitialScreens()
        {
            _screenManager.AddScreen(new BackgroundScreen(), null);
            _screenManager.AddScreen(new MainMenuScreen(), null);
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
        /*
        

        protected override void LoadContent()
        {
            
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
               // The real drawing happens inside the ScreenManager component
        }
        */
    }
}
