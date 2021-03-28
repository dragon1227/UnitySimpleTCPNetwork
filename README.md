## Exploring TCP Network in Unity

I have checked out several network libraries to use with unity. Then I wanted to
make a simple one from scratch. 

I found some tutorials and repos along the way and they helped a lot. 
I tried my best to keep simple and lean to make network and message handling.

Hope that this project helps you, if you're trying to figure out TCP network in your game.

### Sample

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


Open up Server scene.
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

Handler class responsible for 

## Referenced resources
Tom Weiland unity network series
Telepathy
SimpleTCPUnity

## LICENCE
MIT