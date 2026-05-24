using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardPanel : MonoBehaviour
{
    public GameObject root;
    public Button[] buttons;
    public Text[] labels;

    private readonly List<RewardChoice> activeChoices = new();
    private GameManager gameManager;

    private void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        Hide();
    }

    public void Show(IReadOnlyList<RewardChoice> choices)
    {
        activeChoices.Clear();
        activeChoices.AddRange(choices);

        if (root != null)
            root.SetActive(true);

        Time.timeScale = 0f;

        for (var i = 0; i < buttons.Length; i++)
        {
            var index = i;
            var hasChoice = i < activeChoices.Count;
            buttons[i].gameObject.SetActive(hasChoice);
            buttons[i].onClick.RemoveAllListeners();

            if (!hasChoice)
                continue;

            buttons[i].onClick.AddListener(() => Choose(index));
            if (labels != null && i < labels.Length && labels[i] != null)
            {
                var choice = activeChoices[i];
                labels[i].text = choice.title + "\n" + choice.description;
            }
        }
    }

    public void Hide()
    {
        if (root != null)
            root.SetActive(false);
        Time.timeScale = 1f;
    }

    private void Choose(int index)
    {
        if (index < 0 || index >= activeChoices.Count)
            return;

        Hide();
        if (gameManager != null)
            gameManager.ApplyReward(activeChoices[index]);
    }
}
