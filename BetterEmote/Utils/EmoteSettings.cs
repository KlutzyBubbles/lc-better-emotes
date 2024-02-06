using System;
using System.Collections.Generic;
using System.Text;

namespace BetterEmote.Utils
{
    public class EmoteSettings
    {
        public bool enabled = true;
        public bool inventoryCheck = false;

        public bool hasDouble = false;
        public bool doubleEnabled = false;
        public DoubleEmote doubleID;

        public InputBind keybinds = new InputBind("", "");

        public EmoteSettings(InputBind keybinds, DoubleEmote doubleID, bool doubleEnabled)
        {
            hasDouble = true;
            this.doubleEnabled = doubleEnabled;
            this.doubleID = doubleID;
            this.keybinds = keybinds;
        }
        public EmoteSettings(InputBind keybinds)
        {
            this.keybinds = keybinds;
        }
    }
}
