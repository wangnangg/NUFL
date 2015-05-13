// Guids.cs
// MUST match guids.h
using System;

namespace Buaa.NUFL_VSPackage
{
    static class GuidList
    {
        public const string guidNUFL_VSPackagePkgString = "6657f482-4929-4847-832a-0d32ae71902b";
        public const string guidNUFL_VSPackageCmdSetString = "898b0d76-9b50-40f3-833a-e23aa13b32a7";
        public const string guidToolWindowPersistanceString = "4b3135af-cd3a-48f7-b8d5-e55f11501098";

        public static readonly Guid guidNUFL_VSPackageCmdSet = new Guid(guidNUFL_VSPackageCmdSetString);
    };
}