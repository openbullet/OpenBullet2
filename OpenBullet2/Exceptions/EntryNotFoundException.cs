using System;

namespace OpenBullet2.Exceptions
{
    public class EntryNotFoundException : Exception
    {
        public EntryNotFoundException(int id, string collection)
            : base($"The requested entry with id {id} was not found in the collection {collection}")
        {
            
        }

        public EntryNotFoundException(string message)
            : base(message)
        {
            
        }

        public EntryNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
            
        }
    }
}
