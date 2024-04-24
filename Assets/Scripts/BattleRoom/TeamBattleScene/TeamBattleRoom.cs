using System.Runtime.InteropServices;
using System.Threading;
using StandardData;
using UnityEngine;

public class TeamBattleRoom : BattleRoom
{

    [Header("RoomInfo")]
    private static ushort _roomIndex = 0;



    public TeamBattleRoom(BattleRoomPlayer[] players)
    {
        _roomId = GenerateRoomId();
        _players = players;
        stBattlePlayerInfo[] playersInfo = new stBattlePlayerInfo[_players.Length];
        for (ushort i = 0; i < _players.Length; i++)
        {
            playersInfo[i].Index = i;
            playersInfo[i].Nickname = _players[i].GetName();
            playersInfo[i].EquipedItems = DBManager.Instance.GetPlayerEquipedItems(_players[i].GetId());
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
            _players[i].SendTcpMessage(msg);
        }
    }
   

    private ushort GenerateRoomId()
    {
        _roomIndex++;
        return _roomIndex;
    }
   

    public override void PlayerOnAttack(ushort playerIndex)
    {
        base.PlayerOnAttack(playerIndex);
        CheckHit(playerIndex < 3, _players[playerIndex].Position, _players[playerIndex].Direction, 2, 45);
    }

    protected override void CheckHit(bool blueTeam, Vector2 attackPos, Direction direction, float distance, float attackAngle)
    {
        base.CheckHit(blueTeam, attackPos, direction, distance, attackAngle);
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
}

