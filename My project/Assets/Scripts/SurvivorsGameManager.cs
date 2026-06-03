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
    public GrenadeWeapon grenadeWeapon;
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
    public int maxItemLevel = 10;
    public int killCount;

    private float elapsed;
    private bool gameOver;
    private bool gameStarted;
    private bool incendiaryGrenadeUnlocked;
    private readonly Dictionary<RewardType, int> passiveLevels = new();

    private readonly List<RewardChoice> rewardPool = new()
    {
        new RewardChoice(RewardType.Damage, "강화 탄환", "권총 피해량 증가"),
        new RewardChoice(RewardType.FireRate, "빠른 장전", "권총 발사 속도 증가"),
        new RewardChoice(RewardType.BookOrbit, "고철 방어막", "고철이 주변을 돌며 공격"),
        new RewardChoice(RewardType.CoffeeSpill, "화염병", "불길 장판으로 범위 피해"),
        new RewardChoice(RewardType.Grenade, "수류탄", "몸에서 던져 폭발 피해"),
        new RewardChoice(RewardType.MoveSpeed, "아드레날린", "이동 속도 증가"),
        new RewardChoice(RewardType.MaxHealth, "응급 처치", "최대 체력 증가 및 회복"),
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

    public void AddKill()
    {
        if (gameOver)
            return;

        killCount++;
        if (killCount > 0 && killCount % 50 == 0 && player != null)
            FloatingText.Spawn(player.position + Vector3.up * 1.3f, killCount + " 처치", new Color(1f, 0.75f, 0.28f, 1f));

        RefreshHud();
    }

    public void ApplyReward(RewardChoice reward)
    {
        switch (reward.type)
        {
            case RewardType.Damage:
                AddPassiveLevel(reward.type);
                autoAimWeapon.damage += 1;
                break;
            case RewardType.FireRate:
                AddPassiveLevel(reward.type);
                autoAimWeapon.fireCooldown = Mathf.Max(0.08f, autoAimWeapon.fireCooldown * 0.84f);
                break;
            case RewardType.Shotgun:
                autoAimWeapon.UnlockShotgun();
                break;
            case RewardType.MoveSpeed:
                AddPassiveLevel(reward.type);
                playerController.moveSpeed += 0.28f;
                break;
            case RewardType.MaxHealth:
                AddPassiveLevel(reward.type);
                playerHealth.SetMaxHealth(playerHealth.maxHealth + 1, true);
                break;
            case RewardType.ProjectileSpeed:
                AddPassiveLevel(reward.type);
                autoAimWeapon.projectileSpeed += 1.2f;
                break;
            case RewardType.BookOrbit:
                if (bookOrbitWeapon != null)
                    bookOrbitWeapon.Upgrade();
                break;
            case RewardType.CoffeeSpill:
                if (coffeeSpillWeapon != null)
                    coffeeSpillWeapon.Upgrade();
                break;
            case RewardType.Grenade:
                if (grenadeWeapon != null)
                    grenadeWeapon.Upgrade();
                break;
            case RewardType.IncendiaryGrenade:
                UnlockIncendiaryGrenade();
                break;
        }

        RefreshHud();
    }

    private IReadOnlyList<RewardChoice> GetRandomRewards(int count)
    {
        var copy = new List<RewardChoice>();
        var fusionChoice = GetFusionChoice();
        if (fusionChoice != null)
            copy.Add(fusionChoice);

        foreach (var reward in rewardPool)
        {
            if (CanChooseReward(reward.type))
                copy.Add(CreateLeveledChoice(reward));
        }

        var result = new List<RewardChoice>();
        while (result.Count < count && copy.Count > 0)
        {
            var index = Random.Range(0, copy.Count);
            result.Add(copy[index]);
            copy.RemoveAt(index);
        }

        return result;
    }

    private RewardChoice GetFusionChoice()
    {
        if (incendiaryGrenadeUnlocked)
            return null;

        if (grenadeWeapon == null || coffeeSpillWeapon == null)
            return null;

        if (!grenadeWeapon.IsMaxLevel() || !coffeeSpillWeapon.IsMaxLevel())
            return null;

        return new RewardChoice(RewardType.IncendiaryGrenade, "합성: 화염 수류탄", "수류탄 폭발 후 불길 장판 생성");
    }

    private RewardChoice CreateLeveledChoice(RewardChoice reward)
    {
        var currentLevel = GetRewardLevel(reward.type);
        var nextLevel = Mathf.Min(maxItemLevel, currentLevel + 1);
        var title = reward.title + "  Lv." + currentLevel + " -> " + nextLevel;
        var description = reward.description;

        if (nextLevel >= maxItemLevel)
            description += "\n10레벨 달성 시 합성 조건에 사용 가능";

        return new RewardChoice(reward.type, title, description);
    }

    private bool CanChooseReward(RewardType type)
    {
        return GetRewardLevel(type) < maxItemLevel;
    }

    private int GetRewardLevel(RewardType type)
    {
        switch (type)
        {
            case RewardType.BookOrbit:
                return bookOrbitWeapon != null ? bookOrbitWeapon.level : 0;
            case RewardType.CoffeeSpill:
                return coffeeSpillWeapon != null ? coffeeSpillWeapon.level : 0;
            case RewardType.Grenade:
                return grenadeWeapon != null ? grenadeWeapon.level : 0;
            default:
                return passiveLevels.TryGetValue(type, out var value) ? value : 0;
        }
    }

    private void AddPassiveLevel(RewardType type)
    {
        var levelValue = GetRewardLevel(type);
        passiveLevels[type] = Mathf.Min(maxItemLevel, levelValue + 1);
    }

    private void UnlockIncendiaryGrenade()
    {
        incendiaryGrenadeUnlocked = true;

        if (grenadeWeapon != null)
            grenadeWeapon.Evolve();

        if (coffeeSpillWeapon != null)
            coffeeSpillWeapon.Evolve();

        FloatingText.Spawn(player.position + Vector3.up * 1.1f, "합성 완료", new Color(1f, 0.55f, 0.12f, 1f));
        CameraShake.Shake(0.18f, 0.18f);
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
            levelText.text = "LV " + level + "  XP " + experience + " / " + experienceToNextLevel + "  KILL " + killCount;

        if (timerText != null)
            timerText.text = FormatTime(elapsed);

        if (statusText != null && !gameOver && gameStarted)
            statusText.text = "WASD 이동 | 자동 사격 | XP 획득 | 장비 레벨업 | 10레벨 합성";
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
