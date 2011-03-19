using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A span for a camera entity.
    /// </summary>
    public class CameraSpan : Span
    {
        public CameraSpan(Span Environment)
        {
            this._Environment = Environment;
        }

        public override Entity this[double Time]
        {
            get
            {
                return CameraEntity.Singleton;
            }
        }

        public override Signal<Maybe<T>> Read<T>(OutTerminal<T> Terminal)
        {
            if ((Node)Terminal == (Node)CameraEntity.Singleton.Output)
            {
                return Signal.Just<T>((Signal<T>)(object)Visual.GetViewFeed(this._Environment));
            }
            else
            {
                return Signal.Nothing<T>();
            }
        }

        public override Span Update(Span Environment, ControlInput Input)
        {
            return new CameraSpan(Environment);
        }

        private Span _Environment;
    }
}