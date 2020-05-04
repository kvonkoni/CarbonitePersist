using System;

namespace CarbonitePersist.Entity
{
    public class TEntity<T>
    {
        public object Id { get; set; }
        public T Entity { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
