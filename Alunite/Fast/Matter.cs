using System;
using System.Collections.Generic;
using System.Linq;

namespace Alunite.Fast
{
    /// <summary>
    /// Matter in a fast physics system.
    /// </summary>
    public abstract class Matter : IMatter
    {
        public Matter()
        {

        }

        /// <summary>
        /// Estimates the complexity of this matter by getting the amount of unique bits of matter used to
        /// describe it.
        /// </summary>
        public int Complexity
        {
            get
            {
                HashSet<Matter> used = new HashSet<Matter>();
                this.OutputUsed(used);
                return used.Count;
            }
        }

        /// <summary>
        /// Applies a transform to this matter.
        /// </summary>
        public virtual Matter Apply(Physics Physics, Transform Transform)
        {
            return new TransformedMatter(this, Transform);
        }

        /// <summary>
        /// Gets the updated form of this matter in the specified environment after the given time.
        /// </summary>
        public abstract Matter Update(Physics Physics, Matter Environment, double Time);

        /// <summary>
        /// Gets a summary of the location and density of mass inside the matter by getting the total mass, center of mass, 
        /// and extent (distance from the center of mass to the farthest piece of matter).
        /// </summary>
        public abstract void GetMassSummary(Physics Physics, out double Mass, out Vector CenterOfMass, out double Extent);

        /// <summary>
        /// Gets the force of gravity a particle at the specified offset and mass will feel from this matter.
        /// </summary>
        /// <param name="RecurseThreshold">The ratio of (mass * extent) / (distance ^ 2) a piece of matter will have to have in order to have its
        /// gravity force "refined". Set at 0.0 to get the exact gravity.</param>
        public virtual Vector GetGravity(Physics Physics, Vector Position, double Mass, double RecurseThreshold)
        {
            return Physics.GetGravity(Physics.GetMass(this), Position, Mass);
        }

        /// <summary>
        /// Recursively outputs all matter used in the definition of this matter, including this matter itself.
        /// </summary>
        public virtual void OutputUsed(HashSet<Matter> Elements)
        {
            Elements.Add(this);
        }

        /// <summary>
        /// Outputs all particles defined in this matter, after applying the specified transform.
        /// </summary>
        public virtual void OutputParticles(Physics Physics, Transform Transform, List<Particle<Substance>> Particles)
        {

        }

        /// <summary>
        /// Gets all particles defined in this matter.
        /// </summary>
        public IEnumerable<Particle<Substance>> GetParticles(Physics Physics)
        {
            List<Particle<Substance>> parts = new List<Particle<Substance>>();
            this.OutputParticles(Physics, Transform.Identity, parts);
            return parts;
        }
    }
}