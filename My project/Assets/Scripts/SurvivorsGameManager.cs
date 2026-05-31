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

    public GameObject titlePanel;
    public GameObject gameOverPanel;
    public Button startButton;
    public Button restartButton;

    public Text statusText;
    public Text levelText;
    public Text timerText;
    public Text gameOverTitleText;
    public Text gameOverInfoText;

    public int level = 1;
    public int experience;
    public int experienceToNextLevel = 5;

    private float elapsed;
    private bool gameOver;
    private bool gameStarted;

    private readonly List<RewardChoice> rewardPool = new()
    {
        new RewardChoice(RewardType.Damage, "강화 탄환", "권총 피해량 +1"),
        new RewardChoice(RewardType.FireRate, "빠른 장전", "권총 발사 속도 증가"),
        new RewardChoice(RewardType.Shotgun, "낡은 샷건", "부채꼴 산탄 추가 발사"),
        new RewardChoice(RewardType.BookOrbit, "철판 방어막", "고철이 주변을 돌며 공격"),
        new RewardChoice(RewardType.CoffeeSpill, "화염병", "불길 장판으로 범위 피해"),
        new RewardChoice(RewardType.MoveSpeed, "아드레날린", "이동 속도 증가"),
        new RewardChoice(RewardType.MaxHealth, "응급 처치", "최대 체력 +1 및 회복"),
        new RewardChoice(RewardType.ProjectileSpeed, "고속 탄환", "탄환 속도 증가")
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

        if (startButton != null)
            startButton.onClick.AddListener(StartGame);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        ShowTitle();
        RefreshHud();
    }

    private void Update()
    {
        if (KeyboardRestartPressed())
            RestartGame();

        if (gameOver || !gameStarted)
            return;

        elapsed += Time.deltaTime;
        RefreshHud();
    }

    public void StartGame()
    {
        gameStarted = true;
        gameOver = false;
        Time.timeScale = 1f;

        if (titlePanel != null)
            titlePanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        RefreshHud();
    }

    public void AddExperience(int amount)
    {
        if (gameOver || !gameStarted)
            return;

        experience += amount;
        while (experience >= experienceToNextLevel)
        {
            experience -= experienceToNextLevel;
            level++;
            experienceToNextLevel = Mathf.RoundToInt(experienceToNextLevel * 1.35f + 2);

            if (rewardPanel != null)
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
            case RewardType.Shotgun:
                autoAimWeapon.UnlockShotgun();
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

    private void ShowTitle()
    {
        gameStarted = false;
        Time.timeScale = 0f;

        if (titlePanel != null)
            titlePanel.SetActive(true);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (statusText != null)
            statusText.text = "START 버튼을 눌러 폐허 도시로 진입";
    }

    private void OnPlayerDied(Health health)
    {
        gameOver = true;
        Time.timeScale = 0f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (gameOverTitleText != null)
            gameOverTitleText.text = "YOU DIED";

        if (gameOverInfoText != null)
            gameOverInfoText.text = "생존 시간 " + FormatTime(elapsed) + " / 도달 레벨 " + level + "\nR 키 또는 재시작 버튼으로 다시 도전";

        if (statusText != null)
            statusText.text = "사망 - R 키로 재시작";
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void RefreshHud()
    {
        if (levelText != null)
            levelText.text = "LV " + level + "  XP " + experience + " / " + experienceToNextLevel;

        if (timerText != null)
            timerText.text = FormatTime(elapsed);

        if (statusText != null && !gameOver && gameStarted)
            statusText.text = "WASD 이동 | 자동 사격 | 경험치 획득 | 레벨업 보상 선택";
    }

    private static string FormatTime(float time)
    {
        var minutes = Mathf.FloorToInt(time / 60f);
        var seconds = Mathf.FloorToInt(time % 60f);
        return minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    private static bool KeyboardRestartPressed()
    {
        return UnityEngine.InputSystem.Keyboard.current != null
            && UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame;
    }
}
