namespace OpenBullet2.Web.Exceptions;

public class EntryNotFoundException : Exception
{
    public int Id { get; set; }
    public string Collection { get; set; }

    public EntryNotFoundException(int id, string collection)
        : base($"The requested entry with id {id} was not found in the collection {collection}")
    {
        Id = id;
        Collection = collection;
    }
}
