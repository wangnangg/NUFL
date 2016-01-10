#include "stdafx.h"
#include "HostCommunication.h"

DWORD WINAPI FlusherThread(PVOID pvParam)
{
	HostCommunication* host = (HostCommunication*)pvParam;
	HANDLE handles[2];
	handles[0] = host->task_finish;
	handles[1] = host->flush_thread_terminate;
	ATLTRACE("flush_thread: wating for flushing command.");
	DWORD wait_result = WaitForMultipleObjects(2, handles, false, INFINITE);
	if (wait_result == 0)
	{
		ATLTRACE("flush_thread: flush command received.");
		host->TaskFinish();
		SetEvent(host->task_finished);
	}

	return 0;
}