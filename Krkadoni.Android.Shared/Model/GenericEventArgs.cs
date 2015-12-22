using System;

namespace Com.Krkadoni.Utils.Model
{
    public class GenericEventArgs<T> : EventArgs
    {
        public T Item { get; private set;}
                
        public GenericEventArgs(T value)
        {
            this.Item = value;
        }
    }

    public class GenericEventArgs<T1, T2> : EventArgs 
    {
        public T1 Item1 { get; private set;}
        public T2 Item2 { get; private set;}

        public GenericEventArgs(T1 value1, T2 value2)
        {
            this.Item1 = value1;
            this.Item2 = value2;
        }
    }

    public class GenericEventArgs<T1, T2, T3> : EventArgs 
    {
        public T1 Item1 { get; private set;}
        public T2 Item2 { get; private set;}
        public T3 Item3 { get; private set;}

        public GenericEventArgs(T1 value1, T2 value2, T3 value3)
        {
            this.Item1 = value1;
            this.Item2 = value2;
            this.Item3 = value3;
        }
    }
}

