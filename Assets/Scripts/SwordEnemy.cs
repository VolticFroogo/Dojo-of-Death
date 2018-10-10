using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordEnemy : MonoBehaviour, IPooledObject {

    public float speed = 2f;
    public float attackSpeedModifier = 3f;
    public float distanceToAttack = 1.5f;
    public float timeToAttack = 1f;
    public float attackTime = 0.5f;
    public float stunTime = 0.5f;
    public float attackCooldown = 3f;

    [HideInInspector]
    public bool deadly = false;

    private bool inAttack = false;
    private bool setAttackPos = false;
    private Vector3 attackPos;
    private float startAttack;
    private float endAttack;
    private float endStun;
    private float nextAttack;

    private Rigidbody2D rb;

    private GameMaster gm;
    private Player player;

    // Use this for initialization on activation
    public void OnObjectSpawn()
    {
        gm = GameMaster.Instance;   // Set the gamemaster.
        player = gm.player;         // Set the player.
        rb = gameObject.GetComponent<Rigidbody2D>();
    }
	
	// Update is called once per frame
	void Update () {
        if (player == null)
        {   // We don't have the player.
            player = gm.player; // Get player from the game master.

            if (player == null)
                return; // The player is still dead, end logic.
        }

        if (inAttack && Time.time >= endStun)
        {   // Reset all attack variables, we aren't attacking any more.
            inAttack = false; 
            deadly = false;
            setAttackPos = false;
        }

        if (!inAttack && Time.time >= nextAttack && Vector2.Distance(player.transform.position, transform.position) <= distanceToAttack)
        {   // Start attacking, we are in range and don't have an attack cooldown.
            inAttack = true;    // Is in attack state, use attack movement from now on.
            startAttack = Time.time + timeToAttack;
            endAttack = startAttack + attackTime;   // Set all timers for the attack.
            endStun = endAttack + stunTime;
            nextAttack = Time.time + attackCooldown;
        }
    }

    // FixedUpdate is for all of our physics.
    void FixedUpdate()
    {
        if (player != null)
        {   // The player exists.
            if (!inAttack)
            {   // We aren't close enough or have an attack cooldown; chase the player.
                var direction = player.transform.position - transform.position;
                rb.AddRelativeForce(direction.normalized * (speed), ForceMode2D.Force); // Head directly towards the player's position.
                Debug.DrawLine(player.transform.position, transform.position, Color.cyan); // Draw a moving line to the player for debugging.
            }
            else
            {   // We are currently in an attack; do attack logic.
                if (Time.time >= startAttack && Time.time <= endAttack)
                {
                    if (!deadly)
                        deadly = true;

                    if (!setAttackPos)
                    {   // We can swing, set the direction we want to swing in.
                        setAttackPos = true; // Make sure we don't set the swing again, it needs to be a straight line.
                        attackPos = player.transform.position; // Head directly for the player.
                        attackPos.x += (attackPos.x - transform.position.x) * 10; // Increase the range of our attack incase the player is too close.
                        attackPos.y += (attackPos.y - transform.position.y) * 10; // ^^
                    }

                    var direction = attackPos - transform.position;
                    rb.AddRelativeForce(direction.normalized * (speed * attackSpeedModifier), ForceMode2D.Force); // Head towards our attack position with the attack speed modifier.
                    Debug.DrawLine(transform.position, attackPos, Color.blue); // Draw an attack line to our attack position for debugging.
                }
                else if (deadly)
                    deadly = false; // Currently stunned or charging; don't kill.
            }
        }
    }
}
