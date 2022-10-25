using AuthProvider.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AuthProvider.Services;

using MongoDB.Driver;

public class UserService
{
    private readonly IMongoCollection<User> _usersCollection;

    public UserService(IOptions<UsersDatabaseSettings> usersDatabaseSettings)
    {
        var mongoClient = new MongoClient(usersDatabaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(usersDatabaseSettings.Value.DatabaseName);
        _usersCollection = mongoDatabase.GetCollection<User>(usersDatabaseSettings.Value.UsersName);
    }

    public async Task<List<User>> GetAsync() => await _usersCollection.Find(_ => true).ToListAsync();

    public async Task<User?> GetUser(string? email, string? username)
    {
        if (email != null && username == null)
        {
            return await _usersCollection.Find(x => x.email == email).FirstOrDefaultAsync();

        }
        else if (email == null && username != null)
        {
            return await _usersCollection.Find(x => x.username == username).FirstOrDefaultAsync();   
        }

        return null;
    }
    
    public async Task<User?> GetAsync(string id)
    {
        return await _usersCollection.Find(x => x.id == id).FirstOrDefaultAsync();
        }

    public async Task CreateAsync(User newUser) =>
        await _usersCollection.InsertOneAsync(newUser);

    public async Task UpdateAsync(string id, User updateUser) =>
        await _usersCollection.ReplaceOneAsync(x => x.id == id, updateUser);

    public async Task RemoveAsync(string id) =>
        await _usersCollection.DeleteOneAsync(x => x.id == id);
}