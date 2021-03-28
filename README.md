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
private Server _server;

private void Start()
{
    _server = new Server(7777);
    _server.Start();
    _server.ClientConnectedEvent    += OnClientConnect;
    _server.MessageReceivedEvent    += OnMessage;
    _server.ClientDisconnectedEvent += OnClientDisconnect;
}
```

That's it. 

I also added a pipeline, `MessageHandler` class responsible for unpacking messages and invoking responsible method.

```csharp
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
```


```csharp
private void OnMessage(int connId, byte[] data)
{
    _handler.Handle(data);
}
```

```csharp
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
```

Handler will invoke methods based on header info in `Message`

```csharp
[System.Serializable]
public class Message
{
    public string header;
    public byte[] body;
}
```

And the Client scene and `ClientApplicationStartup` is almost identical to server.

```csharp
private void Connect()
{
    _client = new Client();
    _client.Start(ip, port);
    _client.OnMessageReceived   += MessageReceived;
}
```

I also created a chat panel to send and receive chat messages for client scene.

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