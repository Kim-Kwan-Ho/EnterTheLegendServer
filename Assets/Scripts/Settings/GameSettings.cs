namespace StandardData
{
    public static class GameRoomSize
    {
        public const int TeamBattleRoomSize = 6;
    }

    public static class PlayerStartSetting
    {
        public const string Nickname = "Unknown";
        public const uint Credit = 0;
        public const uint Gold = 1000;
        public static readonly int[] StartItems = new int[7] { 10000, 20000, 21000, 30000, 31000, 40000, 41000};
    }
}