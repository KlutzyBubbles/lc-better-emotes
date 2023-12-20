using System;

namespace BetterEmote
{
    public enum Emote : int
    {
        Dance = 1,
        Point = 2,
        Middle_Finger = 3,
        Clap = 4,
        Shy = 5,
        Griddy = 6,
        Twerk = 7,
        Salute = 8
    }
    internal class EmoteDefs
    {
        public static int getEmoteNumber(Emote emote)
        {
            return (int)emote;
        }
        public static int getEmoteNumber(string name)
        {
            return (int)Enum.Parse(typeof(Emote), name);
        }
        public static Emote getEmote(string name)
        {
            return (Emote)Enum.Parse(typeof(Emote), name);
        }
        public static int getEmoteCount()
        {
            return Enum.GetNames(typeof(Emote)).Length;
        }
    }
}
