using AuthProvider.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace AuthProvider.Services;

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

        return await _usersCollection.Find(x => x.email == email).FirstOrDefaultAsync();
    }

    public async Task<User?> GetAsync(string id)
    {
        return await _usersCollection.Find(x => x.id == id).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(User newUser) =>
        await _usersCollection.InsertOneAsync(newUser);

    public async Task UpdateAsync(string id, User updateUser) =>
        await _usersCollection.ReplaceOneAsync(x => x.id == id, updateUser);

    public async Task<User?> GetUserByCode(string code) =>
        await _usersCollection.Find(x => x.code == code).FirstOrDefaultAsync();

    public async Task RemoveCode(string code)
    {
        var user = await _usersCollection.Find(x => x.code == code).FirstOrDefaultAsync();
        user.code = null;
        
        await _usersCollection.ReplaceOneAsync(x => x.id == user.id, user);
    }

    public async Task<User> GetUserByRecoverToken(string token) =>
        await _usersCollection.Find(x => x.recoverToken == token).FirstOrDefaultAsync();

    public async Task<string> GetRecoverToken(string id, string token)
    {
        var user = await GetAsync(id);
        user!.recoverToken = token;
        await _usersCollection.ReplaceOneAsync(x => x.id == user.id, user);
        return token;
    }
    
    public async Task<string> ChangePasswordFromRecoverToken(User credentials, string token)
    {
        var user = await GetAsync(credentials!.id);
        if (user.recoverToken != token) throw new Exception("Invalid recover token");
        credentials.recoverToken = null;
        await _usersCollection.ReplaceOneAsync(x => x.id == credentials.id, credentials);
        return "Password changed. Token removed";
    }
}