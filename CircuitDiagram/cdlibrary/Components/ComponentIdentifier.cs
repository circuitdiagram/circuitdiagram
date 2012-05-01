using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.Components
{
    /// <summary>
    /// Represents a description and configuration for a component.
    /// </summary>
    public class ComponentIdentifier
    {
        /// <summary>
        /// The description.
        /// </summary>
        public ComponentDescription Description { get; set; }

        /// <summary>
        /// The configuration.
        /// </summary>
        public ComponentConfiguration Configuration { get; set; }

        /// <summary>
        /// Creates a new component identifier with the specified description.
        /// </summary>
        /// <param name="description">The component description.</param>
        public ComponentIdentifier(ComponentDescription description)
        {
            Description = description;
        }

        /// <summary>
        /// Creates a new component identifier with the specified description and configuration.
        /// </summary>
        /// <param name="description">The component description.</param>
        /// <param name="configuration">The component configuration.</param>
        public ComponentIdentifier(ComponentDescription description, ComponentConfiguration configuration)
        {
            Description = description;
            Configuration = configuration;
        }

        /// <summary>
        /// Determines whether two ComponentIdentifiers are equal.
        /// </summary>
        /// <param name="a">The first object.</param>
        /// <param name="b">The second object.</param>
        /// <returns>True if they are equal, false otherwise.</returns>
        public static bool operator ==(ComponentIdentifier a, ComponentIdentifier b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return (a.Description == b.Description && a.Configuration == b.Configuration);
        }

        /// <summary>
        /// Determines whether two ComponentIdentifiers are not equal.
        /// </summary>
        /// <param name="a">The first object.</param>
        /// <param name="b">The second object.</param>
        /// <returns>True if they are not equal, false otherwise.</returns>
        public static bool operator !=(ComponentIdentifier a, ComponentIdentifier b)
        {
            // If both are null, or both are the same instance, return false
            if (System.Object.ReferenceEquals(a, b))
                return false;

            // If either is null (but not both), return true.
            if (a == null || b == null)
            {
                return true;
            }

            return !(a.Description == b.Description && a.Configuration == b.Configuration);
        }

        /// <summary>
        /// Determines whether this ComponentIdentifier is equal to another object.
        /// </summary>
        /// <param name="obj">The o</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is ComponentIdentifier))
                return false;

            return (this == obj as ComponentIdentifier);
        }

        /// <summary>
        /// Returns a hash code.
        /// </summary>
        /// <returns>A hash code.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
