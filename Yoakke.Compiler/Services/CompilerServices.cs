using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Services.Impl;
using Yoakke.Dependency;

namespace Yoakke.Compiler.Services
{
    /// <summary>
    /// The main interface to utilize all compiler services.
    /// </summary>
    public class CompilerServices
    {
        private DependencySystem dependencySystem;

        /// <summary>
        /// The input service.
        /// </summary>
        public IInputService Input => dependencySystem.Get<IInputService>();

        /// <summary>
        /// The syntax service.
        /// </summary>
        public ISyntaxService Syntax => dependencySystem.Get<ISyntaxService>();

        /// <summary>
        /// The symbol service.
        /// </summary>
        public ISymbolService Symbol => dependencySystem.Get<ISymbolService>();

        /// <summary>
        /// The typing service.
        /// </summary>
        public ITypeService Type => dependencySystem.Get<ITypeService>();

        /// <summary>
        /// The evaluation service.
        /// </summary>
        public IEvaluationService Evaluation => dependencySystem.Get<IEvaluationService>();

        /// <summary>
        /// The compilation service.
        /// </summary>
        public ICompilationService Compilation => dependencySystem.Get<ICompilationService>();

        /// <summary>
        /// Initializes the compiler service collection
        /// </summary>
        public CompilerServices()
        {
            dependencySystem = new DependencySystem()
                .Register<IInputService>()
                .Register<ISyntaxService, SyntaxService>()
                .Register<ISymbolService, SymbolService>()
                .Register<ITypeService, TypeService>()
                .Register<IEvaluationService, EvaluationService>()
                .Register<ICompilationService, CompilationService>();
        }
    }
}
