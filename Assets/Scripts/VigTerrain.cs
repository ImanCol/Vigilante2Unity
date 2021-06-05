﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TileData
{
    public int uv1_x, uv1_y;
    public int uv2_x, uv2_y;
    public int uv3_x, uv3_y;
    public int uv4_x, uv4_y;

    public short[] unk2;

    public byte unk3; // 0x1C
    public byte unk4; // 0x1D
    public byte unk5; // 0x1E
    public byte unk1; // 0x1F
}

[Serializable]
public class Junction
{
    
}

public class Tile
{
    public Tile[] neighbours = new Tile[4];
    //0 - Up
    //1 - Down
    //2 - Left
    //3 - Right

    public int[] indices = new int[4];
    public float x, y;
}

public struct TerrainScreen
{
    public int ir0; //0x00
    public Vector3Int vert; //0x04
    public Color32 color; //0x0C
}

public struct ScreenPoly
{
    public int index;
    public Vector2Int v1, v2, v3;
    public Color32 clr1, clr2, clr3;
}

public class VigTerrain : MonoBehaviour
{
    public static VigTerrain instance;
    
    public int bitmapID;
    public List<TileData> tileData;
    public ushort[] vertices;
    public byte[] tiles;
    public ushort defaultVertex;
    public byte defaultTile;
    public ushort[] chunks;
    public int unk1, unk2, unk3, unk4;
    public int tileXZ;
    public int tileY;
    public int zoneCount;
    public float drawDistance;
    public Vector3Int[,] DAT_B9270 = new Vector3Int[2, 8];
    public short[,] DAT_B9318 = new short[2, 20];
    public Color32[] DAT_B9370 = new Color32[32];
    public Color32[] DAT_BA4F0 = new Color32[32];
    public VigTransform DAT_BDFF0;

    private Vector3[] terrainWorld = new Vector3[40];
    private Dictionary<int, List<int>> verticesDict;
    private Dictionary<int, Tile> tilesDict;
    private Mesh terrainMesh;
    private Mesh skyboxMesh;
    private List<Vector3> newVertices;
    private List<Vector2> newUVs;
    private List<Color32> newColors;
    private List<int> newTriangles;
    private uint in_t0, in_t1, in_t2, in_t3;
    private int in_t4;
    private int puVar14;
    private int puVar15;
    private int puVar16;
    private int puVar17;
    private int puVar18;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        terrainMesh = new Mesh();
        skyboxMesh = new Mesh();
        GetComponent<MeshFilter>().mesh = terrainMesh;
        GameObject.Find("SkyBox").GetComponent<MeshFilter>().mesh = skyboxMesh;
    }

    // Start is called before the first frame update
    void Start()
    {
        newVertices = new List<Vector3>();
        newColors = new List<Color32>();
        newUVs = new List<Vector2>();
        newTriangles = new List<int>();

        return;

        verticesDict = new Dictionary<int, List<int>>();
        tilesDict = new Dictionary<int, Tile>();

        for (int i = 0, k = 0; i < chunks.Length; i++)
        {
            if (chunks[i] == 0 || zoneCount <= chunks[i])
                continue;

            int zone = chunks[i];

            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 64; y++)
                {
                    int index1 = zone * 4096 + x * 64 + y;

                    int nextZone = x + 1 < 64 ? zone : chunks[i + 32];
                    int nextX = x + 1 < 64 ? x + 1 : 0;
                    int index2 = nextZone * 4096 + nextX * 64 + y;

                    nextZone = y + 1 < 64 ? zone : chunks[i + 1];
                    int nextY = y + 1 < 64 ? y + 1 : 0;
                    int index3 = nextZone * 4096 + x * 64 + nextY;

                    if (x + 1 >= 64 && y + 1 >= 64)
                    {
                        nextZone = chunks[i + 33];
                        nextX = 0;
                        nextY = 0;
                    }
                    else if (x + 1 >= 64)
                    {
                        nextZone = chunks[i + 32];
                        nextX = 0;
                        nextY = y + 1;
                    }
                    else if (y + 1 >= 64)
                    {
                        nextZone = chunks[i + 1];
                        nextX = x + 1;
                        nextY = 0;
                    }
                    else
                    {
                        nextZone = zone;
                        nextX = x + 1;
                        nextY = y + 1;
                    }
                    int index4 = nextZone * 4096 + nextX * 64 + nextY;

                    int tileIndex1 = k * 4096 + x * 64 + y;
                    int vertexIndex = (k * 4096 + x * 64 + y) * 4;

                    if (verticesDict.ContainsKey(index1))
                    {
                        verticesDict.Add(index1, new List<int>());
                        tilesDict.Add(index1, new Tile());
                    }
                    verticesDict[index1].Add(vertexIndex);

                    if (verticesDict.ContainsKey(index2))
                    {
                        verticesDict.Add(index2, new List<int>());
                        tilesDict.Add(index2, new Tile());
                    }
                    verticesDict[index2].Add(vertexIndex + 1);

                    if (verticesDict.ContainsKey(index3))
                    {
                        verticesDict.Add(index3, new List<int>());
                        tilesDict.Add(index3, new Tile());
                    }
                    verticesDict[index3].Add(vertexIndex + 2);

                    if (verticesDict.ContainsKey(index4))
                    {
                        verticesDict.Add(index4, new List<int>());
                        tilesDict.Add(index4, new Tile());
                    }
                    verticesDict[index4].Add(vertexIndex + 3);

                    tilesDict[index1].neighbours[0] = tilesDict[index3];
                    tilesDict[index1].neighbours[3] = tilesDict[index2];
                    tilesDict[index3].neighbours[1] = tilesDict[index1];
                    tilesDict[index2].neighbours[2] = tilesDict[index1];

                    tilesDict[index1].indices[0] = vertexIndex;
                    tilesDict[index1].indices[1] = vertexIndex + 1;
                    tilesDict[index1].indices[2] = vertexIndex + 2;
                    tilesDict[index1].indices[3] = vertexIndex + 3;

                    tilesDict[index1].x = x + i / 32 * 64;
                    tilesDict[index1].y = y + i % 32 * 64;
                }
            }

            k++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        return;

        for (int i = 0; i < GameManager.instance.players.Length; i++)
        {
            Vector3Int cameraPosition = GameManager.instance.players[i].vCamera.vTransform.position;
            Tile onTile = GetTileByPosition2((uint)cameraPosition.x, (uint)cameraPosition.y);
            List<Tile> closeTiles = BreadthFirstSearch(onTile, drawDistance);

        }
    }

    public void ClearTerrainData()
    {
        newVertices.Clear();
        newColors.Clear();
        newUVs.Clear();
        newTriangles.Clear();
        terrainMesh.Clear();
    }

    public void CreateTerrainMesh()
    {
        for (int i = 0; i < newVertices.Count; i++)
            newVertices[i] = new Vector3(newVertices[i].x, -newVertices[i].y, newVertices[i].z);
        terrainMesh.SetVertices(newVertices);
        terrainMesh.SetColors(newColors);
        terrainMesh.SetTriangles(newTriangles, 0);
    }
    
    public void FUN_31678()
    {
        GameManager.instance.FUN_1C134();
        FUN_1C910();
    }

    private bool InsideCircle(Tile center, Tile tile, float radius)
    {
        float dx = center.x - tile.x,
              dy = center.y - tile.y;
        float distance = dx * dx + dy * dy;
        return distance <= radius * radius;
    }

    private List<Tile> BreadthFirstSearch(Tile start, float radius)
    {
        var frontier = new Queue<Tile>();
        frontier.Enqueue(start);

        var reached = new List<Tile>();
        reached.Add(start);
        
        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();

            foreach(var next in current.neighbours)
            {
                if (!reached.Contains(next) && InsideCircle(start, next, radius))
                {
                    frontier.Enqueue(next);
                    reached.Add(next);
                }
            }
        }

        return reached;
    }

    public void SetNumberOfZones()
    {
        vertices = new ushort[zoneCount * 4096];
        tiles = new byte[zoneCount * 4096];

        for (int i = 0; i < 4096; i++)
        {
            vertices[i] = defaultVertex;
            tiles[i] = defaultTile;
        }
    }

    public void FUN_1C910()
    {
        short sVar1;
        uint uVar2;
        int iVar3;
        int iVar5;
        int iVar6;
        int iVar11;
        short sVar10;
        int iVar12;
        VigTransform local_48;
        VigTransform local_28;

        local_48 = GameManager.instance.DAT_F88;
        iVar5 = GameManager.instance.DAT_20;
        iVar6 = GameManager.instance.DAT_20;
        uVar2 = (ushort)Utilities.FUN_2A27C(local_48.rotation);
        local_48.rotation = Utilities.RotMatrixY(-(short)uVar2, local_48.rotation);
        sVar1 = Utilities.FUN_2A248(local_48.rotation);
        iVar3 = sVar1;

        if (iVar3 < 0)
            iVar3 = -iVar3;

        if (1024 < iVar3)
            sVar1 = (short)(2048 - sVar1);

        local_48.rotation = Utilities.RotMatrixX(-sVar1, local_48.rotation);
        uVar2 = (uVar2 & 0xfff) * 5;
        local_48.position.x = (int)(uVar2 - (uVar2 & 0xf000));
        local_48.position.y = sVar1 * -5;
        local_48.position.z = -0xd00;
        local_28 = Utilities.FUN_2A3EC(local_48);
        Utilities.FUN_246BC(local_28);
        Coprocessor.vector0.vx0 = (short)GameManager.DAT_63970[0].x;
        Coprocessor.vector0.vy0 = (short)GameManager.DAT_63970[0].y;
        Coprocessor.vector0.vz0 = (short)GameManager.DAT_63970[0].z;
        Coprocessor.vector1.vx1 = (short)GameManager.DAT_63970[1].x;
        Coprocessor.vector1.vy1 = (short)GameManager.DAT_63970[1].y;
        Coprocessor.vector1.vz1 = (short)GameManager.DAT_63970[1].z;
        Coprocessor.vector2.vx2 = (short)GameManager.DAT_63970[2].x;
        Coprocessor.vector2.vy2 = (short)GameManager.DAT_63970[2].y;
        Coprocessor.vector2.vz2 = (short)GameManager.DAT_63970[2].z;
        Coprocessor.ExecuteRTPT(12, false);
        DAT_B9270[iVar6, 0] = new Vector3Int(Coprocessor.vector0.vx0, Coprocessor.vector0.vy0, Coprocessor.vector0.vz0);
        DAT_B9270[iVar6, 1] = new Vector3Int(Coprocessor.vector1.vx1, Coprocessor.vector1.vy1, Coprocessor.vector1.vz1);
        DAT_B9270[iVar6, 5] = new Vector3Int(Coprocessor.vector2.vx2, Coprocessor.vector2.vy2, Coprocessor.vector2.vz2);
        DAT_B9270[iVar6, 4] = new Vector3Int(Coprocessor.vector1.vx1, Coprocessor.vector1.vy1, Coprocessor.vector1.vz1);
        int sx1_1 = Coprocessor.screenXYFIFO.sx1;
        int sy1_1 = Coprocessor.screenXYFIFO.sy1;
        Coprocessor.vector0.vx0 = (short)GameManager.DAT_63970[3].x;
        Coprocessor.vector0.vy0 = (short)GameManager.DAT_63970[3].y;
        Coprocessor.vector0.vz0 = (short)GameManager.DAT_63970[3].z;
        Coprocessor.vector1.vx1 = (short)GameManager.DAT_63970[4].x;
        Coprocessor.vector1.vy1 = (short)GameManager.DAT_63970[4].y;
        Coprocessor.vector1.vz1 = (short)GameManager.DAT_63970[4].z;
        Coprocessor.vector2.vx2 = (short)GameManager.DAT_63970[5].x;
        Coprocessor.vector2.vy2 = (short)GameManager.DAT_63970[5].y;
        Coprocessor.vector2.vz2 = (short)GameManager.DAT_63970[5].z;
        Coprocessor.ExecuteRTPT(12, false);
        DAT_B9270[iVar6, 2] = new Vector3Int(Coprocessor.vector0.vx0, Coprocessor.vector0.vy0, Coprocessor.vector0.vz0);
        DAT_B9270[iVar6, 3] = new Vector3Int(Coprocessor.vector1.vx1, Coprocessor.vector1.vy1, Coprocessor.vector1.vz1);
        DAT_B9270[iVar6, 7] = new Vector3Int(Coprocessor.vector2.vx2, Coprocessor.vector2.vy2, Coprocessor.vector2.vz2);
        DAT_B9270[iVar6, 6] = new Vector3Int(Coprocessor.vector1.vx1, Coprocessor.vector1.vy1, Coprocessor.vector1.vz1);
        int sx1_2 = Coprocessor.screenXYFIFO.sx1;
        int sy1_2 = Coprocessor.screenXYFIFO.sy1;
        iVar11 = local_28.rotation.V00;
        iVar12 = (sx1_1 + sx1_2) / 2;
        iVar3 = local_28.rotation.V10;
        iVar6 = (sy1_1 + sy1_2) / 2;
        sVar1 = (short)iVar6;

        if (iVar11 < 2897)
        {
            if (iVar11 < -2896)
            {
                iVar6 = GameManager.instance.DAT_EDC - iVar12;
                DAT_B9318[iVar5, 7] = (short)GameManager.instance.DAT_F20;
                DAT_B9318[iVar5, 5] = (short)GameManager.instance.DAT_F20;
                DAT_B9318[iVar5, 19] = 0;
                DAT_B9318[iVar5, 17] = 0;
                DAT_B9318[iVar5, 4] = 0;
                DAT_B9318[iVar5, 0] = 0;
                DAT_B9318[iVar5, 16] = 0;
                DAT_B9318[iVar5, 12] = 0;
                DAT_B9318[iVar5, 6] = (short)GameManager.instance.DAT_EDC;
                DAT_B9318[iVar5, 2] = (short)GameManager.instance.DAT_EDC;
                DAT_B9318[iVar5, 18] = (short)GameManager.instance.DAT_EDC;
                DAT_B9318[iVar5, 14] = (short)GameManager.instance.DAT_EDC;
                sVar10 = (short)(sVar1 + (short)((-iVar12 * iVar3) / iVar11));
                DAT_B9318[iVar5, 13] = sVar10;
                DAT_B9318[iVar5, 1] = sVar10;
                sVar1 = (short)(sVar1 + (short)((iVar6 * iVar3) / iVar11));
                DAT_B9318[iVar5, 15] = sVar1;
                DAT_B9318[iVar5, 3] = sVar1;
            }
            else
            {
                sVar1 = (short)iVar12;

                if (iVar3 < 1)
                {
                    iVar12 = GameManager.instance.DAT_F20 - iVar6;
                    DAT_B9318[iVar5, 6] = 0;
                    DAT_B9318[iVar5, 4] = 0;
                    DAT_B9318[iVar5, 18] = (short)GameManager.instance.DAT_EDC;
                    DAT_B9318[iVar5, 16] = (short)GameManager.instance.DAT_EDC;
                    DAT_B9318[iVar5, 5] = 0;
                    DAT_B9318[iVar5, 1] = 0;
                    DAT_B9318[iVar5, 17] = 0;
                    DAT_B9318[iVar5, 13] = 0;
                    DAT_B9318[iVar5, 7] = (short)GameManager.instance.DAT_F20;
                    DAT_B9318[iVar5, 3] = (short)GameManager.instance.DAT_F20;
                    DAT_B9318[iVar5, 19] = (short)GameManager.instance.DAT_F20;
                    DAT_B9318[iVar5, 15] = (short)GameManager.instance.DAT_F20;
                    sVar10 = (short)(sVar1 + (short)((-iVar6 * iVar11) / iVar3));
                    DAT_B9318[iVar5, 12] = sVar10;
                    DAT_B9318[iVar5, 0] = sVar10;
                    sVar1 = (short)(sVar1 + (short)((iVar12 * iVar11) / iVar3));
                    DAT_B9318[iVar5, 14] = sVar1;
                    DAT_B9318[iVar5, 2] = sVar1;
                }
                else
                {
                    iVar12 = GameManager.instance.DAT_F20 - iVar6;
                    DAT_B9318[iVar5, 6] = (short)GameManager.instance.DAT_EDC;
                    DAT_B9318[iVar5, 4] = (short)GameManager.instance.DAT_EDC;
                    DAT_B9318[iVar5, 18] = 0;
                    DAT_B9318[iVar5, 16] = 0;
                    DAT_B9318[iVar5, 5] = 0;
                    DAT_B9318[iVar5, 1] = 0;
                    DAT_B9318[iVar5, 17] = 0;
                    DAT_B9318[iVar5, 13] = 0;
                    DAT_B9318[iVar5, 7] = (short)GameManager.instance.DAT_F20;
                    DAT_B9318[iVar5, 3] = (short)GameManager.instance.DAT_F20;
                    DAT_B9318[iVar5, 19] = (short)GameManager.instance.DAT_F20;
                    DAT_B9318[iVar5, 15] = (short)GameManager.instance.DAT_F20;
                    sVar10 = (short)(sVar1 + (short)((-iVar6 * iVar11) / iVar3));
                    DAT_B9318[iVar5, 12] = sVar10;
                    DAT_B9318[iVar5, 0] = sVar10;
                    sVar1 = (short)(sVar1 + (short)((iVar12 * iVar11) / iVar3));
                    DAT_B9318[iVar5, 14] = sVar1;
                    DAT_B9318[iVar5, 2] = sVar1;
                }
            }
        }
        else
        {
            iVar6 = GameManager.instance.DAT_EDC - iVar12;
            DAT_B9318[iVar5, 7] = 0;
            DAT_B9318[iVar5, 5] = 0;
            DAT_B9318[iVar5, 19] = (short)GameManager.instance.DAT_F20;
            DAT_B9318[iVar5, 17] = (short)GameManager.instance.DAT_F20;
            DAT_B9318[iVar5, 4] = 0;
            DAT_B9318[iVar5, 0] = 0;
            DAT_B9318[iVar5, 16] = 0;
            DAT_B9318[iVar5, 12] = 0;
            DAT_B9318[iVar5, 6] = (short)GameManager.instance.DAT_EDC;
            DAT_B9318[iVar5, 2] = (short)GameManager.instance.DAT_EDC;
            DAT_B9318[iVar5, 18] = (short)GameManager.instance.DAT_EDC;
            DAT_B9318[iVar5, 14] = (short)GameManager.instance.DAT_EDC;
            sVar10 = (short)(sVar1 + (short)((-iVar12 * iVar3) / iVar11));
            DAT_B9318[iVar5, 13] = sVar10;
            DAT_B9318[iVar5, 1] = sVar10;
            sVar1 = (short)(sVar1 + (short)((iVar6 * iVar3) / iVar11));
            DAT_B9318[iVar5, 15] = sVar1;
            DAT_B9318[iVar5, 3] = sVar1;
        }

        newVertices.Clear();
        newTriangles.Clear();
        newUVs.Clear();
        skyboxMesh.Clear();
        newVertices.Add(DAT_B9270[iVar5, 0]);
        newVertices.Add(DAT_B9270[iVar5, 1]);
        newVertices.Add(DAT_B9270[iVar5, 2]);
        newVertices.Add(DAT_B9270[iVar5, 3]);
        newVertices.Add(DAT_B9270[iVar5, 4]);
        newVertices.Add(DAT_B9270[iVar5, 5]);
        newVertices.Add(DAT_B9270[iVar5, 6]);
        newVertices.Add(DAT_B9270[iVar5, 7]);
        newUVs.Add(new Vector2(0, 0));
        newUVs.Add(new Vector2(1, 0));
        newUVs.Add(new Vector2(0, 1));
        newUVs.Add(new Vector2(1, 1));
        newUVs.Add(new Vector2(0, 0));
        newUVs.Add(new Vector2(1, 0));
        newUVs.Add(new Vector2(0, 1));
        newUVs.Add(new Vector2(1, 1));
        newTriangles.Add(0);
        newTriangles.Add(1);
        newTriangles.Add(2);
        newTriangles.Add(3);
        newTriangles.Add(2);
        newTriangles.Add(1);
        newTriangles.Add(4);
        newTriangles.Add(5);
        newTriangles.Add(6);
        newTriangles.Add(7);
        newTriangles.Add(6);
        newTriangles.Add(5);
        skyboxMesh.SetVertices(newVertices);
        skyboxMesh.SetUVs(0, newUVs);
        skyboxMesh.SetTriangles(newTriangles, 0);
    }

    public void FUN_1BE68(int param1, int param2, int param3)
    {
        if (param1 < param2)
        {
            do
            {
                FUN_288E0((uint)param1, (uint)param3, GameManager.instance.DAT_610);
                param1 += 4;

            } while (param1 < param2);
        }
    }

    public Vector3Int FUN_1B998(uint x, uint z)
    {
        uint uVar1 = x >> 16; //r10
        uint uVar2 = z >> 16; //r11

        if ((x & 0xffff) + (z & 0xffff) < 0x10000)
        {
            uint uVar4 = z >> 22 << 2;
            x = x >> 22 << 7;
            uint uVar6 = (uVar2 & 0x3F) << 1; //r5
            uint uVar5 = (uVar1 & 0x3F) << 7; //r8
            int chunk1ID = chunks[(uVar4 + x) / 4]; //r6
            int vertex1ID = (int)(uVar6 + uVar5) / 2; //r6
            uint uVar = uVar1 + 1; //r3
            uVar4 += uVar >> 6 << 7;
            int chunk2ID = chunks[uVar4 / 4]; //r2
            uVar = (uVar & 0x3F) << 7;
            uVar6 += uVar;
            int vertex2ID = (int)uVar6 / 2;
            int iVar1 = vertices[chunk1ID * 4096 + vertex1ID] & 0x7FF; //r4
            int iVar2 = vertices[chunk2ID * 4096 + vertex2ID] & 0x7FF; //r2
            uVar = uVar2 + 1;
            int chunk3ID = chunks[((uVar >> 6 << 2) + x) / 4];
            uVar = (uVar & 0x3F) << 1;
            int vertex3ID = (int)(uVar + uVar5) / 2;
            int iVar3 = vertices[chunk3ID * 4096 + vertex3ID] & 0x7FF; //r2
            return new Vector3Int(iVar2 - iVar1, -32, iVar3 - iVar1);
        }
        else
        {
            uint uVar3 = uVar1 + 1; //r6
            uint uVar4 = uVar2 + 1; //r4
            uint uVar = uVar4 >> 6 << 2; //r3
            uint uVar5 = uVar3 >> 6 << 7; //r8
            uVar4 = (uVar4 & 0x3F) << 1;
            uVar3 = (uVar3 & 0x3F) << 7;
            int chunk1ID = chunks[(uVar + uVar5) / 4]; //r5
            int vertex1ID = (int)(uVar4 + uVar3) / 2; //r5
            uVar += x >> 22 << 7;
            int chunk2ID = chunks[uVar / 4];
            uVar4 += (uVar1 & 0x3F) << 7;
            int vertex2ID = (int)uVar4 / 2;
            int iVar1 = vertices[chunk1ID * 4096 + vertex1ID] & 0x7FF; //r4
            int iVar2 = vertices[chunk2ID * 4096 + vertex2ID] & 0x7FF; //r2
            uVar = (uVar2 & 0x3F) << 1;
            int chunk3ID = chunks[((z >> 22 << 2) + uVar5) / 4]; //r2
            int vertex3ID = (int)(uVar + uVar3) / 2;
            int iVar3 = vertices[chunk3ID * 4096 + vertex3ID] & 0x7FF;
            return new Vector3Int(iVar1 - iVar2, -32, iVar1 - iVar3);
        }
    }

    // 8001B944
    public TileData GetTileByPosition(uint x, uint z)
    {
        uint uVar1 = z >> 16;
        z = z >> 22 << 2;
        z += x >> 22 << 7;
        uVar1 = uVar1 & 0x3F;
        x = x >> 10 & 0xFC0;
        int chunkID = chunks[z / 4];
        uVar1 += x;
        return tileData[tiles[chunkID * 4096 + uVar1]];
    }

    public Tile GetTileByPosition2(uint x, uint z)
    {
        uint uVar1 = z >> 16;
        z = z >> 22 << 2;
        z += x >> 22 << 7;
        uVar1 = uVar1 & 0x3F;
        x = x >> 10 & 0xFC0;
        int chunkID = chunks[z / 4];
        uVar1 += x;
        return tilesDict[chunkID * 4096 + (int)uVar1];
    }

    public int FUN_1B750(uint x, uint z)
    {
        int iVar1 = (int)x & 0xffff; //r11
        int iVar2 = iVar1; //r15
        uint uVar3 = z & 0xffff; //r12
        int iVar4 = (int)uVar3; //r24
        uint uVar5 = x >> 16; //r10
        uint uVar6 = z >> 16; //r13

        if (iVar1 + uVar3 < 0x10000)
        {
            uint uVar7 = z >> 22 << 2; //r4
            x = x >> 22 << 7;
            uint uVar8 = (uVar6 & 0x3f) << 1; //r5
            uint uVar9 = (uVar5 & 0x3f) << 7; //r7
            int chunkID = chunks[(uVar7 + x) / 4];
            int vertexID = (int)(uVar8 + uVar9);
            uVar7 += (uVar5 + 1) >> 6 << 7;
            uVar8 += (uVar5 + 1 & 0x3f) << 7;
            uVar7 = (uint)vertices[chunks[uVar7 / 4] * 4096 + uVar8 / 2] & 0x7ff;
            uVar8 = (uint)vertices[chunkID * 4096 + vertexID / 2] & 0x7ff;
            int iVar3 = (int)uVar7 - (int)uVar8; //r4
            iVar3 = iVar1 * iVar3;
            chunkID = chunks[(int)((uVar6 + 1 >> 6 << 2) + x) / 4];
            vertexID = (int)(((uVar6 + 1 & 0x3F) << 1) + uVar9);
            int vertexH = (vertices[chunkID * 4096 + vertexID / 2] & 0x7FF) - (int)uVar8;
            vertexH = iVar3 + (int)uVar3 * vertexH;
            vertexH += (int)uVar8 << 16;

            if (vertexH < 0)
                vertexH += 31;

            return vertexH >> 5;
        }
        else
        {
            uint uVar9 = uVar5 + 1;
            uint uVar7 = uVar6 + 1;
            uint uVar = uVar7 >> 6 << 2; //r3
            uint uVar10 = uVar9 >> 6 << 7; //r6
            uVar7 = (uVar7 & 0x3F) << 1;
            uVar9 = (uVar9 & 0x3F) << 7;
            int chunk1ID = chunks[(uVar + uVar10) / 4];
            int vertex1ID = (int)uVar7 + (int)uVar9;
            int chunk2ID = chunks[(uVar + (x >> 22 << 7)) / 4];
            uVar7 += (uVar5 & 0x3F) << 7;
            int vertex2ID = (int)uVar7 / 2;
            int iVar7 = vertices[chunk2ID * 4096 + vertex2ID] & 0x7FF; //r2
            uVar = (z >> 22 << 2) + uVar10;
            int iVar9 = 0x10000;
            int iVar6 = vertices[chunk1ID * 4096 + vertex1ID / 2] & 0x7FF; //r4
            int iVar8 = iVar9 - iVar2; //r5
            iVar7 -= iVar6;
            iVar8 = iVar8 * iVar7;
            int chunk3ID = chunks[uVar / 4];
            iVar9 -= iVar4; //r6
            int vertex3ID = (int)(((uVar6 & 0x3F) << 1) + uVar9);
            int vertexH = vertices[chunk3ID * 4096 + vertex3ID / 2] & 0x7FF; //r2
            vertexH -= iVar6;
            iVar6 = iVar6 << 16;
            vertexH = iVar8 + iVar9 * vertexH;
            vertexH = iVar6 + vertexH;

            if (vertexH < 0)
                vertexH += 31;

            return vertexH >> 5;
        }
    }

    private void FUN_288E0(uint param1, uint param2, Queue<ScreenPoly> param3)
    {
        int iVar1;
        int iVar2;
        Vector2Int v2Var2;
        uint uVar3;
        uint uVar4;
        int iVar5;
        uint uVar6;
        uint uVar7;
        Vector2Int v2Var8;
        int iVar9;
        Vector2Int v2Var9;
        uint uVar10;
        Color32 uVar11;
        Color32 uVar12;
        Color32 uVar13;
        uint uVar19;
        uint uVar20;
        uint uVar21;
        Vector3Int[] terrainVertices = new Vector3Int[4];
        
        iVar1 = (int)(param1 >> 6) * 32 + (int)(param2 >> 6);
        iVar2 = iVar1 * 4;
        iVar5 = (int)((param2 & 63) + (param1 & 63) * 64) * 2;
        puVar14 = chunks[iVar1] * 4096 + iVar5 / 2;
        uVar7 = param1 + 4 & 63;
        puVar15 = puVar14 + 256;

        if (uVar7 == 0)
            puVar15 = chunks[32 + iVar1] * 4096 + (iVar5 - 7680) / 2;

        puVar16 = puVar14 + 4;
        puVar17 = puVar15 + 4;

        if ((param2 + 4 & 63) == 0)
        {
            puVar16 = chunks[1 + iVar1] * 4096 + (iVar5 - 120) / 2;
            puVar17 = puVar16 + 256;

            if (uVar7 == 0)
                puVar17 = chunks[33 + iVar1] * 4096;
        }

        iVar1 = (int)param2 * 256 + GameManager.DAT_1f800084.z;
        uVar4 = (uint)((int)param1 * 256 + GameManager.DAT_1f800084.x & 0xffff);
        iVar5 = GameManager.DAT_1f800084.y * 0x10000;
        Coprocessor.vector0.vx0 = (short)uVar4;
        Coprocessor.vector0.vy0 = (short)((vertices[puVar14] & 0x7ff) * 0x80000 + iVar5 >> 16);
        Coprocessor.vector0.vz0 = (short)iVar1;
        terrainVertices[0] = new Vector3Int(Coprocessor.vector0.vx0, Coprocessor.vector0.vy0, Coprocessor.vector0.vz0);
        Coprocessor.ExecuteRTPS(12, false);
        uVar7 = (uint)GameManager.DAT_1f800098;
        uVar6 = uVar4 + 0x400 & 0xffff;
        iVar2 = Coprocessor.screenZFIFO.sz2;
        GameManager.terrainScreen[0].ir0 = Coprocessor.accumulator.ir0;
        Coprocessor.vector0.vx0 = (short)uVar6;
        Coprocessor.vector0.vy0 = (short)((vertices[puVar15] & 0x7ff) * 0x80000 + iVar5 >> 16);
        Coprocessor.vector0.vz0 = (short)iVar1;
        terrainVertices[1] = new Vector3Int(Coprocessor.vector0.vx0, Coprocessor.vector0.vy0, Coprocessor.vector0.vz0);
        Coprocessor.ExecuteRTPS(12, false);
        uVar19 = (uint)(iVar2 < (int)uVar7 ? 1 : 0);
        iVar9 = Coprocessor.screenZFIFO.sz2;
        GameManager.terrainScreen[4].ir0 = Coprocessor.accumulator.ir0;
        Coprocessor.vector0.vx0 = (short)uVar4;
        Coprocessor.vector0.vy0 = (short)((vertices[puVar16] & 0x7ff) * 0x80000 + iVar5 >> 16);
        Coprocessor.vector0.vz0 = (short)(iVar1 + 1024);
        v2Var2 = new Vector2Int(Coprocessor.screenXYFIFO.sx1, Coprocessor.screenXYFIFO.sy1);
        terrainVertices[2] = new Vector3Int(Coprocessor.vector0.vx0, Coprocessor.vector0.vy0, Coprocessor.vector0.vz0);
        Coprocessor.ExecuteRTPS(12, false);
        uVar10 = (uint)(iVar9 < (int)uVar7 ? 1 : 0) << 1;
        uVar20 = uVar19 | uVar10;
        iVar9 = Coprocessor.screenZFIFO.sz3;
        GameManager.terrainScreen[20].ir0 = Coprocessor.accumulator.ir0;
        Coprocessor.ExecuteNCLIP();
        Coprocessor.vector0.vx0 = (short)uVar6;
        Coprocessor.vector0.vy0 = (short)((vertices[puVar17] & 0x7ff) * 0x80000 + iVar5 >> 16);
        Coprocessor.vector0.vz0 = (short)(iVar1 + 1024);
        terrainVertices[3] = new Vector3Int(Coprocessor.vector0.vx0, Coprocessor.vector0.vy0, Coprocessor.vector0.vz0);
        iVar5 = Coprocessor.mathsAccumulator.mac0;
        Coprocessor.ExecuteRTPS(12, false);
        uVar6 = (uint)(iVar9 < (int)uVar7 ? 1 : 0) << 2;
        iVar9 = Coprocessor.screenZFIFO.sz3;
        GameManager.terrainScreen[24].ir0 = Coprocessor.accumulator.ir0;
        Coprocessor.ExecuteAVSZ4();
        uVar7 = (uint)(iVar9 < (int)uVar7 ? 1 : 0) << 3;
        uVar21 = uVar20 | uVar6 | uVar7;
        Coprocessor.colorCode.r = GameManager.instance.DAT_DDC.r;
        Coprocessor.colorCode.g = GameManager.instance.DAT_DDC.g;
        Coprocessor.colorCode.b = GameManager.instance.DAT_DDC.b;
        Coprocessor.colorCode.code = GameManager.instance.DAT_DDC.a;

        if (uVar21 == 15) //uVar21==15
        {
            GameManager.terrainScreen[0].vert.z = Coprocessor.screenZFIFO.sz0;
            GameManager.terrainScreen[4].vert.z = Coprocessor.screenZFIFO.sz1;
            GameManager.terrainScreen[20].vert.z = Coprocessor.screenZFIFO.sz2;
            GameManager.terrainScreen[24].vert.z = Coprocessor.screenZFIFO.sz3;
            terrainWorld[0].z = terrainVertices[0].z;
            terrainWorld[0].x = terrainVertices[0].x;
            terrainWorld[0].y = terrainVertices[0].y;
            terrainWorld[4].z = terrainVertices[1].z;
            terrainWorld[20].z = terrainVertices[2].z;
            terrainWorld[24].z = terrainVertices[3].z;
            Coprocessor.accumulator.ir1 = (short)((uint)vertices[puVar14] >> 13 << 7);
            Coprocessor.ExecuteCC(12, true);
            GameManager.terrainScreen[4].vert.x = Coprocessor.screenXYFIFO.sx0;
            GameManager.terrainScreen[4].vert.y = Coprocessor.screenXYFIFO.sy0;
            terrainWorld[4].x = terrainVertices[1].x;
            terrainWorld[4].y = terrainVertices[1].y;
            Coprocessor.accumulator.ir1 = (short)((uint)vertices[puVar15] >> 13 << 7);
            GameManager.terrainScreen[0].color = new Color32(Coprocessor.colorFIFO.r2, Coprocessor.colorFIFO.g2, Coprocessor.colorFIFO.b2, Coprocessor.colorFIFO.cd2);
            Coprocessor.ExecuteCC(12, true);
            GameManager.terrainScreen[20].vert.x = Coprocessor.screenXYFIFO.sx1;
            GameManager.terrainScreen[20].vert.y = Coprocessor.screenXYFIFO.sy1;
            terrainWorld[20].x = terrainVertices[2].x;
            terrainWorld[20].y = terrainVertices[2].y;
            Coprocessor.accumulator.ir1 = (short)((uint)vertices[puVar16] >> 13 << 7);
            GameManager.terrainScreen[4].color = new Color32(Coprocessor.colorFIFO.r2, Coprocessor.colorFIFO.g2, Coprocessor.colorFIFO.b2, Coprocessor.colorFIFO.cd2);
            Coprocessor.ExecuteCC(12, true);
            GameManager.terrainScreen[24].vert.x = Coprocessor.screenXYFIFO.sx2;
            GameManager.terrainScreen[24].vert.y = Coprocessor.screenXYFIFO.sy2;
            terrainWorld[24].x = terrainVertices[3].x;
            terrainWorld[24].y = terrainVertices[3].y;
            Coprocessor.accumulator.ir1 = (short)((uint)vertices[puVar17] >> 13 << 7);
            GameManager.terrainScreen[20].color = new Color32(Coprocessor.colorFIFO.r2, Coprocessor.colorFIFO.g2, Coprocessor.colorFIFO.b2, Coprocessor.colorFIFO.cd2);
            Coprocessor.ExecuteCC(12, true);
            uVar7 = (uint)(GameManager.DAT_1f800084.y * 0x10000);
            Coprocessor.vector0.vz0 = (short)iVar1;
            Coprocessor.vector1.vz1 = (short)(iVar1 + 512);
            Coprocessor.vector2.vz2 = (short)(iVar1 + 1024);
            uVar6 = uVar4 + 512 & 0xffff | uVar7;
            uint cop2r0 = (uint)(vertices[puVar14 + 128] & 0x7ff) * 0x80000 + uVar6;
            uint cop2r2 = (uint)(vertices[puVar14 + 130] & 0x7ff) * 0x80000 + uVar6;
            uint cop2r4 = (uint)(vertices[puVar16 + 128] & 0x7ff) * 0x80000 + uVar6;
            Coprocessor.vector0.vx0 = (short)cop2r0;
            Coprocessor.vector0.vy0 = (short)(cop2r0 >> 16);
            Coprocessor.vector1.vx1 = (short)cop2r2;
            Coprocessor.vector1.vy1 = (short)(cop2r2 >> 16);
            Coprocessor.vector2.vx2 = (short)cop2r4;
            Coprocessor.vector2.vy2 = (short)(cop2r4 >> 16);
            terrainVertices[0] = new Vector3Int(Coprocessor.vector0.vx0, Coprocessor.vector0.vy0, Coprocessor.vector0.vz0);
            terrainVertices[1] = new Vector3Int(Coprocessor.vector1.vx1, Coprocessor.vector1.vy1, Coprocessor.vector1.vz1);
            terrainVertices[2] = new Vector3Int(Coprocessor.vector2.vx2, Coprocessor.vector2.vy2, Coprocessor.vector2.vz2);
            GameManager.terrainScreen[24].color = new Color32(Coprocessor.colorFIFO.r2, Coprocessor.colorFIFO.g2, Coprocessor.colorFIFO.b2, Coprocessor.colorFIFO.cd2);
            Coprocessor.ExecuteRTPT(12, false);
            GameManager.terrainScreen[2].vert.x = Coprocessor.screenXYFIFO.sx0;
            GameManager.terrainScreen[2].vert.y = Coprocessor.screenXYFIFO.sy0;
            GameManager.terrainScreen[22].vert.x = Coprocessor.screenXYFIFO.sx2;
            GameManager.terrainScreen[22].vert.y = Coprocessor.screenXYFIFO.sy2;
            terrainWorld[2].x = terrainVertices[0].x;
            terrainWorld[2].y = terrainVertices[0].y;
            terrainWorld[22].x = terrainVertices[2].x;
            terrainWorld[22].y = terrainVertices[2].y;
            Coprocessor.vector0.vx0 = (short)uVar4;
            Coprocessor.vector0.vy0 = (short)((vertices[puVar14 + 2] & 0x7ff) * 0x80000 + uVar7 >> 16);
            cop2r4 = (uint)(vertices[puVar15 + 2] & 0x7ff) * 0x80000 +
                        (uVar4 + 1024 & 0xffff | uVar7);
            Coprocessor.vector2.vx2 = (short)cop2r4;
            Coprocessor.vector2.vy2 = (short)(cop2r4 >> 16);
            Coprocessor.vector0.vz0 = (short)(iVar1 + 512);
            Coprocessor.vector2.vz2 = (short)(iVar1 + 512);
            GameManager.terrainScreen[2].vert.z = Coprocessor.screenZFIFO.sz1;
            GameManager.terrainScreen[22].vert.z = Coprocessor.screenZFIFO.sz3;
            terrainWorld[2].z = terrainVertices[0].z;
            terrainWorld[22].z = terrainVertices[2].z;
            terrainVertices[0] = new Vector3Int(Coprocessor.vector0.vx0, Coprocessor.vector0.vy0, Coprocessor.vector0.vz0);
            terrainVertices[2] = new Vector3Int(Coprocessor.vector2.vx2, Coprocessor.vector2.vy2, Coprocessor.vector2.vz2);
            Coprocessor.ExecuteRTPT(12, false);
            GameManager.terrainScreen[10].vert.x = Coprocessor.screenXYFIFO.sx0;
            GameManager.terrainScreen[10].vert.y = Coprocessor.screenXYFIFO.sy0;
            GameManager.terrainScreen[12].vert.x = Coprocessor.screenXYFIFO.sx1;
            GameManager.terrainScreen[12].vert.y = Coprocessor.screenXYFIFO.sy1;
            terrainWorld[10].x = terrainVertices[0].x;
            terrainWorld[10].y = terrainVertices[0].y;
            terrainWorld[12].x = terrainVertices[1].x;
            terrainWorld[12].y = terrainVertices[1].y;
            cop2r0 = (uint)vertices[puVar14 + 128] >> 13 << 7;
            cop2r2 = (uint)vertices[puVar14 + 130] >> 13 << 7;
            cop2r4 = (uint)vertices[puVar16 + 128] >> 13 << 7;
            Coprocessor.vector0.vx0 = (short)cop2r0;
            Coprocessor.vector0.vy0 = (short)(cop2r0 >> 16);
            Coprocessor.vector1.vx1 = (short)cop2r2;
            Coprocessor.vector1.vy1 = (short)(cop2r2 >> 16);
            Coprocessor.vector2.vx2 = (short)cop2r4;
            Coprocessor.vector2.vy2 = (short)(cop2r4 >> 16);
            GameManager.terrainScreen[10].vert.z = Coprocessor.screenZFIFO.sz1;
            GameManager.terrainScreen[12].vert.z = Coprocessor.screenZFIFO.sz2;
            terrainWorld[10].z = terrainVertices[0].z;
            terrainWorld[12].z = terrainVertices[1].z;
            Coprocessor.ExecuteNCCT(12, true);
            GameManager.terrainScreen[2].color = new Color32(Coprocessor.colorFIFO.r0, Coprocessor.colorFIFO.g0, Coprocessor.colorFIFO.b0, Coprocessor.colorFIFO.cd0);
            Coprocessor.accumulator.ir1 = (short)((uint)vertices[puVar14 + 2] >> 13 << 7);
            GameManager.terrainScreen[12].color = new Color32(Coprocessor.colorFIFO.r1, Coprocessor.colorFIFO.g1, Coprocessor.colorFIFO.b1, Coprocessor.colorFIFO.cd1);
            GameManager.terrainScreen[22].color = new Color32(Coprocessor.colorFIFO.r2, Coprocessor.colorFIFO.g2, Coprocessor.colorFIFO.b2, Coprocessor.colorFIFO.cd2);
            Coprocessor.ExecuteCC(12, true);
            GameManager.terrainScreen[5].color = new Color32(Coprocessor.colorFIFO.r2, Coprocessor.colorFIFO.g2, Coprocessor.colorFIFO.b2, Coprocessor.colorFIFO.cd2);
            Coprocessor.accumulator.ir1 = (short)((uint)vertices[puVar15 + 2] >> 13 << 7);
            GameManager.terrainScreen[14].vert.x = Coprocessor.screenXYFIFO.sx2;
            GameManager.terrainScreen[14].vert.y = Coprocessor.screenXYFIFO.sy2;
            GameManager.terrainScreen[14].vert.z = Coprocessor.screenZFIFO.sz3;
            terrainWorld[14].x = terrainVertices[2].x;
            terrainWorld[14].y = terrainVertices[2].y;
            terrainWorld[14].z = terrainVertices[2].z;
            Coprocessor.ExecuteCC(12, true);
            uVar7 = uVar4 + 512 & 0xffff;
            iVar1 += 512;
            GameManager.terrainScreen[14].color = new Color32(Coprocessor.colorFIFO.r2, Coprocessor.colorFIFO.g2, Coprocessor.colorFIFO.b2, Coprocessor.colorFIFO.cd2);
            GameManager.terrainScreen[0].vert.x = v2Var2.x;
            GameManager.terrainScreen[0].vert.y = v2Var2.y;
            int puVar15_1 = puVar15;
            puVar14 += 130;
            puVar15 += 2;
            puVar16 += 128;
            FUN_290A8(uVar7, iVar1, param3, 12);
            puVar14 -= 128;
            puVar16 -= 128;
            puVar15 = puVar14 + 128;
            puVar17 = puVar16 + 128;
            uVar7 = uVar7 - 512 & 0xffff;
            FUN_290A8(uVar7, iVar1, param3, 10);
            puVar14 += 126;
            puVar15 = puVar15_1;
            puVar16 = puVar14 + 2;
            puVar17 = puVar15 + 2;
            uVar7 = uVar7 + 512 & 0xffff;
            iVar1 -= 512;
            FUN_290A8(uVar7, iVar1, param3, 2);
            puVar14 -= 128;
            puVar15 = puVar14 + 128;
            puVar16 = puVar14 + 2;
            puVar17 = puVar15 + 2;
            uVar7 = uVar7 - 512 & 0xffff;
            FUN_290A8(uVar7, iVar1, param3, 0);
        }
        else
        {
            uVar3 = Coprocessor.averageZ;
            Coprocessor.accumulator.ir0 = (short)GameManager.terrainScreen[0].ir0;
            Coprocessor.accumulator.ir1 = (short)((uint)vertices[puVar14] >> 11 << 7);
            Coprocessor.ExecuteCDP(12, true);
            v2Var9 = new Vector2Int(Coprocessor.screenXYFIFO.sx0, Coprocessor.screenXYFIFO.sy0);
            Coprocessor.accumulator.ir0 = (short)GameManager.terrainScreen[4].ir0;
            Coprocessor.accumulator.ir1 = (short)((uint)vertices[puVar15] >> 11 << 7);
            puVar18 = GameManager.DAT_1f800080 + (int)(uVar3 >> 1);
            Coprocessor.ExecuteCDP(12, true);
            v2Var8 = new Vector2Int(Coprocessor.screenXYFIFO.sx1, Coprocessor.screenXYFIFO.sy1);
            Coprocessor.accumulator.ir0 = (short)GameManager.terrainScreen[20].ir0;
            Coprocessor.accumulator.ir1 = (short)((uint)vertices[puVar16] >> 11 << 7);
            Coprocessor.ExecuteCDP(12, true);
            int tFactor = 1;
            int index;

            if (uVar21 == 0)
            {
                //if (iVar5 < 1)
                //{
                    newVertices.Add((Vector3)terrainVertices[0] / tFactor);
                    newVertices.Add((Vector3)terrainVertices[1] / tFactor);
                    newVertices.Add((Vector3)terrainVertices[2] / tFactor);
                    newColors.Add(new Color32(Coprocessor.colorFIFO.r0, Coprocessor.colorFIFO.g0, Coprocessor.colorFIFO.b0, Coprocessor.colorFIFO.cd0));
                    newColors.Add(new Color32(Coprocessor.colorFIFO.r1, Coprocessor.colorFIFO.g1, Coprocessor.colorFIFO.b1, Coprocessor.colorFIFO.cd1));
                    newColors.Add(new Color32(Coprocessor.colorFIFO.r2, Coprocessor.colorFIFO.g2, Coprocessor.colorFIFO.b2, Coprocessor.colorFIFO.cd2));
                    newUVs.Add(new Vector2(0, 0));
                    newUVs.Add(new Vector2(0, 0));
                    newUVs.Add(new Vector2(0, 0));
                    index = newVertices.Count - 1;
                    newTriangles.Add(index);
                    newTriangles.Add(index - 1);
                    newTriangles.Add(index - 2);
                //}

                Coprocessor.ExecuteNCLIP();
                iVar1 = Coprocessor.mathsAccumulator.mac0;
                Coprocessor.accumulator.ir0 = (short)GameManager.terrainScreen[24].ir0;
                Coprocessor.accumulator.ir1 = (short)((uint)vertices[puVar17] >> 11 << 7);

                //if (-1 < iVar1)
                //{
                    Coprocessor.ExecuteCDP(12, true);
                    newVertices.Add((Vector3)terrainVertices[1] / tFactor);
                    newVertices.Add((Vector3)terrainVertices[2] / tFactor);
                    newVertices.Add((Vector3)terrainVertices[3] / tFactor);
                    newColors.Add(new Color32(Coprocessor.colorFIFO.r0, Coprocessor.colorFIFO.g0, Coprocessor.colorFIFO.b0, Coprocessor.colorFIFO.cd0));
                    newColors.Add(new Color32(Coprocessor.colorFIFO.r1, Coprocessor.colorFIFO.g1, Coprocessor.colorFIFO.b1, Coprocessor.colorFIFO.cd1));
                    newColors.Add(new Color32(Coprocessor.colorFIFO.r2, Coprocessor.colorFIFO.g2, Coprocessor.colorFIFO.b2, Coprocessor.colorFIFO.cd2));
                    newUVs.Add(new Vector2(0, 0));
                    newUVs.Add(new Vector2(0, 0));
                    newUVs.Add(new Vector2(0, 0));
                    index = newVertices.Count - 1;
                    newTriangles.Add(index - 2);
                    newTriangles.Add(index - 1);
                    newTriangles.Add(index);
                //}
            }
            else
            {
                GameManager.terrainScreen[24].vert.x = Coprocessor.screenXYFIFO.sx2;
                GameManager.terrainScreen[24].vert.y = Coprocessor.screenXYFIFO.sy2;
                terrainWorld[24].x = terrainVertices[3].x;
                terrainWorld[24].y = terrainVertices[3].y;
                Coprocessor.accumulator.ir0 = (short)GameManager.terrainScreen[24].ir0;
                Coprocessor.accumulator.ir1 = (short)((uint)vertices[puVar17] >> 11 << 7);
                GameManager.terrainScreen[0].vert.z = Coprocessor.screenZFIFO.sz0;
                GameManager.terrainScreen[4].vert.z = Coprocessor.screenZFIFO.sz1;
                GameManager.terrainScreen[20].vert.z = Coprocessor.screenZFIFO.sz2;
                GameManager.terrainScreen[24].vert.z = Coprocessor.screenZFIFO.sz3;
                terrainWorld[0].z = terrainVertices[0].z;
                terrainWorld[0].x = terrainVertices[0].x;
                terrainWorld[0].y = terrainVertices[0].y;
                terrainWorld[4].z = terrainVertices[1].z;
                terrainWorld[4].x = terrainVertices[1].x;
                terrainWorld[4].y = terrainVertices[1].y;
                terrainWorld[20].z = terrainVertices[2].z;
                terrainWorld[20].x = terrainVertices[2].x;
                terrainWorld[20].y = terrainVertices[2].y;
                terrainWorld[24].z = terrainVertices[3].z;
                GameManager.terrainScreen[0].color = new Color32(Coprocessor.colorFIFO.r0, Coprocessor.colorFIFO.g0, Coprocessor.colorFIFO.b0, Coprocessor.colorFIFO.cd0);
                GameManager.terrainScreen[4].color = new Color32(Coprocessor.colorFIFO.r1, Coprocessor.colorFIFO.g1, Coprocessor.colorFIFO.b1, Coprocessor.colorFIFO.cd1);
                GameManager.terrainScreen[20].color = new Color32(Coprocessor.colorFIFO.r2, Coprocessor.colorFIFO.g2, Coprocessor.colorFIFO.b2, Coprocessor.colorFIFO.cd2);
                Coprocessor.ExecuteCDP(12, true);
                GameManager.terrainScreen[24].color = new Color32(Coprocessor.colorFIFO.r2, Coprocessor.colorFIFO.g2, Coprocessor.colorFIFO.b2, Coprocessor.colorFIFO.cd2);
                iVar5 = GameManager.DAT_1f800084.y * 0x10000;

                if (uVar20 != 0)
                {
                    Coprocessor.vector0.vx0 = (short)(uVar4 + 512 & 0xffff);
                    Coprocessor.vector0.vy0 = (short)((vertices[puVar14 + 128] & 0x7ff) * 0x80000 + iVar5 >> 16);
                    Coprocessor.vector0.vz0 = (short)iVar1;
                    terrainVertices[0] = new Vector3Int(Coprocessor.vector0.vx0, Coprocessor.vector0.vy0, Coprocessor.vector0.vz0);
                    Coprocessor.ExecuteRTPS(12, false);
                    GameManager.terrainScreen[2].vert.x = Coprocessor.screenXYFIFO.sx2;
                    GameManager.terrainScreen[2].vert.y = Coprocessor.screenXYFIFO.sy2;
                    terrainWorld[2].x = terrainVertices[0].x;
                    terrainWorld[2].y = terrainVertices[0].y;
                    Coprocessor.accumulator.ir1 = (short)((uint)vertices[puVar14 + 128] >> 11 << 7);
                    GameManager.terrainScreen[2].vert.z = Coprocessor.screenZFIFO.sz3;
                    terrainWorld[2].z = terrainVertices[0].z;
                    Coprocessor.ExecuteCDP(12, true);
                }

                if ((uVar19 | uVar6) != 0)
                {
                    Coprocessor.vector0.vx0 = (short)uVar4;
                    Coprocessor.vector0.vy0 = (short)((vertices[puVar14 + 2] & 0x7ff) * 0x80000 + iVar5 >> 16);
                    Coprocessor.vector0.vz0 = (short)(iVar1 + 512);
                    terrainVertices[0] = new Vector3Int(Coprocessor.vector0.vx0, Coprocessor.vector0.vy0, Coprocessor.vector0.vz0);
                    uVar11 = new Color32(Coprocessor.colorFIFO.r2, Coprocessor.colorFIFO.g2, Coprocessor.colorFIFO.b2, Coprocessor.colorFIFO.cd2);
                    Coprocessor.ExecuteRTPS(12, false);

                    if (uVar20 != 0)
                        GameManager.terrainScreen[2].color = uVar11;

                    GameManager.terrainScreen[10].vert.x = Coprocessor.screenXYFIFO.sx2;
                    GameManager.terrainScreen[10].vert.y = Coprocessor.screenXYFIFO.sy2;
                    terrainWorld[10].x = terrainVertices[0].x;
                    terrainWorld[10].y = terrainVertices[0].y;
                    Coprocessor.accumulator.ir1 = (short)((uint)vertices[puVar14 + 2] >> 11 << 7);
                    GameManager.terrainScreen[10].vert.z = Coprocessor.screenZFIFO.sz3;
                    terrainWorld[10].z = terrainVertices[0].z;
                    Coprocessor.ExecuteCDP(12, true);
                }

                if ((uVar10 | uVar7) != 0)
                {
                    Coprocessor.vector0.vx0 = (short)(uVar4 + 1024 & 0xffff);
                    Coprocessor.vector0.vy0 = (short)((vertices[puVar15 + 2] & 0x7ff) * 0x80000 + iVar5 >> 16);
                    Coprocessor.vector0.vz0 = (short)(iVar1 + 512);
                    terrainVertices[0] = new Vector3Int(Coprocessor.vector0.vx0, Coprocessor.vector0.vy0, Coprocessor.vector0.vz0);
                    uVar12 = new Color32(Coprocessor.colorFIFO.r2, Coprocessor.colorFIFO.g2, Coprocessor.colorFIFO.b2, Coprocessor.colorFIFO.cd2);
                    Coprocessor.ExecuteRTPS(12, false);
                    uVar11 = uVar12;

                    if ((uVar19 | uVar6) == 0)
                    {
                        uVar11 = GameManager.terrainScreen[10].color;

                        if (uVar20 != 0)
                            GameManager.terrainScreen[2].color = uVar12;
                    }

                    GameManager.terrainScreen[10].color = uVar11;
                    GameManager.terrainScreen[14].vert.x = Coprocessor.screenXYFIFO.sx2;
                    GameManager.terrainScreen[14].vert.y = Coprocessor.screenXYFIFO.sy2;
                    terrainWorld[14].x = terrainVertices[0].x;
                    terrainWorld[14].y = terrainVertices[0].y;
                    Coprocessor.accumulator.ir1 = (short)((uint)vertices[puVar15 + 2] >> 11 << 7);
                    GameManager.terrainScreen[14].vert.z = Coprocessor.screenZFIFO.sz3;
                    terrainWorld[14].z = terrainVertices[0].z;
                    Coprocessor.ExecuteCDP(12, true);
                }

                if ((uVar6 | uVar7) == 0)
                {
                    if ((uVar10 | uVar7) == 0)
                    {
                        if ((uVar19 | uVar6) == 0)
                        {
                            if (uVar20 != 0)
                                GameManager.terrainScreen[2].color = new Color32(Coprocessor.colorFIFO.r2, Coprocessor.colorFIFO.g2, Coprocessor.colorFIFO.b2, Coprocessor.colorFIFO.cd2);
                        }
                        else
                            GameManager.terrainScreen[10].color = new Color32(Coprocessor.colorFIFO.r2, Coprocessor.colorFIFO.g2, Coprocessor.colorFIFO.b2, Coprocessor.colorFIFO.cd2);
                    }
                    else
                        GameManager.terrainScreen[14].color = new Color32(Coprocessor.colorFIFO.r2, Coprocessor.colorFIFO.g2, Coprocessor.colorFIFO.b2, Coprocessor.colorFIFO.cd2);
                }
                else
                {
                    Coprocessor.vector0.vx0 = (short)(uVar4 + 512 & 0xffff);
                    Coprocessor.vector0.vy0 = (short)((vertices[puVar16 + 128] & 0x7ff) * 0x80000 + iVar5 >> 16);
                    Coprocessor.vector0.vz0 = (short)(iVar1 + 1024);
                    terrainVertices[0] = new Vector3Int(Coprocessor.vector0.vx0, Coprocessor.vector0.vy0, Coprocessor.vector0.vz0);
                    uVar13 = new Color32(Coprocessor.colorFIFO.r2, Coprocessor.colorFIFO.g2, Coprocessor.colorFIFO.b2, Coprocessor.colorFIFO.cd2);
                    Coprocessor.ExecuteRTPS(12, false);
                    GameManager.terrainScreen[22].vert.x = Coprocessor.screenXYFIFO.sx2;
                    GameManager.terrainScreen[22].vert.y = Coprocessor.screenXYFIFO.sy2;
                    terrainWorld[22].x = terrainVertices[0].x;
                    terrainWorld[22].y = terrainVertices[0].y;
                    Coprocessor.accumulator.ir1 = (short)((uint)vertices[puVar16 + 128] >> 11 << 7);
                    GameManager.terrainScreen[22].vert.z = Coprocessor.screenZFIFO.sz3;
                    terrainWorld[22].z = terrainVertices[0].z;
                    Coprocessor.ExecuteCDP(12, true);
                    uVar11 = GameManager.terrainScreen[10].color;
                    uVar12 = uVar13;

                    if ((uVar10 | uVar7) == 0)
                    {
                        uVar11 = uVar13;
                        uVar12 = GameManager.terrainScreen[14].color;

                        if ((uVar19 | uVar6) == 0)
                        {
                            uVar11 = GameManager.terrainScreen[10].color;

                            if (uVar20 != 0)
                                GameManager.terrainScreen[2].color = uVar13;
                        }
                    }

                    GameManager.terrainScreen[14].color = uVar12;
                    GameManager.terrainScreen[10].color = uVar11;
                    GameManager.terrainScreen[22].color = new Color32(Coprocessor.colorFIFO.r2, Coprocessor.colorFIFO.g2, Coprocessor.colorFIFO.b2, Coprocessor.colorFIFO.cd2);
                }

                GameManager.terrainScreen[0].vert.x = v2Var2.x;
                GameManager.terrainScreen[0].vert.y = v2Var2.y;
                GameManager.terrainScreen[4].vert.x = v2Var9.x;
                GameManager.terrainScreen[4].vert.y = v2Var9.y;
                GameManager.terrainScreen[20].vert.x = v2Var8.x;
                GameManager.terrainScreen[20].vert.y = v2Var8.y;
                in_t1 = GameManager.DAT_639EC[(uVar21 - 1) * 2];
                in_t0 = GameManager.DAT_639EC[(uVar21 - 1) * 2 + 1];
                FUN_297E8(uVar4, iVar1, param3, 0);
            }
        }
    }

    private void FUN_290A8(uint param1, int param2, Queue<ScreenPoly> param3, int param4)
    {
        short sVar1;
        uint uVar2;
        short sVar3;
        int iVar4;
        Color32 clrVar4;
        uint puVar5;
        int iVar6;
        Color32 clrVar6;
        uint puVar8;
        int iVar9;
        Color32 clrVar9;
        int iVar11;
        Color32 clrVar11;
        uint uVar13;
        Color32 clrVar14;
        uint uVar15;
        uint uVar16;
        int puVar17_1;
        int index;
        int cop2r0;
        int cop2r2;
        int cop2r4;
        Vector3Int[] terrainVertices = new Vector3Int[4];

        int tFactor = 1;
        uVar2 = (ushort)GameManager.DAT_1f80009a;
        iVar4 = GameManager.terrainScreen[param4].vert.z;
        iVar6 = GameManager.terrainScreen[param4 + 2].vert.z;
        iVar9 = GameManager.terrainScreen[param4 + 10].vert.z;
        uVar13 = (uint)(iVar9 < (int)uVar2 ? 1 : 0) << 2;
        iVar11 = GameManager.terrainScreen[param4 + 12].vert.z;
        uVar2 = (uint)(iVar4 < (int)uVar2 ? 1 : 0) | (uint)(iVar6 < (int)uVar2 ? 1 : 0) << 1 | uVar13 |
                (uint)(iVar11 < (int)uVar2 ? 1 : 0) << 3;

        if (uVar2 == 0)
        {
            Coprocessor.screenXYFIFO.sx0 = (short)GameManager.terrainScreen[param4].vert.x;
            Coprocessor.screenXYFIFO.sy0 = (short)GameManager.terrainScreen[param4].vert.y;
            Coprocessor.screenXYFIFO.sx1 = (short)GameManager.terrainScreen[param4 + 2].vert.x;
            Coprocessor.screenXYFIFO.sy1 = (short)GameManager.terrainScreen[param4 + 2].vert.y;
            Coprocessor.screenXYFIFO.sx2 = (short)GameManager.terrainScreen[param4 + 10].vert.x;
            Coprocessor.screenXYFIFO.sy2 = (short)GameManager.terrainScreen[param4 + 10].vert.y;
            Coprocessor.ExecuteNCLIP();
            puVar17_1 = GameManager.DAT_1f800080 + (int)((uint)(iVar4 + iVar6 + iVar9 + iVar11) >> 5);
            clrVar9 = GameManager.terrainScreen[param4 + 4].color;
            clrVar11 = GameManager.terrainScreen[param4 + 10].color;
            iVar6 = Coprocessor.mathsAccumulator.mac0;
            //param3[1].v2 = new Vector2Int(Coprocessor.screenXYFIFO.sx0, Coprocessor.screenXYFIFO.sy0);
            Coprocessor.screenXYFIFO.sx0 = (short)GameManager.terrainScreen[param4 + 12].vert.x;
            Coprocessor.screenXYFIFO.sy0 = (short)GameManager.terrainScreen[param4 + 12].vert.y;

            //if (iVar6 < 1)
            //{
                newVertices.Add(terrainWorld[param4] / tFactor);
                newVertices.Add(terrainWorld[param4 + 2] / tFactor);
                newVertices.Add(terrainWorld[param4 + 10] / tFactor);
                /*param3[2].v2 = new Vector2Int(Coprocessor.screenXYFIFO.sx1, Coprocessor.screenXYFIFO.sy1);
                param3[3].v2 = new Vector2Int(Coprocessor.screenXYFIFO.sx2, Coprocessor.screenXYFIFO.sy2);*/
                clrVar4 = GameManager.terrainScreen[param4].color;
                newColors.Add(clrVar4);
                newColors.Add(clrVar9);
                newColors.Add(clrVar11);
                newUVs.Add(new Vector2(0, 0));
                newUVs.Add(new Vector2(0, 0));
                newUVs.Add(new Vector2(0, 0));
                /*param3[2].clr = clrVar9;
                param3[3].clr = clrVar11;
                param3[1].clr = clrVar4;*/
                index = newVertices.Count - 1;
                newTriangles.Add(index);
                newTriangles.Add(index - 1);
                newTriangles.Add(index - 2);
            //}

            Coprocessor.ExecuteNCLIP();
            iVar6 = Coprocessor.mathsAccumulator.mac0;

            //if (-1 < iVar6)
            //{
                newColors.Add(GameManager.terrainScreen[param4 + 12].color);
                newColors.Add(clrVar9);
                newColors.Add(clrVar11);
                newUVs.Add(new Vector2(0, 0));
                newUVs.Add(new Vector2(0, 0));
                newUVs.Add(new Vector2(0, 0));
                newVertices.Add(terrainWorld[param4 + 12] / tFactor);
                newVertices.Add(terrainWorld[param4 + 2] / tFactor);
                newVertices.Add(terrainWorld[param4 + 10] / tFactor);
                index = newVertices.Count - 1;
                newTriangles.Add(index - 2);
                newTriangles.Add(index - 1);
                newTriangles.Add(index);
            //}

            return;
        }

        if (uVar2 == 15)
        {
            if (GameManager.terrainScreen[param4].vert.x < 1 && GameManager.terrainScreen[param4 + 2].vert.x < 1 &&
                GameManager.terrainScreen[param4 + 10].vert.x < 1)
            {
                sVar1 = GameManager.DAT_1f800094;

                /*if (GameManager.terrainScreen[param4 + 12].vert.x < 0)
                    return;*/
            }
            else
                sVar1 = GameManager.DAT_1f800094;

            if (GameManager.terrainScreen[param4].vert.x < sVar1 || GameManager.terrainScreen[param4 + 2].vert.x < sVar1 || 
                GameManager.terrainScreen[param4 + 10].vert.x < sVar1)
            {
                sVar3 = (short)GameManager.terrainScreen[param4].vert.y;
            }
            else
            {
                sVar3 = (short)GameManager.terrainScreen[param4].vert.y;

                /*if (sVar1 <= GameManager.terrainScreen[param4 + 12].vert.x)
                    return;*/
            }

            sVar1 = (short)GameManager.terrainScreen[param4 + 2].vert.y;

            if (sVar3 < 1 && sVar1 < 1 && GameManager.terrainScreen[param4 + 10].vert.y < 1)
            {
                sVar3 = GameManager.DAT_1f800096;

                /*if (GameManager.terrainScreen[param4 + 12].vert.y < 0)
                    return;*/
            }
            else
                sVar3 = GameManager.DAT_1f800096;

            if (sVar1 == 0 && sVar3 <= sVar1 && sVar3 <= GameManager.terrainScreen[param4 + 10].vert.y)
            {
                iVar4 = GameManager.DAT_1f800084.y;

                /*if (sVar3 <= GameManager.terrainScreen[param4 + 12].vert.y)
                    return;*/
            }
            else
                iVar4 = GameManager.DAT_1f800084.y;

            Coprocessor.vector0.vz0 = (short)param2;
            uVar2 = (uint)(iVar4 * 0x10000);
            Coprocessor.vector1.vz1 = (short)(param2 + 256);
            Coprocessor.vector2.vz2 = (short)(param2 + 512);
            uVar13 = param1 + 256 & 0xffff | uVar2;
            cop2r0 = (vertices[puVar14 + 64] & 0x7ff) * 0x80000 + (int)uVar13;
            cop2r2 = (vertices[puVar14 + 65] & 0x7ff) * 0x80000 + (int)uVar13;
            cop2r4 = (vertices[puVar16 + 64] & 0x7ff) * 0x80000 + (int)uVar13;
            Coprocessor.vector0.vx0 = (short)cop2r0;
            Coprocessor.vector0.vy0 = (short)(cop2r0 >> 16);
            Coprocessor.vector1.vx1 = (short)cop2r2;
            Coprocessor.vector1.vy1 = (short)(cop2r2 >> 16);
            Coprocessor.vector2.vx2 = (short)cop2r4;
            Coprocessor.vector2.vy2 = (short)(cop2r4 >> 16);
            terrainVertices[0] = new Vector3Int(Coprocessor.vector0.vx0, Coprocessor.vector0.vy0, Coprocessor.vector0.vz0);
            terrainVertices[1] = new Vector3Int(Coprocessor.vector1.vx1, Coprocessor.vector1.vy1, Coprocessor.vector1.vz1);
            terrainVertices[2] = new Vector3Int(Coprocessor.vector2.vx2, Coprocessor.vector2.vy2, Coprocessor.vector2.vz2);
            Coprocessor.ExecuteRTPT(12, false);
            sVar1 = (short)vertices[puVar14 + 1];
            sVar3 = (short)vertices[puVar15 + 1];
            GameManager.terrainScreen[param4 + 1].vert.x = Coprocessor.screenXYFIFO.sx0;
            GameManager.terrainScreen[param4 + 1].vert.y = Coprocessor.screenXYFIFO.sy0;
            GameManager.terrainScreen[param4 + 11].vert.x = Coprocessor.screenXYFIFO.sx2;
            GameManager.terrainScreen[param4 + 11].vert.y = Coprocessor.screenXYFIFO.sy2;
            terrainWorld[param4 + 1] = terrainVertices[0];
            terrainWorld[param4 + 11] = terrainVertices[2];
            Coprocessor.vector0.vx0 = (short)param1;
            Coprocessor.vector0.vy0 = (short)((sVar1 & 0x7ff) * 0x80000 + uVar2 >> 16);
            cop2r4 = (sVar3 & 0x7ff) * 0x80000 + (int)(param1 + 512 & 0xffff | uVar2);
            Coprocessor.vector2.vx2 = (short)cop2r4;
            Coprocessor.vector2.vy2 = (short)(cop2r4 >> 16);
            Coprocessor.vector0.vz0 = (short)(param2 + 256);
            Coprocessor.vector2.vz2 = (short)(param2 + 256);
            terrainVertices[0] = new Vector3Int(Coprocessor.vector0.vx0, Coprocessor.vector0.vy0, Coprocessor.vector0.vz0);
            terrainVertices[2] = new Vector3Int(Coprocessor.vector2.vx2, Coprocessor.vector2.vy2, Coprocessor.vector2.vz2);
            GameManager.terrainScreen[param4 + 1].vert.z = Coprocessor.screenZFIFO.sz1;
            GameManager.terrainScreen[param4 + 11].vert.z = Coprocessor.screenZFIFO.sz3;
            Coprocessor.ExecuteRTPT(12, false);
            GameManager.terrainScreen[param4 + 5].vert = new Vector3Int(Coprocessor.screenXYFIFO.sx0, Coprocessor.screenXYFIFO.sy0, Coprocessor.screenZFIFO.sz1);
            GameManager.terrainScreen[param4 + 6].vert = new Vector3Int(Coprocessor.screenXYFIFO.sx1, Coprocessor.screenXYFIFO.sy1, Coprocessor.screenZFIFO.sz2);
            GameManager.terrainScreen[param4 + 7].vert = new Vector3Int(Coprocessor.screenXYFIFO.sx2, Coprocessor.screenXYFIFO.sy2, Coprocessor.screenZFIFO.sz3);
            terrainWorld[param4 + 5] = terrainVertices[0];
            terrainWorld[param4 + 6] = terrainVertices[1];
            terrainWorld[param4 + 7] = terrainVertices[2];
            in_t0 = vertices[puVar14];
            in_t1 = vertices[puVar14 + 64];
            in_t2 = vertices[puVar14 + 1];
            in_t3 = vertices[puVar14 + 65];
            in_t4 = tiles[puVar14];
            FUN_29520((int)param1, param2, param3, param4);
            in_t0 = vertices[puVar14 + 64];
            in_t1 = vertices[puVar15];
            in_t2 = vertices[puVar14 + 65];
            in_t3 = vertices[puVar15 + 1];
            in_t4 = tiles[puVar14 + 64];
            param4++;
            FUN_29520((int)param1, param2, param3, param4);
            in_t0 = vertices[puVar14 + 1];
            in_t1 = vertices[puVar14 + 65];
            in_t2 = vertices[puVar16];
            in_t3 = vertices[puVar16 + 64];
            in_t4 = tiles[puVar14 + 1];
            param4 += 4;
            FUN_29520((int)param1, param2, param3, param4);
            in_t0 = vertices[puVar14 + 65];
            in_t1 = vertices[puVar15 + 1];
            in_t2 = vertices[puVar16 + 64];
            in_t3 = vertices[puVar17];
            in_t4 = tiles[puVar14 + 65];
            param4 += 1;
            FUN_29520((int)param1, param2, param3, param4);
            return;
        }
        
        uVar13 = (uint)(GameManager.DAT_1f800084.y << 16);
        cop2r0 = (vertices[puVar14 + 1] & 0x7ff) * 0x80000 + (int)(uVar13 | param1);
        Coprocessor.vector0.vx0 = (short)cop2r0;
        Coprocessor.vector0.vy0 = (short)(cop2r0 >> 16);
        Coprocessor.vector0.vz0 = (short)(param2 + 256);
        terrainVertices[0] = new Vector3Int(Coprocessor.vector0.vx0, Coprocessor.vector0.vy0, Coprocessor.vector0.vz0);
        Coprocessor.ExecuteRTPS(12, false);
        Coprocessor.vector1.vz1 = (short)(param2 + 512);
        Coprocessor.vector2.vz2 = (short)(param2 + 256);
        uVar15 = param1 + 256 & 0xffff | uVar13;
        uVar16 = param1 + 512 & 0xffff | uVar13;
        Coprocessor.vector0.vz0 = (short)param2;
        cop2r0 = (vertices[puVar14 + 64] & 0x7ff) * 0x80000 + (int)uVar15;
        Coprocessor.vector0.vx0 = (short)cop2r0;
        Coprocessor.vector0.vy0 = (short)(cop2r0 >> 16);
        cop2r2 = (vertices[puVar16 + 64] & 0x7ff) * 0x80000 + (int)uVar15;
        Coprocessor.vector1.vx1 = (short)cop2r2;
        Coprocessor.vector1.vy1 = (short)(cop2r2 >> 16);
        cop2r4 = (vertices[puVar15 + 1] & 0x7ff) * 0x80000 + (int)uVar16;
        Coprocessor.vector2.vx2 = (short)cop2r4;
        Coprocessor.vector2.vy2 = (short)(cop2r4 >> 16);
        GameManager.terrainScreen[param4 + 5].vert = new Vector3Int(Coprocessor.screenXYFIFO.sx2, Coprocessor.screenXYFIFO.sy2, Coprocessor.screenZFIFO.sz3);
        terrainWorld[param4 + 5] = terrainVertices[0];
        terrainVertices[0] = new Vector3Int(Coprocessor.vector0.vx0, Coprocessor.vector0.vy0, Coprocessor.vector0.vz0);
        terrainVertices[1] = new Vector3Int(Coprocessor.vector1.vx1, Coprocessor.vector1.vy1, Coprocessor.vector1.vz1);
        terrainVertices[2] = new Vector3Int(Coprocessor.vector2.vx2, Coprocessor.vector2.vy2, Coprocessor.vector2.vz2);
        Coprocessor.ExecuteRTPT(12, false);
        GameManager.terrainScreen[param4 + 5].color = DAT_BA4F0[vertices[puVar14 + 1] >> 11];
        GameManager.terrainScreen[param4 + 1].color = DAT_BA4F0[vertices[puVar14 + 64] >> 11];
        GameManager.terrainScreen[param4 + 11].color = DAT_BA4F0[vertices[puVar16 + 64] >> 11];
        GameManager.terrainScreen[param4 + 7].color = DAT_BA4F0[vertices[puVar15 + 1] >> 11];
        in_t1 = GameManager.DAT_639EC[(uVar2 - 1) * 2] >> 1;
        in_t0 = GameManager.DAT_639EC[(uVar2 - 1) * 2 + 1] >> 1;
        GameManager.terrainScreen[param4 + 1].vert.x = Coprocessor.screenXYFIFO.sx0;
        GameManager.terrainScreen[param4 + 1].vert.y = Coprocessor.screenXYFIFO.sy0;
        GameManager.terrainScreen[param4 + 11].vert.x = Coprocessor.screenXYFIFO.sx1;
        GameManager.terrainScreen[param4 + 11].vert.y = Coprocessor.screenXYFIFO.sy1;
        GameManager.terrainScreen[param4 + 7].vert.x = Coprocessor.screenXYFIFO.sx2;
        GameManager.terrainScreen[param4 + 7].vert.y = Coprocessor.screenXYFIFO.sy2;
        terrainWorld[param4 + 1] = terrainVertices[0];
        terrainWorld[param4 + 11] = terrainVertices[1];
        terrainWorld[param4 + 7] = terrainVertices[2];
        FUN_297E8(param1, param2, param3, param4);
    }

    private void FUN_29520(int param1, int param2, Queue<ScreenPoly> param3, int param4)
    {
        int iVar3;
        Color32 clrVar3;
        Color32 clrVar4;
        uint uVar5;
        int iVar6;
        Color32 clrVar6;
        int iVar7;
        Color32 clrVar7;
        Color32 clrVar8;
        int iVar9;
        short sVar12;
        uint uVar13;
        int index;

        int tFactor = 1;
        iVar3 = (ushort)GameManager.DAT_1f80009a - 4096;
        iVar9 = in_t4;

        if ((tileData[iVar9].unk1 & 1) == 0)
        {
            uVar5 = (uint)GameManager.terrainScreen[param4].vert.z;

            if (GameManager.terrainScreen[param4].vert.z < GameManager.terrainScreen[param4 + 1].vert.z)
                uVar5 = (uint)GameManager.terrainScreen[param4 + 1].vert.z;

            iVar6 = (int)(in_t1 >> 11);

            if ((int)uVar5 < GameManager.terrainScreen[param4 + 5].vert.z)
                uVar5 = (uint)GameManager.terrainScreen[param4 + 5].vert.z;

            uVar13 = (uint)GameManager.terrainScreen[param4 + 6].vert.z;
            iVar7 = (int)(in_t2 >> 11);

            if ((int)uVar5 < (int)uVar13)
                uVar5 = uVar13;

            Coprocessor.screenXYFIFO.sx0 = (short)GameManager.terrainScreen[param4].vert.x;
            Coprocessor.screenXYFIFO.sy0 = (short)GameManager.terrainScreen[param4].vert.y;

            if (0 < (int)uVar5)
            {
                Coprocessor.screenXYFIFO.sx1 = (short)GameManager.terrainScreen[param4 + 1].vert.x;
                Coprocessor.screenXYFIFO.sy1 = (short)GameManager.terrainScreen[param4 + 1].vert.y;

                if ((tileData[iVar9].unk1 & 2) == 0)
                {
                    Coprocessor.screenXYFIFO.sx2 = (short)GameManager.terrainScreen[param4 + 5].vert.x;
                    Coprocessor.screenXYFIFO.sy2 = (short)GameManager.terrainScreen[param4 + 5].vert.y;
                    iVar3 = (int)uVar5 - iVar3;
                    Coprocessor.ExecuteNCLIP();
                    sVar12 = (short)GameManager.instance.DAT_DA8;

                    if (0 < iVar3)
                        sVar12 = (short)(GameManager.instance.DAT_DA8 + ((uint)iVar3 >> 8) * 64);

                    clrVar6 = GameManager.DAT_1f800000[iVar6];
                    iVar3 = Coprocessor.mathsAccumulator.mac0;
                    clrVar7 = GameManager.DAT_1f800000[iVar7];

                    //if (iVar3 < 1)
                    //{
                        clrVar4 = GameManager.DAT_1f800000[in_t0 >> 11];
                        newVertices.Add(terrainWorld[param4] / tFactor);
                        newVertices.Add(terrainWorld[param4 + 1] / tFactor);
                        newVertices.Add(terrainWorld[param4 + 5] / tFactor);
                        newColors.Add(clrVar4);
                        newColors.Add(clrVar6);
                        newColors.Add(clrVar7);
                        newUVs.Add(new Vector2(tileData[iVar9].uv1_x, tileData[iVar9].uv1_y));
                        newUVs.Add(new Vector2(tileData[iVar9].uv2_x, tileData[iVar9].uv2_y));
                        newUVs.Add(new Vector2(tileData[iVar9].uv3_x, tileData[iVar9].uv2_y));
                        index = newVertices.Count - 1;
                        newTriangles.Add(index);
                        newTriangles.Add(index - 1);
                        newTriangles.Add(index - 2);
                    //}

                    Coprocessor.screenXYFIFO.sx0 = (short)GameManager.terrainScreen[param4 + 6].vert.x;
                    Coprocessor.screenXYFIFO.sy0 = (short)GameManager.terrainScreen[param4 + 6].vert.y;
                    Coprocessor.ExecuteNCLIP();
                    clrVar4 = GameManager.DAT_1f800000[in_t3 >> 11];

                    //if (-1 < Coprocessor.mathsAccumulator.mac0)
                    //{
                        newVertices.Add(terrainWorld[param4 + 6] / tFactor);
                        newVertices.Add(terrainWorld[param4 + 1] / tFactor);
                        newVertices.Add(terrainWorld[param4 + 5] / tFactor);
                        newColors.Add(clrVar4);
                        newColors.Add(clrVar6);
                        newColors.Add(clrVar7);
                        newUVs.Add(new Vector2(tileData[iVar9].uv4_x, tileData[iVar9].uv4_y));
                        newUVs.Add(new Vector2(tileData[iVar9].uv2_x, tileData[iVar9].uv2_y));
                        newUVs.Add(new Vector2(tileData[iVar9].uv3_x, tileData[iVar9].uv3_y));
                        index = newVertices.Count - 1;
                        newTriangles.Add(index - 2);
                        newTriangles.Add(index - 1);
                        newTriangles.Add(index);
                    //}
                }
                else
                {
                    Coprocessor.screenXYFIFO.sx2 = (short)GameManager.terrainScreen[param4 + 6].vert.x;
                    Coprocessor.screenXYFIFO.sy2 = (short)GameManager.terrainScreen[param4 + 6].vert.y;
                    iVar3 = (int)uVar5 - iVar3;
                    Coprocessor.ExecuteNCLIP();
                    sVar12 = (short)GameManager.instance.DAT_DA8;

                    if (0 < iVar3)
                        sVar12 = (short)(GameManager.instance.DAT_DA8 + ((uint)iVar3 >> 8) * 64);

                    clrVar4 = GameManager.DAT_1f800000[in_t0 >> 11];
                    clrVar8 = GameManager.DAT_1f800000[in_t3 >> 11];

                    //if (Coprocessor.mathsAccumulator.mac0 < 1)
                    //{
                        clrVar6 = GameManager.DAT_1f800000[iVar6];
                        newVertices.Add(terrainWorld[param4] / tFactor);
                        newVertices.Add(terrainWorld[param4 + 1] / tFactor);
                        newVertices.Add(terrainWorld[param4 + 6] / tFactor);
                        newColors.Add(clrVar4);
                        newColors.Add(clrVar6);
                        newColors.Add(clrVar8);
                        newUVs.Add(new Vector2(tileData[iVar9].uv1_x, tileData[iVar9].uv1_y));
                        newUVs.Add(new Vector2(tileData[iVar9].uv2_x, tileData[iVar9].uv2_y));
                        newUVs.Add(new Vector2(tileData[iVar9].uv4_x, tileData[iVar9].uv4_y));
                        index = newVertices.Count - 1;
                        newTriangles.Add(index);
                        newTriangles.Add(index - 1);
                        newTriangles.Add(index - 2);
                    //}

                    Coprocessor.screenXYFIFO.sx1 = (short)GameManager.terrainScreen[param4 + 5].vert.x;
                    Coprocessor.screenXYFIFO.sy1 = (short)GameManager.terrainScreen[param4 + 5].vert.y;
                    clrVar3 = GameManager.DAT_1f800000[iVar7];
                    Coprocessor.ExecuteNCLIP();

                    //if (-1 < Coprocessor.mathsAccumulator.mac0)
                    //{
                        newVertices.Add(terrainWorld[param1] / tFactor);
                        newVertices.Add(terrainWorld[param1 + 5] / tFactor);
                        newVertices.Add(terrainWorld[param1 + 6] / tFactor);
                        newColors.Add(clrVar4);
                        newColors.Add(clrVar3);
                        newColors.Add(clrVar8);
                        newUVs.Add(new Vector2(tileData[iVar9].uv1_x, tileData[iVar9].uv1_y));
                        newUVs.Add(new Vector2(tileData[iVar9].uv3_x, tileData[iVar9].uv3_y));
                        newUVs.Add(new Vector2(tileData[iVar9].uv4_x, tileData[iVar9].uv4_y));
                        index = newVertices.Count - 1;
                        newTriangles.Add(index - 2);
                        newTriangles.Add(index - 1);
                        newTriangles.Add(index);
                    //}
                }
            }
        }
    }

    private void FUN_297E8(uint param1, int param2, Queue<ScreenPoly> param3, int param4)
    {
        uint uVar2;
        int iVar4;
        Color clrVar4;
        int iVar6;
        Color32 clrVar6;
        int iVar9;
        Color32 clrVar9;
        int iVar11;
        Color32 clrVar11;
        Color32 clrVar14;
        int index;

        int tFactor = 1;
        iVar4 = (int)(in_t1 & 0x1f) + param4;
        iVar6 = (int)(in_t1 >> 1 & 0x1f0) / 16 + param4;
        iVar9 = (int)(in_t1 >> 6 & 0x1f0) / 16 + param4;
        Coprocessor.screenXYFIFO.sx0 = (short)GameManager.terrainScreen[iVar4].vert.x;
        Coprocessor.screenXYFIFO.sy0 = (short)GameManager.terrainScreen[iVar4].vert.y;
        Coprocessor.screenXYFIFO.sx1 = (short)GameManager.terrainScreen[iVar6].vert.x;
        Coprocessor.screenXYFIFO.sy1 = (short)GameManager.terrainScreen[iVar6].vert.y;
        Coprocessor.screenXYFIFO.sx2 = (short)GameManager.terrainScreen[iVar9].vert.x;
        Coprocessor.screenXYFIFO.sy2 = (short)GameManager.terrainScreen[iVar9].vert.y;
        clrVar11 = GameManager.terrainScreen[iVar6].color;
        Coprocessor.ExecuteNCLIP();
        clrVar14 = GameManager.terrainScreen[iVar9].color;

        //if (-1 < Coprocessor.mathsAccumulator.mac0)
        //{
            newVertices.Add(terrainWorld[iVar4] / tFactor);
            newVertices.Add(terrainWorld[iVar6] / tFactor);
            newVertices.Add(terrainWorld[iVar9] / tFactor);
            newColors.Add(GameManager.terrainScreen[iVar4].color);
            newColors.Add(clrVar11);
            newColors.Add(clrVar14);
            newUVs.Add(new Vector2(0, 0));
            newUVs.Add(new Vector2(0, 0));
            newUVs.Add(new Vector2(0, 0));
            index = newVertices.Count - 1;
            newTriangles.Add(index - 2);
            newTriangles.Add(index - 1);
            newTriangles.Add(index);
        //}

        int _iVar9 = iVar9;
        iVar9 = (int)(in_t1 >> 11 & 0x1f0) / 16 + param4;
        Coprocessor.screenXYFIFO.sx0 = (short)GameManager.terrainScreen[iVar9].vert.x;
        Coprocessor.screenXYFIFO.sy0 = (short)GameManager.terrainScreen[iVar9].vert.y;
        clrVar9 = GameManager.terrainScreen[iVar9].color;
        Coprocessor.ExecuteNCLIP();
        iVar11 = (int)(in_t1 >> 16 & 0x1f0) / 16 + param4;
        Coprocessor.screenXYFIFO.sx1 = (short)GameManager.terrainScreen[iVar11].vert.x;
        Coprocessor.screenXYFIFO.sy1 = (short)GameManager.terrainScreen[iVar11].vert.y;
        clrVar6 = GameManager.terrainScreen[iVar11].color;

        //if (Coprocessor.mathsAccumulator.mac0 < 1)
        //{
            newVertices.Add(terrainWorld[iVar9] / tFactor);
            newVertices.Add(terrainWorld[iVar6] / tFactor);
            newVertices.Add(terrainWorld[_iVar9] / tFactor);
            newColors.Add(clrVar9);
            newColors.Add(clrVar11);
            newColors.Add(clrVar14);
            newUVs.Add(new Vector2(0, 0));
            newUVs.Add(new Vector2(0, 0));
            newUVs.Add(new Vector2(0, 0));
            index = newVertices.Count - 1;
            newTriangles.Add(index);
            newTriangles.Add(index - 1);
            newTriangles.Add(index - 2);
        //}

        Coprocessor.ExecuteNCLIP();
        
        //if (-1 < Coprocessor.mathsAccumulator.mac0)
        //{
            newVertices.Add(terrainWorld[iVar9] / tFactor);
            newVertices.Add(terrainWorld[iVar11] / tFactor);
            newVertices.Add(terrainWorld[_iVar9] / tFactor);
            newColors.Add(clrVar9);
            newColors.Add(clrVar6);
            newColors.Add(clrVar14);
            newUVs.Add(new Vector2(0, 0));
            newUVs.Add(new Vector2(0, 0));
            newUVs.Add(new Vector2(0, 0));
            index = newVertices.Count - 1;
            newTriangles.Add(index - 2);
            newTriangles.Add(index - 1);
            newTriangles.Add(index);
        //}
        
        iVar6 = (int)(in_t0 & 0x1f) + param4;
        iVar9 = (int)(in_t0 >> 1 & 0x1f0) / 16 + param4;
        iVar11 = (int)(in_t0 >> 6 & 0x1f0) / 16 + param4;
        Coprocessor.screenXYFIFO.sx0 = (short)GameManager.terrainScreen[iVar6].vert.x;
        Coprocessor.screenXYFIFO.sy0 = (short)GameManager.terrainScreen[iVar6].vert.y;
        Coprocessor.screenXYFIFO.sx1 = (short)GameManager.terrainScreen[iVar9].vert.x;
        Coprocessor.screenXYFIFO.sy1 = (short)GameManager.terrainScreen[iVar9].vert.y;
        Coprocessor.screenXYFIFO.sx2 = (short)GameManager.terrainScreen[iVar11].vert.x;
        Coprocessor.screenXYFIFO.sy2 = (short)GameManager.terrainScreen[iVar11].vert.y;
        clrVar9 = GameManager.terrainScreen[iVar9].color;
        Coprocessor.ExecuteNCLIP();
        clrVar11 = GameManager.terrainScreen[iVar11].color;
        uVar2 = in_t0 >> 11 & 0x1f0;
        iVar4 = (int)uVar2 / 16 + param4;

        if (uVar2 != 0)
        {
            //if (Coprocessor.mathsAccumulator.mac0 < 1)
            //{
                newVertices.Add(terrainWorld[iVar6] / tFactor);
                newVertices.Add(terrainWorld[iVar9] / tFactor);
                newVertices.Add(terrainWorld[iVar11] / tFactor);
                newColors.Add(GameManager.terrainScreen[iVar6].color);
                newColors.Add(clrVar9);
                newColors.Add(clrVar11);
                newUVs.Add(new Vector2(0, 0));
                newUVs.Add(new Vector2(0, 0));
                newUVs.Add(new Vector2(0, 0));
                index = newVertices.Count - 1;
                newTriangles.Add(index);
                newTriangles.Add(index - 1);
                newTriangles.Add(index - 2);
            //}
            
            Coprocessor.screenXYFIFO.sx0 = (short)GameManager.terrainScreen[iVar4].vert.x;
            Coprocessor.screenXYFIFO.sy0 = (short)GameManager.terrainScreen[iVar4].vert.y;
            clrVar4 = GameManager.terrainScreen[iVar4].color;
            Coprocessor.ExecuteNCLIP();
            uVar2 = in_t0 >> 16 & 0x1f0;
            param4 = (int)uVar2 / 16 + param4;

            if (uVar2 == 0)
            {
                /*if (Coprocessor.mathsAccumulator.mac0 < 0)
                    return;*/

                newVertices.Add(terrainWorld[iVar4] / tFactor);
                newVertices.Add(terrainWorld[iVar9] / tFactor);
                newVertices.Add(terrainWorld[iVar11] / tFactor);
                newColors.Add(clrVar4);
                newColors.Add(clrVar9);
                newColors.Add(clrVar11);
                newUVs.Add(new Vector2(0, 0));
                newUVs.Add(new Vector2(0, 0));
                newUVs.Add(new Vector2(0, 0));
                index = newVertices.Count - 1;
                newTriangles.Add(index - 2);
                newTriangles.Add(index - 1);
                newTriangles.Add(index);
                return;
            }

            Coprocessor.screenXYFIFO.sx1 = (short)GameManager.terrainScreen[param4].vert.x;
            Coprocessor.screenXYFIFO.sy1 = (short)GameManager.terrainScreen[param4].vert.y;
            clrVar6 = GameManager.terrainScreen[param4].color;

            //if (-1 < Coprocessor.mathsAccumulator.mac0)
            //{
                newVertices.Add(terrainWorld[iVar4] / tFactor);
                newVertices.Add(terrainWorld[iVar9] / tFactor);
                newVertices.Add(terrainWorld[iVar11] / tFactor);
                newColors.Add(clrVar4);
                newColors.Add(clrVar9);
                newColors.Add(clrVar11);
                newUVs.Add(new Vector2(0, 0));
                newUVs.Add(new Vector2(0, 0));
                newUVs.Add(new Vector2(0, 0));
                index = newVertices.Count - 1;
                newTriangles.Add(index - 2);
                newTriangles.Add(index - 1);
                newTriangles.Add(index);
            //}

            Coprocessor.ExecuteNCLIP();

            /*if (0 < Coprocessor.mathsAccumulator.mac0)
                return;*/

            newVertices.Add(terrainWorld[iVar4] / tFactor);
            newVertices.Add(terrainWorld[param4] / tFactor);
            newVertices.Add(terrainWorld[iVar11] / tFactor);
            newColors.Add(clrVar4);
            newColors.Add(clrVar6);
            newColors.Add(clrVar11);
            newUVs.Add(new Vector2(0, 0));
            newUVs.Add(new Vector2(0, 0));
            newUVs.Add(new Vector2(0, 0));
            index = newVertices.Count - 1;
            newTriangles.Add(index);
            newTriangles.Add(index - 1);
            newTriangles.Add(index - 2);
        }
        else
        {
            /*if (0 < Coprocessor.mathsAccumulator.mac0)
                return;*/

            newVertices.Add(terrainWorld[iVar6] / tFactor);
            newVertices.Add(terrainWorld[iVar9] / tFactor);
            newVertices.Add(terrainWorld[iVar11] / tFactor);
            newColors.Add(GameManager.terrainScreen[iVar6].color);
            newColors.Add(clrVar9);
            newColors.Add(clrVar11);
            newUVs.Add(new Vector2(0, 0));
            newUVs.Add(new Vector2(0, 0));
            newUVs.Add(new Vector2(0, 0));
            index = newVertices.Count - 1;
            newTriangles.Add(index);
            newTriangles.Add(index - 1);
            newTriangles.Add(index - 2);
        }
    }
}
