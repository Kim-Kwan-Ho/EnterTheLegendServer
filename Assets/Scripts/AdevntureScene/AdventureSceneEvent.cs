using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using StandardData;
using UnityEngine;

public class AdventureSceneEvent : MonoBehaviour
{
    public Action<AdventureSceneEvent, AdventureRequestMatchArgs> OnRequestMatch;
    public Action<AdventureSceneEvent, AdventurePlayerLoadedArgs> OnPlayerLoaded;
    public Action<AdventureSceneEvent, AdventurePlayerPositionChangedArgs> OnPlayerPositionChanged;
    public Action<AdventureSceneEvent, AdventurePlayerStateChangedArgs> OnPlayerStateChanged;
    public Action<AdventureSceneEvent, AdventurePlayerDirectionChangedArgs> OnPlayerDirectionChanged;

    public void CallRequestAdventureMatch(GameRoomType roomType, NetworkModule module)
    {
        OnRequestMatch?.Invoke(this,
            new AdventureRequestMatchArgs() { roomType = roomType, module = module });
    }

    public void CallPlayerLoaded(ushort roomId, ushort playerIndex)
    {
        OnPlayerLoaded?.Invoke(this, new AdventurePlayerLoadedArgs() { roomId = roomId, playerIndex = playerIndex });
    }

    public void CallPlayerPositionChanged(ushort roomId, stPlayerPosition playerPosition)
    {
        OnPlayerPositionChanged?.Invoke(this,
            new AdventurePlayerPositionChangedArgs()
                { roomId = roomId, playerPosition = playerPosition });
    }
    public void CallPlayerStateChanged( ushort roomId, ushort playerIndex ,ushort state)
    {
        OnPlayerStateChanged?.Invoke(this,
            new AdventurePlayerStateChangedArgs()
                {  roomId = roomId, playerIndex = playerIndex,state = state });
    }
    public void CallPlayerDirectionChanged(ushort roomId, ushort playerIndex, ushort direction)
    {
        OnPlayerDirectionChanged?.Invoke(this,
            new AdventurePlayerDirectionChangedArgs()
                { roomId = roomId, playerIndex = playerIndex, direction = direction });
    }
}

public class AdventureRequestMatchArgs : EventArgs
{
    public GameRoomType roomType;
    public NetworkModule module;
}
public class AdventurePlayerLoadedArgs : EventArgs
{
    public ushort roomId;
    public ushort playerIndex;
}

public class AdventurePlayerPositionChangedArgs : EventArgs
{
    public ushort roomId;
    public stPlayerPosition playerPosition;
}

public class AdventurePlayerStateChangedArgs : EventArgs
{
    public ushort roomId;
    public ushort playerIndex;
    public ushort state;
}
public class AdventurePlayerDirectionChangedArgs : EventArgs
{
    public ushort roomId;
    public ushort playerIndex;
    public ushort direction;
}