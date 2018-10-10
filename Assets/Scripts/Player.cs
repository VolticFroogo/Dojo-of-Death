using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [System.Serializable]
    public class Stats
    {
        public float Speed = 5f;
        public float AttackTime = 0.3f;
        public float AttackStun = 0.4f;
        public float AttackSpeedModifier = 3f;
    }

    public Stats stats = new Stats();

    private bool inAttack = false;
    private bool deadly = false;
    private float endAttack;
    private float endStun;
    private Vector3 targetPos;

    private Rigidbody2D rb;

    private GameMaster gm;

    // Use this for initialization
    void Start()
    {
        gm = GameMaster.Instance;
        rb = gameObject.GetComponent<Rigidbody2D>();

        // Calculate mouse position
        targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        targetPos.z = transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        if (inAttack && Time.time >= endStun)
        {
            inAttack = false;
            deadly = false;
        }

        if (Input.GetMouseButtonDown(0) && Time.time >= endStun)
        {
            inAttack = true;
            endAttack = Time.time + stats.AttackTime;
            endStun = endAttack + stats.AttackStun;
        }
    }

    // FixedUpdate is for all of our physics.
    void FixedUpdate()
    {
        if (rb.drag == 50)
            rb.drag = 20;

        if (!inAttack)
        {
            // Calculate mouse position
            targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetPos.z = transform.position.z;

            if (Vector3.Distance(targetPos, transform.position) > 0.2)
            {
                var direction = targetPos - transform.position;
                rb.AddRelativeForce(direction.normalized * stats.Speed, ForceMode2D.Force);
                Debug.DrawLine(targetPos, transform.position, Color.green);
            } else
            {
                rb.drag = 50;
            }
        }
        else
        {
            if (Time.time <= endAttack)
            {
                if (!deadly)
                    deadly = true;

                targetPos.x += targetPos.x - transform.position.x;
                targetPos.y += targetPos.y - transform.position.y;
                var direction = targetPos - transform.position;
                rb.AddRelativeForce(direction.normalized * (stats.Speed * stats.AttackSpeedModifier), ForceMode2D.Force);
                Debug.DrawLine(targetPos, transform.position, Color.red);
            }
            else if (deadly)
                deadly = false;
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.name == "SwordEnemy(Clone)")
        {   // We collided with a sword enemy.
            SwordEnemy swordEnemy = col.gameObject.GetComponent<SwordEnemy>();

            if (deadly)
            {   // We are currently dashing, kill enemy.
                gm.KillSwordEnemy(swordEnemy);
            }
            else if (swordEnemy.deadly)
            {   // The enemy is dashing and we are vulnerable, die.
                gm.KillPlayer(this);   
            }
        }
        else if (col.gameObject.name == "RangedEnemy(Clone)")
        {   // We collided with a ranged enemy.
            RangedEnemy rangedEnemy = col.gameObject.GetComponent<RangedEnemy>();

            if (deadly)
            {   // We are currently dashing, kill enemy.
                gm.KillRangedEnemy(rangedEnemy);
            }
        }
        else if (col.gameObject.name == "Arrow(Clone)")
        {   // We collided with an arrow.
            if (deadly)
            {   // We are currently dashing, destroy arrow.
                Destroy(col.gameObject);
            }
            else
            {   // We are currently vulnerable, die.
                gm.KillPlayer(this);
                Destroy(col.gameObject); // Destroy arrow anyway, it is inside us after all.
            }
        }
    }
}
