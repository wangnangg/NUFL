using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
namespace NUFL.Framework.ProfilerCommunication
{
    public class ProfilerMessageDispatcher:IDisposable
    {
        IPCStream _msg_stream;
        IPCStream _data_stream;
        Thread _worker;
        Action<object, IPCStream>[] _handlers;
        AutoResetEvent _stopped_event;
        public ProfilerMessageDispatcher()
        {
            _msg_stream = new IPCStream(16 * 1024);
            _data_stream = new IPCStream(256 * 1024);
            _stopped_event = new AutoResetEvent(false);
            //there will never be more than ten kinds of messages.
            _handlers = new Action<object, IPCStream>[10];

        }
        public string MsgStreamGuid
        {
            get
            {
                return _msg_stream.UniqueGuid;
            }
        }
        public UInt32 MsgStreamBufferSize
        {
            get
            {
                return _msg_stream.BufferSize;
            }
        }
        public IPCStream DataStream
        {
            get
            {
                return _data_stream;
            }
        }

        public void Start()
        {
            _worker = new Thread(new ThreadStart(WorkerThreadFunc));
            _worker.Start();
        }
        public void Stop()
        {
            _msg_stream.StopWaitingIncoming();
            _data_stream.StopWaitingIncoming();
            _worker.Join();
        }

        public void RegisterHandler(MSG_Type msg_type, Action<object, IPCStream> handler)
        {
            _handlers[(int)msg_type] += handler;
        }

        public void UnregisterHandler(MSG_Type msg_type, Action<object, IPCStream> handler)
        {
            _handlers[(int)msg_type] -= handler;
        }

        private void WorkerThreadFunc()
        {
            byte[] msg_type_buffer = new byte[MsgMaxSize];
            while (_msg_stream.Read(msg_type_buffer, 0, 4) == 4)
            {
                MSG_Type msg_type = (MSG_Type)BitConverter.ToInt32(msg_type_buffer, 0);
                object msg = null;
                int remain_size = 0;
                switch (msg_type)
                {
                    case MSG_Type.MSG_TrackAssembly:
                        remain_size = Marshal.SizeOf(typeof(MSG_TrackAssembly_Request)) - 4;
                        _msg_stream.Read(msg_type_buffer, 4, (UInt32)remain_size);
                        msg = GetMsgFromBytes<MSG_TrackAssembly_Request>(msg_type_buffer);
                        break;
                    case MSG_Type.MSG_GetSequencePoints:
                        remain_size = Marshal.SizeOf(typeof(MSG_GetSequencePoints_Request)) - 4;
                        _msg_stream.Read(msg_type_buffer, 4, (UInt32)remain_size);
                        msg = GetMsgFromBytes<MSG_GetSequencePoints_Request>(msg_type_buffer);
                        break;
                    default:
                        Debug.WriteLine("Unexpected Message. Abort");
                        return;

                }
                try
                {
                    _handlers[(Int32)msg_type](msg, _msg_stream);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Message handler exception: {0}", ex.ToString());
                    return;
                }
            }
        }


        private static T GetMsgFromBytes<T>(byte[] buffer)
        {
            T msg = default(T);
            int size = Marshal.SizeOf(msg);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(buffer, 0, ptr, size);
            msg = (T)Marshal.PtrToStructure(ptr, msg.GetType());
            Marshal.FreeHGlobal(ptr);
            return msg;
        }



        static UInt32 _read_size = 0;
        public static UInt32 MsgMaxSize
        {
            get
            {
                if (_read_size == 0)
                {
                    _read_size = (UInt32)((new[] { 
                        Marshal.SizeOf(typeof(MSG_TrackAssembly_Request)), 
                        Marshal.SizeOf(typeof(MSG_TrackAssembly_Response)), 
                        Marshal.SizeOf(typeof(MSG_GetSequencePoints_Request)),
                    }).Max());
                }
                return _read_size;
            }
        }

        public static void SendMessage<T>(IPCStream msg_stream, ref T msg)
        {
            byte[] buffer = ToBytes<T>(ref msg);
            msg_stream.Write(buffer, 0, (UInt32)buffer.Length);
            msg_stream.Flush();
        }

        public static void SendMessageNoFlush<T>(IPCStream msg_stream, ref T msg)
        {
            byte[] buffer = ToBytes<T>(ref msg);
            msg_stream.Write(buffer, 0, (UInt32)buffer.Length);
        }
        public static byte[] ToBytes<T>(ref T msg)
        {
            int size = Marshal.SizeOf(msg);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(msg, ptr, true);
            byte[] buffer = new byte[size];
            Marshal.Copy(ptr, buffer, 0, size);
            Marshal.FreeHGlobal(ptr);
            return buffer;
        }

        public void Dispose()
        {
            _msg_stream.Dispose();
            _data_stream.Dispose();
            _stopped_event.Dispose();
        }
    }
}
