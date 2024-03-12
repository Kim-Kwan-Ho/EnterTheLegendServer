using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DBEvent : MonoBehaviour
{
    public Action<DBEvent, DBRequestPlayerDataEventArgs> OnRequestData;

    public void CallRequestData(NetworkModule module, string id)
    {
        OnRequestData?.Invoke(this, new DBRequestPlayerDataEventArgs() { module = module, id = id });
    }
}

public class DBRequestPlayerDataEventArgs
{
    public NetworkModule module;
    public string id;
}
