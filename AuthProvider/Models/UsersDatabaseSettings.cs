namespace AuthProvider.Models;

public class UsersDatabaseSettings
{
    public string ConnectionString { get; set; } = null!;
    
    public string DatabaseName { get; set; } = null!;
    
    public string UsersName { get; set; } = null!;
}