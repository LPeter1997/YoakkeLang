using System;
using System.Collections.Generic;
using System.Text;

namespace Yoakke.IR
{
    /// <summary>
    /// A context for unique names inside an <see cref="Assembly"/>.
    /// Makes sure that every global name is unique and every local name is unique in global space and in it's
    /// local space.
    /// </summary>
    class NamingContext
    {
        /// <summary>
        /// The <see cref="Assembly"/> this <see cref="NamingContext"/> manages.
        /// </summary>
        public readonly Assembly Assembly;

        private HashSet<string> globalNames = new HashSet<string>();
        private HashSet<string> localNames = new HashSet<string>();
        private Dictionary<object, string> names = new Dictionary<object, string>();

        /// <summary>
        /// Initializes a new <see cref="NamingContext"/>.
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/> to manage.</param>
        public NamingContext(Assembly assembly)
        {
            Assembly = assembly;
            PreallocateNames();
        }

        private void PreallocateNames()
        {
            // First externals
            foreach (var external in Assembly.Externals) globalNames.Add(external.LinkName);
            // Then procedures that have link name
            foreach (var proc in Assembly.Procedures)
            {
                if (proc.LinkName != null) globalNames.Add(proc.LinkName);
            }
        }

        /// <summary>
        /// Erases the current local names, so new ones can take the same names.
        /// </summary>
        public void NewLocals()
        {
            localNames.Clear();
        }

        /// <summary>
        /// Gets the name for the given <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to get the name of.</param>
        /// <returns>The name for the <see cref="Type"/>.</returns>
        public string GetTypeName(Type type)
        {
            if (names.TryGetValue(type, out var name)) return name;
            name = GetNewGlobalName("usertype");
            names.Add(type, name);
            return name;
        }

        /// <summary>
        /// Gets the name for the given <see cref="Proc"/>. If it has a link name, that will be returned.
        /// </summary>
        /// <param name="proc">The <see cref="Proc"/> to get the name of.</param>
        /// <returns>The name for the <see cref="Proc"/>.</returns>
        public string GetProcName(Proc proc)
        {
            if (names.TryGetValue(proc, out var name)) return name;
            name = proc.LinkName ?? GetNewGlobalName("proc");
            names.Add(proc, name);
            return name;
        }

        /// <summary>
        /// Gets the name for the given <see cref="BasicBlock"/>.
        /// </summary>
        /// <param name="basicBlock">The <see cref="BasicBlock"/> to get the name of.</param>
        /// <returns>The name for the <see cref="BasicBlock"/>.</returns>
        public string GetBasicBlockName(BasicBlock basicBlock)
        {
            if (names.TryGetValue(basicBlock, out var name)) return name;
            name = GetNewLocalName("label");
            names.Add(basicBlock, name);
            return name;
        }

        /// <summary>
        /// Gets the name for the given <see cref="Value.Register"/>.
        /// </summary>
        /// <param name="register">The <see cref="Value.Register"/> to get the name of.</param>
        /// <returns>The name for the <see cref="Value.Register"/>.</returns>
        public string GetRegisterName(Value.Register register)
        {
            if (names.TryGetValue(register, out var name)) return name;
            name = GetNewLocalName($"r{register.Index}");
            names.Add(register, name);
            return name;
        }

        /// <summary>
        /// Allocates a new global name.
        /// </summary>
        /// <param name="name">The suggestion for the name.</param>
        /// <returns>A new, unique global name based on the given suggestion.</returns>
        public string GetNewGlobalName(string name)
        {
            if (globalNames.Add(name)) return name;
            int i = 0;
            while (true)
            {
                string nextName = $"{name}{i}";
                if (globalNames.Add(nextName)) return nextName;
                ++i;
            }
        }

        /// <summary>
        /// Allocates a new local name.
        /// </summary>
        /// <param name="name">The suggestion for the name.</param>
        /// <returns>A new, unique local name based on the given suggestion.</returns>
        public string GetNewLocalName(string name)
        {
            if (!globalNames.Contains(name) && localNames.Add(name)) return name;
            int i = 0;
            while (true)
            {
                string nextName = $"{name}{i}";
                if (!globalNames.Contains(nextName) && localNames.Add(nextName)) return nextName;
                ++i;
            }
        }
    }
}
