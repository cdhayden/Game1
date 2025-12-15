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
    public class LevelThreeScreen : GameplayScreen
    {
        

        public LevelThreeScreen() : base()
        {
            
        }

        // Load graphics content for the game
        public override void Activate()
        {
            base.Activate();

            gemSpawnThreshold = 12;
            fireballSpawnThreshold = 4;

            //load screen dimensions for convenience
            playableScreen.Left = screen.Left + 85;
            playableScreen.Width = screen.Width - 165;
            playableScreen.Top = screen.Top + 80;
            playableScreen.Height = screen.Height - 195;

            backgroundRectangle = new Rectangle(screen.Left + 35, screen.Top - 20, screen.Width - 45, screen.Height - 40);


            obstacles =
                [
                    new CollisionRectangle(220, 140, 160, 1),
                    new CollisionRectangle(425, 140, 160, 1),

                    new CollisionRectangle(212, 210, 50, 75),
                    new CollisionRectangle(539, 210, 50, 75),

                    new CollisionRectangle(210, playableScreen.Top + 15, 5, 40),
                    new CollisionRectangle(585, playableScreen.Top + 15, 5, 40),

                    new CollisionRectangle(425, 230, 320, 20),
                    new CollisionRectangle(60, 230, 315, 20)
                ];

            background = _content.Load<Texture2D>("Sample_Map3");
            player = new PlayerSprite(new Vector2(playableScreen.Right - 48, playableScreen.Bottom - 30), playableScreen, obstacles);
            player.LoadContent(_content);
        }

        protected override void WinLevel()
        {
            LoadingScreen.Load(ScreenManager, true, ControllingPlayer, true, new MainMenuScreen());
        }
    }
}
