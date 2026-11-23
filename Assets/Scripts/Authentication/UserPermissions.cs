using UnityEngine;

public static class UserPermissions
{
    public static bool IsAdmin()
    {
        return PlayerPrefs.GetString("UserRole", "Regular") == "Admin";
    }

    public static void SetUserRole(UserRole role)
    {
        PlayerPrefs.SetString("UserRole", role.ToString());
        PlayerPrefs.Save();
    }
}