#include "stdafx.h"
#include "HostCommunication.h"

DWORD WINAPI FlusherThread(PVOID pvParam)
{
	HostCommunication* host = (HostCommunication*)pvParam;
	HANDLE handles[2];
	handles[0] = host->data_flush;
	handles[1] = host->flush_thread_terminate;
	ATLTRACE("flush_thread: wating for flushing command.");
	DWORD wait_result = WaitForMultipleObjects(2, handles, false, INFINITE);
	while (wait_result == 0)
	{
		ATLTRACE("flush_thread: flush command received.");
		host->FlushCovData();
		SetEvent(host->data_flushed);
		wait_result = WaitForMultipleObjects(2, handles, false, INFINITE);
	}

	return 0;
}