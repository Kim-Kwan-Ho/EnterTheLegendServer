using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DBEvent : MonoBehaviour
{
    public Action<DBEvent, DBRequestPlayerDataEventArgs> OnRequestData;
    public Action<DBEvent, DBPlayerEquipChangedEventArgs> OnPlayerEquipChanged;
    public void CallRequestData(NetworkModule module, string id)
    {
        OnRequestData?.Invoke(this, new DBRequestPlayerDataEventArgs() { module = module, id = id });
    }

    public void CallPlayerEquipChanged(string id,ushort itemType ,int afterItem)
    {
        OnPlayerEquipChanged?.Invoke(this, new DBPlayerEquipChangedEventArgs()
        {
            id = id,
            itemType = itemType,
            afterItem = afterItem,
        });
    }
}

public class DBRequestPlayerDataEventArgs
{
    public NetworkModule module;
    public string id;
}
public class DBPlayerEquipChangedEventArgs
{
    public string id;
    public ushort itemType;
    public int afterItem;
}