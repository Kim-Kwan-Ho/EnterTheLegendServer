using StandardData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using PimDeWitte.UnityMainThreadDispatcher;
using TMPro;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.UI;

public class ServerManager : SingletonMonobehaviour<ServerManager>
{
    private Thread _connectListenerThread = null;
    private Thread _tcpListenerThread = null;
    private Thread _udpListenerThread = null;


    private TcpListener _tcpListener = null;
    private NetworkStream _theStream = null;
    private UdpClient _udpReceive = null;


    private List<NetworkModule> _connectedClients = new List<NetworkModule>();
    private List<NetworkModule> _disConnectedClients = new List<NetworkModule>();



    [Header("InputFields")]
    [SerializeField] 
    private TMP_InputField _ipInputField;
    [SerializeField]
    private TMP_InputField _portInputField;
    [SerializeField]
    private TMP_InputField _udpPortInputField;
    [SerializeField] 
    private TextMeshProUGUI _clientsCountText;

    private string _ip;
    private ushort _port;
    private ushort _udpPort;
    private ushort _udpIndex = 1;


    public void OpenServer()
    {
        if (_tcpListener != null)
            return;

        Debug.Log("Server Opened");


        _ip = _ipInputField.text;
        _port = Convert.ToUInt16(_portInputField.text);
        _udpPort = Convert.ToUInt16(_udpPortInputField.text);


        _connectListenerThread = new Thread(new ThreadStart(ListenForIncomingRequest));
        _connectListenerThread.IsBackground = true;
        _connectListenerThread.Start();

        _tcpListenerThread = new Thread(new ThreadStart(TcpListenForIncomingRequest));
        _tcpListenerThread.IsBackground = true;
        _tcpListenerThread.Start();

        _udpListenerThread = new Thread(new ThreadStart(UdpListenForIncomingRequest));
        _udpListenerThread.IsBackground = true;
        _udpListenerThread.Start();
    }

    private void ListenForIncomingRequest()
    {
        try
        {
            _tcpListener = new TcpListener(IPAddress.Any, _port);
            _tcpListener.Start();
            while (true)
            {

                if (_tcpListener.Pending())
                {

                    TestDebugLog.DebugLog("Client" + _udpIndex + " Connected");
                    NetworkModule client = new NetworkModule(_tcpListener.AcceptTcpClient(),
                        new IPEndPoint(IPAddress.Parse(_ip), _udpPort + _udpIndex),
                        "Client_" + _udpIndex.ToString());

                    stSetUdpPort setUdpPort = new stSetUdpPort();
                    setUdpPort.Header.MsgID = MessageIdTcp.SetUdpPort;
                    setUdpPort.Header.PacketSize = (ushort)Marshal.SizeOf(setUdpPort);
                    setUdpPort.Name = "Client_" + _udpIndex.ToString();
                    setUdpPort.UdpPortSend = _udpPort;
                    setUdpPort.UdpPortReceive = (ushort)(_udpPort + _udpIndex);
                    byte[] msg = Utilities.GetObjectToByte(setUdpPort);
                    client.SendTcpMessage(msg);
                    _connectedClients.Add(client);
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>_clientsCountText.text = _connectedClients.Count.ToString());
                    _udpIndex++;
                }

                foreach (NetworkModule client in _connectedClients)
                {
                    if (client != null)
                    {
                        if (!IsConnected(client.TcpSocket))
                        {
                            _disConnectedClients.Add(client);
                        }
                    }
                }

                for (int i = _disConnectedClients.Count - 1; i >= 0; i--)
                {
                    _disConnectedClients[i].CloseModule();
                    _connectedClients.Remove(_disConnectedClients[i]);
                    _disConnectedClients.Remove(_disConnectedClients[i]);
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _clientsCountText.text = _connectedClients.Count.ToString());
                }

                Thread.Sleep(10);
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("SocketException " + socketException.ToString());
        }
    }

    private void TcpListenForIncomingRequest()
    {
        while (true)
        {
            if (_connectedClients.Count <= 0)
            {
                Thread.Sleep(100);
            }

            for (int c = 0; c < _connectedClients.Count; c++)
            {
                if (_connectedClients[c].IsConnected())
                {
                    _theStream = _connectedClients[c].Stream;
                    if (_theStream is { DataAvailable: true } && _connectedClients[c].IsConnected())
                    {
                        int length = 0;
                        if ((length = _theStream.Read(_connectedClients[c].Bytes, 0,
                                _connectedClients[c].Bytes.Length)) != 0)
                        {

                            byte[] inData = new byte[length + _connectedClients[c].TempByteSize];
                            if (_connectedClients[c].IsTempByte)
                            {
                                Array.Copy(_connectedClients[c].TempBytes, 0, inData, 0, _connectedClients[c].TempByteSize);
                                Array.Copy(_connectedClients[c].Bytes, 0, inData, _connectedClients[c].TempByteSize, length);
                            }
                            else
                                Array.Copy(_connectedClients[c].Bytes, 0, inData, 0, length);

                            int nDataCur = 0;

                            while (true)
                            {
                                byte[] headerData = new byte[NetworkSize.HeaderSize];
                                Array.Copy(inData, nDataCur, headerData, 0, NetworkSize.HeaderSize);
                                stHeaderTcp header = Utilities.GetObjectFromByte<stHeaderTcp>(headerData);
                                if (header.PacketSize > length - nDataCur)
                                {
                                    Array.Copy(inData, nDataCur, _connectedClients[c].TempBytes, 0,
                                        length - nDataCur);
                                    _connectedClients[c].SetIsTempByte(true);
                                    _connectedClients[c].SetTempByteSize(length - nDataCur);
                                    break;
                                }
                                byte[] msgData = new byte[header.PacketSize];
                                Array.Copy(inData, nDataCur, msgData, 0, header.PacketSize);
                                TcpIncomingDataProcess(header.MsgID, msgData, c);

                                nDataCur += header.PacketSize;
                                if (length == nDataCur)
                                {
                                    _connectedClients[c].SetIsTempByte(false);
                                    _connectedClients[c].SetTempByteSize(0);
                                    break;
                                }

                            }
                        }
                    }
                }

            }

            Thread.Sleep(1);
        }
    }
    private void TcpIncomingDataProcess(ushort msgId, byte[] msgData, int c)
    {
        switch (msgId)
        {
            case MessageIdTcp.RequestPlayerData:
                stRequestPlayerData requestPlayerData = Utilities.GetObjectFromByte<stRequestPlayerData>(msgData);
                DBManager.Instance.EventDB.CallRequestData(_connectedClients[c], requestPlayerData.Id);
                break;
            case MessageIdTcp.PlayerEquipChanged:
                stPlayerEquipChangedInfo equipChangedInfo =
                    Utilities.GetObjectFromByte<stPlayerEquipChangedInfo>(msgData);
                DBManager.Instance.EventDB.CallPlayerEquipChanged(_connectedClients[c].Id, equipChangedInfo.ItemType , equipChangedInfo.AfterItem);
                break;
            case MessageIdTcp.RequestForMatch:
                stRequestForMatch requestMatch = Utilities.GetObjectFromByte<stRequestForMatch>(msgData);
                TeamBattleSceneServer.Instance.EventTeamBattleScene.CallRequestTeamBattleMatch((GameRoomType)requestMatch.MatchType, _connectedClients[c]);
                break;
            case MessageIdTcp.TeamBattlePlayerLoadInfo:
                stTeamBattleRoomPlayerLoadInfo roomLoaded = Utilities.GetObjectFromByte<stTeamBattleRoomPlayerLoadInfo>(msgData);
                TeamBattleSceneServer.Instance.EventTeamBattleScene.CallPlayerLoaded(roomLoaded.RoomId, roomLoaded.PlayerIndex);
                break;
            case MessageIdTcp.TeamBattleRoomPlayerStateChangedToServer:
                stTeamBattleRoomPlayerStateChangedToServer stateChanged =
                    Utilities.GetObjectFromByte<stTeamBattleRoomPlayerStateChangedToServer>(msgData);

                TeamBattleSceneServer.Instance.EventTeamBattleScene.CallPlayerStateChanged(stateChanged.PlayerInfo.RoomId,
                    stateChanged.PlayerInfo.PlayerIndex, stateChanged.State);
                break;
            case MessageIdTcp.TeamBattleRoomPlayerDirectionChangedToServer:
                stTeamBattleRoomPlayerDirectionChangedToServer directionChanged =
                    Utilities.GetObjectFromByte<stTeamBattleRoomPlayerDirectionChangedToServer>(msgData);
                TeamBattleSceneServer.Instance.EventTeamBattleScene.CallPlayerDirectionChanged(directionChanged.PlayerInfo.RoomId,
                    directionChanged.PlayerInfo.PlayerIndex, directionChanged.Direction);
                break;
            default:
                break;
        }
    }
    private bool IsConnected(TcpClient client)
    {
        if (client?.Client == null || !client.Client.Connected)
            return false;

        try
        {
            return !(client.Client.Poll(0, SelectMode.SelectRead) && client.Client.Receive(new byte[1], SocketFlags.Peek) == 0);
        }
        catch
        {
            return false;
        }
    }

    
    public void CloseSocket()
    {

        if (_tcpListener != null)
        {
            _tcpListener.Stop();
            _tcpListener = null;
            _connectListenerThread.Abort();
            _connectListenerThread = null;
            _tcpListenerThread.Abort();
            _tcpListenerThread = null;
            _udpListenerThread.Abort();
            _udpListenerThread = null;



            foreach (NetworkModule client in _connectedClients)
            {
                client.CloseModule();
            }

            _connectedClients.Clear();
        }
    }



    private void OnApplicationQuit()
    {
        CloseSocket();
    }


    public void UdpListenForIncomingRequest()
    {
        try
        {
            _udpReceive = new UdpClient(_udpPort);
            while (true)
            {
                IPEndPoint IPEndPointReceive = new IPEndPoint(IPAddress.Any, _udpPort);
                byte[] udpBuffer = _udpReceive.Receive(ref IPEndPointReceive);
                stHeaderUdp header = Utilities.GetObjectFromByte<stHeaderUdp>(udpBuffer);
                UdpIncomingDataProcess(header.MsgID, udpBuffer);
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("UDPSocketException " + socketException.ToString());
        }
    }

    private void UdpIncomingDataProcess(ushort msgId, byte[] msgData)
    {
        switch (msgId)
        {
            case MessageIdUdp.TeamBattlePlayerPositionToServer:
                stTeamBattlePlayerPositionToServer position = Utilities.GetObjectFromByte<stTeamBattlePlayerPositionToServer>(msgData);
                TeamBattleSceneServer.Instance.EventTeamBattleScene.CallPlayerPositionChanged(
                    position.RoomId, position.PlayerPosition);
                break;
        }
    }


#if UNITY_EDITOR
    protected override void OnBindField()
    {
        base.OnBindField();
        _ipInputField = GameObject.Find("IpInputField").GetComponent<TMP_InputField>();
        _portInputField = GameObject.Find("PortInputField").GetComponent<TMP_InputField>();
        _udpPortInputField = GameObject.Find("UdpPortInputField").GetComponent<TMP_InputField>();
        _clientsCountText = GameObject.Find("ClientsCountText").GetComponent<TextMeshProUGUI>();

    }

    private void OnValidate()
    {
        CheckNullValue(this.name, _ipInputField);
        CheckNullValue(this.name, _portInputField);
        CheckNullValue(this.name, _udpPortInputField);
        CheckNullValue(this.name, _clientsCountText);
    }

#endif
}
