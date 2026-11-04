using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UserListManager : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private GameObject userItemPrefab;
    [SerializeField] private UserDatabase userDatabase;

    void Start()
    {
        UpdateUserList();
    }

    public void UpdateUserList()
    {
        // Очистить старые элементы
        foreach (Transform child in scrollRect.content.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        // Добавить новые элементы
        foreach (var user in userDatabase.users)
        {
            GameObject item = Instantiate(userItemPrefab, scrollRect.content);
            TMP_Text text = item.GetComponent<TMP_Text>();
            text.text = $"User: {user.username} (Role: {user.role})";
        }
    }
}
