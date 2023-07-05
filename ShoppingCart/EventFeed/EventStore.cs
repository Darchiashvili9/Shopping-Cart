using Dapper;
using System.Data.SqlClient;
using System.Text.Json;

namespace ShoppingCart.EventFeed
{
    public interface IEventStore
    {
        Task<IEnumerable<Event>> GetEvents(long firstEventSequenceNumber, long lastEventSequenceNumber);
        Task Raise(string eventName, object content);
    }
    public class EventStore : IEventStore
    {
        private string connectionString = @"Data Source=localhost;Initial Catalog=ShoppingCart;User Id=SA; Password=yourStrong(!)Password";

        private const string writeEventSql = @"insert into EventStore(Name, OccurredAt, Content) values (@Name, @OccurredAt, @Content)";

        private const string readEventsSql = @"select * from EventStore where ID >= @Start and ID <= @End";

        public async Task<IEnumerable<Event>> GetEvents(long firstEventSequenceNumber, long lastEventSequenceNumber)
        {
            await using var conn = new SqlConnection(this.connectionString);

            return await conn.QueryAsync<Event>(readEventsSql, new
            {
                Start = firstEventSequenceNumber,
                End = lastEventSequenceNumber
            });
        }

        public async Task Raise(string eventName, object content)
        {

            //ვლოგავთ ივენთებს სათითაოდ თან ჯეისონად ვინახავთ მთლიანად რაც მოხდა
            var jsonContent = JsonSerializer.Serialize(content);

            await using var conn = new SqlConnection(this.connectionString);
            await conn.ExecuteAsync(writeEventSql, new
            {
                Name = eventName,
                OccurredAt = DateTimeOffset.Now,
                Content = jsonContent
            });
        }
    }
}