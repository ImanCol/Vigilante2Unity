using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineDr : VigObject
{
    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    //FUN_47B5C
    public override uint UpdateW(int arg1, int arg2)
    {
        uint uVar7;

        switch (arg1)
        {
            case 0:
                FUN_42330(arg2);
                uVar7 = 0;
                break;
            default:
                uVar7 = 0;
                break;
        }

        return uVar7;
    }

    public override uint UpdateW(int arg1, VigObject arg2)
    {
        short sVar2;
        uint uVar4;
        long lVar5;
        int iVar6;
        uint uVar7;
        int iVar9;
        int iVar10;
        VigObject oVar10;
        Vector3Int local_18;
        Vector3Int local_8;

        switch (arg1)
        {
            case 1:
                maxHalfHealth = 6;
                goto default;
            default:
                uVar7 = 0;
                break;
            case 12:
                FUN_479DC(arg2, 182, typeof(Mine));
                maxHalfHealth--;
                uVar7 = 60;

                if (maxHalfHealth == 0)
                {
                    FUN_3A368();
                    uVar7 = 60;
                }

                break;
            case 13:
                uVar4 = GameManager.FUN_2AC5C();

                if ((uVar4 & 0x3ff) == 0 ||
                    ((uVar4 & 0xff) == 0 &&
                    (sbyte)LevelManager.instance.FUN_35778(arg2.vTransform.position.x, arg2.vTransform.position.z) == -128))
                    uVar7 = 1;
                else
                {
                    uVar7 = 0;

                    if ((uVar4 & 15) == 0)
                    {
                        oVar10 = ((Vehicle)arg2).target;
                        local_18 = Utilities.FUN_24304(arg2.vTransform, oVar10.vTransform.position);

                        if (local_18.z < 0 && -0x96000 < local_18.z)
                        {
                            lVar5 = Utilities.Ratan2(local_18.x, local_18.z);
                            iVar6 = (int)(lVar5 << 20) >> 20;

                            if (iVar6 < 0)
                                iVar6 = -iVar6;

                            if (0x6aa < iVar6)
                            {
                                Utilities.FUN_2A168(out local_8, oVar10.vTransform.position, arg2.vTransform.position);
                                iVar6 = oVar10.physics1.X;

                                if (iVar6 < 0)
                                    iVar6 += 127;

                                iVar9 = oVar10.physics1.Y;

                                if (iVar9 < 0)
                                    iVar9 += 127;

                                iVar10 = oVar10.physics1.Z;

                                if (iVar10 < 0)
                                    iVar10 += 127;

                                iVar10 = (iVar6 >> 7) * local_8.x + (iVar9 >> 7) * local_8.y +
                                         (iVar10 >> 7) * local_8.z;

                                if (iVar10 < 0)
                                    iVar10 += 4095;

                                uVar7 = (uint)(-local_18.z < (iVar10 >> 12) * 60 ? 1 : 0);
                            }
                        }
                    }
                }

                break;
        }

        return uVar7;
    }

    private VigObject FUN_479DC(VigObject param1, short param2, Type param3)
    {
        VigObject puVar1;
        int iVar2;
        ushort uVar3;
        uint uVar4;
        int iVar5;

        puVar1 = LevelManager.instance.FUN_42408(param1, this, (ushort)param2, param3, null);
        uVar4 = 0x20000080;

        if (DAT_64 != 0)
            uVar4 = 0x20000084;

        puVar1.flags = uVar4;
        uVar3 = 150;

        if (((Vehicle)param1).doubleDamage != 0)
            uVar3 = 300;

        puVar1.maxHalfHealth = uVar3;
        puVar1.FUN_305FC();
        puVar1.FUN_2D1DC();
        puVar1.physics2.M2 = 0;
        iVar5 = param1.physics1.X;

        if (iVar5 < 0)
            iVar5 += 127;

        iVar2 = puVar1.vTransform.rotation.V01;

        if (iVar2 < 0)
            iVar2 += 3;

        puVar1.physics1.Z = (iVar5 >> 7) - (iVar2 >> 2);
        iVar5 = param1.physics1.Y;

        if (iVar5 < 0)
            iVar5 += 127;

        iVar2 = puVar1.vTransform.rotation.V11;

        if (iVar2 < 0)
            iVar2 += 3;

        puVar1.physics1.W = (iVar5 >> 7) - (iVar2 >> 2);
        iVar5 = param1.physics1.Z;

        if (iVar5 < 0)
            iVar5 += 127;

        iVar2 = puVar1.vTransform.rotation.V21;

        if (iVar2 < 0)
            iVar2 += 3;

        puVar1.physics2.X = (iVar5 >> 7) - (iVar2 >> 2);
        param1.FUN_2B1FC(GameManager.DAT_A68, screen);
        //sound
        return puVar1;
    }
}
