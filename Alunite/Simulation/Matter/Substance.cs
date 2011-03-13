using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A description of a solid pattern of matter. Substances are orientation and scale dependant.
    /// </summary>
    public abstract class Substance
    {
        /// <summary>
        /// Gets the density of this substance in kilograms per cubic meter.
        /// </summary>
        public abstract double Density { get; }

        /// <summary>
        /// A completely empty substance.
        /// </summary>
        public static VacuumSubstance Vacuum
        {
            get
            {
                return VacuumSubstance.Singleton;
            }
        }

        /// <summary>
        /// A stable, solid substance of common iron.
        /// </summary>
        public static IronSubstance Iron
        {
            get
            {
                return IronSubstance.Singleton;
            }
        }
    }

    /// <summary>
    /// A completely empty substance.
    /// </summary>
    public class VacuumSubstance : Substance
    {
        private VacuumSubstance()
        {

        }

        /// <summary>
        /// The only instance of this class.
        /// </summary>
        public static readonly VacuumSubstance Singleton = new VacuumSubstance();

        public override double Density
        {
            get
            {
                return 0.0;
            }
        }
    }

    /// <summary>
    /// A stable, solid substance of common iron.
    /// </summary>
    public class IronSubstance : Substance
    {
        private IronSubstance()
        {

        }

        /// <summary>
        /// The only instance of this class.
        /// </summary>
        public static readonly IronSubstance Singleton = new IronSubstance();

        public override double Density
        {
            get
            {
                return 7874.0;
            }
        }
    }
}