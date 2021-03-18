using System;
using System.Collections.Generic;
using System.Text;

namespace Yoakke.Dependency.Generator
{
    internal static class TypeNames
    {
        private static readonly string NamespaceBase           = "Yoakke.Dependency";
        private static readonly string InternalNamespace       = $"{NamespaceBase}.Internal";

        public static readonly string SystemCancellationToken  = "System.Threading.CancellationToken";
        public static readonly string SystemEventHandler       = "System.EventHandler`1";

        public static readonly string QueryGroupAttribute      = $"{NamespaceBase}.QueryGroupAttribute";
        public static readonly string InputQueryGroupAttribute = $"{NamespaceBase}.InputQueryGroupAttribute";
        public static readonly string QueryChannelAttribute    = $"{NamespaceBase}.QueryChannelAttribute";

        public static readonly string Revision                 = $"{NamespaceBase}.Revision";
        public static readonly string DependencySystem         = $"{NamespaceBase}.DependencySystem";

        public static readonly string IInputQueryGroup         = $"{InternalNamespace}.IInputQueryGroup";
        public static readonly string IDependencyValue         = $"{InternalNamespace}.IDependencyValue";
        public static readonly string InputDependencyValue     = $"{InternalNamespace}.InputDependencyValue";
        public static readonly string DerivedDependencyValue   = $"{InternalNamespace}.DerivedDependencyValue";
        public static readonly string KeyValueCache            = $"{InternalNamespace}.KeyValueCache";
        public static readonly string EventProxy               = $"{InternalNamespace}.EventProxy";
    }
}
