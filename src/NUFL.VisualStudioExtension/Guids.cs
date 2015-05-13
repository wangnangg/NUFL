// Guids.cs
// MUST match guids.h
using System;

namespace Buaa.NUFL_VisualStudioExtension
{
    static class GuidList
    {
        public const string guidNUFL_VisualStudioExtensionPkgString = "3020f699-cc47-46da-bfba-02c1859c84aa";
        public const string guidNUFL_VisualStudioExtensionCmdSetString = "72ec57b5-56b9-46f4-8023-1fc041aad0ce";
        public const string guidToolWindowPersistanceString = "78bb2a8c-c28f-4ddf-b533-e47b112b37e4";

        public static readonly Guid guidNUFL_VisualStudioExtensionCmdSet = new Guid(guidNUFL_VisualStudioExtensionCmdSetString);
    };
}