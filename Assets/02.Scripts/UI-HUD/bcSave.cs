﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class bcSave : ButtonClicked
{
    [SerializeField] private PlayerInformation playerInformation;
    public override void ButtonEvent(PointerEventData eventData)
    {
        playerInformation.SavePlayer();
    }
}