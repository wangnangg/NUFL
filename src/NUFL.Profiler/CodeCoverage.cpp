//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
// CodeCoverage.cpp : Implementation of CCodeCoverage

#include "stdafx.h"
#include "CodeCoverage.h"
#include "NativeCallback.h"
#include "dllmain.h"

#define CUCKOO_SAFE_METHOD_NAME L"SafeVisited"
#define CUCKOO_CRITICAL_METHOD_NAME L"VisitedCritical"
#define CUCKOO_NEST_TYPE_NAME L"System.CannotUnloadAppDomainException"
#define MSCORLIB_NAME L"mscorlib"

CCodeCoverage* CCodeCoverage::g_pProfiler = NULL;
// CCodeCoverage

/// <summary>Handle <c>ICorProfilerCallback::Initialize</c></summary>
/// <remarks>Initialize the profiling environment and establish connection to the host</remarks>
HRESULT STDMETHODCALLTYPE CCodeCoverage::Initialize( 
    /* [in] */ IUnknown *pICorProfilerInfoUnk) 
{
    ATLTRACE(_T("::Initialize"));
	ATLTRACE(_T("NUFLTrace: Initialize"));
    
    OLECHAR szGuid[40]={0};
    int nCount = ::StringFromGUID2(CLSID_CodeCoverage, szGuid, 40);
    RELTRACE(L"    ::Initialize(...) => CLSID == %s", szGuid);
    //::OutputDebugStringW(szGuid);

    WCHAR szExeName[MAX_PATH];
    GetModuleFileNameW(NULL, szExeName, MAX_PATH);
    RELTRACE(L"    ::Initialize(...) => EXE = %s", szExeName);

    WCHAR szModuleName[MAX_PATH];
    GetModuleFileNameW(_AtlModule.m_hModule, szModuleName, MAX_PATH);
    RELTRACE(L"    ::Initialize(...) => PROFILER = %s", szModuleName);
    //::OutputDebugStringW(szModuleName);

    if (g_pProfiler!=NULL) 
        RELTRACE(_T("Another instance of the profiler is running under this process..."));

    m_profilerInfo = pICorProfilerInfoUnk;
    if (m_profilerInfo != NULL) ATLTRACE(_T("    ::Initialize (m_profilerInfo OK)"));
    if (m_profilerInfo == NULL) return E_FAIL;
    m_profilerInfo2 = pICorProfilerInfoUnk;
    if (m_profilerInfo2 != NULL) ATLTRACE(_T("    ::Initialize (m_profilerInfo2 OK)"));
    if (m_profilerInfo2 == NULL) return E_FAIL;
    m_profilerInfo3 = pICorProfilerInfoUnk;
#ifndef _TOOLSETV71
    m_profilerInfo4 = pICorProfilerInfoUnk;
#endif

    ZeroMemory(&m_runtimeVersion, sizeof(m_runtimeVersion));
    if (m_profilerInfo3 != NULL) 
    {
        ATLTRACE(_T("    ::Initialize (m_profilerInfo3 OK)"));
        
        ZeroMemory(&m_runtimeVersion, sizeof(m_runtimeVersion));
        m_profilerInfo3->GetRuntimeInformation(NULL, &m_runtimeType, 
            &m_runtimeVersion.usMajorVersion, 
            &m_runtimeVersion.usMinorVersion, 
            &m_runtimeVersion.usBuildNumber, 
            &m_runtimeVersion.usRevisionNumber, 0, NULL, NULL); 

        ATLTRACE(_T("    ::Initialize (Runtime %d)"), m_runtimeType);
    }

    TCHAR key[1024] = {0};
    ::GetEnvironmentVariable(_T("OpenCover_Profiler_Key"), key, 1024);
    RELTRACE(_T("    ::Initialize(...) => key = %s"), key);

    TCHAR ns[1024] = {0};
    ::GetEnvironmentVariable(_T("OpenCover_Profiler_Namespace"), ns, 1024);
    ATLTRACE(_T("    ::Initialize(...) => ns = %s"), ns);

    TCHAR instrumentation[1024] = {0};
    ::GetEnvironmentVariable(_T("OpenCover_Profiler_Instrumentation"), instrumentation, 1024);
    ATLTRACE(_T("    ::Initialize(...) => instrumentation = %s"), instrumentation);

    TCHAR threshold[1024] = {0};
    ::GetEnvironmentVariable(_T("OpenCover_Profiler_Threshold"), threshold, 1024);
    m_threshold = _tcstoul(threshold, NULL, 10);
    ATLTRACE(_T("    ::Initialize(...) => threshold = %ul"), m_threshold);

    TCHAR tracebyTest[1024] = {0};
    ::GetEnvironmentVariable(_T("OpenCover_Profiler_TraceByTest"), tracebyTest, 1024);
    bool tracingEnabled = _tcslen(tracebyTest) != 0;
    ATLTRACE(_T("    ::Initialize(...) => tracingEnabled = %s (%s)"), tracingEnabled ? _T("true") : _T("false"), tracebyTest);

	TCHAR msg_buffer_guid[1024] = { 0 };
	::GetEnvironmentVariable(_T("OpenCover_Msg_Buffer_Guid"), msg_buffer_guid, 1024);

	TCHAR msg_buffer_size_array[1024] = {0};
	::GetEnvironmentVariable(_T("OpenCover_Msg_Buffer_Size"), msg_buffer_size_array, 1024);

	TCHAR data_buffer_guid[1024] = { 0 };
	::GetEnvironmentVariable(_T("OpenCover_Data_Buffer_Guid"), data_buffer_guid, 1024);

	TCHAR data_buffer_size_array[1024] = { 0 };
	::GetEnvironmentVariable(_T("OpenCover_Data_Buffer_Size"), data_buffer_size_array, 1024);


    m_useOldStyle = (tstring(instrumentation) == _T("oldSchool"));
	UINT32 msg_buffer_size = (UINT32)_tcstoul(msg_buffer_size_array, NULL, 10);
	UINT32 data_buffer_size = (UINT32)_tcstoul(data_buffer_size_array, NULL, 10);
	if (!m_host.Initialize(msg_buffer_guid, msg_buffer_size, data_buffer_guid, data_buffer_size))
    {
        RELTRACE(_T("    ::Initialize => Failed to initialise the profiler communications -> GetLastError() => %d"), ::GetLastError());
        return E_FAIL;
    }

    DWORD dwMask = 0;
    dwMask |= COR_PRF_MONITOR_MODULE_LOADS;			// Controls the ModuleLoad, ModuleUnload, and ModuleAttachedToAssembly callbacks.
    dwMask |= COR_PRF_MONITOR_JIT_COMPILATION;	    // Controls the JITCompilation, JITFunctionPitched, and JITInlining callbacks.
    dwMask |= COR_PRF_DISABLE_INLINING;				// Disables all inlining.
    dwMask |= COR_PRF_DISABLE_OPTIMIZATIONS;		// Disables all code optimizations.
    dwMask |= COR_PRF_USE_PROFILE_IMAGES;           // Causes the native image search to look for profiler-enhanced images
    
    if (tracingEnabled)
        dwMask |= COR_PRF_MONITOR_ENTERLEAVE;       // Controls the FunctionEnter, FunctionLeave, and FunctionTailcall callbacks.

    if (m_useOldStyle)
       dwMask |= COR_PRF_DISABLE_TRANSPARENCY_CHECKS_UNDER_FULL_TRUST;      // Disables security transparency checks that are normally done during just-in-time (JIT) compilation and class loading for full-trust assemblies. This can make some instrumentation easier to perform.

#ifndef _TOOLSETV71
    if (m_profilerInfo4 != NULL)
    {
        ATLTRACE(_T("    ::Initialize (m_profilerInfo4 OK)"));
        dwMask |= COR_PRF_DISABLE_ALL_NGEN_IMAGES;
    }
#endif
    COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo2->SetEventMask(dwMask), 
        _T("    ::Initialize(...) => SetEventMask => 0x%X"));

    if(m_profilerInfo3 != NULL)
    {
        COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo3->SetFunctionIDMapper2(FunctionMapper2, this), 
            _T("    ::Initialize(...) => SetFunctionIDMapper2 => 0x%X"));
    }
    else
    {
        COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo2->SetFunctionIDMapper(FunctionMapper), 
            _T("    ::Initialize(...) => SetFunctionIDMapper => 0x%X"));
    }

    g_pProfiler = this;

#ifndef _TOOLSETV71
    COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo2->SetEnterLeaveFunctionHooks2(
        _FunctionEnter2, _FunctionLeave2, _FunctionTailcall2), 
        _T("    ::Initialize(...) => SetEnterLeaveFunctionHooks2 => 0x%X"));
#endif
    RELTRACE(_T("::Initialize - Done!"));
    
    return S_OK; 
}

/// <summary>Handle <c>ICorProfilerCallback::Shutdown</c></summary>
HRESULT STDMETHODCALLTYPE CCodeCoverage::Shutdown( void) 
{
	ATLTRACE(_T("NUFLTrace: Shutdown"));
	m_host.Finialize();
    WCHAR szExeName[MAX_PATH];
    GetModuleFileNameW(NULL, szExeName, MAX_PATH);
    RELTRACE(_T("::Shutdown - Nothing left to do but return S_OK(%s)"), szExeName);
    g_pProfiler = NULL;
    return S_OK; 
}

/// <summary>An unmanaged callback that can be called from .NET that has a single I4 parameter</summary>
/// <remarks>
/// void (__fastcall *pt)(long) = &amp;SequencePointVisit ;
/// mdSignature pmsig = GetUnmanagedMethodSignatureToken_I4(moduleId);
/// </remarks>
static void __fastcall InstrumentPointVisit(ULONG seq)
{
    CCodeCoverage::g_pProfiler->AddVisitPoint(seq);
}

void CCodeCoverage::AddVisitPoint(ULONG uniqueId)
{
    m_host.AddVisitPointWithThreshold(uniqueId, m_threshold);
}

static COR_SIGNATURE visitedMethodCallSignature[] = 
{
    IMAGE_CEE_CS_CALLCONV_DEFAULT,   
    0x01,                                   
    ELEMENT_TYPE_VOID,
    ELEMENT_TYPE_I4
};

static COR_SIGNATURE ctorCallSignature[] = 
{
    IMAGE_CEE_CS_CALLCONV_DEFAULT | IMAGE_CEE_CS_CALLCONV_HASTHIS,   
    0x00,                                   
    ELEMENT_TYPE_VOID
};

HRESULT STDMETHODCALLTYPE CCodeCoverage::ModuleLoadFinished( 
        /* [in] */ ModuleID moduleId,
        /* [in] */ HRESULT hrStatus) 
{
	ATLTRACE(_T("NUFLTrace: ModuleLoadFinished"));
    CComPtr<IMetaDataEmit> metaDataEmit;
    COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo->GetModuleMetaData(moduleId, 
        ofRead | ofWrite, IID_IMetaDataEmit, (IUnknown**)&metaDataEmit), 
        _T("    ::ModuleLoadFinished(...) => GetModuleMetaData => 0x%X"));
    if (metaDataEmit==NULL) return S_OK;

    CComPtr<IMetaDataImport> metaDataImport;
    COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo->GetModuleMetaData(moduleId, 
        ofRead | ofWrite, IID_IMetaDataImport, (IUnknown**)&metaDataImport), 
        _T("    ::ModuleLoadFinished(...) => GetModuleMetaData => 0x%X"));
    if (metaDataImport==NULL) return S_OK;

    mdTypeDef systemObject = mdTokenNil;
    if (S_OK == metaDataImport->FindTypeDefByName(L"System.Object", mdTokenNil, &systemObject))
    {
        RELTRACE(_T("::ModuleLoadFinished(...) => Adding methods to mscorlib..."));
        mdMethodDef systemObjectCtor;
        COM_FAIL_MSG_RETURN_ERROR(metaDataImport->FindMethod(systemObject, L".ctor", 
            ctorCallSignature, sizeof(ctorCallSignature), &systemObjectCtor), 
            _T("    ::ModuleLoadFinished(...) => FindMethod => 0x%X)"));

        ULONG ulCodeRVA = 0;
        COM_FAIL_MSG_RETURN_ERROR(metaDataImport->GetMethodProps(systemObjectCtor, NULL, NULL, 
            0, NULL, NULL, NULL, NULL, &ulCodeRVA, NULL), 
            _T("    ::ModuleLoadFinished(...) => GetMethodProps => 0x%X"));

        mdCustomAttribute customAttr;
        mdToken attributeCtor;
        mdTypeDef attributeTypeDef;
        mdTypeDef nestToken;

        COM_FAIL_MSG_RETURN_ERROR(metaDataImport->FindTypeDefByName(CUCKOO_NEST_TYPE_NAME, mdTokenNil, &nestToken), 
            _T("    ::ModuleLoadFinished(...) => FindTypeDefByName => 0x%X"));

        // create a method that we will mark up with the SecurityCriticalAttribute
        COM_FAIL_MSG_RETURN_ERROR(metaDataEmit->DefineMethod(nestToken, CUCKOO_CRITICAL_METHOD_NAME,
            mdPublic | mdStatic | mdHideBySig, visitedMethodCallSignature, sizeof(visitedMethodCallSignature), 
            ulCodeRVA, miIL | miManaged | miPreserveSig | miNoInlining, &m_cuckooCriticalToken), 
            _T("    ::ModuleLoadFinished(...) => DefineMethod => 0x%X"));

        COM_FAIL_MSG_RETURN_ERROR(metaDataImport->FindTypeDefByName(L"System.Security.SecurityCriticalAttribute",
            NULL, &attributeTypeDef), _T("    :ModuleLoadFinished(...) => FindTypeDefByName => 0x%X")); 

        if (m_runtimeType == COR_PRF_DESKTOP_CLR)
        {
            // for desktop we use the .ctor that takes a SecurityCriticalScope argument as the 
            // default (no arguments) constructor fails with "0x801311C2 - known custom attribute value is bad" 
            // when we try to attach it in .NET2 - .NET4 version doesn't care which one we use
            mdTypeDef scopeToken;
            COM_FAIL_MSG_RETURN_ERROR(metaDataImport->FindTypeDefByName(L"System.Security.SecurityCriticalScope", mdTokenNil, &scopeToken), 
                _T("    ::ModuleLoadFinished(...) => FindTypeDefByName => 0x%X"));

            ULONG sigLength=4;
            COR_SIGNATURE ctorCallSignatureEnum[] = 
            {
                IMAGE_CEE_CS_CALLCONV_DEFAULT | IMAGE_CEE_CS_CALLCONV_HASTHIS,   
                0x01,                                   
                ELEMENT_TYPE_VOID,
                ELEMENT_TYPE_VALUETYPE,
                0x00,0x00, 0x00, 0x00 // make room for our compressed token - should always be 2 but...
            };

            sigLength += CorSigCompressToken(scopeToken, &ctorCallSignatureEnum[4]);

            COM_FAIL_MSG_RETURN_ERROR(metaDataImport->FindMember(attributeTypeDef, 
                L".ctor", ctorCallSignatureEnum, sigLength, &attributeCtor), 
                _T("    ::ModuleLoadFinished(...) => FindMember => 0x%X"));
        
            unsigned char blob[] = {0x01, 0x00, 0x01, 0x00, 0x00, 0x00}; // prolog U2 plus an enum of I4 (little-endian)
            COM_FAIL_MSG_RETURN_ERROR(metaDataEmit->DefineCustomAttribute(m_cuckooCriticalToken, attributeCtor, blob, sizeof(blob), &customAttr), 
                _T("    ::ModuleLoadFinished(...) => DefineCustomAttribute => 0x%X"));
        }
        else
        {
            // silverlight only has one .ctor for this type
            COM_FAIL_MSG_RETURN_ERROR(metaDataImport->FindMember(attributeTypeDef, 
                L".ctor", ctorCallSignature, sizeof(ctorCallSignature), &attributeCtor), 
                _T("    ::ModuleLoadFinished(...) => FindMember => 0x%X"));
        
            COM_FAIL_MSG_RETURN_ERROR(metaDataEmit->DefineCustomAttribute(m_cuckooCriticalToken, attributeCtor, NULL, 0, &customAttr), 
                _T("    ::ModuleLoadFinished(...) => DefineCustomAttribute => 0x%X"));
        }

        // create a method that we will mark up with the SecuritySafeCriticalAttribute
        COM_FAIL_MSG_RETURN_ERROR(metaDataEmit->DefineMethod(nestToken, CUCKOO_SAFE_METHOD_NAME,
            mdPublic | mdStatic | mdHideBySig, visitedMethodCallSignature, sizeof(visitedMethodCallSignature), 
            ulCodeRVA, miIL | miManaged | miPreserveSig | miNoInlining, &m_cuckooSafeToken), 
            _T("    ::ModuleLoadFinished(...) => DefineMethod => 0x%X"));

        COM_FAIL_MSG_RETURN_ERROR(metaDataImport->FindTypeDefByName(L"System.Security.SecuritySafeCriticalAttribute",
            NULL, &attributeTypeDef), 
            _T("    ::ModuleLoadFinished(...) => FindTypeDefByName => 0x%X")); 

        COM_FAIL_MSG_RETURN_ERROR(metaDataImport->FindMember(attributeTypeDef, 
            L".ctor", ctorCallSignature, sizeof(ctorCallSignature), &attributeCtor), 
            _T("    ::ModuleLoadFinished(...) => FindMember => 0x%X"));

        COM_FAIL_MSG_RETURN_ERROR(metaDataEmit->DefineCustomAttribute(m_cuckooSafeToken, attributeCtor, NULL, 0, &customAttr), 
            _T("    ::ModuleLoadFinished(...) => DefineCustomAttribute => 0x%X"));
        
        RELTRACE(_T("::ModuleLoadFinished(...) => Added methods to mscorlib"));
    }

    return S_OK;
}

/// <summary>Handle <c>ICorProfilerCallback::ModuleAttachedToAssembly</c></summary>
/// <remarks>Inform the host that we have a new module attached and that it may be 
/// of interest</remarks>
HRESULT STDMETHODCALLTYPE CCodeCoverage::ModuleAttachedToAssembly( 
    /* [in] */ ModuleID moduleId,
    /* [in] */ AssemblyID assemblyId)
{
	ATLTRACE(_T("NUFLTrace: ModuleAttachedToAssembly"));
    std::wstring modulePath = GetModulePath(moduleId);
    std::wstring assemblyName = GetAssemblyName(assemblyId);
    ATLTRACE(_T("::ModuleAttachedToAssembly(...) => (%X => %s, %X => %s)"), 
        moduleId, W2CT(modulePath.c_str()), 
        assemblyId, W2CT(assemblyName.c_str()));
  //  m_allowModules[modulePath] = m_host.TrackAssembly((LPWSTR)modulePath.c_str(), (LPWSTR)assemblyName.c_str());
    m_allowModulesAssemblyMap[modulePath] = assemblyName;

    return S_OK; 
}

mdMemberRef CCodeCoverage::RegisterSafeCuckooMethod(ModuleID moduleId)
{
	ATLTRACE(_T("NUFLTrace: RegisterSafeCuckooMethod"));
    ATLTRACE(_T("::RegisterSafeCuckooMethod(%X) => %s"), moduleId, CUCKOO_SAFE_METHOD_NAME);

    // for modules we are going to instrument add our reference to the method marked 
    // with the SecuritySafeCriticalAttribute
    CComPtr<IMetaDataEmit> metaDataEmit;
    COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo->GetModuleMetaData(moduleId, 
        ofRead | ofWrite, IID_IMetaDataEmit, (IUnknown**)&metaDataEmit), 
        _T("    ::RegisterSafeCuckooMethod(...) => GetModuleMetaData => 0x%X"));

    mdModuleRef mscorlibRef;
    COM_FAIL_MSG_RETURN_ERROR(GetModuleRef(moduleId, MSCORLIB_NAME, mscorlibRef), 
        _T("    ::RegisterSafeCuckooMethod(...) => GetModuleRef => 0x%X")); 

    mdTypeDef nestToken;
    COM_FAIL_MSG_RETURN_ERROR(metaDataEmit->DefineTypeRefByName(mscorlibRef, CUCKOO_NEST_TYPE_NAME, &nestToken), 
        _T("    ::RegisterSafeCuckooMethod(...) => DefineTypeRefByName => 0x%X"));

    mdMemberRef cuckooSafeToken;
    COM_FAIL_MSG_RETURN_ERROR(metaDataEmit->DefineMemberRef(nestToken, CUCKOO_SAFE_METHOD_NAME,  
        visitedMethodCallSignature, sizeof(visitedMethodCallSignature), &cuckooSafeToken) , 
        _T("    ::RegisterSafeCuckooMethod(...) => DefineMemberRef => 0x%X"));

    return cuckooSafeToken;
}


/// <summary>This is the method marked with the SecurityCriticalAttribute</summary>
/// <remarks>This method makes the call into the profiler</remarks>
HRESULT CCodeCoverage::AddCriticalCuckooBody(ModuleID moduleId)
{
	ATLTRACE(_T("NUFLTrace: AddCriticalCuckooBody"));
    ATLTRACE(_T("::AddCriticalCuckooBody => Adding VisitedCritical..."));

    // our profiler hook
    mdSignature pvsig = GetMethodSignatureToken_I4(moduleId);
    void (__fastcall *pt)(ULONG) = &InstrumentPointVisit ;

    BYTE data[] = {(0x01 << 2) | CorILMethod_TinyFormat, CEE_RET};
    Method criticalMethod((IMAGE_COR_ILMETHOD*)data);
    InstructionList instructions;
    instructions.push_back(new Instruction(CEE_LDARG_0));
#if _WIN64
    instructions.push_back(new Instruction(CEE_LDC_I8, (ULONGLONG)pt));
#else
    instructions.push_back(new Instruction(CEE_LDC_I4, (ULONG)pt));
#endif
    instructions.push_back(new Instruction(CEE_CALLI, pvsig));

    criticalMethod.InsertInstructionsAtOffset(0, instructions);
    criticalMethod.DumpIL();

    CComPtr<IMethodMalloc> methodMalloc;
    COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo->GetILFunctionBodyAllocator(moduleId, &methodMalloc), 
        _T("    ::AddCriticalCuckooBody(...) => GetILFunctionBodyAllocator => 0x%X"));

    void* pMethodBody = methodMalloc->Alloc(criticalMethod.GetMethodSize());
    criticalMethod.WriteMethod((IMAGE_COR_ILMETHOD*)pMethodBody);

    COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo->SetILFunctionBody(moduleId, 
        m_cuckooCriticalToken, (LPCBYTE)pMethodBody), 
        _T("    ::AddCriticalCuckooBody(...) => SetILFunctionBody => 0x%X"));

    ATLTRACE(_T("::AddCriticalCuckooBody => Adding VisitedCritical - Done!"));

    return S_OK;
}

/// <summary>This is the body of our method marked with the SecuritySafeCriticalAttribute</summary>
/// <remarks>Calls the method that is marked with the SecurityCriticalAttribute</remarks>
HRESULT CCodeCoverage::AddSafeCuckooBody(ModuleID moduleId)
{
	ATLTRACE(_T("NUFLTrace: AddSafeCuckooBody"));
    ATLTRACE(_T("::AddSafeCuckooBody => Adding SafeVisited..."));

    BYTE data[] = {(0x01 << 2) | CorILMethod_TinyFormat, CEE_RET};
    Method criticalMethod((IMAGE_COR_ILMETHOD*)data);
    InstructionList instructions;
    instructions.push_back(new Instruction(CEE_LDARG_0));
    instructions.push_back(new Instruction(CEE_CALL, m_cuckooCriticalToken));

    criticalMethod.InsertInstructionsAtOffset(0, instructions);
    criticalMethod.DumpIL();

    CComPtr<IMethodMalloc> methodMalloc;
    COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo->GetILFunctionBodyAllocator(moduleId, &methodMalloc), 
        _T("    ::AddSafeCuckooBody(...) => GetILFunctionBodyAllocator => 0x%X"));

    void* pMethodBody = methodMalloc->Alloc(criticalMethod.GetMethodSize());
    criticalMethod.WriteMethod((IMAGE_COR_ILMETHOD*)pMethodBody);

    COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo->SetILFunctionBody(moduleId, 
        m_cuckooSafeToken, (LPCBYTE)pMethodBody), 
        _T("    ::AddSafeCuckooBody(...) => SetILFunctionBody => 0x%X"));

    ATLTRACE(_T("::AddSafeCuckooBody => Adding SafeVisited - Done!"));

    return S_OK;
}

/// <summary>Handle <c>ICorProfilerCallback::JITCompilationStarted</c></summary>
/// <remarks>The 'workhorse' </remarks>
HRESULT STDMETHODCALLTYPE CCodeCoverage::JITCompilationStarted( 
        /* [in] */ FunctionID functionId,
        /* [in] */ BOOL fIsSafeToBlock)
{
	ATLTRACE(_T("NUFLTrace: JITCompilationStarted"));
    std::wstring modulePath;
    mdToken functionToken;
    ModuleID moduleId;
    AssemblyID assemblyId;
    if (GetTokenAndModule(functionId, functionToken, moduleId, modulePath, &assemblyId))
    {
        // add the bodies for our cuckoo methods when required
        if (MSCORLIB_NAME == GetAssemblyName(assemblyId))
        {
            if (m_cuckooCriticalToken == functionToken)
            {
                COM_FAIL_MSG_RETURN_ERROR(AddCriticalCuckooBody(moduleId), 
                    _T("    ::JITCompilationStarted(...) => AddCriticalCuckooBody => 0x%X"));
            }

            if (m_cuckooSafeToken == functionToken)
            {
                COM_FAIL_MSG_RETURN_ERROR(AddSafeCuckooBody(moduleId), 
                    _T("    ::JITCompilationStarted(...) => AddSafeCuckooBody => 0x%X"));
            }
        }

		if (!m_host.TrackAssembly(modulePath, m_allowModulesAssemblyMap[modulePath]))
		{
			ATLTRACE(_T("::JITCompilationStarted Exit for not allowed. %s"), modulePath.c_str());
			return S_OK;
		}

        ATLTRACE(_T("::JITCompilationStarted(%X, ...) => %d, %X => %s"), functionId, functionToken, moduleId, W2CT(modulePath.c_str()));

        std::vector<SequencePoint> seqPoints;
        std::vector<BranchPoint> brPoints;
        
        if (m_host.GetPoints(functionToken, (LPWSTR)modulePath.c_str(), 
            (LPWSTR)m_allowModulesAssemblyMap[modulePath].c_str(), seqPoints, brPoints))
        {
            if (seqPoints.size()==0) return S_OK;

            LPCBYTE pMethodHeader = NULL;
            ULONG iMethodSize = 0;
            COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo2->GetILFunctionBody(moduleId, functionToken, &pMethodHeader, &iMethodSize),
                _T("    ::JITCompilationStarted(...) => GetILFunctionBody => 0x%X"));

            IMAGE_COR_ILMETHOD* pMethod = (IMAGE_COR_ILMETHOD*)pMethodHeader;
            
            Method instumentedMethod(pMethod);
            instumentedMethod.IncrementStackSize(2);

            ATLTRACE(_T("::JITCompilationStarted(...) => Instrumenting..."));
            //seqPoints.clear();
            //brPoints.clear();

            // Instrument method
            InstrumentMethod(moduleId, instumentedMethod, seqPoints, brPoints);

            instumentedMethod.DumpIL();

            CComPtr<IMethodMalloc> methodMalloc;
            COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo2->GetILFunctionBodyAllocator(moduleId, &methodMalloc),
                _T("    ::JITCompilationStarted(...) => GetILFunctionBodyAllocator=> 0x%X"));
            IMAGE_COR_ILMETHOD* pNewMethod = (IMAGE_COR_ILMETHOD*)methodMalloc->Alloc(instumentedMethod.GetMethodSize());
            instumentedMethod.WriteMethod(pNewMethod);
            COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo2->SetILFunctionBody(moduleId, functionToken, (LPCBYTE) pNewMethod), 
                _T("    ::JITCompilationStarted(...) => SetILFunctionBody => 0x%X"));

            ULONG mapSize = instumentedMethod.GetILMapSize();
            COR_IL_MAP * pMap = (COR_IL_MAP *)CoTaskMemAlloc(mapSize * sizeof(COR_IL_MAP));
            instumentedMethod.PopulateILMap(mapSize, pMap);
            COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo2->SetILInstrumentedCodeMap(functionId, TRUE, mapSize, pMap), 
                _T("    ::JITCompilationStarted(...) => SetILInstrumentedCodeMap => 0x%X"));

            // only do this for .NET4 and above as there are issues with earlier runtimes (Access Violations)
            if (m_runtimeVersion.usMajorVersion >= 4)
                CoTaskMemFree(pMap);

            // resize the threshold array 
            if (m_threshold != 0)
            {
                if (seqPoints.size() > 0) 
                    m_host.Resize(seqPoints.back().UniqueId + 1);
                if (brPoints.size() > 0) 
                    m_host.Resize(brPoints.back().UniqueId + 1);
            }
        }
    }
    
    return S_OK; 
}

void CCodeCoverage::InstrumentMethod(ModuleID moduleId, Method& method,  std::vector<SequencePoint> seqPoints, std::vector<BranchPoint> brPoints)
{
	ATLTRACE(_T("NUFLTrace: InstrumentMethod"));
    if (m_useOldStyle)
    {
		ATLTRACE("Using old style");
        mdSignature pvsig = GetMethodSignatureToken_I4(moduleId);
        void (__fastcall *pt)(ULONG) = &InstrumentPointVisit;

        InstructionList instructions;
        CoverageInstrumentation::InsertFunctionCall(instructions, pvsig, (FPTR)pt, seqPoints[0].UniqueId);
        if (method.IsInstrumented(0, instructions)) return;
  
        CoverageInstrumentation::AddBranchCoverage([pvsig, pt](InstructionList& instructions, ULONG uniqueId)->Instruction*
        {
            return CoverageInstrumentation::InsertFunctionCall(instructions, pvsig, (FPTR)pt, uniqueId);
        }, method, brPoints, seqPoints);

        CoverageInstrumentation::AddSequenceCoverage([pvsig, pt](InstructionList& instructions, ULONG uniqueId)->Instruction*
        {
            return CoverageInstrumentation::InsertFunctionCall(instructions, pvsig, (FPTR)pt, uniqueId);
        }, method, seqPoints);
    }
    else
    {
		ATLTRACE("Using new style.");
        mdMethodDef injectedVisitedMethod = RegisterSafeCuckooMethod(moduleId);

        InstructionList instructions;
        CoverageInstrumentation::InsertInjectedMethod(instructions, injectedVisitedMethod, seqPoints[0].UniqueId);
        if (method.IsInstrumented(0, instructions)) return;
  
        CoverageInstrumentation::AddBranchCoverage([injectedVisitedMethod](InstructionList& instructions, ULONG uniqueId)->Instruction*
        {
            return CoverageInstrumentation::InsertInjectedMethod(instructions, injectedVisitedMethod, uniqueId);
        }, method, brPoints, seqPoints);
        
        CoverageInstrumentation::AddSequenceCoverage([injectedVisitedMethod](InstructionList& instructions, ULONG uniqueId)->Instruction*
        {
            return CoverageInstrumentation::InsertInjectedMethod(instructions, injectedVisitedMethod, uniqueId);
        }, method, seqPoints);
    }
}
