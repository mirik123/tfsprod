// Guids.cs
// MUST match guids.h
using System;

namespace tfsprod
{
    static class GuidList
    {
        public const string guidtfsprodPkgString = "16bf0a1d-6239-485f-ac25-6fcd37d514c7";
        public const string guidtfsprodCmdSetString = "45982852-bea4-4f30-951e-8d426535ee39";

        public static readonly Guid guidtfsprodCmdSet = new Guid(guidtfsprodCmdSetString);
    };
}