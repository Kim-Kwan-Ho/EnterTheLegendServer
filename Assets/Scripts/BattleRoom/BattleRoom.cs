using StandardData;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

public class BattleRoom
{
    protected BattleRoomPlayer[] _players;
    protected ushort _roomId;
    public ushort RoomId { get { return _roomId; } }
    protected bool _gameStarted = false;


    public void PlayerLoaded(ushort playerIndex, Vector2 position)
    {
        _players[playerIndex].SetLoaded();
        _players[playerIndex].SetPosition(position);
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

        foreach (BattleRoomPlayer player in _players)
        {
            player.SendTcpMessage(msg);
        }
        Thread.Sleep(100);
        _gameStarted = true;
    }

    public void PlayerPositionChanged(stPlayerPosition position)
    {
        _players[position.PlayerIndex].SetPosition(new Vector2(position.PositionX, position.PositionY));
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
        _players[playerIndex].SetDirection((Direction)direction);
        SendPlayersMessage(directionChanged, false, playerIndex);
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
            player.SendUdpMessage(msg);
        }
    }
    protected void SendPlayersMessage<T>(T str, bool sendAll = false, int index = 0) where T : struct
    {
        byte[] msg = Utilities.GetObjectToByte(str);

        if (sendAll)
        {
            for (int i = 0; i < _players.Length; i++)
            {
                _players[i].SendTcpMessage(msg);
            }
        }
        else
        {
            for (int i = 0; i < _players.Length; i++)
            {
                if (i == index)
                    continue;
                _players[i].SendTcpMessage(msg);
            }
        }
    }

    public virtual void PlayerOnAttack(ushort playerIndex)
    {
        stBattleRoomPlayerAttackFromServer playerAttack = new stBattleRoomPlayerAttackFromServer();
        playerAttack.Header.MsgID = MessageIdTcp.TeamBattleRoomPlayerAttackToServer;
        playerAttack.Header.PacketSize = (ushort)Marshal.SizeOf(playerAttack);
        playerAttack.PlayerIndex = playerIndex;
        SendPlayersMessage(playerAttack, false, playerIndex);
    }
    protected virtual void CheckHit(bool blueTeam, Vector2 attackPos, Direction direction, float distance, float attackAngle)
    {
    }
}

public class BattleRoomPlayer
{
    private NetworkModule _module;
    private bool _loaded = false;
    public bool Loaded { get { return _loaded; } }
    private Vector2 _position;
    public Vector2 Position { get { return _position; } }
    private State _state;
    public State State { get { return _state; } }
    private Direction _direction;
    public Direction Direction { get { return _direction; } }

    public PlayerStat Stat;

    public BattleRoomPlayer(NetworkModule module)
    {
        _module = module;
    }


    public void SetLoaded()
    {
        _loaded = true;
    }
    public void SetPosition(Vector2 position)
    {
        _position = position;
    }
    public void SetState(State state)
    {
        _state = state;
    }

    public void SetDirection(Direction direction)
    {
        _direction = direction;
    }
    public string GetName()
    {
        return _module.Nickname;
    }

    public string GetId()
    {
        return _module.Id;
    }
    public void SendTcpMessage(byte[] msg)
    {
        _module.SendTcpMessage(msg);
    }
    public void SendUdpMessage(byte[] msg)
    {
        _module.SendUdpMessage(msg);
    }


}

public class PlayerStat
{
    public ushort Hp;
    public ushort Def;
    public ushort Attack;
}

