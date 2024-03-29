﻿using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AuthProvider.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? id { get; set;  }
    
    public string? username { get; set; }
    
    public string? email { get; set; }
    
    [BsonRequired]
    public string password { get; set; }
    
    [BsonIgnoreIfNull]
    public string? code { get; set; }
    
    [BsonIgnoreIfNull]
    public string? recoverToken { get; set; }
}