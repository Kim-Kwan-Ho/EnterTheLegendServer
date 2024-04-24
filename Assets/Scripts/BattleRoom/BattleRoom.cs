using System;
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

    public virtual void PlayerAttack(ushort playerIndex)
    {
        stBattleRoomPlayerAttackFromServer playerAttack = new stBattleRoomPlayerAttackFromServer();
        playerAttack.Header.MsgID = MessageIdTcp.TeamBattleRoomPlayerAttackToServer;
        playerAttack.Header.PacketSize = (ushort)Marshal.SizeOf(playerAttack);
        playerAttack.PlayerIndex = playerIndex;
        SendPlayersMessage(playerAttack, false, playerIndex);
        bool isBlueTeam = playerIndex < _players.Length / 2;
        CheckPlayerOnAttack(playerIndex, isBlueTeam ? _players.Length / 2 : 0, isBlueTeam ? _players.Length : _players.Length / 2);
    }

    private void CheckPlayerOnAttack(ushort attackPlayer, int startIndex, int endIndex)
    {
        for (int i = startIndex; i < endIndex; i++)
        {
            if (_players[attackPlayer].CheckHit(_players[i].Position))
            {
                stBattleRoomPlayerTakeDamage takeDamage = new stBattleRoomPlayerTakeDamage();
                takeDamage.Header.MsgID = MessageIdTcp.BattleRoomPlayerTakeDamage;
                takeDamage.Header.PacketSize = (ushort)Marshal.SizeOf(takeDamage);
                takeDamage.PlayerIndex = (ushort)i;
                takeDamage.Damage = _players[i].TakeDamage(_players[attackPlayer].Attack);
                SendPlayersMessage(takeDamage, true);
            }
        }
    }



}

public class BattleRoomPlayer
{
    private NetworkModule _module;


    [Header("Character Info")]
    private bool _loaded = false;
    public bool Loaded { get { return _loaded; } }
    private Vector2 _position;
    public Vector2 Position { get { return _position; } }
    private CharacterState _state;
    public CharacterState State { get { return _state; } }
    private Direction _direction;
    public Direction Direction { get { return _direction; } }


    [Header("Stats")]
    private ushort _hp;
    public ushort Hp { get { return _hp; } }
    private ushort _defense;
    public ushort Defense { get { return _defense; } }
    private ushort _attack;
    public ushort Attack { get { return _attack; } }

    [Header("Equipments")]
    private EquipmentSO _characterEquip;
    private WeaponEquipmentSO _weaponEquip;
    private EquipmentSO _helmetEquip;
    private EquipmentSO _armorEquip;
    private EquipmentSO _shoesEquip;


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
    public void SetState(CharacterState state)
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



    public void SetEquipment(EquipmentType type, EquipmentSO equipment)
    {
        _hp += equipment.StatHp;
        _attack += equipment.StatAttack;
        _defense += equipment.StatDefense;
        if (type == EquipmentType.Character)
        {
            _characterEquip = equipment;
        }
        else if (type == EquipmentType.Weapon)
        {
            _weaponEquip = (WeaponEquipmentSO)equipment;
        }
        else if (type == EquipmentType.Helmet)
        {
            _helmetEquip = equipment;
        }
        else if (type == EquipmentType.Armor)
        {
            _armorEquip = equipment;
        }
        else if (type == EquipmentType.Shoes)
        {
            _shoesEquip = equipment;
        }
    }
    public bool CheckHit(Vector2 attackPosition)
    {
        Vector2 forward = DirectionToVector(_direction);
        Vector2 dir = attackPosition - _position;

        if (dir.magnitude <= _weaponEquip.AttackRange && IsInAttackAngle(dir, forward))
            return true;
        else
            return false;
    }
    private Vector2 DirectionToVector(Direction direction)
    {
        switch (direction)
        {
            case Direction.Down:
                return Vector2.down;
            case Direction.Left:
                return Vector2.left;
            case Direction.Right:
                return Vector2.right;
            case Direction.Up:
                return Vector2.up;
            default:
                return Vector2.zero;
        }
    }

    private bool IsInAttackAngle(Vector2 direction, Vector2 forward)
    {
        float dot = Vector2.Dot(direction.normalized, forward);
        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
        return angle <= _weaponEquip.AttackAngle;
    }

    public ushort TakeDamage(ushort damage)
    {
        int amount = damage - _defense;

        if (amount <= 0)
            amount = 1;

        if (_hp > amount)
        {
            return (ushort)amount;
        }
        else
        {
            amount = _hp;
            _hp = 0;
            _state = CharacterState.Death;
            return (ushort)amount;
        }
    }
}
