﻿using System;
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
            this._Output = new Terminal<Void, View>();
        }

        /// <summary>
        /// Gets the only instance of this class.
        /// </summary>
        public static readonly CameraEntity Singleton = new CameraEntity();

        /// <summary>
        /// Gets the output image of this camera sensor.
        /// </summary>
        public Terminal<Void, View> Output
        {
            get
            {
                return this._Output;
            }
        }

        private Terminal<Void, View> _Output;
    }

    /// <summary>
    /// A view of a simulation that can be rendered to a graphics context.
    /// </summary>
    public class View
    {

    }
}