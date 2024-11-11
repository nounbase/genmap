namespace Nounbase.Core.Interfaces.Readers
{
    public interface IDbJsonReader
    {
        Task<T> Get<T>(string sqlQuery);
        Task<IList<T>> GetList<T>(string sqlQuery);
    }
}
