using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerStats", menuName = "Scriptable Objects/Player Stats")]
public class PlayerScriptableObject : ScriptableObject
{
    public int health;
    public int damage;
    public float movementSpeed;
    public float attackSpeed;
    public float meleeRange;

    private bool _isInvincible = false;

    public bool IsInvincible
    {
        get => _isInvincible;
        private set
        {
            if (_isInvincible != value)
            {
                _isInvincible = value;
            }
        }
    }

    // Public method to modify the value
    public void SetInvincible(bool value)
    {
        IsInvincible = value;
    }
}

