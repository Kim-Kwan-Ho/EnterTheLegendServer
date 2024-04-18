using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using StandardData;

public class NetworkModule
{
    private TcpClient _tcpSocket;

    public TcpClient TcpSocket
    {
        get { return _tcpSocket; }
    }

    private NetworkStream _stream;

    public NetworkStream Stream
    {
        get { return _stream; }
    }
    private string _id;
    public string Id
    {
        get { return this._id; }
    }

    private string _nickname;

    public string Nickname
    {
        get { return this._nickname; }
    }
    private IPEndPoint _ipEndPoint;

    private UdpClient _udpSocket;

    private byte[] _bytes = new byte[NetworkSize.BufferSize];

    public byte[] Bytes
    {
        get { return _bytes; }
    }
   
    private byte[] _tempBytes = new byte[NetworkSize.TempBufferSize];
    public byte[] TempBytes
    {
        get { return _tempBytes; }
    }

    private bool _isTempByte = false;
    public bool IsTempByte
    {
        get {return this._isTempByte;}
    }
    private int _tempByteSize = 0;
    public int TempByteSize
    {
        get { return this._tempByteSize; }
    }
    public NetworkModule(TcpClient clientSocket, IPEndPoint ipEndPoint, string name = "client")
    {
        _tcpSocket = clientSocket;
        _stream = _tcpSocket.GetStream();
        _id = name;
        _ipEndPoint = ipEndPoint;
        _udpSocket = new UdpClient();
    }

    public void SetId(string id)
    {
        _id = id;
    }

    public void SetNickname(string nickname)
    {
        _nickname = nickname;
    }
    public void SetIsTempByte(bool isTempByte)
    {
        this._isTempByte = isTempByte;
    }
    
    public bool IsConnected()
    {
        return _tcpSocket != null && _tcpSocket.Connected && _stream != null;
    }


    public void SetTempByteSize(int tempByteSize)
    {
        this._tempByteSize = tempByteSize;
    }
    public void SendTcpMessage(byte[] bytes)
    {
        if (!IsConnected())
            return;
        _stream.Write(bytes, 0, bytes.Length);
        _stream.Flush();
    }

    public void CloseModule()
    {
        _tcpSocket.Close();
        _udpSocket.Close();
        _tcpSocket = null;
        _udpSocket = null;
    }

    public void SendUdpMessage(byte[] message)
    {
        if (_udpSocket != null)
            _udpSocket.Send(message, message.Length, _ipEndPoint);
    }


}
