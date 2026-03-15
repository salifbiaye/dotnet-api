using MongoDB.Driver;

namespace API_web.Models;

public class TodoContext
{
    private readonly IMongoDatabase _database;

    public TodoContext(IMongoClient client, IConfiguration configuration)
    {
        var databaseName = configuration["MongoDB:DatabaseName"] ?? "todoapi";
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<TodoItem> TodoItems => _database.GetCollection<TodoItem>("todoitems");
    public IMongoCollection<User> Users => _database.GetCollection<User>("users");
}
