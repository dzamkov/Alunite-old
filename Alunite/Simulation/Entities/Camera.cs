using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A sensor entity that exposes a terminal which gives the image seen starting at (0.0, 0.0, 0.0) and looking towards (1.0, 0.0, 0.0) in local
    /// coordinates.
    /// </summary>
    public class CameraEntity : PhantomEntity
    {
        internal CameraEntity()
        {
            this._Output = new OutTerminal<View>();
        }

        /// <summary>
        /// Gets the only instance of this class.
        /// </summary>
        public static readonly CameraEntity Singleton = new CameraEntity();

        /// <summary>
        /// Gets the output image terminal of this camera sensor.
        /// </summary>
        public OutTerminal<View> Output
        {
            get
            {
                return this._Output;
            }
        }

        public override Span CreateSpan(Span Environment, ControlInput Input)
        {
            return new CameraSpan(Environment);
        }

        private OutTerminal<View> _Output;
    }
}