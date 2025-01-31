using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HealthManager : MonoBehaviour
{
    public Slider healthBar;
    private void Start()
    {
        healthBar = GetComponent<Slider>();
    }
    public void SetHealth(float hp)
    {
        healthBar.value = hp;
    }
}