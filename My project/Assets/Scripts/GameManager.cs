using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Transform player;
    public PlayerShooter playerShooter;
    public PlayerController playerController;
    public Health playerHealth;
    public RewardPanel rewardPanel;
    public Text statusText;
    public Text healthText;
    public EnemyController[] enemyPrefabs;
    public Transform[] spawnPoints;
    public int enemiesPerRoom = 4;

    private readonly List<EnemyController> aliveEnemies = new();
    private readonly List<RewardChoice> rewardPool = new()
    {
        new RewardChoice(RewardType.Damage, "Sharp Lead", "Damage +1"),
        new RewardChoice(RewardType.FireRate, "Sticky Note Combo", "Attack speed up"),
        new RewardChoice(RewardType.MoveSpeed, "Coffee Drop", "Move speed up"),
        new RewardChoice(RewardType.MaxHealth, "Eraser Shield", "Max health +1"),
        new RewardChoice(RewardType.ProjectileSpeed, "Rubber Band", "Projectile speed up")
    };

    private int roomIndex = 1;
    private bool runCompleted;

    private void Start()
    {
        if (playerHealth != null)
        {
            playerHealth.destroyOnDeath = false;
            playerHealth.Died += OnPlayerDied;
            playerHealth.Changed += _ => RefreshHud();
        }

        SpawnRoom();
        RefreshHud();
    }

    private void Update()
    {
        if (KeyboardRestartPressed())
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        if (runCompleted)
            return;

        aliveEnemies.RemoveAll(enemy => enemy == null);

        if (aliveEnemies.Count == 0 && Time.timeScale > 0f)
            CompleteRoom();
    }

    private bool KeyboardRestartPressed()
    {
        return UnityEngine.InputSystem.Keyboard.current != null
            && UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame;
    }

    private void SpawnRoom()
    {
        aliveEnemies.Clear();

        if (enemyPrefabs == null || enemyPrefabs.Length == 0 || spawnPoints == null || spawnPoints.Length == 0)
            return;

        var count = Mathf.Min(enemiesPerRoom + roomIndex - 1, spawnPoints.Length);
        for (var i = 0; i < count; i++)
        {
            var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            var spawnPoint = spawnPoints[i % spawnPoints.Length];
            var enemy = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
            aliveEnemies.Add(enemy);
        }

        if (statusText != null)
            statusText.text = "Room " + roomIndex + " - clear all desk hazards";
    }

    private void CompleteRoom()
    {
        if (roomIndex >= 3)
        {
            runCompleted = true;
            if (statusText != null)
                statusText.text = "Prototype clear! Press R to restart";
            return;
        }

        roomIndex++;
        if (rewardPanel != null)
            rewardPanel.Show(GetRandomRewards(3));
        else
            SpawnRoom();
    }

    public void ApplyReward(RewardChoice reward)
    {
        if (playerShooter != null)
            playerShooter.ApplyUpgrade(reward.type);

        switch (reward.type)
        {
            case RewardType.MoveSpeed:
                if (playerController != null)
                    playerController.moveSpeed += 0.35f;
                break;
            case RewardType.MaxHealth:
                if (playerHealth != null)
                    playerHealth.SetMaxHealth(playerHealth.maxHealth + 1, true);
                break;
        }

        SpawnRoom();
        RefreshHud();
    }

    private List<RewardChoice> GetRandomRewards(int count)
    {
        var copy = new List<RewardChoice>(rewardPool);
        var choices = new List<RewardChoice>();

        while (choices.Count < count && copy.Count > 0)
        {
            var index = Random.Range(0, copy.Count);
            choices.Add(copy[index]);
            copy.RemoveAt(index);
        }

        return choices;
    }

    private void OnPlayerDied(Health health)
    {
        Time.timeScale = 0f;
        if (statusText != null)
            statusText.text = "Desk overrun. Press R to restart";
    }

    private void RefreshHud()
    {
        if (healthText != null && playerHealth != null)
            healthText.text = "HP " + playerHealth.CurrentHealth + " / " + playerHealth.maxHealth;
    }
}
