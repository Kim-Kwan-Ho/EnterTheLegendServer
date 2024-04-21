using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
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
            players[i].Stat = DBManager.Instance.GetPlayerStat(playersInfo[i].EquipedItems);
            playersInfo[i].Hp = players[i].Stat.Hp;
        }

        for (ushort i = 0; i < _players.Length; i++)
        {
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
        _players[playerIndex].Position = position;
        for (int i = 0; i < _players.Length; i++)
        {
            if (!_players[i].Loaded)
                return;
        }

        stTeamBattleRoomLoadInfo loadSucceed = new stTeamBattleRoomLoadInfo();
        loadSucceed.Header.MsgID = MessageIdTcp.TeamBattleRoomLoadInfo;
        loadSucceed.Header.PacketSize = (ushort)Marshal.SizeOf(loadSucceed);
        loadSucceed.IsAllSucceed = true;
        byte[] msg = Utilities.GetObjectToByte(loadSucceed);

        foreach (BattleRoomPlayerInfo player in _players)
        {
            player.Module.SendTcpMessage(msg);
        }
        Thread.Sleep(100);
        _gameStarted = true;
    }

    public void PlayerPositionChanged(stPlayerPosition position)
    {
        _players[position.PlayerIndex].Position = new Vector2(position.PositionX, position.PositionY);
    }

    public void PlayerStateChanged(ushort playerIndex, ushort state)
    {
        stBattleRoomPlayerStateChangedFromServer
            stateChanged = new stBattleRoomPlayerStateChangedFromServer();

        stateChanged.Header.MsgID = MessageIdTcp.BattleRoomPlayerStateChangedFromServer;
        stateChanged.Header.PacketSize = (ushort)Marshal.SizeOf(stateChanged);
        stateChanged.PlayerIndex = playerIndex;
        stateChanged.State = state;
        SendPlayersMessage(stateChanged, false, playerIndex);
    }
    public void PlayerDirectionChanged(ushort playerIndex, ushort direction)
    {
        stBattleRoomPlayerDirectionChangedFromServer
            directionChanged = new stBattleRoomPlayerDirectionChangedFromServer();

        directionChanged.Header.MsgID = MessageIdTcp.BattleRoomPlayerDirectionChangedFromServer;
        directionChanged.Header.PacketSize = (ushort)Marshal.SizeOf(directionChanged);
        directionChanged.PlayerIndex = playerIndex;
        directionChanged.Direction = direction;
        _players[playerIndex].Direction = (Direction)direction;
        SendPlayersMessage(directionChanged, false, playerIndex);
    }

    public void PlayerOnAttack(ushort playerIndex)
    {
        stBattleRoomPlayerAttackFromServer playerAttack = new stBattleRoomPlayerAttackFromServer();
        playerAttack.Header.MsgID = MessageIdTcp.TeamBattleRoomPlayerAttackToServer;
        playerAttack.Header.PacketSize = (ushort)Marshal.SizeOf(playerAttack);
        playerAttack.PlayerIndex = playerIndex;
        SendPlayersMessage(playerAttack, false, playerIndex);

        CheckHit(playerIndex < 3, _players[playerIndex].Position, _players[playerIndex].Direction, 2, 45);
    }

    private void CheckHit(bool blueTeam, Vector2 attackPos, Direction direction, float distance, float attackAngle)
    {
        Vector2 forward = new Vector2();
        switch (direction)
        {
            case Direction.Down:
                forward = Vector2.down;
                break;
            case Direction.Left:
                forward = Vector2.left;
                break;
            case Direction.Right:
                forward = Vector2.right;
                break;
            case Direction.Up:
                forward = Vector2.up;
                break;
        }
        int teamStartIndex = blueTeam ? GameRoomSize.TeamBattleRoomSize / 2 : 0;
        int teamEndIndex = blueTeam ? GameRoomSize.TeamBattleRoomSize : GameRoomSize.TeamBattleRoomSize / 2;

        for (int i = teamStartIndex; i < teamEndIndex; i++)
        {
            Vector2 dir = _players[i].Position - attackPos;
            if (dir.magnitude <= distance)
            {
                float dot = Vector2.Dot(dir.normalized, forward);
                float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
                if (angle <= attackAngle)
                {
                    stBattleRoomPlayerTakeDamage takeDamage = new stBattleRoomPlayerTakeDamage();
                    takeDamage.Header.MsgID = MessageIdTcp.BattleRoomPlayerTakeDamage;
                    takeDamage.Header.PacketSize = (ushort)Marshal.SizeOf(takeDamage);
                    takeDamage.PlayerIndex = (ushort)i;
                    takeDamage.Damage = 0;
                    SendPlayersMessage(takeDamage, true);
                }
            }
        }
    }



    private void SendPlayersMessage<T>(T str, bool sendAll = false, int index = 0) where T : struct
    {
        byte[] msg = Utilities.GetObjectToByte(str);

        if (sendAll)
        {
            for (int i = 0; i < _players.Length; i++)
            {
                _players[i].Module.SendTcpMessage(msg);
            }
        }
        else
        {
            for (int i = 0; i < _players.Length; i++)
            {
                if (i == index)
                    continue;
                _players[i].Module.SendTcpMessage(msg);
            }
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
            positions[i].PositionX = _players[i].Position.x;
            positions[i].PositionY = _players[i].Position.y;
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
    public Vector2 Position;
    public Direction Direction;
    public PlayerStat Stat;

}

public class PlayerStat
{
    public ushort Hp;
    public ushort Def;
    public ushort Attack;
}

public class PlayerEquipment
{

}

public class Player
{
    private ushort _hp;
    public ushort Hp { get { return _hp; } }


    private WeaponEquipmentSO _weaponEquip;


}