﻿using GeoGen.Constructor;
using GeoGen.Core;

namespace GeoGen.TheoremsAnalyzer
{
    /// <summary>
    /// Represents an input of the <see cref="ISubtheoremAnalyzer"/>.
    /// </summary>
    public class SubtheoremAnalyzerInput
    {
        /// <summary>
        /// Gets or sets the trivial that is tested whether it implies the examined one.
        /// </summary>
        public Theorem TemplateTheorem { get; set; }

        /// <summary>
        /// Gets or sets the theorem about which we want to found out whether it is a consequence of a simple one.
        /// </summary>
        public Theorem ExaminedTheorem { get; set; }

        /// <summary>
        /// Gets or sets the contextual picture where the examined configuration is drawn.
        /// </summary>
        public ContextualPicture ExaminedConfigurationContexualPicture { get; set; }
    }
}
