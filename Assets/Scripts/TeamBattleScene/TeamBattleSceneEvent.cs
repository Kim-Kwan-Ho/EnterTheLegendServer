using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using StandardData;
using UnityEngine;

public class TeamBattleSceneEvent : MonoBehaviour
{
    public Action<TeamBattleSceneEvent, TeamBattleRequestMatchArgs> OnRequestMatch;
    public Action<TeamBattleSceneEvent, TeamBattlePlayerLoadedArgs> OnPlayerLoaded;
    public Action<TeamBattleSceneEvent, TeamBattlePlayerPositionChangedArgs> OnPlayerPositionChanged;
    public Action<TeamBattleSceneEvent, TeamBattlePlayerStateChangedArgs> OnPlayerStateChanged;
    public Action<TeamBattleSceneEvent, TeamBattlePlayerDirectionChangedArgs> OnPlayerDirectionChanged;
    public Action<TeamBattleSceneEvent, TeamBattlePlayerAttackEventArgs> OnPlayerAttack;


    public void CallRequestTeamBattleMatch(GameRoomType roomType, NetworkModule module)
    {
        OnRequestMatch?.Invoke(this,
            new TeamBattleRequestMatchArgs() { roomType = roomType, module = module });
    }

    public void CallPlayerLoaded(ushort roomId, ushort playerIndex, Vector2 playerPosition)
    {
        OnPlayerLoaded?.Invoke(this, new TeamBattlePlayerLoadedArgs() { roomId = roomId, playerIndex = playerIndex, playerPosition = playerPosition });
    }

    public void CallPlayerPositionChanged(ushort roomId, stPlayerPosition playerPosition)
    {
        OnPlayerPositionChanged?.Invoke(this,
            new TeamBattlePlayerPositionChangedArgs()
            { roomId = roomId, playerPosition = playerPosition });
    }
    public void CallPlayerStateChanged(ushort roomId, ushort playerIndex, ushort state)
    {
        OnPlayerStateChanged?.Invoke(this,
            new TeamBattlePlayerStateChangedArgs()
            { roomId = roomId, playerIndex = playerIndex, state = state });
    }
    public void CallPlayerDirectionChanged(ushort roomId, ushort playerIndex, ushort direction)
    {
        OnPlayerDirectionChanged?.Invoke(this,
            new TeamBattlePlayerDirectionChangedArgs()
            { roomId = roomId, playerIndex = playerIndex, direction = direction });
    }

    public void CallPlayerOnAttack(ushort roomId, ushort playerIndex)
    {
        OnPlayerAttack?.Invoke(this, new TeamBattlePlayerAttackEventArgs() { roomId = roomId, playerIndex = playerIndex });
    }
}

public class TeamBattleRequestMatchArgs : EventArgs
{
    public GameRoomType roomType;
    public NetworkModule module;
}
public class TeamBattlePlayerLoadedArgs : EventArgs
{
    public ushort roomId;
    public ushort playerIndex;
    public Vector2 playerPosition;
}

public class TeamBattlePlayerPositionChangedArgs : EventArgs
{
    public ushort roomId;
    public stPlayerPosition playerPosition;
}

public class TeamBattlePlayerStateChangedArgs : EventArgs
{
    public ushort roomId;
    public ushort playerIndex;
    public ushort state;
}
public class TeamBattlePlayerDirectionChangedArgs : EventArgs
{
    public ushort roomId;
    public ushort playerIndex;
    public ushort direction;
}

public class TeamBattlePlayerAttackEventArgs : EventArgs
{
    public ushort roomId;
    public ushort playerIndex;
}
