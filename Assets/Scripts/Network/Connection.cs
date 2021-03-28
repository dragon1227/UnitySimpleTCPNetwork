using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class Connection: ISocketSend
{
    public readonly int id;
    public event Action<int, byte[]> OnMessageReceived;
    public event Action<int>         OnDisconnected;

    private const int DATA_BUFFER_SIZE = 4096;

    private TcpClient _client;
    private NetworkStream _stream;
    private byte[] _receiveBuffer;

    public Connection(int id)
    {
        this.id = id;
    }

    public void Connect(TcpClient client)
    {
        _client = client;
        _client.ReceiveBufferSize   = DATA_BUFFER_SIZE;
        _client.SendBufferSize      = DATA_BUFFER_SIZE;

        _stream = _client.GetStream();

        _receiveBuffer = new byte[DATA_BUFFER_SIZE];
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
                    OnMessageReceived?.Invoke(id, _receiveBuffer);
                });
            }
            else
            {
                OnDisconnected?.Invoke(id);
                Debug.Log("Error: recieved byte length " + byteLength);
                return;
            }


            _stream.BeginRead(_receiveBuffer, 0, DATA_BUFFER_SIZE, MessageRecieved, null);
        }
        catch (Exception ex)
        {
            OnDisconnected?.Invoke(id);
            Debug.Log($"Error receiving TCP data: {ex}");
        }

    }
    
    public void Send(byte[] data)
    {
        try
        {
            _stream.BeginWrite(data, 0, data.Length, null, null);
            
        }
        catch (Exception ex)
        {
            Debug.Log($"Error sending data to player {id} via TCP: {ex}");
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