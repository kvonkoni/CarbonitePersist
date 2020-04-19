using System;

namespace CarbonitePersist
{
    public class TEntity<T>
    {
        public object Id { get; set; }

        public T Entity { get; set; }
    }
}
