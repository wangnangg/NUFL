﻿//
// OpenCover - S. Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System.Runtime.InteropServices;

namespace NUFL.Framework.ProfilerCommunication
{
    public enum MSG_Type : int
    {
        MSG_TrackAssembly = 1,
        MSG_GetSequencePoints = 2,
        MSG_GetBranchPoints = 3,
        MSG_TrackMethod = 4,
    }

    public enum MSG_IdType : uint
    {
        IT_VisitPoint = 0x00000000,
        IT_MethodEnter = 0x40000000,
        IT_MethodLeave = 0x80000000,
        IT_MethodTailcall = 0xC0000000,

        IT_Mask = 0x3FFFFFFF,
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct MSG_TrackAssembly_Request
    {
        public MSG_Type type;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
        public string modulePath;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
        public string assemblyName;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MSG_TrackAssembly_Response
    {
        [MarshalAs(UnmanagedType.Bool)]
        public bool track;
        public int testInvokerCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=10)]
        public int[] invokerTokens;
        public static int TOKENS_SIZE_CONST = 10;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct MSG_GetSequencePoints_Request
    {
        public MSG_Type type;
        public int functionToken;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
        public string modulePath;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
        public string assemblyName;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MSG_SequencePoint
    {
        public uint uniqueId;
        public int offset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MSG_GetSequencePoints_Response
    {
        [MarshalAs(UnmanagedType.Bool)]
        public bool more;

        public int count;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct MSG_GetBranchPoints_Request
    {
        public MSG_Type type;
        public int functionToken;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
        public string modulePath;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
        public string assemblyName;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MSG_BranchPoint
    {
        public uint uniqueId;
        public int offset;
        public int path;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MSG_GetBranchPoints_Response
    {
        [MarshalAs(UnmanagedType.Bool)]
        public bool more;

        public int count;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct MSG_TrackMethod_Request
    {
        public MSG_Type type;
        public int functionToken;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
        public string modulePath;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
        public string assemblyName;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MSG_TrackMethod_Response
    {
        [MarshalAs(UnmanagedType.Bool)]
        public bool track;
        public uint uniqueId;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct MSG_AllocateBuffer_Request
    {
        public MSG_Type type;
        public int bufferSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MSG_AllocateBuffer_Response
    {
        [MarshalAs(UnmanagedType.Bool)]
        public bool allocated;
        public uint bufferId;
    }

}
