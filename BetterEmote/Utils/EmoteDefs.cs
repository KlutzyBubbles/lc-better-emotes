using System;

namespace BetterEmote.Utils
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
        Salute = 8,
        Prisyadka = 9,
        Sign = 10
    }
    public enum DoubleEmote : int
    {
        Double_Clap = 1004,
        Double_Middle_Finger = 1003
    }
    public enum AltEmote : int
    {
        Sign_Text = 1010
    }

    internal class EmoteDefs
    {
        public static int getEmoteNumber(Emote emote)
        {
            return (int)emote;
        }
        public static int getEmoteNumber(DoubleEmote emote)
        {
            return (int)emote;
        }
        public static int getEmoteNumber(AltEmote emote)
        {
            return (int)emote;
        }
        public static int getEmoteNumber(string name)
        {
            return (int)Enum.Parse(typeof(Emote), name);
        }
        public static Emote getEmote(string name)
        {
            try
            {
                return (Emote)Enum.Parse(typeof(Emote), name);
            }
            catch (Exception)
            {
                try
                {
                    DoubleEmote dEmote = (DoubleEmote)Enum.Parse(typeof(DoubleEmote), name);
                    switch (dEmote)
                    {
                        case DoubleEmote.Double_Middle_Finger:
                            return Emote.Middle_Finger;
                        case DoubleEmote.Double_Clap:
                            return Emote.Clap;
                        default:
                            return Emote.Dance;
                    }
                } catch (Exception)
                {
                    AltEmote aEmote = (AltEmote)Enum.Parse(typeof(AltEmote), name);
                    switch (aEmote)
                    {
                        case AltEmote.Sign_Text:
                            return Emote.Sign;
                        default:
                            return Emote.Dance;
                    }
                }
            }
        }
        public static DoubleEmote getDoubleEmote(string name)
        {
            return (DoubleEmote)Enum.Parse(typeof(DoubleEmote), name);
        }
        public static AltEmote getAltEmote(string name)
        {
            return (AltEmote)Enum.Parse(typeof(AltEmote), name);
        }
        public static int getEmoteCount()
        {
            return Enum.GetNames(typeof(Emote)).Length;
        }

        public static int normalizeEmoteNumber(int number)
        {
            if (number >= 1000)
            {
                number -= 1000;
            }
            return number;
        }
    }
}
