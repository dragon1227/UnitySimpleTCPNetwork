## Exploring TCP Network in Unity

I have checked out several network libraries to use with unity. Then I wanted to
make a simple one from scratch. 

I found some tutorials and repos along the way and they helped a lot. But mostly, they were too complicated or abstracted. I just needed a dune buggy.

So, I tried my best to keep it simple and lean.

Hope that this project helps you, if you're trying to figure out TCP network in your game.

### Why Unity as a Server?

Why would I use unity as a server while dotnet core is great?

Because unity has an editor, scene management, physics, navigation and so on. 

### Sample

Here is the complete network folder below.

```
...
Network
└───Message
│   │   Message.cs
│   │   MessageHandler.cs
│   │   MessageSerializeExtensions.cs
│   Client.cs
│   Connection.cs    
│   Server.cs
│   ThreadManager.cs
...
```

Open up the Server scene.
There is a component created for server to setup and listen `ServerApplicationStartup`.

```csharp
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
        _server.ClientConnectedEvent    += OnClientConnect;
        _server.MessageReceivedEvent    += OnMessage;
        _server.ClientDisconnectedEvent += OnClientDisconnect;

        _handler = new MessageHandler();
        _handler.AddHandler("chat", HandleChatMessage);
    }

    //  event callbacks
    private void OnClientConnect(int connId)
    {
        var greet = new GreetData()
        {
            id = connId,
            greetMessage = "Server connection success. Welcome to chat."
        };

        var msg = new Message()
        {
            header = "greet",
            body   = greet.Serialize()
        };

        _server.Send(connId, msg.Serialize());
    }

    private void OnMessage(int connId, byte[] data)
    {
        _handler.Handle(data);
    }

    private void OnClientDisconnect(int connId)
    {
        Debug.Log("Client connection lost. connId: " + connId);
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
```

For the client scene I created some ui elements. 

```csharp
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

    //  event callbacks
    private void MessageReceived(byte[] msg)
    {
        _handler.Handle(msg);
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

    private void OnApplicationQuit()
    {
        _client.Disconnect();
    }

    //  handlers
    private void HandleChatMessage(byte[] data)
    {
        var chat = data.Deserialize<ChatData>();
        _chatPanel.AddLog(chat.author + ": " + chat.entry);
    }

    private void GreetHandleMessage(byte[] data)
    {
        var greet = data.Deserialize<GreetData>();
        _chatPanel.AddLog(greet.greetMessage);
        _chatPanel.AddLog("Connection id: " + greet.id);
    }
}
```

Finally, `ThreadManager` is responsible for executing on main thread. 
Further info here,
https://stackoverflow.com/questions/41330771/use-unity-api-from-another-thread-or-call-a-function-in-the-main-thread

## Closing words

When I search for this topic, I saw so much discussion about unity, 
old and new unity network stack and it's server capabilities.

Also, in this forum thread, Joachim Ante(CTO, Unity) gives detailed answers to some good questions.

https://forum.unity.com/threads/dots-netcode-for-mmorpg.867463/

At this point, would I use a custom homebrew server from scratch in my in-production game?

Well if I need to fill my custom needs, I would bet my money on Telepathy and it's aproach to solve unity bounds and
battle tested codebase. Here some detailed info on this topic from the author,

https://mirror-networking.com/apathy/

https://github.com/vis2k/Telepathy


## Referenced resources
[Telepathy](https://github.com/vis2k/Telepathy)

[SimpleUnityTCP](https://github.com/EricBatlle/SimpleUnityTCP)

[Tom Weiland unity network series](https://www.youtube.com/watch?v=uh8XaC0Y5MA&list=PLXkn83W0QkfnqsK8I0RAz5AbUxfg3bOQ5)

[Facepunch.Steamworks](https://wiki.facepunch.com/steamworks/Creating_A_Socket_Server)


## LICENCE
MIT