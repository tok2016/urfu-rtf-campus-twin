using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UserDatabase", menuName = "ScriptableObjects/UserDatabase")]
public class UserDatabase : ScriptableObject
{
    [System.Serializable]
    public class User
    {
        public string username;
        public string password;
        public UserRole role;
    }

    public List<User> users = new List<User>();
}

public enum UserRole
{
    Regular,
    Admin
}