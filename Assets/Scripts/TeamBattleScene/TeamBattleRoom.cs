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

    private TeamBattleRoomPlayerInfo[] _players;

    private bool _gameStarted;


    public TeamBattleRoom(TeamBattleRoomPlayerInfo[] players)
    {
        _players = players;
        _gameStarted = false;
        _roomId = _roomIndex;
        _roomIndex++;

        stTeamBattlePlayerInfo[] playersInfo = new stTeamBattlePlayerInfo[_players.Length];

        for (ushort i = 0; i < _players.Length; i++)
        {
            //_playerPositions.PlayerPosition[i].PlayerIndex = i;
            playersInfo[i].Name = _players[i].Module.Nickname;
            playersInfo[i].Index = i;
        }

        stCreateTeamBattleRoom createTeamBattleRoom = new stCreateTeamBattleRoom();
        createTeamBattleRoom.Header.MsgID = MessageIdTcp.CreateTeamBattleRoom;
        createTeamBattleRoom.Header.PacketSize = (ushort)Marshal.SizeOf(createTeamBattleRoom);
        createTeamBattleRoom.RoomId = _roomId;
        createTeamBattleRoom.playersInfo = playersInfo;
        byte[] msg = Utilities.GetObjectToByte(createTeamBattleRoom);
        foreach (TeamBattleRoomPlayerInfo player in _players)
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
        stTeamBattleRoomLoadInfo loadSucceed = new stTeamBattleRoomLoadInfo();
        loadSucceed.Header.MsgID = MessageIdTcp.TeamBattleRoomLoadInfo;
        loadSucceed.Header.PacketSize = (ushort)Marshal.SizeOf(loadSucceed);
        loadSucceed.IsAllSucceed = true;
        byte[] msg = Utilities.GetObjectToByte(loadSucceed);

        foreach (TeamBattleRoomPlayerInfo player in _players)
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
        stTeamBattleRoomPlayerStateChangedFromServer
            stateChanged = new stTeamBattleRoomPlayerStateChangedFromServer();

        stateChanged.Header.MsgID = MessageIdTcp.TeamBattleRoomPlayerStateChangedFromServer;
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
        stTeamBattleRoomPlayerDirectionChangedFromServer
            directionChanged = new stTeamBattleRoomPlayerDirectionChangedFromServer();

        directionChanged.Header.MsgID = MessageIdTcp.TeamBattleRoomPlayerDirectionChangedFromServer;
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

        stTeamBattlePlayerPositionFromSever playerPositions = new stTeamBattlePlayerPositionFromSever();
        playerPositions.Header.MsgID = MessageIdUdp.TeamBattlePlayerPositionFromServer;
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


public class TeamBattleRoomPlayerInfo
{
    public NetworkModule Module = null;
    public bool Loaded = false;
    public Vector2 Positions = Vector2.zero;
    // �÷��̾� ����
}
