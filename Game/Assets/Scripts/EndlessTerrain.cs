﻿using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public class EndlessTerrain : MonoBehaviour
{
    public const float maxViewDist = 450;
    public Transform viewer;

    public static Vector2 viewerPosition;
    static MapGenerator mapGenerator;
    int chunkSize;
    int chunkVisibleInViewDst;
    
    Dictionary<Vector2,TerrainChunk> terrainChunkDictionary=new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> 
        terrainChunksVisibleLastUpdate=new List<TerrainChunk>();

    private void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunkVisibleInViewDst = Mathf.RoundToInt(maxViewDist / chunkSize);
    }

    private void Update()
    {
        viewerPosition=new Vector2(viewer.position.x,viewer.position.z);
        UpdateVisibleChunks();
    }

    void UpdateVisibleChunks()
    {
        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();
        
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunkVisibleInViewDst; yOffset <= chunkSize; yOffset++)
        {
            for (int xOffset = -chunkVisibleInViewDst; xOffset <= chunkSize; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX+xOffset,currentChunkCoordY+yOffset);

                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunks();
                    if (terrainChunkDictionary[viewedChunkCoord].IsVisible())
                    {
                        terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
                    }
                }
                else
                {
                    terrainChunkDictionary.Add(viewedChunkCoord,new TerrainChunk(viewedChunkCoord,chunkSize,transform));
                }
            }
        }
    }
    
    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;
        
        public TerrainChunk(Vector2 coord, int size,Transform parent)
        {
            position = coord * size;
            bounds=new Bounds(position,Vector2.one*size);
            Vector3 positionV3=new Vector3(position.x,0,position.y);

            meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshObject.transform.position = positionV3;
            meshObject.transform.localScale = Vector3.one * size / 10;
            meshObject.transform.parent = parent;
            SetVisible(false);
            
            mapGenerator.RequestMapData(OnMapDataRecieve);
        }

        void OnMapDataRecieve(MapData mapData)
        {
            print("Map data recieved;  ");
        }

        public void UpdateTerrainChunks()
        {
            float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewerDstFromNearestEdge <= maxViewDist;
            SetVisible(visible);
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }
}