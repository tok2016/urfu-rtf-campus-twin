using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginSystem : MonoBehaviour
{

    public TextMeshProUGUI ErrorText;
    [SerializeField] private UserDatabase userDatabase;
    [SerializeField] private string enteredUsername;
    [SerializeField] private string enteredPassword;

    public void CheckLogin()
    {
        bool isLoggedIn = false;
        UserRole userRole = UserRole.Regular;

        foreach (var user in userDatabase.users)
        {
            if (user.username == enteredUsername && user.password == enteredPassword)
            {
                isLoggedIn = true;
                userRole = user.role;
                break;
            }
        }

        if (isLoggedIn)
        {
            if (userRole == UserRole.Admin)
            {
                SceneManager.LoadScene("AdminTestScene");
            }
            else
            {
                SceneManager.LoadScene("MovementTest");
            }
        }
        else
        {
            ShowError();
        }
    }

    public void SetUsername(string username)
    {
        enteredUsername = username;
    }

    public void SetPassword(string password)
    {
        enteredPassword = password;
    }

    public void ShowError()
    {
        ErrorText.gameObject.SetActive(true);
    }
}