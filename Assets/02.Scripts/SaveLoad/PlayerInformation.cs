﻿using UnityEditor;
using UnityEngine;

public class PlayerInformation : MonoBehaviour
{

    private int score = 0;
    [field: SerializeField] public int Score
    {
        get { return score; }
        set { if ((value) <= 0) { score = 0; } else score = value; } // Do not allow score to go below 0
    }
    [field: SerializeField] public int Money { get; set; }

    private float health = 100f;
    [field: SerializeField] public float Health 
    { 
        get { return health; }
        set { if ((health += value) <= 0) { health = 0; } else health = value; } // Do not allow health to go below 0
    }

    public Inventory PlayerInventory = new Inventory();

    public PlayerStateController player;

    public Transform transform;

    public GameObject[] guns;

    public int equipped;

    public int Ammo =100;

    void Start()
    {
        transform = GetComponent<Transform>();
        if (player.HasWeapon)
        {
            player.UnEquipWeapon();
        }

        for (int i = 0; i < guns.Length; i++)
        {
            var gun = guns[i].GetComponent<ImWeapon>();
            gun.InstanceID = i;
            gun.StackAmount = 1;

            PlayerInventory.Add(gun);
        }
        equipped = PlayerInventory.Primary.EntityID;
    }

    void Update()
    {


        if(equipped != PlayerInventory.selected.EntityID)
        {
            player.UnEquipWeapon();
            for (int i = 0; i < guns.Length; i++)
            {
                if (PlayerInventory.selected.EntityID == guns[i].GetComponent<ImWeapon>().EntityID)
                {
                    player.EquipWeapon(guns[i]);
                    player.IsHoldingRifle = true;
                    equipped = guns[i].GetComponent<ImWeapon>().EntityID;
                }
            }
        }
    }


    public bool SavePlayer()
    {
        return SaveSystem.SavePlayer(this);
    }

    public bool LoadPlayer()
    {
        PlayerSaveData loadedData = SaveSystem.LoadPlayer();

        if (loadedData == null) return false;

        Vector3 position;
        position.x = loadedData.position[0];
        position.y = loadedData.position[1];
        position.z = loadedData.position[2];

        this.transform.position = position;
        this.Health = loadedData.Health;
        this.Score = loadedData.Score;
        this.Money = loadedData.Money;

        Debug.Log($"Player was LOADED. Health:{loadedData.Health} Money:{loadedData.Money} " +
        $"Score:{loadedData.Score} Position: x={loadedData.position[0]} y={loadedData.position[1]} y={loadedData.position[2]}");

        return true;

    }
}
