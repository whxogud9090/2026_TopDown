using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SurvivorsGameManager : MonoBehaviour
{
    public static SurvivorsGameManager Instance { get; private set; }

    public Transform player;
    public PlayerController playerController;
    public AutoAimWeapon autoAimWeapon;
    public BookOrbitWeapon bookOrbitWeapon;
    public CoffeeSpillWeapon coffeeSpillWeapon;
    public Health playerHealth;
    public SurvivorsRewardPanel rewardPanel;
    public Text statusText;
    public Text healthText;
    public Text levelText;
    public Text timerText;

    public int level = 1;
    public int experience;
    public int experienceToNextLevel = 5;

    private float elapsed;
    private bool gameOver;

    private readonly List<RewardChoice> rewardPool = new()
    {
        new RewardChoice(RewardType.Damage, "강화 탄환", "권총 피해량이 1 증가합니다."),
        new RewardChoice(RewardType.FireRate, "빠른 장전", "권총을 더 빠르게 발사합니다."),
        new RewardChoice(RewardType.BookOrbit, "철판 방어막", "고철이 주변을 돌며 좀비를 공격합니다."),
        new RewardChoice(RewardType.CoffeeSpill, "화염병", "좀비 위치에 불길을 남겨 범위 피해를 줍니다."),
        new RewardChoice(RewardType.MoveSpeed, "아드레날린", "이동 속도가 증가합니다."),
        new RewardChoice(RewardType.MaxHealth, "응급 처치", "최대 체력이 1 증가합니다."),
        new RewardChoice(RewardType.ProjectileSpeed, "고속 탄환", "탄환이 더 빠르게 날아갑니다.")
    };

    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    private void Start()
    {
        if (playerHealth != null)
        {
            playerHealth.Died += OnPlayerDied;
            playerHealth.Changed += _ => RefreshHud();
        }

        RefreshHud();
    }

    private void Update()
    {
        if (KeyboardRestartPressed())
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        if (gameOver)
            return;

        elapsed += Time.deltaTime;
        RefreshHud();
    }

    public void AddExperience(int amount)
    {
        if (gameOver)
            return;

        experience += amount;
        while (experience >= experienceToNextLevel)
        {
            experience -= experienceToNextLevel;
            level++;
            experienceToNextLevel = Mathf.RoundToInt(experienceToNextLevel * 1.35f + 2);
            rewardPanel.Show(GetRandomRewards(3));
        }

        RefreshHud();
    }

    public void ApplyReward(RewardChoice reward)
    {
        switch (reward.type)
        {
            case RewardType.Damage:
                autoAimWeapon.damage += 1;
                break;
            case RewardType.FireRate:
                autoAimWeapon.fireCooldown = Mathf.Max(0.08f, autoAimWeapon.fireCooldown * 0.82f);
                break;
            case RewardType.MoveSpeed:
                playerController.moveSpeed += 0.35f;
                break;
            case RewardType.MaxHealth:
                playerHealth.SetMaxHealth(playerHealth.maxHealth + 1, true);
                break;
            case RewardType.ProjectileSpeed:
                autoAimWeapon.projectileSpeed += 1.5f;
                break;
            case RewardType.BookOrbit:
                bookOrbitWeapon.Upgrade();
                break;
            case RewardType.CoffeeSpill:
                coffeeSpillWeapon.Upgrade();
                break;
        }

        RefreshHud();
    }

    private IReadOnlyList<RewardChoice> GetRandomRewards(int count)
    {
        var copy = new List<RewardChoice>(rewardPool);
        var result = new List<RewardChoice>();

        while (result.Count < count && copy.Count > 0)
        {
            var index = Random.Range(0, copy.Count);
            result.Add(copy[index]);
            copy.RemoveAt(index);
        }

        return result;
    }

    private void OnPlayerDied(Health health)
    {
        gameOver = true;
        Time.timeScale = 0f;
        if (statusText != null)
            statusText.text = "GAME OVER - Press R to restart";
    }

    private void RefreshHud()
    {
        if (healthText != null && playerHealth != null)
            healthText.text = "HP " + playerHealth.CurrentHealth + " / " + playerHealth.maxHealth;

        if (levelText != null)
            levelText.text = "LV " + level + "  XP " + experience + " / " + experienceToNextLevel;

        if (timerText != null)
        {
            var minutes = Mathf.FloorToInt(elapsed / 60f);
            var seconds = Mathf.FloorToInt(elapsed % 60f);
            timerText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
        }

        if (statusText != null && !gameOver)
            statusText.text = "WASD 이동 | 권총 자동 사격 | 좀비 웨이브 생존";
    }

    private static bool KeyboardRestartPressed()
    {
        return UnityEngine.InputSystem.Keyboard.current != null
            && UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame;
    }
}
