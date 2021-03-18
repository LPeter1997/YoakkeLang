using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Yoakke.Dependency.Generator
{
    internal static class Diagnostics
    {
        public static readonly DiagnosticDescriptor QueryGroupInterfaceMustBePartial = new DiagnosticDescriptor(
            id: "YKDEPENDENCYGEN001",
            title: "Query group interface definitions must be partial",
            messageFormat: "Query group interface '{0}' definition must be partial",
            category: "Yoakke.Dependency.Generator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor QueryGroupInterfaceMustBeTopLevel = new DiagnosticDescriptor(
            id: "YKDEPENDENCYGEN002",
            title: "Query group interface definitions must be top-level",
            messageFormat: "Query group interface '{0}' definition must be a top-level definition",
            category: "Yoakke.Dependency.Generator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor QueryGroupElementMustBePropertyOrMethod = new DiagnosticDescriptor(
            id: "YKDEPENDENCYGEN003",
            title: "Query group interface must only contain methods and get-only properties",
            messageFormat: "Query group interface '{0}' must only contain methods and get-only properties, '{1}' is illegal",
            category: "Yoakke.Dependency.Generator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor InputQueryGroupElementMustBePropertyOrMethod = new DiagnosticDescriptor(
            id: "YKDEPENDENCYGEN004",
            title: "Input query group interface must only contain methods and get-set properties",
            messageFormat: "Input query group interface '{0}' must only contain methods and get-set properties, '{1}' is illegal",
            category: "Yoakke.Dependency.Generator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor SpecifiedQueryChannelIsNotAnEventMember = new DiagnosticDescriptor(
            id: "YKDEPENDENCYGEN005",
            title: "Quey channel names must point to a declared event in the interface definition",
            messageFormat: "Quey channel name '{0}' does not point to an event member",
            category: "Yoakke.Dependency.Generator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor NoMemberForQueryChannelName = new DiagnosticDescriptor(
            id: "YKDEPENDENCYGEN006",
            title: "Quey channel names must point to a declared event in the interface definition",
            messageFormat: "Quey channel name '{0}' does not point to an event member",
            category: "Yoakke.Dependency.Generator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor QueryChannelEventMustBeEventHandler = new DiagnosticDescriptor(
            id: "YKDEPENDENCYGEN007",
            title: "Quey channel types must be System.EventHandler<T> types",
            messageFormat: "Quey channel event '{0}' does not have a System.EventHandler<T> type",
            category: "Yoakke.Dependency.Generator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);
    }
}
