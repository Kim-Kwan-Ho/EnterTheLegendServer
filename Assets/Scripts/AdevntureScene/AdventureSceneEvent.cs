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

    public void CallRequestAdventureMatch(GameRoomType roomType, NetworkModule module)
    {
        OnRequestMatch?.Invoke(this,
            new AdventureRequestMatchArgs() { roomType = roomType, module = module });
    }

    public void CallPlayerLoaded(ushort roomId, ushort playerIndex)
    {
        OnPlayerLoaded?.Invoke(this, new AdventurePlayerLoadedArgs() { roomId = roomId, playerIndex = playerIndex });
    }

    public void CallPlayerPositionChanged(GameRoomType roomType, ushort roomId, stPlayerPosition playerPosition)
    {
        OnPlayerPositionChanged?.Invoke(this,
            new AdventurePlayerPositionChangedArgs()
                { roomType = roomType, roomId = roomId, playerPosition = playerPosition });
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
    public GameRoomType roomType;
    public ushort roomId;
    public stPlayerPosition playerPosition;
}