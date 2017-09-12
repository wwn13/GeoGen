﻿namespace GeoGen.Generator.Constructing.Arguments.SignatureMatching
{
    /// <summary>
    /// A default implementation of <see cref="IConstructionSignatureMatcherFactory"/>.
    /// </summary>
    internal class ConstructionSignatureMatcherFactory : IConstructionSignatureMatcherFactory
    {
        /// <summary>
        /// Creates a construction signature matcher.
        /// </summary>
        /// <returns>The construction signature matcher.</returns>
        public IConstructionSignatureMatcher CreateMatcher()
        {
            // We need to create new instances of this class, because it's not thread-safe
            return new ConstructionSignatureMatcher();
        }
    }
}
