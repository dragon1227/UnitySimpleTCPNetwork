using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientApplicationStartup : MonoBehaviour
{
    public string ip = "127.0.0.1";
    public int port = 7777;
    public Transform canvasParent;
    public GameObject loginPrefab;
    public GameObject chatPrefab;

    private string _clientNickname;
    private ChatPanel _chatPanel;

    private Client _client;
    private MessageHandler _handler;

    private void Start()
    {
        var login = Instantiate(loginPrefab, canvasParent).GetComponent<LoginPanel>();
        login.inputField.onEndEdit.AddListener(input => _clientNickname = input );
        login.connectButton.onClick.AddListener(() => Connect());
    }

    private void CreateChatPanel()
    {
        _chatPanel = Instantiate(chatPrefab, canvasParent).GetComponent<ChatPanel>();
        _chatPanel.chatLogText.text = "";
        _chatPanel.inputField.onEndEdit.AddListener(OnChatInput);
        _chatPanel.inputField.ActivateInputField();
    }
    
    private void Connect()
    {
        CreateChatPanel();

        _client = new Client();
        _client.Start(ip, port);
        _client.OnMessageReceived   += MessageReceived;
        
        _handler = new MessageHandler();
        _handler.AddHandler("chat",     HandleChatMessage);
        _handler.AddHandler("greet",    GreetHandleMessage);

    }

    private void OnChatInput(string input)
    {
        if (!string.IsNullOrEmpty(input) && !string.IsNullOrWhiteSpace(input))
        {
            var chat = new ChatData()
            {
                author = _clientNickname,
                entry  = input
            };

            var msg = new Message()
            {
                header = "chat",
                body   = chat.Serialize()
            };

            _client.Send(msg.Serialize());

            _chatPanel.inputField.text = "";
            _chatPanel.inputField.ActivateInputField();
        }
    }




    private void MessageReceived(byte[] msg)
    {
        _handler.Handle(msg);
    }

    private void OnApplicationQuit()
    {
        _client.Disconnect();
    }

    private void HandleChatMessage(byte[] data)
    {
        var chat = data.Deserialize<ChatData>();

        //  log to view
        _chatPanel.AddLog(chat.author + ": " + chat.entry);
    }

    private void GreetHandleMessage(byte[] data)
    {
        var greet = data.Deserialize<GreetData>();

        _chatPanel.AddLog(greet.greetMessage);
        _chatPanel.AddLog("Connection id: " + greet.id);
    }
}
