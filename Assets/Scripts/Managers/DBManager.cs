using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using TMPro;
using StandardData;
using System.Data;
using System.Runtime.InteropServices;

[RequireComponent(typeof(DBEvent))]
public class DBManager : SingletonMonobehaviour<DBManager>
{
    private MySqlConnection _connection = null;


    [SerializeField]
    public DBEvent EventDB;

    [Header("InputFields")]
    [SerializeField]
    private TMP_InputField _ipInputField;
    [SerializeField]
    private TMP_InputField _dbNameInputField;
    [SerializeField]
    private TMP_InputField _dbIdInputField;
    [SerializeField]
    private TMP_InputField _dbPasswordInputField;


    private string _ip;
    private string _dbName;
    private string _dbId;
    private string _dbPassword;

    protected override void Awake()
    {
        base.Awake();
        EventDB.OnRequestData += Event_GetPlayerDataAsync;
        EventDB.OnPlayerEquipChanged += Event_ChangePlayerEquipItem;
    }

    private void OnDisable()
    {
        EventDB.OnRequestData -= Event_GetPlayerDataAsync;
        EventDB.OnPlayerEquipChanged -= Event_ChangePlayerEquipItem;
    }

    public void OpenDB()
    {
        _ip = _ipInputField.text;
        _dbName = _dbNameInputField.text;
        _dbId = _dbIdInputField.text;
        _dbPassword = _dbPasswordInputField.text;
        if (_connection != null)
            return;
        string conStr = string.Format($"Server=localhost;DataBase={_dbName};Uid={_dbId};Pwd={_dbPassword};");
        try
        {
            _connection = new MySqlConnection(conStr);
            _connection.Open();
            Debug.Log("DB Opened");
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    private async void Event_GetPlayerDataAsync(DBEvent dbEvent, DBRequestPlayerDataEventArgs dbRequestPlayerDataEventArgs) // 로그인
    {
        string id = dbRequestPlayerDataEventArgs.id;
        MySqlCommand cmd = new MySqlCommand();
        cmd.Connection = _connection;
        cmd.CommandText = $"SELECT * FROM playerinfo WHERE Id = '{id}'"; 
        var result = await cmd.ExecuteScalarAsync();
        if (result == null) // 정보가 없을경우 플레이어 데이터 추가
        {
            await AddPlayerDataAsync(id);
        }

        stResponsePlayerData data = new stResponsePlayerData();
        data.Header.MsgID = MessageIdTcp.ResponsePlayerData;
        data.Header.PacketSize = (ushort)Marshal.SizeOf(data);
        DataSet ds = new DataSet();
        cmd.Connection = _connection;

        // 플레이어 정보
        cmd.CommandText = $"SELECT * FROM playerinfo WHERE Id = '{id}';";
        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
        adapter.Fill(ds, "playerinfo");
        data.Nickname = (string)ds.Tables[0].Rows[0]["Nickname"];
        data.Credit = (int)ds.Tables[0].Rows[0]["Credit"];
        data.Gold = (int)ds.Tables[0].Rows[0]["Gold"];



        // 착용중인 아이템
        cmd.CommandText = $"SELECT * FROM playerequipeditem WHERE Id = '{id}';";
        adapter = new MySqlDataAdapter(cmd);
        adapter.Fill(ds, "playerequipeditem");
        int[] equipedItem = new int[NetworkSize.EquipedItemLength];
        for (int i = 0; i < equipedItem.Length; i++)
        {
            equipedItem[i] = (int)ds.Tables[1].Rows[i]["ItemId"];
        }
        data.EquipedItems = equipedItem;


        // 보유중인 아이템
        cmd.CommandText = $"SELECT * FROM playeritem WHERE Id = '{id}';";
        adapter = new MySqlDataAdapter(cmd);
        adapter.Fill(ds, "playeritem");
        int[] playerItems = new int[ds.Tables[2].Rows.Count];
        data.ItemCount = (ushort)playerItems.Length;
        for (int i = 0; i < playerItems.Length; i++)
        {
            playerItems[i] = (int)ds.Tables[2].Rows[i]["ItemId"];
        }
        data.Items = playerItems;

        byte[] msg = Utilities.GetObjectToByte(data);
        dbRequestPlayerDataEventArgs.module.SetId(dbRequestPlayerDataEventArgs.id);
        dbRequestPlayerDataEventArgs.module.SetNickname(data.Nickname);
        dbRequestPlayerDataEventArgs.module.SendTcpMessage(msg);
    }

    private async Task AddPlayerDataAsync(string id) // 플레이어 데이터 추가
    {
        try
        {
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = _connection;
            cmd.CommandText = $"INSERT INTO playerinfo(Id,Nickname,Credit,Gold) VALUES('{id}'," +
                              $"'{PlayerStartSetting.Nickname}',{PlayerStartSetting.Credit},{PlayerStartSetting.Gold});"; // 플렝이어 정보 생성
            await cmd.ExecuteNonQueryAsync();
            for (int i = 0; i < NetworkSize.EquipedItemLength; i++)
            {
                cmd.CommandText = $"INSERT INTO playerequipeditem(Id,ItemType,ItemId) VALUES('{id}',{i},{0});";
                await cmd.ExecuteNonQueryAsync();
            }
            for (int i = 0; i < PlayerStartSetting.StartItems.Length; i++)
            {
                cmd.CommandText = $"INSERT INTO playeritem(Id,ItemId) VALUES('{id}',{PlayerStartSetting.StartItems[i]});";
                await cmd.ExecuteNonQueryAsync();
            }
        }
        catch (MySqlException e)
        {
            Debug.Log(e);
            throw;
        }
    }

    private async void Event_ChangePlayerEquipItem(DBEvent dbEvent, DBPlayerEquipChangedEventArgs playerEquipChangedEventArgs )
    {
        MySqlCommand cmd = new MySqlCommand();
        cmd.Connection = _connection;
        cmd.CommandText =
            $"Update playerequipeditem SET ItemId = {playerEquipChangedEventArgs.afterItem} WHERE Id = '{playerEquipChangedEventArgs.id}' AND ItemType = {playerEquipChangedEventArgs.itemType};";
        await cmd.ExecuteNonQueryAsync();
    }


#if UNITY_EDITOR
    protected override void OnBindField()
    {
        base.OnBindField();
        EventDB = GetComponent<DBEvent>();
        _ipInputField = GameObject.Find("IpInputField").GetComponent<TMP_InputField>();
        _dbNameInputField = GameObject.Find("DBNameInputField").GetComponent<TMP_InputField>();
        _dbIdInputField = GameObject.Find("DBIdInputField").GetComponent<TMP_InputField>();
        _dbPasswordInputField = GameObject.Find("DBPasswordInputField").GetComponent<TMP_InputField>();
    }

    private void OnValidate()
    {
        CheckNullValue(this.name, EventDB);
        CheckNullValue(this.name, _ipInputField);
        CheckNullValue(this.name, _dbNameInputField);
        CheckNullValue(this.name, _dbIdInputField);
        CheckNullValue(this.name, _dbPasswordInputField);
    }

#endif

}