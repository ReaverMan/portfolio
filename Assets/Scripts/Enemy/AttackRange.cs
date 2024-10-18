using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackRange : MonoBehaviour
{
    EnemyBace enemyBace;

    private void Awake()
    {
        enemyBace = GetComponentInParent<EnemyBace>();
    }


}
