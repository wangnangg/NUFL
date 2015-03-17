using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUFL.Framework.ProfilerCommunication;
using System.Threading;

namespace NUFL.Framework.Test.ProfilerCommunication
{
    [TestFixture]
    public class IPCStreamTests
    {
        IPCStream server;
        IPCStream client;

        [SetUp]
        public void Initialize()
        {
            UInt32 buffer_size = 1024 * 10;
            server = new IPCStream(buffer_size);
            client = new IPCStream(server.UniqueGuid, server.BufferSize);
        }
        [TearDown]
        public void Finialize()
        {
            server.Dispose();
            client.Dispose();
        }
        UInt32 num;
        [Test, Repeat(1000), Category("WangNan")]
        public void IPCStreamSimpleCommunication()
        {
            Thread server_thread = new Thread(new ThreadStart(IPCStreamSimpleCommunicationServerThreadFunc));
            Thread client_thread = new Thread(new ThreadStart(IPCStreamSimpleCommunicationClientThreadFunc));
            server_thread.Start();
            client_thread.Start();
            server_thread.Join();
            client_thread.Join();
            Assert.AreEqual(num, (UInt32)42);
        }

        private void IPCStreamSimpleCommunicationServerThreadFunc()
        {
            var data = BitConverter.GetBytes((UInt32)42);
            server.Write(data, 0, 4);
            server.Flush();
            server.StopWaitingIncoming();
        }
        private void IPCStreamSimpleCommunicationClientThreadFunc()
        {
            var data = new byte[4];
            client.Read(data, 0, 4);
            num = BitConverter.ToUInt32(data, 0);
            client.StopWaitingIncoming();
        }


        UInt32[] received_data;
        [Test, Repeat(100), Category("WangNan")]
        public void IPCStreamBulkCommunication()
        {
            Thread server_thread = new Thread(new ThreadStart(IPCStreamBulkCommunicationServerThreadFunc));
            Thread client_thread = new Thread(new ThreadStart(IPCStreamBulkCommunicationClientThreadFunc));
            server_thread.Start();
            client_thread.Start();
            server_thread.Join();
            client_thread.Join();
            Assert.AreEqual(1023 * 77, received_data.Length);
            foreach(var num in received_data)
            {
                Assert.AreEqual(num, (UInt32)42);
            }
        }

        private void IPCStreamBulkCommunicationServerThreadFunc()
        {
            var data = new byte[1023 * 77 * 4];
            int offset = 0;
            for (int i = 0; i < 1023 * 77; i++)
            {
                var tmp = BitConverter.GetBytes((UInt32)42);
                tmp.CopyTo(data, offset);
                offset += 4;
            }
            server.Write(data, 0, (UInt32)data.Length);
            server.Flush();
            server.StopWaitingIncoming();
        }
        private void IPCStreamBulkCommunicationClientThreadFunc()
        {
            var data = new byte[1023 * 77 * 4];
            UInt32 remain_bytes = 1023 * 77 * 4;
            int count = 0;

            client.Read(data, 0, remain_bytes);
            System.Console.WriteLine("read {0} times", ++count);
            

            received_data = new UInt32[1023 * 77];
            int offset = 0;
            for (int i = 0; i < received_data.Length; i++)
            {
                received_data[i] = BitConverter.ToUInt32(data, (int)offset);
                offset += 4;
            }

            client.StopWaitingIncoming();

        }
    }
}
