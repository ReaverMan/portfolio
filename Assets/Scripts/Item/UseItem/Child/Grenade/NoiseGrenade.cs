using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseGrenade : GrenadeBase
{
    [Tooltip("���� ũ��")]
    public float noiseVolume = 10.0f;

    protected override void Explode()
    {
        Factory.Instance.GetNoise(noiseVolume, transform);
        Destroy(this.gameObject, 0.1f);
    }
}
