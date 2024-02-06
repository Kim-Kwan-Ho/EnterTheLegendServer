using StandardData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class ServerManager : SingletonMonobehaviour<ServerManager>
{

    private Thread _connectListenerThread = null;
    private Thread _tcpListenerThread = null;
    private Thread _udpListenerThread = null;


    private TcpListener _tcpListener = null;
    private NetworkStream _theStream = null;
    private UdpClient _udpReceive = null;


    public bool ServerReady = false;
    private List<NetworkModule> _connectedClients = new List<NetworkModule>();
    private List<NetworkModule> _disConnectedClients = new List<NetworkModule>();
    public string IP = "127.0.0.1";
    public int Port = 9001;
    public ushort UdpPort = 9002;
    public ushort UdpIndex = 1;

    private void Start()
    {
        Init();
    }



    public void Init()
    {
        CreateServer();
    }


    private void CreateServer()
    {
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
            _tcpListener = new TcpListener(IPAddress.Any, Port);
            _tcpListener.Start();

            ServerReady = true;

            while (true)
            {
                if (!ServerReady)
                    break;

                if (_tcpListener.Pending())
                {

                    Debug.Log("Client" + UdpIndex + " Connected");
                    NetworkModule client = new NetworkModule(_tcpListener.AcceptTcpClient(),
                        new IPEndPoint(IPAddress.Parse(IP), UdpPort + UdpIndex),
                        "Client_" + UdpIndex.ToString());

                    stSetUdpPort setUdpPort = new stSetUdpPort();
                    setUdpPort.Header.MsgID = MessageIdTcp.SetUdpPort;
                    setUdpPort.Header.PacketSize = (ushort)Marshal.SizeOf(setUdpPort);
                    setUdpPort.UdpPortSend = UdpPort;
                    setUdpPort.UdpPortReceive = (ushort)(UdpPort + UdpIndex);
                    client.SendTcpMessage(Utilities.GetObjectToByte(setUdpPort));
                    _connectedClients.Add(client);
                    UdpIndex++;
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
                    _connectedClients.Remove(_disConnectedClients[i]);
                    _disConnectedClients.Remove(_disConnectedClients[i]);

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
                    Thread.Sleep(10);
                    if (_theStream is { DataAvailable: true })
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
        Debug.Log("Name: " + _connectedClients[c].Name + " MsgId: " + msgId);
        switch (msgId)
        {
            case MessageIdTcp.RequestForMatch:
                stRequestForMatch request = Utilities.GetObjectFromByte<stRequestForMatch>(msgData);
                AdventureSceneServer.Instance.EventAdventureScene.CallRequestAdventureMatch((GameRoomType)request.MatchType, _connectedClients[c]);
                break;
            case MessageIdTcp.AdventurePlayerLoadInfo:
                stAdventureRoomPlayerLoadInfo roomLoaded = Utilities.GetObjectFromByte<stAdventureRoomPlayerLoadInfo>(msgData);
                AdventureSceneServer.Instance.EventAdventureScene.CallPlayerLoaded(roomLoaded.RoomId, roomLoaded.PlayerIndex);
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
        if (!ServerReady)
        {
            return;
        }

        if (_tcpListener != null)
        {
            _tcpListener.Stop();
            _tcpListener = null;
            ServerReady = false;
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

    private void Update()
    {
        if (_connectedClients.Count > 0)
        {
            //연결된 클라이언트가 모든조건이 만족하면
            foreach (NetworkModule client in _connectedClients)
            {
                //SendMsgUdp(Utilities.GetObjectToByte(client.dataSync));
            }
        }

        Thread.Sleep(30);
    }
    public void UdpListenForIncomingRequest()
    {
        try
        {
            _udpReceive = new UdpClient(UdpPort);
            while (true)
            {
                IPEndPoint IPEndPointReceive = new IPEndPoint(IPAddress.Any, UdpPort);
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
            case MessageIdUdp.AdventurePlayerPositionToServer:
                stAdventurePlayerPositionToServer position = Utilities.GetObjectFromByte<stAdventurePlayerPositionToServer>(msgData);

                break;
        }
    }

}
