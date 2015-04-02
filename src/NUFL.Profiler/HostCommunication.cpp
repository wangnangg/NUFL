#include "stdafx.h"
#include "HostCommunication.h"

DWORD WINAPI FlusherThread(PVOID pvParam)
{
	HostCommunication* host = (HostCommunication*)pvParam;
	HANDLE handles[2];
	handles[0] = host->data_flush;
	handles[1] = host->flush_thread_terminate;
	DWORD wait_result = WaitForMultipleObjects(2, handles, false, INFINITE);
	while (wait_result != 1)
	{
		if (wait_result == 0) //flush
		{
			host->FlushCovData();
			SetEvent(host->data_flushed);
		}
		wait_result = WaitForMultipleObjects(2, handles, false, INFINITE);
	}

	return 0;
}