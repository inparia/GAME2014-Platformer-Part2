using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaxMeleeCheck : MonoBehaviour
{

    public bool ableToAttack;
    public GameObject enemyObj;
    // Start is called before the first frame update
    void Start()
    {
        ableToAttack = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Enemy"))
        {
            ableToAttack = true;
            enemyObj = collision.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            ableToAttack = false;
            enemyObj = null;
        }
    }
}
