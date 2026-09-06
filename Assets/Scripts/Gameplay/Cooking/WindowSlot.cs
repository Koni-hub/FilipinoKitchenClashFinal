using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowSlot : MonoBehaviour
{
    [Header("Slot Info")]
    public int slotIndex;
    public bool isOccupied = false;

    [Header("Cooldown")]
    public float cooldownTime = 30f;
    public float cooldownTimer = 0f;
    public bool isOnCooldown = false;

    [Header("References")]
    public Customer currentCustomer;

    private void Update()
    {
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;

            if (cooldownTimer <= 0f)
            {
                isOnCooldown = false;
                cooldownTimer = 0f;
            }
        }
    }

    public Vector3 GetSpawnPosition()
    {
        return transform.position;
    }

    public void AssignCustomer(Customer customer)
    {
        currentCustomer = customer;
        isOccupied = true;
        isOnCooldown = false;
        cooldownTimer = 0f;
    }

    public void ClearSlot()
    {
        currentCustomer = null;
        isOccupied = false;
        StartCooldown();
    }

    public void StartCooldown()
    {
        isOnCooldown = true;
        cooldownTimer = cooldownTime;
    }

    public bool CanSpawn()
    {
        return !isOccupied && !isOnCooldown;
    }

    public bool HasOrder(string dishName)
    {
        if (currentCustomer == null) return false;
        return currentCustomer.dishOrder == dishName;
    }
}
