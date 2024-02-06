using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using StandardData;
using UnityEngine;

public class AdventureRoom
{

    [Header("RoomInfo")]
    private static ushort _roomId = 0;
    public ushort RoomId { get { return _roomId; } }

    private AdventureRoomPlayerInfo[] _players;
    

    private stAdventurePlayerPositionFromSever _playerPositions;

    private WaitForSeconds _sendCycle;




    public AdventureRoom(AdventureRoomPlayerInfo[] players)
    {
        _players = players;
        _playerPositions = new stAdventurePlayerPositionFromSever();
        _playerPositions.Header.MsgID = MessageIdUdp.AdventurePlayerPositionFromServer;
        _sendCycle = new WaitForSeconds(UdpSendCycle.AdventureRoomSendCycle);

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
        createAdventureRoom.playersInfo = playersInfo;

        for (int i = 0; i < _players.Length; i++)
        {
            _players[i].Module.SendTcpMessage(Utilities.GetObjectToByte(createAdventureRoom));
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


    }

    private IEnumerator SendPlayerPositions()
    {
        while (true)
        {
            foreach (var player in _players)
            {
                player.Module.SendUdpMessage(Utilities.GetObjectToByte(_playerPositions));
            }

            yield return _sendCycle;
        }
    }


}


public class AdventureRoomPlayerInfo
{
    public NetworkModule Module = null;
    public bool Loaded = false;
    // 플레이어 정보
}
