using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class Client: ISocketSend
{
    public event Action<byte[]> OnMessageReceived;

    private const int DATA_BUFFER_SIZE = 4096;

    private TcpClient _client;
    private NetworkStream _stream;
    private byte[] _receiveBuffer;

    public Client()
    {
        _client = new TcpClient
        {
            ReceiveBufferSize   = DATA_BUFFER_SIZE,
            SendBufferSize      = DATA_BUFFER_SIZE
        };
    }

    public void Start(string ip, int port)
    {
        _receiveBuffer = new byte[DATA_BUFFER_SIZE];
        _client.BeginConnect(ip, port, ConnectCallback, _client);
    }

    private void ConnectCallback(IAsyncResult result)
    {
        _client.EndConnect(result);

        if (_client.Connected == false)
        {
            return;
        }

        _stream = _client.GetStream();

        _stream.BeginRead(_receiveBuffer, 0, DATA_BUFFER_SIZE, MessageRecieved, null);
    }

    private void MessageRecieved(IAsyncResult result)
    {
        try
        {
            int byteLength = _stream.EndRead(result);
            if (byteLength > 0)
            {
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    OnMessageReceived?.Invoke(_receiveBuffer);
                });
            }
            else
            {
                Debug.Log("Error: recieved byte length " + byteLength);
                return;
            }


            _stream.BeginRead(_receiveBuffer, 0, DATA_BUFFER_SIZE, MessageRecieved, null);
        }
        catch (Exception ex)
        {
            Debug.Log($"Error receiving TCP data: {ex}");
        }

    }
    
    public void Send(byte[] data)
    {
        try
        {
            _stream.BeginWrite(data, 0, data.Length, null, null);
            
        }
        catch (Exception _ex)
        {
            Debug.Log($"Error sending data to server via TCP: {_ex}");
        }
    }

    public void Disconnect()
    {
        _client.Close();
        _stream = null;
        _receiveBuffer = null;
        _client = null;
    }
}