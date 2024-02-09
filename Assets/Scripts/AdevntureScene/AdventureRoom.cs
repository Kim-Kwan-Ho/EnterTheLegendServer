using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using StandardData;
using UnityEngine;

public class AdventureRoom
{

    [Header("RoomInfo")]
    private static ushort _roomIndex = 0;
    private ushort _roomId;
    public ushort RoomId { get { return _roomId; } }

    private AdventureRoomPlayerInfo[] _players;

    private bool _gameStarted;


    public AdventureRoom(AdventureRoomPlayerInfo[] players)
    {
        _players = players;
        _gameStarted = false;
        _roomId = _roomIndex;
        _roomIndex++;

        stAdventurePlayerInfo[] playersInfo = new stAdventurePlayerInfo[_players.Length];

        for (ushort i = 0; i < _players.Length; i++)
        {
            //_playerPositions.PlayerPosition[i].PlayerIndex = i;
            playersInfo[i].Name = _players[i].Module.Name;
            playersInfo[i].Index = i;
        }

        stCreateAdventureRoom createAdventureRoom = new stCreateAdventureRoom();
        createAdventureRoom.Header.MsgID = MessageIdTcp.CreateAdventureRoom;
        createAdventureRoom.Header.PacketSize = (ushort)Marshal.SizeOf(createAdventureRoom);
        createAdventureRoom.RoomId = _roomId;
        createAdventureRoom.playersInfo = playersInfo;
        byte[] msg = Utilities.GetObjectToByte(createAdventureRoom);
        foreach (AdventureRoomPlayerInfo player in _players)
        {
            player.Module.SendTcpMessage(msg);
        }
    }

    public void PlayerLoaded(ushort playerIndex)
    {
        _players[playerIndex].Loaded = true;
        for (int i = 0; i < _players.Length; i++)
        {
            if (!_players[i].Loaded)
                return;
        }

        _gameStarted = true;
        stAdventureRoomLoadInfo loadSucceed = new stAdventureRoomLoadInfo();
        loadSucceed.Header.MsgID = MessageIdTcp.AdventureRoomLoadInfo;
        loadSucceed.Header.PacketSize = (ushort)Marshal.SizeOf(loadSucceed);
        loadSucceed.IsAllSucceed = true;
        byte[] msg = Utilities.GetObjectToByte(loadSucceed);

        TestDebugLog.DebugLog("RoomID: " + _roomId);
        foreach (AdventureRoomPlayerInfo player in _players)
        {
            player.Module.SendTcpMessage(msg);
        }

    }

    public void GetPlayerChangedPositions(stPlayerPosition position)
    {
        _players[position.PlayerIndex].Positions = new Vector2(position.PositionX, position.PositionY);
    }



    public void UpdatePlayerPositions()
    {
        if (!_gameStarted)
            return;

        stAdventurePlayerPositionFromSever playerPositions = new stAdventurePlayerPositionFromSever();
        playerPositions.Header.MsgID = MessageIdUdp.AdventurePlayerPositionFromServer;
        stPlayerPosition[] positions = new stPlayerPosition[_players.Length];
        for (ushort i = 0; i < _players.Length; i++)
        {
            positions[i].PlayerIndex = i;
            positions[i].PositionX = _players[i].Positions.x;
            positions[i].PositionY = _players[i].Positions.y;
        }
        playerPositions.PlayerPosition = positions;
        byte[] msg = Utilities.GetObjectToByte(playerPositions);
        foreach (var player in _players)
        {
            if (player.Module.IsConnected())
                player.Module.SendUdpMessage(msg);
        }
    }


}


public class AdventureRoomPlayerInfo
{
    public NetworkModule Module = null;
    public bool Loaded = false;
    public Vector2 Positions = Vector2.zero;
    // 플레이어 정보
}
