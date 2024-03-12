namespace StandardData
{
    public static class GameRoomSize
    {
        public const int BattleRoomSize = 4;
        public const int AdventureRoomSize = 3;
    }

    public static class PlayerStartSetting
    {
        public const string Nickname = "Unknown";
        public const uint Credit = 0;
        public const uint Gold = 1000;
        public static readonly int[] ItemId = new int[4] { 10000, 20000, 30000, 40000 };
    }
}