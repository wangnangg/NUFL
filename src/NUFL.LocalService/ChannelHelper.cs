﻿using System;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Reflection;
namespace NUFL.Service
{
    public static class ChannelHelper
    {

        /// <summary>
        ///  Create a TcpChannel with a given name, on a given port.
        /// </summary>
        /// <param name="port"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static TcpChannel CreateTcpChannel(string name, int port, int limit)
        {
            Hashtable props = new Hashtable();
            props.Add("port", port);
            props.Add("name", name);
            props.Add("bindTo", "127.0.0.1");
            //props.Add("timeout", 20);

            BinaryServerFormatterSinkProvider serverProvider =
                new BinaryServerFormatterSinkProvider();

            // NOTE: TypeFilterLevel and "clientConnectionLimit" property don't exist in .NET 1.0.
            Type typeFilterLevelType = typeof(object).Assembly.GetType("System.Runtime.Serialization.Formatters.TypeFilterLevel");
            if (typeFilterLevelType != null)
            {
                PropertyInfo typeFilterLevelProperty = serverProvider.GetType().GetProperty("TypeFilterLevel");
                object typeFilterLevel = Enum.Parse(typeFilterLevelType, "Full");
                typeFilterLevelProperty.SetValue(serverProvider, typeFilterLevel, null);

                //                props.Add("clientConnectionLimit", limit);
            }

            BinaryClientFormatterSinkProvider clientProvider =
                new BinaryClientFormatterSinkProvider();
            return new TcpChannel(props, clientProvider, serverProvider);
        }

        public static TcpChannel GetTcpChannel()
        {
            return GetTcpChannel("", 0, 2);
        }

        /// <summary>
        /// Get a channel by name, casting it to a TcpChannel.
        /// Otherwise, create, register and return a TcpChannel with
        /// that name, on the port provided as the second argument.
        /// </summary>
        /// <param name="name">The channel name</param>
        /// <param name="port">The port to use if the channel must be created</param>
        /// <returns>A TcpChannel or null</returns>
        public static TcpChannel GetTcpChannel(string name, int port)
        {
            return GetTcpChannel(name, port, 2);
        }


        // a nameless channel
        public static TcpChannel GetTcpChannel(int port, int limit)
        {
            return GetTcpChannel(Guid.NewGuid().ToString(), port, limit);
        }
        //nameless, we cares litte about the port
        public static TcpChannel GetTcpChannel(int limit)
        {
            return GetTcpChannel(Guid.NewGuid().ToString(), 0, limit);
        }


        /// <summary>
        /// Get a channel by name, casting it to a TcpChannel.
        /// Otherwise, create, register and return a TcpChannel with
        /// that name, on the port provided as the second argument.
        /// </summary>
        /// <param name="name">The channel name</param>
        /// <param name="port">The port to use if the channel must be created</param>
        /// <param name="limit">The client connection limit or negative for the default</param>
        /// <returns>A TcpChannel or null</returns>
        public static TcpChannel GetTcpChannel(string name, int port, int limit)
        {
            TcpChannel channel = ChannelServices.GetChannel(name) as TcpChannel;

           if (channel == null)
            {
                
                try
                {
                    channel = CreateTcpChannel(name, port, limit);
                    ChannelServices.RegisterChannel(channel, false);

                } catch(Exception)
                {
                    channel = null;
                }
            }

            return channel;
        }

        public static void SafeReleaseChannel(IChannel channel)
        {
            if (channel != null)
                try
                {
                    ChannelServices.UnregisterChannel(channel);
                }
                catch (RemotingException)
                {
                    // Channel was not registered - ignore
                }
        }
    }
}
