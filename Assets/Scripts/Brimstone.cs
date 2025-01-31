using UnityEngine;

public class Brimstone : VigObject
{
    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    public override uint OnCollision(HitDetection hit)
    {
        VigObject self = hit.self;
        Color32 param2;
        uint param;
        if (!(self.GetType() == typeof(OilSlick2)))
        {
            if (hit.object2.type == 3)
            {
                return 0u;
            }
            if (self.type == 8 && self.GetType() == typeof(Bullet))
            {
                return 0u;
            }
            if (self.type == 2)
            {
                Vehicle vehicle = (Vehicle)self;
                vehicle.FUN_3A064(-200, vTransform.position, param3: true);
                param = (uint)GameManager.instance.FUN_1DD9C();
                GameManager.instance.FUN_1E628((int)param, GameManager.instance.DAT_C2C, 63, vTransform.position);
                if (LevelManager.instance.FUN_39AF8(vehicle))
                {
                    LevelManager.instance.FUN_4DE54(vTransform.position, 35);
                    param2 = new Color32(128, 128, 0, 8);
                    goto IL_013e;
                }
                goto IL_0155;
            }
        }
        LevelManager.instance.FUN_4DE54(vTransform.position, 35);
        param = (uint)GameManager.instance.FUN_1DD9C();
        GameManager.instance.FUN_1E580((int)param, GameManager.instance.DAT_C2C, 63, vTransform.position);
        param2 = new Color32(128, 0, 0, 8);
        goto IL_013e;
    IL_0155:
        GameManager.instance.FUN_309A0(this);
        return uint.MaxValue;
    IL_013e:
        UIManager.instance.FUN_4E414(vTransform.position, param2);
        goto IL_0155;
    }

    public override uint UpdateW(int arg1, int arg2)
    {
        if (arg1 == 0)
        {
            vTransform.position.x += physics1.Z;
            vTransform.position.y += physics1.W;
            int dAT_DA = GameManager.instance.DAT_DA0;
            vTransform.position.z += physics2.X;
            physics1.W += 56;
            int param;
            if (dAT_DA <= vTransform.position.z || vTransform.position.y <= GameManager.instance.DAT_DB0)
            {
                dAT_DA = GameManager.instance.terrain.FUN_1B750((uint)vTransform.position.x, (uint)vTransform.position.z);
                if (vTransform.position.y <= dAT_DA)
                {
                    if (((GameManager.instance.DAT_28 - DAT_19) & 3) == 0)
                    {
                        LevelManager.instance.FUN_4DE54(vTransform.position, 22);
                        return 0u;
                    }
                    return 0u;
                }
                Particle2 particle = LevelManager.instance.FUN_4E128(vTransform.position, 83, 40);
                Utilities.ParentChildren(particle, particle);
                particle.FUN_2D114(particle.screen, ref particle.vTransform);
                param = GameManager.instance.FUN_1DD9C();
                GameManager.instance.FUN_1E628(param, GameManager.instance.DAT_C2C, 63, particle.vTransform.position);
                short m = physics2.M3;
                physics2.M3 = (short)(m - 1);
                if (m != 1)
                {
                    TileData tileByPosition = GameManager.instance.terrain.GetTileByPosition((uint)vTransform.position.x, (uint)vTransform.position.z);
                    if (tileByPosition.DAT_10[3] == 0 || tileByPosition.DAT_10[3] == 7)
                    {
                        Vector3Int n = GameManager.instance.terrain.FUN_1B998((uint)vTransform.position.x, (uint)vTransform.position.z);
                        n = Utilities.VectorNormal(n);
                        vTransform.position.y = dAT_DA;
                        dAT_DA = n.x * physics1.Z + n.y * physics1.W + n.z * physics2.X;
                        if (dAT_DA < 0)
                        {
                            dAT_DA += 2047;
                        }
                        dAT_DA >>= 11;
                        int num = dAT_DA * n.x;
                        if (num < 0)
                        {
                            num += 4095;
                        }
                        physics1.Z = (physics1.Z - (num >> 12)) / 2;
                        num = dAT_DA * n.y;
                        if (num < 0)
                        {
                            num += 4095;
                        }
                        physics1.W = (physics1.W - (num >> 12)) / 2;
                        dAT_DA *= n.z;
                        if (dAT_DA < 0)
                        {
                            dAT_DA += 4095;
                        }
                        physics2.X = (physics2.X - (dAT_DA >> 12)) / 2;
                        return 0u;
                    }
                }
                LevelManager.instance.FUN_309C8(this, 1);
                return uint.MaxValue;
            }
            LevelManager.instance.FUN_4DE54(vTransform.position, 138);
            param = GameManager.instance.FUN_1DD9C();
            GameManager.instance.FUN_1E14C(param, GameManager.instance.DAT_C2C, 70);
            GameManager.instance.FUN_309A0(this);
            return uint.MaxValue;
        }
        return 0u;
    }
}
