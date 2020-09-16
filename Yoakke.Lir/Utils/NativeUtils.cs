using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Lir.Utils
{
    /// <summary>
    /// Utilities for working with native binary files.
    /// </summary>
    public static class NativeUtils
    {
        /// <summary>
        /// Loads a native procedure from a DLL.
        /// </summary>
        /// <typeparam name="T">The procedure type.</typeparam>
        /// <param name="dllPath">The path to the DLL.</param>
        /// <param name="procName">The name of the procedure.</param>
        /// <param name="callConv">The <see cref="CallConv"/> of the native procedure.</param>
        /// <returns>The procedure callable from C#.</returns>
        public static T LoadNativeProcedure<T>(
            string dllPath, 
            string procName, 
            CallConv callConv) 
            where T : Delegate
        {
            return (T)LoadNativeProcedure(dllPath, procName, typeof(T), callConv);
        }

        /// <summary>
        /// Loads a native procedure from a DLL.
        /// </summary>
        /// <param name="dllPath">The path to the DLL.</param>
        /// <param name="procName">The name of the procedure.</param>
        /// <param name="delegateType">The <see cref="Delegate"/> type of the procedure..</param>
        /// <param name="callConv">The <see cref="CallConv"/> of the native procedure.</param>
        /// <returns>The procedure's <see cref="Delegate"/> callable from C#.</returns>
        public static Delegate LoadNativeProcedure(
            string dllPath, 
            string procName, 
            Type delegateType, 
            CallConv callConv)
        {
            MethodInfo? method = delegateType.GetMethod("Invoke");
            Debug.Assert(method != null);
            var returnType = method.ReturnType;
            var parameterTypes = method.GetParameters().Select(x => x.ParameterType).ToArray();
            var nativeMethod = LoadNativeProcedure(dllPath, procName, returnType, parameterTypes, callConv);
            return Delegate.CreateDelegate(delegateType, nativeMethod);
        }

        /// <summary>
        /// Loads a native procedure from a DLL.
        /// </summary>
        /// <param name="dllPath">The path to the DLL.</param>
        /// <param name="procName">The name of the procedure.</param>
        /// <param name="returnType">The return type of the procedure.</param>
        /// <param name="parameterTypes">The parameter types of the procedure.</param>
        /// <param name="callConv">The <see cref="CallConv"/> of the native procedure.</param>
        /// <returns>The <see cref="MethodInfo"/> of the procedure.</returns>
        public static MethodInfo LoadNativeProcedure(
            string dllPath, 
            string procName, 
            Type returnType, 
            Type[] parameterTypes, 
            CallConv callConv)
        {
            var asmName = new AssemblyName("TempAssembly");
            var asmBuilder = AssemblyBuilder.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
            var modBuilder = asmBuilder.DefineDynamicModule("TempModule");
            var typeBuilder = modBuilder.DefineType("TempType", TypeAttributes.Class | TypeAttributes.Public);

            // Optional: Use if you need to set properties on DllImportAttribute
            ConstructorInfo? dllImportCtor = typeof(DllImportAttribute).GetConstructor(new Type[] { typeof(string) });
            Debug.Assert(dllImportCtor != null);
            var dllImportBuilder = new CustomAttributeBuilder(dllImportCtor, new object[] { dllPath });

            var pinvokeBuilder = typeBuilder.DefinePInvokeMethod(
                name: procName,
                dllName: dllPath,
                entryName: procName,
                attributes: MethodAttributes.Static | MethodAttributes.Public,
                callingConvention: CallingConventions.Standard,
                returnType: returnType,
                parameterTypes: parameterTypes,
                nativeCallConv: ToCallingConvention(callConv),
                nativeCharSet: CharSet.Unicode);

            pinvokeBuilder.SetCustomAttribute(dllImportBuilder);

            Type? tempType = typeBuilder.CreateType();
            Debug.Assert(tempType != null);
            MethodInfo? result = tempType.GetMethod(procName, BindingFlags.Static | BindingFlags.Public);
            Debug.Assert(result != null);
            return result;
        }

        private static CallingConvention ToCallingConvention(CallConv callConv) => callConv switch
        {
            CallConv.Cdecl => CallingConvention.Cdecl,
            _ => throw new NotImplementedException(),
        };
    }
}
