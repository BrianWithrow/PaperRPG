﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    public int damage;

    protected BattleCameraController cameraController;

    public virtual void Start()
    {
        cameraController = FindObjectOfType<BattleCameraController>();
    }

    public virtual IEnumerator Behavior(GiuseppeBattleScript player, Transform enemy)
    {
        return null;
    }
}
