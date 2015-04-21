#ifndef __HOSTCOMMUNICATION_H
#define __HOSTCOMMUNICATION_H
#include "IPCStream.h"
#include "Messages.h"
#include <string>
#include <unordered_map>
#include <utility>
#include <set>

enum TrackModuleState
{
	NotKnown = 0,
	Track,
	NoTrack,
	
};
DWORD WINAPI FlusherThread(PVOID pvParam);
class HostCommunication
{
private:
	IPCStream _msg_stream;
	IPCStream _data_stream;
	std::vector<bool> hit_account;
	std::unordered_map<std::wstring, TrackModuleState> module_track_cache;
	std::set<std::pair<std::wstring, int>> test_method_invokers;
	
public:
	HANDLE data_flush;
	HANDLE data_flushed;
	HANDLE flush_thread_terminate;
	HANDLE flush_thread;

public:
	HostCommunication(){}
	~HostCommunication(){}
	bool Initialize(const std::wstring& msg_buffer_guid, UINT32 msg_buffer_size, 
					const std::wstring& data_buffer_guid, UINT32 data_buffer_size)
	{
		ATLTRACE(_T("HostCommunication Initialization entered.\n"));
		data_flush = CreateEvent(
			NULL,
			false,
			false,
			(_T("DataFlush#") + data_buffer_guid).c_str());
		data_flushed = CreateEvent(
			NULL,
			false,
			false,
			(_T("DataFlushed#") + data_buffer_guid).c_str());
		flush_thread_terminate = CreateEvent(
			NULL,
			true,
			false,
			NULL);
		flush_thread = CreateThread(NULL, 0, FlusherThread, (PVOID)this,
			0, NULL);


		return _msg_stream.Initialize(msg_buffer_guid, msg_buffer_size) 
			&& _data_stream.Initialize(data_buffer_guid, data_buffer_size);
	}
	void Finialize()
	{
		ATLTRACE(_T("HostCommunication.Finalize"));
		_data_stream.Flush();
		SetEvent(flush_thread_terminate);
		WaitForSingleObject(flush_thread, INFINITE);
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
		ATLTRACE(_T("Response Size:%d"), sizeof(MSG_TrackAssembly_Response));
		_msg_stream.Read((char*)&msg_resp, 0, sizeof(MSG_TrackAssembly_Response));
		ATLTRACE(_T("Track?:%d"), msg_resp.bResponse);
		module_track_cache[module_path] = msg_resp.bResponse ? Track : NoTrack;
		ATLTRACE(_T("Invoker Count:%d"), msg_resp.iTestInvokerCount);
		for (int i = 0; i < msg_resp.iTestInvokerCount; i++)
		{
			
			auto function = std::make_pair(module_path, msg_resp.invokerTokens[i]);
			test_method_invokers.insert(function);
			ATLTRACE(_T("Invoker Token:%d"), function.second);
		}
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
	bool TrackMethod(mdToken functionToken, std::wstring module_path, ULONG &uniqueId) 
	{
		ATLTRACE(_T("HostCommunication.TrackMethod."));
		uniqueId = 0;
		if (module_path.find(_T("nunit.framework")))
		{
		    ATLTRACE(_T("TryingTrackMethod in %s."), module_path.c_str());
		}
		if (test_method_invokers.find(std::make_pair(module_path, functionToken))
			!= test_method_invokers.end())
		{
			//This is invoker method
		    ATLTRACE(_T("TrackMethodin %s."), module_path.c_str());
			return true;
		}
		else
		{
			//This isn't invoker method
			return false;
		}
	}
	inline void AddTestEnterPoint(ULONG uniqueId) 
	{ 
		VisitPoint vp;
		vp.UniqueId = uniqueId | IT_MethodEnter;
		_data_stream.Write((char*)&vp, 0, sizeof(VisitPoint));
		ClearHit();
	}
	inline void ClearHit()
	{
		for (int i = 0; i < hit_account.size(); i++)
		{
			hit_account[i] = false;
		}
	}
	inline void FillHit()
	{
		for (int i = 0; i < hit_account.size(); i++)
		{
			hit_account[i] = true;
		}
	}
	inline void AddTestLeavePoint(ULONG uniqueId) 
	{
		VisitPoint vp;
		vp.UniqueId = uniqueId | IT_MethodLeave;
		_data_stream.Write((char*)&vp, 0, sizeof(VisitPoint));
		FillHit();
	}
	inline void AddTestTailcallPoint(ULONG uniqueId) 
	{
		VisitPoint vp;
		vp.UniqueId = uniqueId | IT_MethodTailcall;
		_data_stream.Write((char*)&vp, 0, sizeof(VisitPoint));
		FillHit();
	}
	inline void AddVisitPointWithThreshold(ULONG uniqueId, ULONG threshold) 
	{ 
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
	void FlushCovData()
	{
		_data_stream.Flush();
	}
};


#endif
