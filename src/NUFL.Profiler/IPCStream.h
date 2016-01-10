#ifndef __IPCSTREAM_H
#define __IPCSTREAM_H
#include "windows.h"
#include "stdafx.h"

class MemoryStream
{
private:
	char* _memory_buffer;
	UINT32 _size;
	UINT32 _position;
public:
	MemoryStream() {}
	void SetBuffer(char *buffer, UINT32 size)
	{
		_memory_buffer = buffer;
		_size = size;
		_position = 0;
	}
	void Write(const char buffer[], int size)
	{
		memcpy(_memory_buffer + _position, buffer, size);
		_position += size;
	}
	void Read(char buffer[], int size)
	{
		memcpy(buffer, _memory_buffer + _position, size);
		_position += size;
	}
	void Rewind()
	{
		_position = 0;
	}
	UINT32 GetPostion()
	{
		return _position;
	}
	BOOL Full()
	{
		return _position == _size;
	}
	UINT32 RemainSizeInBytes()
	{
		return _size - _position;
	}
};

class IPCStream
{
private:
	const wchar_t* SERVER_WRITE_BUFFER_PREFIX = _T("ServerWriteBuffer#");
	const wchar_t* CLIENT_WRITE_BUFFER_PREFIX = _T("ClientWriteBuffer#");
	const wchar_t* SERVER_SENT_PREFIX = _T("ServerSent#");
	const wchar_t* SERVER_RECEIVED_PREFIX = _T("ServerReceived#");
	const wchar_t* CLIENT_SENT_PREFIX = _T("ClientSent#");
	const wchar_t* CLIENT_RECEIVED_PREFIX = _T("ClientReceived#");
	std::wstring _unique_guid;
	UINT32 _buffer_size;

	HANDLE _write_mapped_file;
	MemoryStream _write_stream;
	LPVOID _view_of_write_buff;

	HANDLE _read_mapped_file;
	MemoryStream _read_stream;
	LPVOID _view_of_read_buff;

	HANDLE _local_sent_event;
	HANDLE _local_received_event;
	HANDLE _remote_sent_event;
	HANDLE _remote_received_event;
	HANDLE _stop_waiting_for_incoming;
	HANDLE _waiting_stopped;
public:
	/// <summary>constructor for client side</summary>
	bool Initialize(const std::wstring& unique_guid, UINT32 buffer_size)
	{
		OutputDebugString(_T("Enter client side constructor\n"));
		_unique_guid = unique_guid;
		_buffer_size = buffer_size + 4;
		return ClientInitialize();
	}
	/// <summary>constructor for server side only for test purpose</summary>
	bool Initialize(UINT32 buffer_size)
	{
		OutputDebugString(_T("Enter server side constructor\n"));
		_unique_guid = std::wstring(_T("guid1"));
		_buffer_size = buffer_size + 4;
		return ServerInitialize();
	}
	IPCStream(){}
	~IPCStream()
	{
		UnmapViewOfFile(_view_of_write_buff);
		CloseHandle(_write_mapped_file);

		UnmapViewOfFile(_view_of_read_buff);
		CloseHandle(_read_mapped_file);

		CloseHandle(_local_sent_event);
		CloseHandle(_local_received_event);
		CloseHandle(_remote_sent_event);
		CloseHandle(_remote_received_event);
		CloseHandle(_stop_waiting_for_incoming);
		CloseHandle(_waiting_stopped);

	}
private:
	bool ClientInitialize()
	{
		std::wstring unique_write_buffer_name = CLIENT_WRITE_BUFFER_PREFIX + _unique_guid;

		_write_mapped_file = CreateFileMapping(
			INVALID_HANDLE_VALUE,
			NULL, //security
			PAGE_READWRITE,  //access rights
			0,  //size high
			_buffer_size, //size low
			unique_write_buffer_name.c_str() //kernel object name
			);
		if (_write_mapped_file == NULL)
		{
			OutputDebugString(_T("Failed to create client write buffer\n"));
			return false;
		}
		_view_of_write_buff = MapViewOfFile(
			_write_mapped_file, //handle of the mapped file
			FILE_MAP_WRITE,  //access rights
			0, //offset high
			0, //offset low
			_buffer_size //size to map
			);
		if (_view_of_write_buff == NULL)
		{
			OutputDebugString(_T("Failed to create client view of write buffer\n"));
			return false;
		}
		//we don't touch the first four bytes
		_write_stream.SetBuffer((char*)_view_of_write_buff + 4, _buffer_size - 4);


		std::wstring unique_read_buffer_name = SERVER_WRITE_BUFFER_PREFIX + _unique_guid;
		_read_mapped_file = CreateFileMapping(
			INVALID_HANDLE_VALUE,
			NULL, //security
			PAGE_READWRITE,  //access rights
			0,  //size high
			_buffer_size, //size low
			unique_read_buffer_name.c_str() //kernel object name
			);
		if (_read_mapped_file == NULL)
		{
			OutputDebugString(_T("Failed to create server read buffer\n"));
			return false;
		}
		_view_of_read_buff = MapViewOfFile(
			_read_mapped_file, //handle of the mapped file
			FILE_MAP_READ,  //access rights
			0, //offset high
			0, //offset low
			_buffer_size //size to map
			);
		if (_view_of_read_buff == NULL)
		{
			OutputDebugString(_T("Failed to create client view of read buffer\n"));
			return false;
		}
		//we don't read the first four bytes
		_read_stream.SetBuffer((char*)_view_of_read_buff + 4, _buffer_size - 4);

		_local_sent_event = CreateEventWithPrefix(CLIENT_SENT_PREFIX);
		_local_received_event = CreateEventWithPrefix(CLIENT_RECEIVED_PREFIX);
		_remote_sent_event = CreateEventWithPrefix(SERVER_SENT_PREFIX);
		_remote_received_event = CreateEventWithPrefix(SERVER_RECEIVED_PREFIX);
		_stop_waiting_for_incoming = CreateEvent(
			NULL, //security
			TRUE, //manual reset?
			FALSE, //signaled?
			NULL //name?
			);
		_waiting_stopped = CreateEvent(
			NULL, //security
			TRUE, //manual reset?
			TRUE, //signaled?
			NULL //name?
			);

		if (!(_local_sent_event &&
			_local_received_event &&
			_remote_sent_event &&
			_remote_received_event &&
			_stop_waiting_for_incoming &&
			_waiting_stopped))
		{
			OutputDebugString(_T("Failed to initialize events\n"));
			return false;
		}
		read_events[0] = _remote_sent_event;
		read_events[1] = _stop_waiting_for_incoming;
		return true;
	}

	bool ServerInitialize()
	{
		std::wstring unique_write_buffer_name = SERVER_WRITE_BUFFER_PREFIX + _unique_guid;

		_write_mapped_file = CreateFileMapping(
			INVALID_HANDLE_VALUE,
			NULL, //security
			PAGE_READWRITE,  //access rights
			0,  //size high
			_buffer_size, //size low
			unique_write_buffer_name.c_str() //kernel object name
			);
		if (_write_mapped_file == NULL)
		{
			OutputDebugString(_T("Failed to create server write buffer\n"));
			return false;
		}
		_view_of_write_buff = MapViewOfFile(
			_write_mapped_file, //handle of the mapped file
			FILE_MAP_WRITE,  //access rights
			0, //offset high
			0, //offset low
			_buffer_size //size to map
			);
		if (_view_of_write_buff == NULL)
		{
			OutputDebugString(_T("Failed to create server view of write buffer\n"));
			return false;
		}
		//we don't touch the first four bytes
		_write_stream.SetBuffer((char*)_view_of_write_buff + 4, _buffer_size - 4);


		std::wstring unique_read_buffer_name = CLIENT_WRITE_BUFFER_PREFIX + _unique_guid;
		_read_mapped_file = CreateFileMapping(
			INVALID_HANDLE_VALUE,
			NULL, //security
			PAGE_READWRITE,  //access rights
			0,  //size high
			_buffer_size, //size low
			unique_read_buffer_name.c_str() //kernel object name
			);
		if (_read_mapped_file == NULL)
		{
			OutputDebugString(_T("Failed to create server read buffer\n"));
			return false;
		}
		_view_of_read_buff = MapViewOfFile(
			_read_mapped_file, //handle of the mapped file
			FILE_MAP_READ,  //access rights
			0, //offset high
			0, //offset low
			_buffer_size //size to map
			);
		if (_view_of_read_buff == NULL)
		{
			OutputDebugString(_T("Failed to create server view of read buffer\n"));
			return false;
		}
		//we don't read the first four bytes
		_read_stream.SetBuffer((char*)_view_of_read_buff + 4, _buffer_size - 4);

		_local_sent_event = CreateEventWithPrefix(SERVER_SENT_PREFIX);
		_local_received_event = CreateEventWithPrefix(SERVER_RECEIVED_PREFIX);
		_remote_sent_event = CreateEventWithPrefix(CLIENT_SENT_PREFIX);
		_remote_received_event = CreateEventWithPrefix(CLIENT_RECEIVED_PREFIX);
		_stop_waiting_for_incoming = CreateEvent(
			NULL, //security
			TRUE, //manual reset?
			FALSE, //signaled?
			NULL //name?
			);
		_waiting_stopped = CreateEvent(
			NULL, //security
			TRUE, //manual reset?
			TRUE, //signaled?
			NULL //name?
			);

		if (!(_local_sent_event &&
			_local_received_event &&
			_remote_sent_event &&
			_remote_received_event &&
			_stop_waiting_for_incoming &&
			_waiting_stopped))
		{
			OutputDebugString(_T("Failed to initialize events\n"));
			return false;
		}
		read_events[0] = _remote_sent_event;
		read_events[1] = _stop_waiting_for_incoming;
		return true;
	}

	HANDLE CreateEventWithPrefix(const wchar_t* prefix)
	{
		std::wstring unique_event_name = prefix + _unique_guid;
		return CreateEvent(
			NULL, //security
			FALSE, //manual reset?
			FALSE, //signaled?
			unique_event_name.c_str()  //name
			);
	}

public:
	void Write(const char buffer[], UINT32 offset, UINT32 length)
	{
		UINT32 remain_bytes = length;
		while (remain_bytes > 0)
		{
			UINT32 write_bytes = min(remain_bytes, _write_stream.RemainSizeInBytes());
			_write_stream.Write(buffer + offset, write_bytes);
			remain_bytes -= write_bytes;
			offset += write_bytes;
			if (_write_stream.Full())
			{
				Flush();
			}
		}
	}

	void Flush()
	{
		UINT32* first_four_bytes = (UINT32*)_view_of_write_buff;
		*first_four_bytes = _write_stream.GetPostion();
		//rewind write stream
		_write_stream.Rewind();
		SetEvent(_local_sent_event);
		ATLTRACE("NUFL.Profiler : Flushing data");
		WaitForSingleObject(_remote_received_event, INFINITE);
		ATLTRACE("NUFL.Profiler : Data received.");
	}

private:
	UINT32 unread_bytes = 0;
	HANDLE read_events[2];
public:
	
	UINT32 Read(char buffer[], UINT32 offset, UINT32 length)
	{
		ResetEvent(_waiting_stopped);
		UINT32 actual_read_bytes = 0;
		while (actual_read_bytes < length)
		{
			if (unread_bytes == 0)
			{
				int wait_result = WaitForMultipleObjects(
					2, //number of objects
					read_events, //objects array
					FALSE, //wait all?
					INFINITE //wait time
					);
				switch (wait_result)
				{
				case 0:
				{
					//incoming data
					UINT32* p_total_bytes = (UINT32*)_view_of_read_buff;
					unread_bytes = *p_total_bytes;
					break;
				}
				case 1:
				{
					//we should return;
					SetEvent(_waiting_stopped);
					return actual_read_bytes;
				}
				}
			}

			UINT32 read_bytes = min(unread_bytes, length);
			_read_stream.Read(buffer + offset, read_bytes);
			offset += read_bytes;
			actual_read_bytes += read_bytes;
			unread_bytes -= read_bytes;
			if (unread_bytes == 0)
			{
				//rewind
				_read_stream.Rewind();
				SetEvent(_local_received_event);
			}
		}

		SetEvent(_waiting_stopped);
		return actual_read_bytes;
	}
	void StopWaitingIncoming()
	{
		SetEvent(_stop_waiting_for_incoming);
		WaitForSingleObject(_waiting_stopped, INFINITE);
	}
};

#endif