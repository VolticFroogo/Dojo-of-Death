using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemy : MonoBehaviour, IPooledObject
{

    public Object projectilePrefab;
    public float speed = 40f;
    public float attackSpeedModifier = 3f;
    public float distanceToAttack = 1.5f;
    public float timeToShoot = 1f;
    public float attackTime = 0.5f;
    public float stunTime = 0.5f;
    public float projectileVelocity = 5f;

    [HideInInspector]
    public bool deadly = false;

    private bool inAttack = false;
    private bool shot = false;
    private float shoot;
    private float endAttack;

    private Rigidbody2D rb;

    private GameMaster gm;
    private Player player;

    // Use this for initialization on activation
    public void Start()
    {
        gm = GameMaster.Instance;   // Set the gamemaster.
        player = gm.player;         // Set the player.
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    public void OnObjectSpawn()
    {
        // TODO: Rename the func above to this ^^^^^
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {   // We don't have the player.
            player = gm.player; // Get player from the game master.

            if (player == null)
                return; // The player is still dead, end logic.
        }

        if (inAttack && Time.time >= endAttack)
        {   // Reset all attack variables, we aren't attacking any more.
            inAttack = false;
            shot = false;
        }

        if (!inAttack && Vector2.Distance(player.transform.position, transform.position) <= distanceToAttack)
        {   // Start attacking, are in range.
            inAttack = true; // Is in attack state, use attack logic from now on.
            shoot = Time.time + timeToShoot; // Set all timers for the attack.
            endAttack = shoot + attackTime; 
        }

        if (inAttack && !shot && Time.time >= shoot)
        {   // We are ready to shoot.
            shot = true;

            Vector3 difference = transform.position - player.transform.position;
            difference.Normalize();
            float rotZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;

            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.Euler(0f, 0f, rotZ + 90)) as GameObject;
            Rigidbody2D projectileRB = projectile.GetComponent<Rigidbody2D>();
            projectileRB.velocity = projectile.transform.up * projectileVelocity;
            Destroy(projectile, 5f);
        }
    }

    // FixedUpdate is for all of our physics.
    void FixedUpdate()
    {
        if (player != null && !inAttack)
        {   // The player exists and we aren't attacking.
            var direction = player.transform.position - transform.position;
            rb.AddRelativeForce(direction.normalized * (speed), ForceMode2D.Force); // Head directly towards the player's position.
            Debug.DrawLine(player.transform.position, transform.position, Color.cyan); // Draw a moving line to the player for debugging.
        }
    }
}
