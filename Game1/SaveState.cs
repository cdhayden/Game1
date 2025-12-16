using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;


namespace Game1
{
    public class SaveState
    {

        public bool LevelOneUnlocked { get; set; }
        public bool LevelTwoUnlocked { get; set; }
        public bool LevelThreeUnlocked { get; set; }

        public SaveState(bool leveloneunlocked, bool leveltwounlocked, bool levelthreeunlocked) 
        { 
            LevelOneUnlocked = leveloneunlocked;
            LevelTwoUnlocked = leveltwounlocked;
            LevelThreeUnlocked = levelthreeunlocked;
        }
        public string Serialize()
        {
            return System.Text.Json.JsonSerializer.Serialize(this);
        }
    }
}
