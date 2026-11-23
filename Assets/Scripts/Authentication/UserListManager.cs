using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UserListManager : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private GameObject userItemPrefab;
    [SerializeField] private UserDatabase userDatabase;
    [SerializeField] private AdminPanel adminPanel;

    private Image selectedImage;
    private Color selectedColor = new Color(0.5f, 0.7f, 1f, 1f);
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
            TMP_Text text = item.GetComponentInChildren<TMP_Text>();
            text.text = $"{user.username} : {user.role}";

            //Image image = item.GetComponent<Image>();
            Button button = item.GetComponentInChildren<Button>();
            //Color originalColor = image.color;
            string selectedUsername = user.username;
            button.onClick.AddListener(() =>
            {
                 if (selectedImage != null)
                {
                    selectedImage.color = Color.white;
                }

                // Выделить текущий элемент
             //   image.color = selectedColor;
              //  selectedImage = image;
                adminPanel.SetUsername(selectedUsername);
            });
        }
    }
}
