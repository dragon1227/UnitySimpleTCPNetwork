using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server
{
    public event Action<int>            ClientConnectedEvent;
    public event Action<int, byte[]>    MessageReceivedEvent;
    public event Action<int>            ClientDisconnectedEvent;

    private TcpListener _server;
    private Dictionary<int, Connection> _connections;

    public Server(int port)
    {
        _server = new TcpListener(IPAddress.Any, port);
        _connections = new Dictionary<int, Connection>();
    }

    public void Start()
    {        
        _server.Start();
        _server.BeginAcceptTcpClient(ClientConnected, null);
        Debug.Log("Server started.");
    }

    public void SendAll(byte[] data)
    {
        foreach (var c in _connections)
        {
            c.Value.Send(data);
        }
    }

    public void Send(int id, byte[] data)
    {
        _connections[id].Send(data);
    }

    private void ClientConnected(IAsyncResult result)
    {
        var tcpClient = _server.EndAcceptTcpClient(result);
        _server.BeginAcceptTcpClient(ClientConnected, null);

        int id = System.Guid.NewGuid().GetHashCode();
        var conn = new Connection(id);
        _connections.Add(id, conn);
        _connections[id].Connect(tcpClient);
        _connections[id].OnMessageReceived += OnMessageReceived;
        _connections[id].OnDisconnected    += OnClientDisconnected;

        ClientConnectedEvent?.Invoke(conn.id);
        Debug.Log($"Incoming connection from {tcpClient.Client.RemoteEndPoint}...");
    }

    private void OnClientDisconnected(int id)
    {
        _connections[id].Disconnect();
        _connections.Remove(id);

        ClientDisconnectedEvent?.Invoke(id);
    }

    private void OnMessageReceived(int id, byte[] data)
    {
        MessageReceivedEvent?.Invoke(id, data);
    }

    public void Stop()
    {
        _server.Stop();
    }

}