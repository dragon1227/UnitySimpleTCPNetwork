using System;
using System.Collections.Generic;

public class MessageHandler
{
    private Dictionary<string, Action<byte[]>> _handlers;

    public MessageHandler()
    {
        _handlers = new Dictionary<string, Action<byte[]>>();
    }

    public void AddHandler(string type, Action<byte[]> func)
    {
        _handlers.Add(type, func);
    }

    public void Handle(byte[] data)
    {
        var msg = data.Deserialize<Message>();
        _handlers[msg.header](msg.body);
    }
}