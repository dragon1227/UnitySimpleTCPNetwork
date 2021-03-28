using System;
using UnityEngine;

public class ServerApplicationStartup : MonoBehaviour
{
    private Server _server;
    private MessageHandler _handler;

    private void Start()
    {
        _server = new Server(7777);
        _server.Start();
        _server.ClientConnectedEvent += OnClientConnect;
        _server.MessageReceivedEvent += OnMessage;
        _server.ClientConnectedEvent += OnClientDisconnect;

        _handler = new MessageHandler();
        _handler.AddHandler("chat", HandleChatMessage);

    }

    //  event callbacks
    private void OnClientConnect(int connId)
    {

    }

    private void OnMessage(int connId, byte[] data)
    {
        _handler.Handle(data);
    }

    private void OnClientDisconnect(int connId)
    {

    }

    private void OnApplicationQuit()
    {
        _server.Stop();
    }

    //  handlers
    private void HandleChatMessage(byte[] data)
    {
        var chat = data.Deserialize<ChatData>();

        Debug.Log("received chat, author: " + chat.author);

        var msg = new Message()
        {
            header = "chat",
            body = data
        };
        
        _server.SendAll(msg.Serialize());
    }
}