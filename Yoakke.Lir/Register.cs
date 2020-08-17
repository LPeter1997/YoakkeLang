﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Yoakke.Lir
{
    /// <summary>
    /// Storage type for the VM.
    /// </summary>
    public record Register
    {
        /// <summary>
        /// The register index.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Initializes a new <see cref="Register"/>.
        /// </summary>
        /// <param name="index">The index of the <see cref="Register"/>.</param>
        public Register(int index)
        {
            Index = index;
        }
    }
}