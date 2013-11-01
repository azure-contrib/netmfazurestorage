using System;
using Microsoft.SPOT;

// Required for NETMF to recognized extension methods
namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Attribute required for extension methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class ExtensionAttribute : Attribute { }
}

