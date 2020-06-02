﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTranslate : ICharacterTranslate
{
    public Transform Character { get; set; }
    public Rigidbody CharacterRigidbody { get; set; }
    public Vector3 DesiredPosition { get; set; }
    public float Speed { get; set; } = 1.5f;
    public Vector3 MoveDirection { get; set; }
    public IUnityServiceManager UnityService { get; set; } = UnityServiceManager.Instance;

    private bool isRunning = false;
    public bool IsRunning
    {
        get
        {
            return isRunning;
        }
        set
        {
            if (value)
            {
                Speed = 4.5f;
            }
            else
            {
                Speed = 1.5f;
            }
        }
    }

    private bool isSitting = false;
    public bool IsSitting
    {
        get
        {
            return isSitting;
        }
        set
        {
            if (value)
            {
                Speed = 1.5f;
            }

        }
    }

    public PlayerTranslate(Transform character)
    {
        this.Character = character;
        this.UnityService = UnityServiceManager.Instance;
        if (character != null)
            CharacterRigidbody = character.gameObject.GetComponent<Rigidbody>();
    } 

    public void TranslateCharacter(Vector3 moveDir)
    {
        if (CharacterRigidbody != null)
        {
            MoveDirection = moveDir;
            DesiredPosition = Character.position + moveDir.normalized * Speed;
            CharacterRigidbody.MovePosition(DesiredPosition * Speed * UnityService.DeltaTime);
        }
    }

}
