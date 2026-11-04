using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class AdminPanel : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_Dropdown roleDropdown;
    [SerializeField] private UserDatabase userDatabase;

    public void AssignRole()
    {
        string username = usernameInput.text;
        UserRole newRole = (UserRole)roleDropdown.value;

        foreach (var user in userDatabase.users)
        {
            if (user.username == username)
            {
                user.role = newRole;
                break;
            }
        }

        Debug.Log("Assigned role " + newRole + " to " + username);
    }
}