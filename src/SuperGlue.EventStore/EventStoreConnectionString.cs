﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Common.Log;
using EventStore.ClientAPI.SystemData;

namespace SuperGlue.EventStore
{
    public class EventStoreConnectionString
    {
        private readonly ConnectionOptions _connectionOptions;

        public EventStoreConnectionString(string connectionStringName)
        {
            var connectionString = ConfigurationManager.ConnectionStrings[connectionStringName];

            var options = connectionString
                .ConnectionString
                .Split(';')
                .Select(x => new ConnectionPart(x))
                .ToDictionary(x => x.Name, x => x.Value);

            _connectionOptions = new ConnectionOptions(options, connectionStringName);
        }

        public IEventStoreConnection CreateConnection(Action<ConnectionSettingsBuilder> modifySettings = null)
        {
            modifySettings = modifySettings ?? (x => { });

            var connectionSettings = ConnectionSettings.Create();

            var userCredentials = _connectionOptions.GetUserCredentials();

            if (userCredentials != null)
                connectionSettings.SetDefaultUserCredentials(userCredentials);

            modifySettings(connectionSettings);

            return EventStoreConnection.Create(connectionSettings, _connectionOptions.GetIpEndPoint());
        }

        public ProjectionsManager CreateProjectionsManager()
        {
            return new ProjectionsManager(new ConsoleLogger(), _connectionOptions.GetHttpIpEndPoint(), TimeSpan.FromMilliseconds(5000));
        }

        public UserCredentials GetUserCredentials()
        {
            return _connectionOptions.GetUserCredentials();
        }

        private class ConnectionPart
        {
            public ConnectionPart(string part)
            {
                var parts = part.Split('=');

                if (!parts.Any())
                    return;

                Name = parts[0];

                if (parts.Length < 2)
                    return;

                Value = parts[1];
            }

            public string Name { get; private set; }
            public string Value { get; private set; }
        }

        private class ConnectionOptions
        {
            public ConnectionOptions(IReadOnlyDictionary<string, string> options, string connectionStringName)
            {
                if (!options.ContainsKey("Ip"))
                    throw new InvalidEventstoreConnectionStringException(string.Format("The connection string named \"{0}\" doesn't contain a ip.", connectionStringName));

                Ip = options["Ip"];

                Api = ConnectionApi.Tcp;

                ConnectionApi connectionApi;
                if (options.ContainsKey("Api") && Enum.TryParse(options["Api"], out connectionApi))
                    Api = connectionApi;

                HttpPort = 2113;
                int httpPort;
                if (options.ContainsKey("HttpPort") && int.TryParse(options["HttpPort"], out httpPort))
                    HttpPort = httpPort;

                TcpPort = 1113;
                int tcpPort;
                if (options.ContainsKey("TcpPort") && int.TryParse(options["TcpPort"], out tcpPort))
                    TcpPort = tcpPort;

                if (options.ContainsKey("UserName"))
                    UserName = options["UserName"];

                if (options.ContainsKey("Password"))
                    Password = options["Password"];
            }

            public string Ip { get; private set; }
            public int HttpPort { get; private set; }
            public int TcpPort { get; private set; }
            public string UserName { get; private set; }
            public string Password { get; private set; }
            public ConnectionApi Api { get; private set; }

            public IPEndPoint GetIpEndPoint()
            {
                switch (Api)
                {
                    case ConnectionApi.Http:
                        return new IPEndPoint(IPAddress.Parse(Ip), HttpPort);
                    case ConnectionApi.Tcp:
                        return new IPEndPoint(IPAddress.Parse(Ip), TcpPort);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            public IPEndPoint GetHttpIpEndPoint()
            {
                return new IPEndPoint(IPAddress.Parse(Ip), HttpPort);
            }

            public UserCredentials GetUserCredentials()
            {
                if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password))
                    return null;

                return new UserCredentials(UserName, Password);
            }
        }

        private enum ConnectionApi
        {
            Http,
            Tcp
        }
    }
}