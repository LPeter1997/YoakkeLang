﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Yoakke
{
    /// <summary>
    /// Represents the base for every compile error.
    /// </summary>
    abstract class CompileError : Exception
    {
        /// <summary>
        /// Dumps this error to the standard output.
        /// </summary>
        public abstract void Show();
    }
}