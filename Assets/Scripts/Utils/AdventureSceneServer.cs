using StandardData;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(AdventureSceneEvent))]

public class AdventureSceneServer : SingletonMonobehaviour<AdventureSceneServer>
{
    public AdventureSceneEvent EventAdventureScene;

    private ManualResetEventSlim _adventurePlayerAddedEvent = new ManualResetEventSlim(false);
    private ConcurrentQueue<NetworkModule> _adventureMatchingQueue = new ConcurrentQueue<NetworkModule>();
    private HashSet<string> _adventureCanceledMatch = new HashSet<string>();
    private ConcurrentDictionary<int, AdventureRoom> _adventureRoomDic = new ConcurrentDictionary<int, AdventureRoom>();
    private ManualResetEventSlim _adventureRoomAddedEvent = new ManualResetEventSlim(false);



    private void Start()
    {
        Task.Run(AdventureMatchMakingSystem);
        Task.Run(UpdatePlayerPositions);
    }

    private void OnEnable()
    {
        EventAdventureScene.OnRequestMatch += Event_RequestMatch;
        EventAdventureScene.OnPlayerLoaded += Event_PlayerLoaded;
        EventAdventureScene.OnPlayerPositionChanged += Event_PlayerPositionChanged;
        EventAdventureScene.OnPlayerStateChanged += Event_PlayerStateChanged;
        EventAdventureScene.OnPlayerDirectionChanged += Event_PlayerDirectionChanged;
    }

    private void OnDisable()
    {
        EventAdventureScene.OnRequestMatch -= Event_RequestMatch;
        EventAdventureScene.OnPlayerLoaded -= Event_PlayerLoaded;
        EventAdventureScene.OnPlayerPositionChanged -= Event_PlayerPositionChanged;
        EventAdventureScene.OnPlayerStateChanged -= Event_PlayerStateChanged;
        EventAdventureScene.OnPlayerDirectionChanged -= Event_PlayerDirectionChanged;
    }

    private void Update()
    {


    }
    private void Event_RequestMatch(AdventureSceneEvent adventureSceneEvent,
        AdventureRequestMatchArgs adventureRequestMatchArgs)
    {
        if (adventureRequestMatchArgs.roomType == GameRoomType.AdventureRoom)
        {
            _adventureMatchingQueue.Enqueue(adventureRequestMatchArgs.module);
            _adventurePlayerAddedEvent.Set();
        }
        else
        {
            
        }
    }

    private void Event_PlayerLoaded(AdventureSceneEvent adventureSceneEvent,
        AdventurePlayerLoadedArgs adventurePlayerLoadedArgs)
    {
        _adventureRoomDic[adventurePlayerLoadedArgs.roomId].PlayerLoaded(adventurePlayerLoadedArgs.playerIndex);
    }

    private void Event_PlayerPositionChanged(AdventureSceneEvent adventureSceneEvent,
        AdventurePlayerPositionChangedArgs adventurePlayerPositionChangedArgs)
    {
        _adventureRoomDic[adventurePlayerPositionChangedArgs.roomId]
            .PlayerPositionChanged(adventurePlayerPositionChangedArgs.playerPosition);
    }

    private void Event_PlayerStateChanged(AdventureSceneEvent adventureSceneEvent,
        AdventurePlayerStateChangedArgs adventurePlayerStateChangedArgs)
    {
        _adventureRoomDic[adventurePlayerStateChangedArgs.roomId]
            .PlayerStateChanged(adventurePlayerStateChangedArgs.playerIndex, adventurePlayerStateChangedArgs.state);

    }
    private void Event_PlayerDirectionChanged(AdventureSceneEvent adventureSceneEvent,
        AdventurePlayerDirectionChangedArgs adventurePlayerDirectionChangedArgs)
    {
        _adventureRoomDic[adventurePlayerDirectionChangedArgs.roomId]
            .PlayerDirectionChanged(adventurePlayerDirectionChangedArgs.playerIndex, adventurePlayerDirectionChangedArgs.direction);
    }
    private Task AdventureMatchMakingSystem()
    {
        while (true)
        {
            _adventurePlayerAddedEvent.Wait();
            if (_adventureMatchingQueue.Count >= GameRoomSize.AdventureRoomSize)
            {
                AdventureRoomPlayerInfo[] playerInfos = new AdventureRoomPlayerInfo[GameRoomSize.AdventureRoomSize];
                int count = 0;
                for (int i = 0; i < GameRoomSize.AdventureRoomSize; i++)
                {
                    playerInfos[i] = GetAdventurePlayer();
                    if (playerInfos[i].Module == null)
                    {
                        count = i;
                        break;
                    }
                }
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        _adventureMatchingQueue.Enqueue(playerInfos[i].Module);
                    }
                }
                else
                {
                    AdventureRoom adventureRoom = new AdventureRoom(playerInfos);
                    _adventureRoomDic.TryAdd(adventureRoom.RoomId, adventureRoom);
                    _adventureRoomAddedEvent.Set();
                }
                
            }
            else
            {
                _adventurePlayerAddedEvent.Reset();
            }
        }
    }

    private AdventureRoomPlayerInfo GetAdventurePlayer()
    {
        AdventureRoomPlayerInfo playerInfo = new AdventureRoomPlayerInfo();
        while (_adventureMatchingQueue.Count > 0)
        {
            _adventureMatchingQueue.TryDequeue(out NetworkModule player);
            if (!_adventureCanceledMatch.Contains(player.Id))
            {
                playerInfo.Module = player;
                return playerInfo;
            }
            _adventureCanceledMatch.Remove(player.Id);
        }
        return playerInfo;
    }

    private Task UpdatePlayerPositions()
    {
        while (true)
        {
            _adventureRoomAddedEvent.Wait();
            if (_adventureRoomDic.Count > 0)
            {
                foreach (var value in _adventureRoomDic.Values)
                {
                    value.SendPlayerPositions();
                }
            }
            else
            {
                _adventureRoomAddedEvent.Reset();
            }

            Thread.Sleep((int)(1000 * UdpSendCycle.AdventureRoomSendCycle));


        }
    }






#if UNITY_EDITOR
    protected override void OnBindField()
    {
        base.OnBindField();
        EventAdventureScene = GetComponent<AdventureSceneEvent>();
    }

    private void OnValidate()
    {
        CheckNullValue(this.name, EventAdventureScene);
    }


#endif
}
