﻿using RabbitMQ.Client;
using System;

namespace EventBus.RabbitMq
{
    public interface IPersistConnection
       : IDisposable
    {
        bool IsConnected { get; }

        bool TryConnect();

        IModel CreateModel();
    }
}
