﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace NUFL.Framework.ProfilerCommunication
{
    public class IPCStream:IDisposable
    {
        const string SERVER_WRITE_BUFFER_PREFIX = "ServerWriteBuffer#";
        const string CLIENT_WRITE_BUFFER_PREFIX = "ClientWriteBuffer#";
        const string SERVER_SENT_PREFIX = "ServerSent#";
        const string SERVER_RECEIVED_PREFIX = "ServerReceived#";
        const string CLIENT_SENT_PREFIX = "ClientSent#";
        const string CLIENT_RECEIVED_PREFIX = "ClientReceived#";
        string _unique_guid;
        MemoryMappedFile _write_mapped_file;
        MemoryMappedViewStream _write_stream;
        MemoryMappedFile _read_mapped_file;
        MemoryMappedViewStream _read_stream;
        UInt32 _buffer_size;
        EventWaitHandle _local_sent_event;
        EventWaitHandle _local_received_event;
        EventWaitHandle _remote_sent_event;
        EventWaitHandle _remote_received_event;
        ManualResetEvent _stop_waiting_for_incoming;
        ManualResetEvent _waiting_stopped;

        public string UniqueGuid
        {
            get
            {
                return _unique_guid;
            }
        }
        public UInt32 BufferSize
        {
            get
            {
                //actual buffer size is four byte larger
                return _buffer_size - 4;
            }
        }

        /// <summary>
        /// Server constructor
        /// </summary>
        /// <param name="buffer_size">specify memory mapped file size</param>
        public IPCStream(UInt32 buffer_size)
        {
            _unique_guid = Guid.NewGuid().ToString("X");
            _buffer_size = buffer_size + 4;
            ServerInitialize();
        }
        /// <summary>
        /// Client constructor
        /// </summary>
        /// <param name="unique_guid">the guid from server side</param>
        /// <param name="buffer_size">buffer size from server side</param>
        public IPCStream(string unique_guid, UInt32 buffer_size)
        {
            _unique_guid = unique_guid;
            _buffer_size = buffer_size + 4;
            ClientInitialize();
        }
        private void ServerInitialize()
        {
            string unique_write_buffer_name = SERVER_WRITE_BUFFER_PREFIX + _unique_guid;
            _write_mapped_file = MemoryMappedFile.CreateOrOpen(unique_write_buffer_name, _buffer_size, MemoryMappedFileAccess.ReadWrite);
            _write_stream = _write_mapped_file.CreateViewStream(0, _buffer_size, MemoryMappedFileAccess.Write);
            //first four bytes is the length of the buffer.
            _write_stream.Seek(4, SeekOrigin.Begin);

            string unique_read_buffer_name = CLIENT_WRITE_BUFFER_PREFIX + _unique_guid;
            _read_mapped_file = MemoryMappedFile.CreateOrOpen(unique_read_buffer_name, _buffer_size, MemoryMappedFileAccess.ReadWrite);
            _read_stream = _read_mapped_file.CreateViewStream(0, _buffer_size, MemoryMappedFileAccess.Read);
            _read_stream.Seek(0, SeekOrigin.Begin);

            _local_sent_event = CreateEvent(SERVER_SENT_PREFIX);
            _local_received_event = CreateEvent(SERVER_RECEIVED_PREFIX);
            _remote_sent_event = CreateEvent(CLIENT_SENT_PREFIX);
            _remote_received_event = CreateEvent(CLIENT_RECEIVED_PREFIX);

            _stop_waiting_for_incoming = new ManualResetEvent(false);
            _waiting_stopped = new ManualResetEvent(true);
            
        }
        private void ClientInitialize()
        {
            string unique_write_buffer_name = CLIENT_WRITE_BUFFER_PREFIX + _unique_guid;
            _write_mapped_file = MemoryMappedFile.CreateOrOpen(unique_write_buffer_name, _buffer_size, MemoryMappedFileAccess.ReadWrite);
            _write_stream = _write_mapped_file.CreateViewStream(0, _buffer_size, MemoryMappedFileAccess.Write);
            //first four bytes is the length of the buffer.
            _write_stream.Seek(4, SeekOrigin.Begin);

            string unique_read_buffer_name = SERVER_WRITE_BUFFER_PREFIX + _unique_guid;
            _read_mapped_file = MemoryMappedFile.CreateOrOpen(unique_read_buffer_name, _buffer_size, MemoryMappedFileAccess.ReadWrite);
            _read_stream = _read_mapped_file.CreateViewStream(0, _buffer_size, MemoryMappedFileAccess.Read);
            _read_stream.Seek(0, SeekOrigin.Begin);

            _local_sent_event = CreateEvent(CLIENT_SENT_PREFIX);
            _local_received_event = CreateEvent(CLIENT_RECEIVED_PREFIX);
            _remote_sent_event = CreateEvent(SERVER_SENT_PREFIX);
            _remote_received_event = CreateEvent(SERVER_RECEIVED_PREFIX);

            _stop_waiting_for_incoming = new ManualResetEvent(false);
            _waiting_stopped = new ManualResetEvent(true);
        }
        private EventWaitHandle CreateEvent(string prefix)
        {
            return new EventWaitHandle(false, EventResetMode.AutoReset, prefix + _unique_guid);
        }

        public void Write(byte[] buffer,UInt32 offset, UInt32 length)
        {
            UInt32 remain_bytes = length;
            UInt32 current_position = (UInt32)_write_stream.Position;
            while( remain_bytes > 0 )
            {
                UInt32 write_bytes = Math.Min(remain_bytes, _buffer_size - current_position);
                _write_stream.Write(buffer, (int)offset, (int)write_bytes);
                remain_bytes -= write_bytes;
                offset += write_bytes;
                current_position = (UInt32)_write_stream.Position;
                if(current_position == _buffer_size)
                {
                    Flush();
                    current_position = (UInt32)_write_stream.Position;
                }
            }
        }

        UInt32 unread_bytes = 0;
        public UInt32 Read(byte[] buffer, UInt32 offset, UInt32 length)
        {
            _waiting_stopped.Reset();
            UInt32 actual_read_bytes = 0;
            while (actual_read_bytes < length)
            {
                if (unread_bytes == 0)
                {
                    int wait_result = WaitHandle.WaitAny(new WaitHandle[] { _remote_sent_event, _stop_waiting_for_incoming });
                    switch (wait_result)
                    {
                        case 0:
                            byte[] tmp = new byte[4];
                            _read_stream.Read(tmp, 0, 4);
                            unread_bytes = BitConverter.ToUInt32(tmp, 0);
                            break;
                        case 1:
                            //we should return;
                            _waiting_stopped.Set();
                            return actual_read_bytes;
                    }
                }

                UInt32 read_bytes = Math.Min(unread_bytes, length);
                _read_stream.Read(buffer, (int)offset, (int)read_bytes);
                offset += read_bytes;
                actual_read_bytes += read_bytes;
                unread_bytes -= read_bytes;
                if (unread_bytes == 0)
                {
                    _read_stream.Seek(0, SeekOrigin.Begin);
                    _local_received_event.Set();
                }
            }

            _waiting_stopped.Set();
            return actual_read_bytes;
        }

        public UInt32 ReadOnce(byte[] buffer, UInt32 offset, UInt32 length)
        {
            _waiting_stopped.Reset();
            UInt32 actual_read_bytes = 0;
            if (unread_bytes == 0)
            {
                int wait_result = WaitHandle.WaitAny(new WaitHandle[] { _remote_sent_event, _stop_waiting_for_incoming });
                switch (wait_result)
                {
                    case 0:
                        byte[] tmp = new byte[4];
                        _read_stream.Read(tmp, 0, 4);
                        unread_bytes = BitConverter.ToUInt32(tmp, 0);
                        break;
                    case 1:
                        //we should return;
                        _waiting_stopped.Set();
                        return actual_read_bytes;
                }
            }

            UInt32 read_bytes = Math.Min(unread_bytes, length);
            _read_stream.Read(buffer, (int)offset, (int)read_bytes);
            offset += read_bytes;
            actual_read_bytes += read_bytes;
            unread_bytes -= read_bytes;
            if (unread_bytes == 0)
            {
                _read_stream.Seek(0, SeekOrigin.Begin);
                _local_received_event.Set();
            }
            

            _waiting_stopped.Set();
            return actual_read_bytes;
        }

        

        public void Flush()
        {
            UInt32 length_in_bytes = (UInt32)_write_stream.Position - (UInt32)4;
            if (length_in_bytes <= 0)
            {
                return;
            }
            _write_stream.Seek(0, SeekOrigin.Begin);
            _write_stream.Write(BitConverter.GetBytes(length_in_bytes), 0, 4);
            _local_sent_event.Set();
            _remote_received_event.WaitOne();
        }

        public void StopWaitingIncoming()
        {
            _stop_waiting_for_incoming.Set();
            _waiting_stopped.WaitOne();
        }

        public void Dispose()
        {
            _write_stream.Dispose();
            _write_mapped_file.Dispose();
            _read_stream.Dispose();
            _read_mapped_file.Dispose();
            _local_sent_event.Dispose();
            _local_received_event.Dispose();
            _remote_sent_event.Dispose();
            _remote_received_event.Dispose();
        }
    }
}
