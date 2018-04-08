using System;

namespace DotNetHelper
{
    public abstract class StrongTyping<T>
    {
        protected StrongTyping(T tRef)
        {
            Value = tRef;
        }

        public T Value { get; protected set; }
    }

    public abstract class StrongTypingNotNull<T> : StrongTyping<T>
        where T : class
    {
        protected StrongTypingNotNull(T tRef)
            : base(tRef)
        {
            if (tRef == null)
                throw new ArgumentNullException();
        }
    }
}