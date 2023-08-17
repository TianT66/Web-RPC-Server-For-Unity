using System;

namespace TFramework.Web
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public sealed class WebRPCAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public sealed class WebRPCParamAttribute : Attribute { }
}