using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Cooldown
{
    [SerializeField] private float cooldownTime;
    private float _nextActiveTime;

    public float CooldownTime => cooldownTime;
    public bool IsCoolingDown => Time.time < _nextActiveTime;
    public float NextActiveTime => _nextActiveTime;
    public float StartCooldown() => _nextActiveTime = Time.time + cooldownTime;

}
