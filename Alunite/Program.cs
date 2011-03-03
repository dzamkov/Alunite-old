using System;
using System.Collections.Generic;

using OpenTK;
using OpenTKGUI;

namespace Alunite
{
    /// <summary>
    /// Program main class.
    /// </summary>
    public static class Program
    {
        private class _Person
        {

        }

        private class _AngryPerson : _Person
        {

        }

        private class _SadPerson : _Person
        {

        }

        private class _HappyPerson : _Person
        {

        }

        /// <summary>
        /// Program main entry-point.
        /// </summary>
        public static void Main(string[] Args)
        {
            // Match matrix test
            MatchMatrix<_Person, bool> getsalongwith = new MatchMatrix<_Person, bool>((a, b) => true);
            getsalongwith.AddRule<_AngryPerson, _Person>((a, b) => false);
            getsalongwith.AddRule<_SadPerson, _HappyPerson>((a, b) => false);

            bool testa = getsalongwith.GetResult(new _HappyPerson(), new _AngryPerson());
            bool testb = getsalongwith.GetResult(new _HappyPerson(), new _SadPerson());
            bool testc = getsalongwith.GetResult(new _HappyPerson(), new _HappyPerson());

            // Set up a world
            FastPhysics fp = new FastPhysics();
            List<FastPhysicsMatter> elems = new List<FastPhysicsMatter>();
            Random r = new Random();
            for(int t = 0; t < 1000; t++)
            {
                elems.Add(fp.Create(new Particle<FastPhysicsSubstance>()
                {
                    Substance = FastPhysicsSubstance.Default,
                    Mass = 1.0,
                    Position = new Vector(r.NextDouble(), r.NextDouble(), r.NextDouble()),
                    Velocity = new Vector(0.0, 0.0, 0.0),
                    Orientation = Quaternion.Identity,
                    Spin = AxisAngle.Identity
                }));
            }
            FastPhysicsMatter world = fp.Compose(elems);

            for (int t = 0; t < 10; t++)
            {
                world = fp.Update(world, fp.Null, 0.1);
            }

            /*
            HostWindow hw = new HostWindow("Alunite", 640, 480);
            hw.WindowState = WindowState.Maximized;
            hw.Control = new Visualizer(world);
            hw.Run(60.0);*/
        }
    }
}