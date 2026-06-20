using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SurvivorsGameManager : MonoBehaviour
{
    private const string MainMenuSceneName = "MainMenu";
    private const string MasterVolumeKey = "survivors_master_volume";

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
    public Text experienceBarText;
    public Image experienceBarFill;
    public RectTransform experienceBarFillRect;
    public Text gameOverTitleText;
    public Text gameOverInfoText;

    public int level = 1;
    public int experience;
    public int experienceToNextLevel = 5;
    public int maxItemLevel = 10;
    public int killCount;
    public int shotgunUnlockKillRequirement = 50;

    private float elapsed;
    private bool gameOver;
    private bool gameStarted;
    private bool incendiaryGrenadeUnlocked;
    private bool selectedCharacterApplied;
    private SurvivorsSaveData saveData;
    private string selectedCharacterId = SurvivorsCharacterIds.Pistol;
    private readonly Dictionary<RewardType, int> passiveLevels = new();

    private readonly List<RewardChoice> rewardPool = new()
    {
        new RewardChoice(RewardType.Damage, "화력 강화", "기본 탄환 피해량 증가"),
        new RewardChoice(RewardType.FireRate, "빠른 사격", "기본 탄환 발사 속도 증가"),
        new RewardChoice(RewardType.BookOrbit, "방어 궤도", "주변을 도는 궤도 무기 강화"),
        new RewardChoice(RewardType.CoffeeSpill, "화염 장판", "불타는 장판 범위 피해 강화"),
        new RewardChoice(RewardType.Grenade, "수류탄", "범위 폭발 무기 강화"),
        new RewardChoice(RewardType.MoveSpeed, "기동 훈련", "이동 속도 증가"),
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
        if (!GameLaunchConfig.StartImmediately)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(MainMenuSceneName);
            return;
        }

        GameLaunchConfig.StartImmediately = false;

        saveData = SurvivorsSaveSystem.Load();
        selectedCharacterId = saveData.selectedCharacterId;
        if (string.IsNullOrEmpty(selectedCharacterId))
            selectedCharacterId = SurvivorsCharacterIds.Pistol;
        if (selectedCharacterId == SurvivorsCharacterIds.Shotgun && !saveData.shotgunSurvivorUnlocked)
            selectedCharacterId = SurvivorsCharacterIds.Pistol;

        if (playerHealth != null)
        {
            playerHealth.Died += OnPlayerDied;
            playerHealth.Changed += _ => RefreshHud();
        }

        if (startButton != null)
            startButton.onClick.AddListener(StartGame);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (titlePanel != null)
            titlePanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        RefreshHud();
        StartGame();
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
        ApplySelectedCharacter();
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

        return new RewardChoice(RewardType.IncendiaryGrenade, "합성: 화염 수류탄", "수류탄 폭발 지점에 불타는 장판 생성");
    }

    private RewardChoice CreateLeveledChoice(RewardChoice reward)
    {
        var currentLevel = GetRewardLevel(reward.type);
        var nextLevel = Mathf.Min(maxItemLevel, currentLevel + 1);
        var title = reward.title + "  Lv." + currentLevel + " -> " + nextLevel;
        var description = reward.description;

        if (nextLevel >= maxItemLevel)
            description += "\n10레벨 달성 시 합성 조건으로 사용 가능";

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

        if (player != null)
            FloatingText.Spawn(player.position + Vector3.up * 1.1f, "합성 완료", new Color(1f, 0.55f, 0.12f, 1f));
        CameraShake.Shake(0.18f, 0.18f);
    }

    private void OnPlayerDied(Health health)
    {
        gameOver = true;
        Time.timeScale = 0f;
        var shotgunUnlockedNow = TryUnlockShotgunSurvivor();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (gameOverTitleText != null)
            gameOverTitleText.text = "YOU DIED";

        if (gameOverInfoText != null)
        {
            var unlockLine = shotgunUnlockedNow
                ? "\n신규 캐릭터 해금: 샷건 생존자"
                : "\n샷건 생존자 해금 조건: 한 판에 " + shotgunUnlockKillRequirement + "킬";
            gameOverInfoText.text = "생존 시간 " + FormatTime(elapsed) + " / 도달 레벨 " + level + " / 처치 " + killCount
                + unlockLine
                + "\nR 키 또는 다시하기 버튼으로 재도전";
        }

        if (statusText != null)
            statusText.text = "사망 - R 키로 다시하기";
    }

    private void ApplySelectedCharacter()
    {
        if (selectedCharacterApplied)
            return;

        selectedCharacterApplied = true;

        var spriteRenderer = player != null ? player.GetComponent<SpriteRenderer>() : null;
        if (selectedCharacterId == SurvivorsCharacterIds.Shotgun && saveData.shotgunSurvivorUnlocked)
        {
            if (autoAimWeapon != null)
            {
                autoAimWeapon.shotgunUnlocked = true;
                autoAimWeapon.shotgunPellets = Mathf.Max(autoAimWeapon.shotgunPellets, 5);
                autoAimWeapon.shotgunCooldown = Mathf.Min(autoAimWeapon.shotgunCooldown, 1.35f);
                autoAimWeapon.damage = Mathf.Max(autoAimWeapon.damage, 1);
            }

            if (playerController != null)
                playerController.moveSpeed = Mathf.Max(0.8f, playerController.moveSpeed - 0.12f);

            if (spriteRenderer != null)
                spriteRenderer.color = new Color(0.72f, 0.88f, 1f, 1f);

            return;
        }

        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;
    }

    private bool TryUnlockShotgunSurvivor()
    {
        if (saveData == null)
            saveData = SurvivorsSaveSystem.Load();

        saveData.bestKillCount = Mathf.Max(saveData.bestKillCount, killCount);
        var unlockedNow = !saveData.shotgunSurvivorUnlocked && killCount >= shotgunUnlockKillRequirement;
        if (unlockedNow)
            saveData.shotgunSurvivorUnlocked = true;

        SurvivorsSaveSystem.Save(saveData);
        return unlockedNow;
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;
        GameLaunchConfig.StartImmediately = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void RefreshHud()
    {
        if (levelText != null)
            levelText.text = "LV " + level + "  KILL " + killCount;

        var experienceRatio = experienceToNextLevel > 0 ? Mathf.Clamp01((float)experience / experienceToNextLevel) : 0f;
        if (experienceBarFill != null)
            experienceBarFill.fillAmount = experienceRatio;

        if (experienceBarFillRect != null)
            experienceBarFillRect.anchorMax = new Vector2(experienceRatio, 1f);

        if (experienceBarText != null)
            experienceBarText.text = "XP " + experience + " / " + experienceToNextLevel;

        if (timerText != null)
            timerText.text = FormatTime(elapsed);

        if (statusText != null && !gameOver && gameStarted)
            statusText.text = "WASD 이동 | 자동 사격 | XP 획득 | 레벨업 보상 | 10레벨 합성";
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
