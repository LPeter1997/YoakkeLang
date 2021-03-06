using System;
using System.Collections.Generic;
using System.Text;

namespace Yoakke.Dependency.Generator
{
    internal static class TypeNames
    {
        public static readonly string QueryGroupAttribute = "Yoakke.Dependency.QueryGroupAttribute";
        public static readonly string InputQueryGroupAttribute = "Yoakke.Dependency.InputQueryGroupAttribute";
        public static readonly string SystemCancellationToken = "System.Threading.CancellationToken";

        public static readonly string IInputQueryGroup = "Yoakke.Dependency.Internal.IInputQueryGroup";
        public static readonly string DependencySystem = "Yoakke.Dependency.DependencySystem";
        public static readonly string IDependencyValue = "Yoakke.Dependency.Internal.IDependencyValue";
        public static readonly string InputDependencyValue = "Yoakke.Dependency.Internal.InputDependencyValue";
        public static readonly string DerivedDependencyValue = "Yoakke.Dependency.Internal.DerivedDependencyValue";
        public static readonly string KeyValueCache = "Yoakke.Dependency.Internal.KeyValueCache";

        public static string GetComputeDelegateName(bool asynch, bool ct) =>
            $"{DerivedDependencyValue}.ComputeValue{(asynch ? "Async" : string.Empty)}{(ct ? "Ct" : string.Empty)}Delegate";
    }
}
