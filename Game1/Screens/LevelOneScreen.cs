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
    public class LevelOneScreen : GameplayScreen
    {
        

        public LevelOneScreen() : base()
        {
            
        }

        // Load graphics content for the game
        public override void Activate()
        {
            base.Activate();

            obstacles = [];

            //load screen dimensions for convenience
            playableScreen.Left = screen.Left + 95;
            playableScreen.Width = screen.Width - 185;
            playableScreen.Top = screen.Top + 120;
            playableScreen.Height = screen.Height - 250;
 
            background = _content.Load<Texture2D>("Sample_Map4");
            player = new PlayerSprite(new Vector2(screen.Right / 2, screen.Bottom / 2), playableScreen, obstacles);
            player.LoadContent(_content);
        }

        protected override void WinLevel()
        {
            LoadingScreen.Load(ScreenManager, true, ControllingPlayer, true, new LevelOneScreen());
        }
    }
}
