using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using TMPro;
using StandardData;

public class DBManager : SingletonMonobehaviour<DBManager>
{
    private MySqlConnection _connection = null;

    [Header("InputFields")]
    [SerializeField]
    private TMP_InputField _ipInputField;
    [SerializeField]
    private TMP_InputField _dbNameInputField;
    [SerializeField]
    private TMP_InputField _dbIdInputField;
    [SerializeField]
    private TMP_InputField _passwordInputField;


    private string _ip;
    private string _dbName;
    private ushort _dbId;
    private ushort _dbPassword;


    public void OpenDB()
    {
        if (_connection != null)
            return;
        string conStr = string.Format($"Server={_ip};DataBase={_dbName};Uid={_dbId};Pwd={_dbPassword};");
        try
        {
            _connection = new MySqlConnection(conStr);
            _connection.Open();
            Debug.Log("DB Opened");
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public async Task<bool> LoginAsync(string id, string pwd) // �α���
    {
        MySqlCommand cmd = new MySqlCommand();
        cmd.Connection = _connection;
        cmd.CommandText = $"SELECT password FROM playerinfo WHERE Id = '{id}'"; // id�� �´� ��й�ȣ ��������
        var result = await cmd.ExecuteScalarAsync();
        if (result == null) // ������ ������� ���� ��ȯ
            return false;
        else
            return (string)result == pwd; // ��й�ȣ�� �´��� ���� ��ȯ
    }
    public async Task<bool> RegisterAsync(string id, string pwd) // ȸ������
    {
        try
        {
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = _connection;
            cmd.CommandText = $"INSERT INTO playerinfo(Id,Password,Nickname,Credit,Gold) VALUES('{id}','{pwd}'," +
                              $"'{PlayerStartSetting.Nickname}','{PlayerStartSetting.Credit}','{PlayerStartSetting.Gold}');"; // �÷��̾� ���� ����
            await cmd.ExecuteNonQueryAsync();
            return true;
        }
        catch (MySqlException e)
        {
            if (e.Number == 1062) // �ش� ���̵� ���� ��� ���� ��ȯ
            {
                return false;
            }
            else
            {
                Debug.Log(e);
                throw;
            }
        }
    }






#if UNITY_EDITOR
    protected override void OnBindField()
    {
        base.OnBindField();
        _ipInputField = GameObject.Find("IpInputField").GetComponent<TMP_InputField>();
        _dbNameInputField = GameObject.Find("DBNameInputField").GetComponent<TMP_InputField>();
        _dbIdInputField = GameObject.Find("DBIdInputField").GetComponent<TMP_InputField>();
        _passwordInputField = GameObject.Find("DBPasswordInputField").GetComponent<TMP_InputField>();
    }

    private void OnValidate()
    {
        CheckNullValue(this.name, _ipInputField);
        CheckNullValue(this.name, _dbNameInputField);
        CheckNullValue(this.name, _dbIdInputField);
        CheckNullValue(this.name, _passwordInputField);
    }

#endif

}