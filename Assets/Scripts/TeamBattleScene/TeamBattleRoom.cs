using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using StandardData;
using UnityEngine;

public class TeamBattleRoom
{

    [Header("RoomInfo")]
    private static ushort _roomIndex = 0;
    private ushort _roomId;
    public ushort RoomId { get { return _roomId; } }

    private BattleRoomPlayerInfo[] _players;

    private bool _gameStarted;


    public TeamBattleRoom(BattleRoomPlayerInfo[] players)
    {
        _players = players;
        _gameStarted = false;
        _roomId = _roomIndex;
        _roomIndex++;

        stBattlePlayerInfo[] playersInfo = new stBattlePlayerInfo[_players.Length];
        
        for (ushort i = 0; i < _players.Length; i++)
        {
            playersInfo[i].Index = i;
            playersInfo[i].Nickname = _players[i].Module.Nickname;
            playersInfo[i].EquipedItems = DBManager.Instance.GetPlayerEquipedItems(_players[i].Module.Id);
        }

        for (ushort i = 0; i < _players.Length; i++)
        {
            players[i].Stat = DBManager.Instance.GetPlayerStat(playersInfo[i].EquipedItems);
            stCreateTeamBattleRoom createTeamBattleRoom = new stCreateTeamBattleRoom();
            createTeamBattleRoom.Header.MsgID = MessageIdTcp.CreateTeamBattleRoom;
            createTeamBattleRoom.Header.PacketSize = (ushort)Marshal.SizeOf(createTeamBattleRoom);
            createTeamBattleRoom.RoomId = _roomId;
            createTeamBattleRoom.PlayersInfo = playersInfo;
            createTeamBattleRoom.PlayerIndex = i;
            byte[] msg = Utilities.GetObjectToByte(createTeamBattleRoom);
            _players[i].Module.SendTcpMessage(msg);
        }

    }

    public void PlayerLoaded(ushort playerIndex, Vector2 position)
    {
        _players[playerIndex].Loaded = true;
        _players[playerIndex].Positions = position;
        for (int i = 0; i < _players.Length; i++)
        {
            if (!_players[i].Loaded)
                return;
        }

        _gameStarted = true;
        stTeamBattleRoomLoadInfo loadSucceed = new stTeamBattleRoomLoadInfo();
        loadSucceed.Header.MsgID = MessageIdTcp.TeamBattleRoomLoadInfo;
        loadSucceed.Header.PacketSize = (ushort)Marshal.SizeOf(loadSucceed);
        loadSucceed.IsAllSucceed = true;
        byte[] msg = Utilities.GetObjectToByte(loadSucceed);

        foreach (BattleRoomPlayerInfo player in _players)
        {
            player.Module.SendTcpMessage(msg);
        }
    }

    public void PlayerPositionChanged(stPlayerPosition position)
    {
        _players[position.PlayerIndex].Positions = new Vector2(position.PositionX, position.PositionY);
    }

    public void PlayerStateChanged(ushort playerIndex, ushort state)
    {
        stBattleRoomPlayerStateChangedFromServer
            stateChanged = new stBattleRoomPlayerStateChangedFromServer();

        stateChanged.Header.MsgID = MessageIdTcp.BattleRoomPlayerStateChangedFromServer;
        stateChanged.Header.PacketSize = (ushort)Marshal.SizeOf(stateChanged);
        stateChanged.PlayerIndex = playerIndex;
        stateChanged.State = state;
        byte[] msg = Utilities.GetObjectToByte(stateChanged);
        for (int i = 0; i < _players.Length; i++)
        {
            if (i == playerIndex)
                continue;
            _players[i].Module.SendTcpMessage(msg);
        }
    }
    public void PlayerDirectionChanged(ushort playerIndex, ushort direction)
    {
        stBattleRoomPlayerDirectionChangedFromServer
            directionChanged = new stBattleRoomPlayerDirectionChangedFromServer();

        directionChanged.Header.MsgID = MessageIdTcp.BattleRoomPlayerDirectionChangedFromServer;
        directionChanged.Header.PacketSize = (ushort)Marshal.SizeOf(directionChanged);
        directionChanged.PlayerIndex = playerIndex;
        directionChanged.Direction = direction;
        byte[] msg = Utilities.GetObjectToByte(directionChanged);
        for (int i = 0; i < _players.Length; i++)
        {
            if (i == playerIndex)
                continue;
            _players[i].Module.SendTcpMessage(msg);
        }
    }

    public void SendPlayerPositions()
    {
        if (!_gameStarted)
            return;

        stBattlePlayerPositionFromSever playerPositions = new stBattlePlayerPositionFromSever();
        playerPositions.Header.MsgID = MessageIdUdp.BattlePlayerPositionFromServer;
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
            player.Module.SendUdpMessage(msg);
        }
    }

    

}


public class BattleRoomPlayerInfo
{
    public NetworkModule Module = null;
    public bool Loaded = false;
    public Vector2 Positions;
    public PlayerStat Stat;

}

public class PlayerStat
{
    public ushort Hp;
    public ushort Def;
    public ushort Attack;
}

