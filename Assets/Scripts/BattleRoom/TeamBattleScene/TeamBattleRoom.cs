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

            for (int j = 0; j < playersInfo[i].EquipedItems.Length; j++)
            {
                EquipmentSO equip = DBManager.Instance.GetEquipmentSO(playersInfo[i].EquipedItems[j]);
                if (equip == null)
                    continue;
                players[i].SetEquipment(equip.Type, equip);
            }
            playersInfo[i].Hp = players[i].Hp;
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
}

