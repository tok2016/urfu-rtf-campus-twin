using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class AdminPanel : MonoBehaviour
{
    [SerializeField] private string usernameInput;
    [SerializeField] private TMP_Dropdown roleDropdown;
    [SerializeField] private UserDatabase userDatabase;
     [SerializeField] private UserListManager userListManager;


    public void AssignRole()
    {
        UserRole newRole = (UserRole)roleDropdown.value;

        foreach (var user in userDatabase.users)
        {
            if (user.username == usernameInput)
            {
                user.role = newRole;
                break;
            }
        }

        Debug.Log("Assigned role " + newRole + " to " + usernameInput);
        userListManager.UpdateUserList();
    }

    public void SetUsername(string username)
    {
        usernameInput = username;
    }
}