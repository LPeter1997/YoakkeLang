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
        /// The semantic service.
        /// </summary>
        public ISemanticService Semantic => dependencySystem.Get<ISemanticService>();

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
                .Register<ISemanticService, SemanticService>()
                .Register<ICompilationService, CompilationService>();
        }
    }
}
