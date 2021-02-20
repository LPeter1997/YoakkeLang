﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Dependency.Internal
{
    public class InputDependencyValue<TValue> : IDependencyValue
    {
        private object value;

        public int ChangedAt { get; private set; } = -1;
        public int VerifiedAt => ChangedAt;
        public bool NeedsRecomputing => false;

        public T GetValue<T>([CallerMemberName] string memberName = "")
        {
            if (ChangedAt == -1)
            {
                throw new InvalidOperationException($"Tried to retrieve {memberName} before it was ever set!");
            }
            return (T)value;
        }

        public void SetValue(DependencySystem system, TValue value)
        {
            this.value = value;
            ChangedAt = system.GetNextRevision();
        }
    }
}
