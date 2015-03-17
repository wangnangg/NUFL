#ifndef __HOSTCOMMUNICATION_H
#define __HOSTCOMMUNICATION_H
#include "IPCStream.h"
#include "Messages.h"
#include <string>
#include <unordered_map>
enum TrackModuleState
{
	NotKnown = 0,
	Track,
	NoTrack,
	
};
class HostCommunication
{
private:
	IPCStream _msg_stream;
	IPCStream _data_stream;
	HANDLE debugging_event;
	std::vector<bool> hit_account;
	std::unordered_map<std::wstring, TrackModuleState> module_track_cache;

public:
	HostCommunication(){}
	~HostCommunication(){}
	bool Initialize(const std::wstring& msg_buffer_guid, UINT32 msg_buffer_size, 
					const std::wstring& data_buffer_guid, UINT32 data_buffer_size)
	{
		ATLTRACE(_T("HostCommunication Initialization entered.\n"));
		//something terrible could happen.
		debugging_event = CreateEvent(
			NULL, //security
			false,
			false,
			_T("wangnan_debugging_event"));
		return _msg_stream.Initialize(msg_buffer_guid, msg_buffer_size) 
			&& _data_stream.Initialize(data_buffer_guid, data_buffer_size);
	}
	void Finialize()
	{
		ATLTRACE(_T("HostCommunication.Flushing"));
		_data_stream.Flush();
		ATLTRACE(_T("HostCommunication.Flushed"));
	}
public:
	bool TrackAssembly(const std::wstring& module_path, const std::wstring& assembly_name)
	{
		ATLTRACE(_T("HostCommunication.TrackAssembly."));
		ATLTRACE(_T("Module Path:%s"), module_path.c_str());
		ATLTRACE(_T("Assembly Name:%s"), assembly_name.c_str());
		if (module_track_cache[module_path] != NotKnown)
		{
			//we already asked, so return
			return module_track_cache[module_path] == Track ? true : false;
		}
		MSG_TrackAssembly_Request msg;
		msg.type = MSG_TrackAssembly;
		wcscpy_s(msg.szModulePath, module_path.c_str());
		wcscpy_s(msg.szAssemblyName, assembly_name.c_str());
		//we write the sizeof the Union, because that's what the server expects.
		_msg_stream.Write((char*)&msg, 0, sizeof(MSG_TrackAssembly_Request));
		_msg_stream.Flush();
		//get response, the size is known by us.
		MSG_TrackAssembly_Response msg_resp;
		_msg_stream.Read((char*)&msg_resp, 0, sizeof(MSG_TrackAssembly_Response));
		ATLTRACE(_T("Track?:%d"), msg_resp.bResponse);
		module_track_cache[module_path] = msg_resp.bResponse ? Track : NoTrack;
		return msg_resp.bResponse;
	}
	bool GetPoints(mdToken functionToken, WCHAR* pModulePath, WCHAR* pAssemblyName, std::vector<SequencePoint> &seqPoints, std::vector<BranchPoint> &brPoints) 
	{
		ATLTRACE(_T("HostCommunication.GetPoints."));
		MSG_GetSequencePoints_Request msg;
		msg.type = MSG_GetSequencePoints;
		msg.functionToken = functionToken;
		wcscpy_s(msg.szModulePath, pModulePath);
		wcscpy_s(msg.szAssemblyName, pAssemblyName);
		_msg_stream.Write((char*)&msg, 0, sizeof(MSG_GetSequencePoints_Request));
		_msg_stream.Flush();
		//get sequence points
		MSG_GetSequencePoints_Response2 msg_resp;
		_msg_stream.Read((char*)&msg_resp, 0, sizeof(MSG_GetSequencePoints_Response2));
		ATLTRACE(_T("Sequence points count:%d"), msg_resp.count);
		for (int i = 0; i < msg_resp.count; i++)
		{
			SequencePoint sp;
			_msg_stream.Read((char*)&sp, 0, sizeof(SequencePoint));
			seqPoints.push_back(sp);
			ATLTRACE(_T("Sequence point: %d, %d"), sp.UniqueId, sp.Offset);
		}
		return true;
	}
	bool TrackMethod(mdToken functionToken, WCHAR* pModulePath, WCHAR* pAssemblyName, ULONG &uniqueId) 
	{
		ATLTRACE(_T("HostCommunication.TrackMethod."));
		return false;
	}
	inline void AddTestEnterPoint(ULONG uniqueId) { }
	inline void AddTestLeavePoint(ULONG uniqueId) { }
	inline void AddTestTailcallPoint(ULONG uniqueId) { }
	inline void AddVisitPointWithThreshold(ULONG uniqueId, ULONG threshold) 
	{ 
		ATLTRACE(_T("HostCommunication.AddVisitPointWithThreshold."));
		ATLTRACE(_T("hit_account length: %d"), hit_account.size());
		ATLTRACE(_T("uniqueId: %d"), uniqueId);
		if (hit_account[uniqueId])
		{
			return;
		}
		hit_account[uniqueId] = true;
		VisitPoint vp;
		vp.UniqueId = uniqueId | IT_VisitPoint;
		_data_stream.Write((char*)&vp, 0, sizeof(VisitPoint));
	}
	inline void Resize(ULONG minSize) 
	{
		if (minSize > hit_account.size())
		{
			hit_account.resize(minSize);
		}
	}
	
};


#endif
