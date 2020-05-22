﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IEGame.FiniteStateMachine;

[CreateAssetMenu(menuName = "PluggableScript/EnemyAction/AttackAction")]
public class AttackAction : Action
{
    public override void Act(IStateController controller)
    {
        var monsterController = controller as MonsterController;

        monsterController.Attack();
    }


}