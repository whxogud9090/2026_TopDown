using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SurvivorsRewardPanel : MonoBehaviour
{
    public GameObject root;
    public Button[] buttons;
    public Text[] labels;
    public Image[] icons;
    public Sprite pencilIcon;
    public Sprite bookIcon;
    public Sprite coffeeIcon;
    public Sprite eraserIcon;
    public Sprite rubberBandIcon;

    private readonly List<RewardChoice> choices = new();

    private void Awake()
    {
        Hide();
    }

    public void Show(IReadOnlyList<RewardChoice> newChoices)
    {
        choices.Clear();
        choices.AddRange(newChoices);

        root.SetActive(true);
        Time.timeScale = 0f;

        for (var i = 0; i < buttons.Length; i++)
        {
            var index = i;
            var hasChoice = i < choices.Count;
            buttons[i].gameObject.SetActive(hasChoice);
            buttons[i].onClick.RemoveAllListeners();

            if (!hasChoice)
                continue;

            labels[i].text = choices[i].title + "\n" + choices[i].description;
            if (icons != null && i < icons.Length && icons[i] != null)
                icons[i].sprite = GetIcon(choices[i].type);
            buttons[i].onClick.AddListener(() => Choose(index));
        }
    }

    private void Choose(int index)
    {
        Time.timeScale = 1f;
        root.SetActive(false);
        SurvivorsGameManager.Instance.ApplyReward(choices[index]);
    }

    private void Hide()
    {
        if (root != null)
            root.SetActive(false);
    }

    private Sprite GetIcon(RewardType type)
    {
        switch (type)
        {
            case RewardType.BookOrbit:
                return bookIcon;
            case RewardType.CoffeeSpill:
            case RewardType.MoveSpeed:
                return coffeeIcon;
            case RewardType.Shotgun:
                return pencilIcon;
            case RewardType.MaxHealth:
                return eraserIcon;
            case RewardType.ProjectileSpeed:
                return rubberBandIcon;
            default:
                return pencilIcon;
        }
    }
}
