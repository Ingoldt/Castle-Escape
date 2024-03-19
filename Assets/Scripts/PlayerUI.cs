using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public Slider healthSlider;
    public Slider staminaSlider;
    public Slider manaSlider;
    public GameObject AttackUI;
    public GameObject SpeedUI;

    public void SetMaxHealth(float maxHealth)
    {
        healthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;
    }
    public void SetMaxStamina(float maxStamina)
    {
        staminaSlider.maxValue = maxStamina;
        staminaSlider.value = maxStamina;
    }

    public void SetMaxMana(float maxMana)
    {
        manaSlider.maxValue = maxMana;
        manaSlider.value = maxMana;
    }

    public void SetHealth(float health)
    {
        healthSlider.value = health;
    }

    public void SetStamina(float stamina)
    {
        staminaSlider.value = stamina;
    }

    public void SetMana(float mana)
    {
        manaSlider.value = mana;
    }

    public void SetAttackValue(int value)
    {
        TextMeshProUGUI text = AttackUI.GetComponentInChildren<TextMeshProUGUI>();
        text.text = value.ToString();
    }

    public void SetSpeedValue(float value)
    {
        TextMeshProUGUI text = SpeedUI.GetComponentInChildren<TextMeshProUGUI>();
        text.text = value.ToString("F1");
    }
}
