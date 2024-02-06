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
    public void CallRequestAdventureMatch(GameRoomType roomType, NetworkModule module)
    {
        OnRequestMatch?.Invoke(this,
            new AdventureRequestMatchArgs() { roomType = roomType, module = module });
    }

    public void CallPlayerLoaded(ushort roomId, ushort playerIndex)
    {
        OnPlayerLoaded?.Invoke(this, new AdventurePlayerLoadedArgs() { roomId = roomId, playerIndex = playerIndex });
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