using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnNoOverlap : MonoBehaviour
{
    [SerializeField] GameObject enemy;
    [SerializeField] int count;
    public void Start()
    {
        //count = Random.Range(2, 6);
        float enemyRadius = enemy.GetComponent<Collider2D>().bounds.extents.x;
        for (int i = 0; i < count; i++)
        {
            float x = Random.Range(-1.0f, 1.0f);
            float y = Random.Range(-1.0f, 1.0f);
            Vector2 spawnPoint = new Vector2(x, y);
            //Assuming you are 2D
            Collider2D CollisionWithEnemy = Physics2D.OverlapCircle(spawnPoint, enemyRadius, LayerMask.GetMask("EnemyLayer"));
            //If the Collision is empty then, we can instantiate
            if (CollisionWithEnemy == false)
                Instantiate(enemy, new Vector3(x, y, 0), Quaternion.identity);
        }
    }
}
