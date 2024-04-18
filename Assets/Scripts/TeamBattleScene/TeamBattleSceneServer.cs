using System;
using StandardData;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(TeamBattleSceneEvent))]

public class TeamBattleSceneServer : SingletonMonobehaviour<TeamBattleSceneServer>
{
    public TeamBattleSceneEvent EventTeamBattleScene;

    private ManualResetEventSlim _teamBattlePlayerAddedEvent = new ManualResetEventSlim(false);
    private ConcurrentQueue<NetworkModule> _teamBattleMatchingQueue = new ConcurrentQueue<NetworkModule>();
    private HashSet<string> _teamBattleCanceledMatch = new HashSet<string>();
    private ConcurrentDictionary<int, TeamBattleRoom> _teamBattleRoomDic = new ConcurrentDictionary<int, TeamBattleRoom>();
    private ManualResetEventSlim _teamBattleRoomAddedEvent = new ManualResetEventSlim(false);

    private Task _updatePlayerPositionTask;


    private void Start()
    {
        Task.Run(TeamBattleMatchMakingSystem);
        _updatePlayerPositionTask = Task.Run(() => { UpdatePlayerPositions(); });
    }

    private void OnEnable()
    {
        EventTeamBattleScene.OnRequestMatch += Event_RequestMatch;
        EventTeamBattleScene.OnPlayerLoaded += Event_PlayerLoaded;
        EventTeamBattleScene.OnPlayerPositionChanged += Event_PlayerPositionChanged;
        EventTeamBattleScene.OnPlayerStateChanged += Event_PlayerStateChanged;
        EventTeamBattleScene.OnPlayerDirectionChanged += Event_PlayerDirectionChanged;
    }

    private void OnDisable()
    {
        EventTeamBattleScene.OnRequestMatch -= Event_RequestMatch;
        EventTeamBattleScene.OnPlayerLoaded -= Event_PlayerLoaded;
        EventTeamBattleScene.OnPlayerPositionChanged -= Event_PlayerPositionChanged;
        EventTeamBattleScene.OnPlayerStateChanged -= Event_PlayerStateChanged;
        EventTeamBattleScene.OnPlayerDirectionChanged -= Event_PlayerDirectionChanged;
    }

    private void Update()
    {


    }
    private void Event_RequestMatch(TeamBattleSceneEvent teamBattleSceneEvent,
        TeamBattleRequestMatchArgs teamBattleRequestMatchArgs)
    {
        if (teamBattleRequestMatchArgs.roomType == GameRoomType.TeamBattle)
        {
            _teamBattleMatchingQueue.Enqueue(teamBattleRequestMatchArgs.module);
            _teamBattlePlayerAddedEvent.Set();
        }
        else
        {
            
        }
    }

    private void Event_PlayerLoaded(TeamBattleSceneEvent teamBattleSceneEvent,
        TeamBattlePlayerLoadedArgs teamBattlePlayerLoadedArgs)
    {
        _teamBattleRoomDic[teamBattlePlayerLoadedArgs.roomId].PlayerLoaded(teamBattlePlayerLoadedArgs.playerIndex, teamBattlePlayerLoadedArgs.playerPosition);
    }

    private void Event_PlayerPositionChanged(TeamBattleSceneEvent teamBattleSceneEvent,
        TeamBattlePlayerPositionChangedArgs teamBattlePlayerPositionChangedArgs)
    {
        _teamBattleRoomDic[teamBattlePlayerPositionChangedArgs.roomId]
            .PlayerPositionChanged(teamBattlePlayerPositionChangedArgs.playerPosition);
    }

    private void Event_PlayerStateChanged(TeamBattleSceneEvent teamBattleSceneEvent,
        TeamBattlePlayerStateChangedArgs teamBattlePlayerStateChangedArgs)
    {
        _teamBattleRoomDic[teamBattlePlayerStateChangedArgs.roomId]
            .PlayerStateChanged(teamBattlePlayerStateChangedArgs.playerIndex, teamBattlePlayerStateChangedArgs.state);

    }
    private void Event_PlayerDirectionChanged(TeamBattleSceneEvent teamBattleSceneEvent,
        TeamBattlePlayerDirectionChangedArgs teamBattlePlayerDirectionChangedArgs)
    {
        _teamBattleRoomDic[teamBattlePlayerDirectionChangedArgs.roomId]
            .PlayerDirectionChanged(teamBattlePlayerDirectionChangedArgs.playerIndex, teamBattlePlayerDirectionChangedArgs.direction);
    }
    private Task TeamBattleMatchMakingSystem()
    {
        while (true)
        {
            _teamBattlePlayerAddedEvent.Wait();
            if (_teamBattleMatchingQueue.Count >= GameRoomSize.TeamBattleRoomSize)
            {
                BattleRoomPlayerInfo[] playerInfos = new BattleRoomPlayerInfo[GameRoomSize.TeamBattleRoomSize];
                int count = 0;
                for (int i = 0; i < GameRoomSize.TeamBattleRoomSize; i++)
                {
                    playerInfos[i] = GetTeamBattlePlayer();
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
                        _teamBattleMatchingQueue.Enqueue(playerInfos[i].Module);
                    }
                }
                else
                {
                    TeamBattleRoom teamBattleRoom = new TeamBattleRoom(playerInfos);
                    _teamBattleRoomDic.TryAdd(teamBattleRoom.RoomId, teamBattleRoom);
                    _teamBattleRoomAddedEvent.Set();
                }
                
            }
            else
            {
                _teamBattlePlayerAddedEvent.Reset();
            }
        }
    }

    private BattleRoomPlayerInfo GetTeamBattlePlayer()
    {
        BattleRoomPlayerInfo playerInfo = new BattleRoomPlayerInfo();
        while (_teamBattleMatchingQueue.Count > 0)
        {
            _teamBattleMatchingQueue.TryDequeue(out NetworkModule player);
            if (!_teamBattleCanceledMatch.Contains(player.Id))
            {
                playerInfo.Module = player;
                return playerInfo;
            }
            _teamBattleCanceledMatch.Remove(player.Id);
        }
        return playerInfo;
    }

    private Task UpdatePlayerPositions()
    {
        try
        {
            while (true)
            {
                _teamBattleRoomAddedEvent.Wait();
                if (_teamBattleRoomDic.Count > 0)
                {
                    foreach (var value in _teamBattleRoomDic.Values)
                    {
                        value.SendPlayerPositions();
                    }
                }
                else
                {
                    _teamBattleRoomAddedEvent.Reset();
                }

                Thread.Sleep((int)(1000 * UdpSendCycle.TeamBattleRoomSendCycle));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
       
    }






#if UNITY_EDITOR
    protected override void OnBindField()
    {
        base.OnBindField();
        EventTeamBattleScene = GetComponent<TeamBattleSceneEvent>();
    }

    private void OnValidate()
    {
        CheckNullValue(this.name, EventTeamBattleScene);
    }


#endif
}
