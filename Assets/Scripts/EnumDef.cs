using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnumDef
{
    [Serializable]

    public enum LimbType
    {
        Body = 0,
        Arm = 1,
        Leg = 2,
    }

    public enum CatState
    {
        Still = 0,
        Walk = 1,
        Run = 2,
        Adjust = 3,
    }
}