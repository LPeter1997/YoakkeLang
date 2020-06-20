using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;

namespace Yoakke.Compiler.Tests
{
    /// <summary>
    /// Utilities for loading native code.
    /// </summary>
    static class NativeUtils
    {
        public static T LoadNativeMethod<T>(string dllPath, string methodName) where T: Delegate
        {
            return (T)LoadNativeMethod(dllPath, methodName, typeof(T));
        }

        public static Delegate LoadNativeMethod(string dllPath, string methodName, Type delegateType)
        {
            MethodInfo method = delegateType.GetMethod("Invoke");
            var returnType = method.ReturnType;
            var parameterTypes = method.GetParameters().Select(x => x.ParameterType).ToArray();
            var nativeMethod = LoadNativeMethod(dllPath, methodName, returnType, parameterTypes);
            return Delegate.CreateDelegate(delegateType, nativeMethod);
        }

        public static MethodInfo LoadNativeMethod(string dllPath, string methodName, Type returnType, Type[] parameterTypes)
        {
            var asmName = new AssemblyName("TempAssembly");
            var asmBuilder = AssemblyBuilder.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
            var modBuilder = asmBuilder.DefineDynamicModule("TempModule");
            var typeBuilder = modBuilder.DefineType("TempType", TypeAttributes.Class | TypeAttributes.Public);

            // Optional: Use if you need to set properties on DllImportAttribute
            var dllImportCtor = typeof(DllImportAttribute).GetConstructor(new Type[] { typeof(string) });
            var dllImportBuilder = new CustomAttributeBuilder(dllImportCtor, new object[] { dllPath });

            var pinvokeBuilder = typeBuilder.DefinePInvokeMethod(
                name: methodName,
                dllName: dllPath,
                entryName: methodName,
                attributes: MethodAttributes.Static | MethodAttributes.Public,
                callingConvention: CallingConventions.Standard,
                returnType: returnType,
                parameterTypes: parameterTypes,
                nativeCallConv: CallingConvention.StdCall,
                nativeCharSet: CharSet.Unicode);

            pinvokeBuilder.SetCustomAttribute(dllImportBuilder);

            Type tempType = typeBuilder.CreateType();
            return tempType.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
        }
    }
}
