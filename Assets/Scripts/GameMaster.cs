using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour {
    public Player player = null;
    private ObjectPooler op;

    public Object playerPrefab;
    public Transform playerSpawn;
    public Transform[] enemySpawns;

    public Text scoreText;
    public Text comboText;
    public Text killsText;

    #region Singleton

    public static GameMaster Instance;

    private void Awake()
    {
        Instance = this;
    }

    #endregion

    public int scorePerKill = 200;
    public int comboBonus = 100;
    public float comboTimer = 1f;
    public float playerSearchInterval = 0.5f;
    public int enemies = 5;
    public float minEnemyRespawnTimer = 0.5f;
    public float maxEnemyRespawnTimer = 2f;
    public float enemyRespawnDecrement = 0.1f;
    public int killsPerNewEnemy = 8;

    private float nextSpawn = 1.5f;
    public int currentEnemies = 0;

    public int kills = 0;
    public int score = 0;
    public int combo = 0;
    private float resetCombo;

    private float nextSearchForPlayer;

    // Use this for initialization
    void Start()
    {
        op = ObjectPooler.Instance; // Store the object pooler's singleton instance.

        UpdateHUD(); // Set the HUD.
    }
	
	// Update is called once per frame
	void Update () {
        if (player == null && Time.time >= nextSearchForPlayer)
        {   // We don't have a player and it's time to search (again).
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player"); // Get the player from the "Player" tag.

            if (player == null)
            {   // The player still doesn't exist; set next search time.
                nextSearchForPlayer = Time.time + playerSearchInterval;
            }
            else
            {   // The player exists; get the Player component and store it.
                player = playerObject.GetComponent<Player>();
            }
        }

        if (player != null)
        {   // If the player is exists.
            if (Time.time >= resetCombo)
            {   // If it's time to reset the combo.
                combo = 0;  // Reset the combo to 0.
                UpdateHUD(); // Update the HUD to reflect the new combo.
            }

            if (Time.time >= nextSpawn && currentEnemies < enemies)
            {   // It's time to spawn a new enemy and we have room to add more.
                nextSpawn = Time.time + Random.Range(minEnemyRespawnTimer, maxEnemyRespawnTimer); // Set the next spawn time to a random time between the min and max.

                currentEnemies++; // Add 1 to the current enemies alive.
                op.SpawnFromPool(RandomEnemy(), enemySpawns[Random.Range(0, enemySpawns.Length)].position, Quaternion.identity); // Spawn the enemy at a random spawn point.
            }
        }
    }

    private string RandomEnemy()
    {
        int random = Random.Range(1, 100); // Pick a random number for us to use.

        if (random <= 20)
        {   // 20% chance to spawn ranged enemy.
            return "RangedEnemy";
        }
        else
        {   // 80% chance to spawn sword enemy.
            return "SwordEnemy";
        }
    }

    public void KillPlayer(Player player)
    {
        Destroy(player.gameObject); // Destroy the player.
    }

    public void KillSwordEnemy(SwordEnemy enemy)
    {
        currentEnemies--; // Subtract 1 from the current enemies alive.
        enemy.gameObject.SetActive(false); // Set the enemy to inactive (destroy but using an efficient object pooler).
        op.RequeueObject("SwordEnemy", enemy.gameObject); // Requeue the enemy into the object pooler.
        kills++; // Increase the amount of kills by 1.
        combo++; // Increase the combo by 1.
        score += scorePerKill + (comboBonus * (combo - 1));
        resetCombo = Time.time + comboTimer; // Set the next time the combo should be reset if we don't get any kills.
        UpdateHUD(); // Update the HUD to reflect all of the new stats.

        if (kills % killsPerNewEnemy == 0 && kills <= 200)
        {   // We have reached a new kill milestone and haven't maxed out our players yet.
            maxEnemyRespawnTimer = Mathf.Clamp(maxEnemyRespawnTimer - enemyRespawnDecrement, minEnemyRespawnTimer, maxEnemyRespawnTimer); // Reduce the max timer by how much we should decrement then clamp it so it doesn't go below the minimum.

            enemies++; // Increase the amount of enemies we are allowed to have spawned.
        }
    }

    public void KillRangedEnemy(RangedEnemy enemy)
    {
        currentEnemies--; // Subtract 1 from the current enemies alive.
        enemy.gameObject.SetActive(false); // Set the enemy to inactive (destroy but using an efficient object pooler).
        op.RequeueObject("RangedEnemy", enemy.gameObject); // Requeue the enemy into the object pooler.
        kills++; // Increase the amount of kills by 1.
        combo++; // Increase the combo by 1.
        score += scorePerKill + (comboBonus * (combo - 1));
        resetCombo = Time.time + comboTimer; // Set the next time the combo should be reset if we don't get any kills.
        UpdateHUD(); // Update the HUD to reflect all of the new stats.

        if (kills % killsPerNewEnemy == 0 && kills <= 200)
        {   // We have reached a new kill milestone and haven't maxed out our players yet.
            maxEnemyRespawnTimer = Mathf.Clamp(maxEnemyRespawnTimer - enemyRespawnDecrement, minEnemyRespawnTimer, maxEnemyRespawnTimer); // Reduce the max timer by how much we should decrement then clamp it so it doesn't go below the minimum.

            enemies++; // Increase the amount of enemies we are allowed to have spawned.
        }
    }

    private void UpdateHUD()
    {   // Update all of the text on the HUD.
        scoreText.text = "Score: " + score.ToString();
        comboText.text = "Combo: " + combo.ToString();
        killsText.text = "Kills: " + kills.ToString();
    }
}
