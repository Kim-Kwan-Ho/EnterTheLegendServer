using System.Runtime.InteropServices;
namespace StandardData
{
    public static class NetworkSize
    {

        public const int HeaderSize = 4;
        public const int BufferSize = 2048;
        public const int TempBufferSize = 1048;
        public const int MaxNameSize = 10;
    }

    public static class GameRoomSize
    {
        public const int BattleRoomSize = 4;
        public const int AdventureRoomSize = 3;
    }
    public static class MessageIdTcp
    {
        public const ushort RequestForMatch = 1;
        public const ushort AdventureRoomLoaded = 2;
        public const ushort CreateAdventureRoom = 3;
        public const ushort AdventurePlayerLoadInfo = 4;
        public const ushort AdventureRoomLoadInfo = 5;
        public const ushort SetUdpPort = 6;
        public const ushort AdventureRoomPlayerStateChangedToServer = 7;
        public const ushort AdventureRoomPlayerStateChangedFromServer = 77;
        public const ushort AdventureRoomPlayerDirectionChangedToServer = 8;
        public const ushort AdventureRoomPlayerDirectionChangedFromServer = 88;

    }

    public static class MessageIdUdp
    {
        public const ushort AdventurePlayerPositionToServer = 1;
        public const ushort AdventurePlayerPositionFromServer = 2;

    }
    public static class UdpSendCycle
    {
        public const float AdventureRoomSendCycle = 0.016f;
    }
    public enum GameRoomType
    {
        BattleRoom,
        AdventureRoom,

    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct stHeaderTcp
    {
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public ushort MsgID;
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public ushort PacketSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct stSetUdpPort
    {
        public stHeaderTcp Header;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
        public string Name;
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public ushort UdpPortSend;
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public ushort UdpPortReceive;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct stRequestForMatch
    {
        public stHeaderTcp Header;
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public ushort MatchType;

    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct stAdventureRoomPlayerLoadInfo
    {
        public stHeaderTcp Header;
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public ushort RoomId;
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public ushort PlayerIndex;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct stCreateAdventureRoom
    {
        public stHeaderTcp Header;
        public ushort RoomId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)GameRoomSize.AdventureRoomSize)]
        public stAdventurePlayerInfo[] playersInfo;
    }
    public struct stAdventurePlayerInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)(NetworkSize.MaxNameSize))]
        public string Name;
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public ushort Index;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct stAdventureRoomPlayerInfo
    {
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public ushort RoomId;
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public ushort PlayerIndex;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct stAdventureRoomLoadInfo
    {
        public stHeaderTcp Header;
        [MarshalAs(UnmanagedType.Bool, SizeConst = 1)]
        public bool IsAllSucceed;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct stAdventureRoomPlayerStateChangedToServer
    {
        public stHeaderTcp Header;
        public stAdventureRoomPlayerInfo PlayerInfo;
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public ushort State;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct stAdventureRoomPlayerStateChangedFromServer
    {
        public stHeaderTcp Header;
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public ushort PlayerIndex;
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public ushort State;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct stAdventureRoomPlayerDirectionChangedToServer
    {
        public stHeaderTcp Header;
        public stAdventureRoomPlayerInfo PlayerInfo;
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public ushort Direction;
    }
    public struct stAdventureRoomPlayerDirectionChangedFromServer
    {
        public stHeaderTcp Header;
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public ushort PlayerIndex;
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public ushort Direction;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct stAdventureRoomPlayerOnAttack
    {
        public stHeaderTcp Header;
        public stAdventureRoomPlayerInfo PlayerInfo;
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public ushort State;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct stHeaderUdp
    {
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public ushort MsgID;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct stPlayerPosition
    {
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public ushort PlayerIndex;
        [MarshalAs(UnmanagedType.R4, SizeConst = 4)]
        public float PositionX;
        [MarshalAs(UnmanagedType.R4, SizeConst = 4)]
        public float PositionY;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct stAdventurePlayerPositionToServer
    {
        public stHeaderUdp Header;
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public ushort RoomType;
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public ushort RoomId;
        public stPlayerPosition PlayerPosition;
    }

    public struct stAdventurePlayerPositionFromSever
    {
        public stHeaderUdp Header;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = GameRoomSize.AdventureRoomSize)]
        public stPlayerPosition[] PlayerPosition;

    }



}