using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Fireball : MonoBehaviour
{
    private Vector3 shootDir;

    public void Setup(Vector3 shootDir)
    {
        this.shootDir = shootDir;
        transform.eulerAngles = new Vector3(0, 0, GetAngleFromVectorFloat(shootDir));
    }

    public static float GetAngleFromVectorFloat(Vector3 dir)
    {
        dir = dir.normalized;
        float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        if (n < 0) n += 360;

        return n;
    }

    private void Update()
    {
        float speed = 10f;
        transform.position += shootDir * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerCombat player = collision.GetComponent<PlayerCombat>();
        Tilemap level = collision.GetComponent<Tilemap>();
        if (player != null)
        {
            // Hit Player
            player.TakeDamage(10);
            Destroy(gameObject);

        }

        if (level != null)
        {
            Destroy(gameObject);
        }
    }

}
