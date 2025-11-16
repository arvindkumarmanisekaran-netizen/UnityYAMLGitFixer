// Copyright (c) 2019-2024 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
//
using System;
using System.Collections.Generic;

namespace SoftCircuits.IniFileParser
{
    /// <summary>
    /// Represents an INI file section.
    /// </summary>
    /// <remarks>
    /// Constructs a new <see cref="IniSection"></see> instance.
    /// </remarks>
    /// <param name="name">Name of this INI section.</param>
    /// <param name="comparer"><see cref="StringComparer"></see> used to
    /// look up setting names.</param>
    public class IniSection : Dictionary<string, IniSetting>
    {
        // Traditional constructor used in C# 8
        public IniSection(string name, StringComparer comparer)
            : base(comparer)
        {
            // Explicitly assign the 'name' parameter to the property
            this.Name = name;
        }

        /// <summary>
        /// The name of this INI section.
        /// </summary>
        public string Name { get; private set; }
    }
}
