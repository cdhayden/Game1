using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Screens
{
    public class StageSelectScreen : MenuScreen
    {
        private Texture2D _level1;
        private Texture2D _level2;
        private Texture2D _level3;

        public StageSelectScreen() : base("Stage Select")
        {
            var level1Selection = new MenuEntry("Level 1");
            var level2Selection = new MenuEntry("Level 2");
            var level3Selection = new MenuEntry("Level 3");

            var goBack = new MenuEntry("Back to Main Menu");


            level1Selection.Selected += Level1Selected;
            level2Selection.Selected += Level2Selected;
            level3Selection.Selected += Level3Selected;
            goBack.Selected += OnCancel;


            if(LevelOneScreen.Unlocked) MenuEntries.Add(level1Selection);
            if (LevelTwoScreen.Unlocked) MenuEntries.Add(level2Selection);
            if (LevelThreeScreen.Unlocked) MenuEntries.Add(level3Selection);
            MenuEntries.Add(goBack);

        }

        public override void Activate()
        {
            _level1 = ScreenManager.Game.Content.Load<Texture2D>("Sample_Map4");
            _level2 = ScreenManager.Game.Content.Load<Texture2D>("Sample_Map2");
            _level3 = ScreenManager.Game.Content.Load<Texture2D>("Sample_Map3");
            base.Activate();
            
        }

        private void Level1Selected(object sender, PlayerIndexEventArgs e)
        {
            Game1.CurrentMusic = MusicType.Gameplay;
            LoadingScreen.Load(ScreenManager, true, e.PlayerIndex, false, new LevelOneScreen());
        }

        // This uses the loading screen to transition from the game back to the main menu screen.
        private void Level2Selected(object sender, PlayerIndexEventArgs e)
        {
            Game1.CurrentMusic = MusicType.Gameplay;
            LoadingScreen.Load(ScreenManager, true, e.PlayerIndex, false, new LevelTwoScreen());
        }

        private void Level3Selected(object sender, PlayerIndexEventArgs e)
        {
            Game1.CurrentMusic = MusicType.Gameplay;
            LoadingScreen.Load(ScreenManager, true, e.PlayerIndex, false, new LevelThreeScreen());
        }

        public override void Draw(GameTime gameTime)
        {
            var graphics = ScreenManager.GraphicsDevice;
            graphics.Clear(ClearOptions.Target, Color.Black, 0, 0);
            var viewport = graphics.Viewport;
            Rectangle imageBox = new Rectangle(viewport.X + viewport.Width / 2 - 150, viewport.Y + viewport.Height - 225, 300, 200);
            Texture2D texture = GetTexture();

            if (texture != null) 
            {
                var spriteBatch = ScreenManager.SpriteBatch;
                spriteBatch.Begin();
                spriteBatch.Draw(texture, imageBox, Color.White);
                spriteBatch.End();
            }
            base.Draw(gameTime);
        }

        private Texture2D? GetTexture() 
        { 
            string selectedEntry = MenuEntries[_selectedEntry].Text;
            if (selectedEntry.Equals("Level 1")) return _level1;
            else if (selectedEntry.Equals("Level 2")) return _level2;
            else if (selectedEntry.Equals("Level 3")) return _level3;
            else return null;
        }
    }
}
