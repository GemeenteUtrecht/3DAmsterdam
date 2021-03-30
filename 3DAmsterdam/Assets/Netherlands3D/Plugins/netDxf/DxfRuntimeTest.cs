﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConvertCoordinates;
using Netherlands3D.LayerSystem;
using Netherlands3D.Interface;

public class DxfRuntimeTest : MonoBehaviour
{
    [SerializeField]
    private LoadingScreen loadingScreen;
    private MeshClipper.RDBoundingBox boundingbox;
    // Start is called before the first frame update
    void Start()
    {
        //List<Vector3RD> verts = new List<Vector3RD>();
        //verts.Add(new Vector3RD(0, 0, 0));
        //verts.Add(new Vector3RD(0, 1, 0));
        //verts.Add(new Vector3RD(0, 0, 1));
        //verts.Add(new Vector3RD(0, 0, 0));
        //verts.Add(new Vector3RD(0, 1, 0));
        //verts.Add(new Vector3RD(0, 0, 1));
        //verts.Add(new Vector3RD(0, 0, 0));
        //verts.Add(new Vector3RD(0, 1, 0));
        //verts.Add(new Vector3RD(0, 0, 1));

        //DxfFile file = new DxfFile();
        //file.SetupDXF();
        //file.AddLayer(verts,"testlaag");
        //file.Save();
    }

    public void CreateDXF(Bounds UnityBounds, List<Layer> layerList)
    {

        StartCoroutine(createFile(UnityBounds, layerList));
    }

    private IEnumerator createFile(Bounds UnityBounds, List<Layer> layerList)
    {

        Debug.Log(layerList.Count);
        Vector3RD bottomLeftRD = CoordConvert.UnitytoRD(UnityBounds.min);
        Vector3RD topRightRD = CoordConvert.UnitytoRD(UnityBounds.max);
        boundingbox = new MeshClipper.RDBoundingBox(bottomLeftRD.x, bottomLeftRD.y, topRightRD.x, topRightRD.y);
        DxfFile file = new DxfFile();
        file.SetupDXF();
        yield return null;
        MeshClipper meshClipper = new MeshClipper();
        
        loadingScreen.ShowMessage("dxf-bestand genereren...");
        loadingScreen.ProgressBar.Percentage(0f);
        int layercounter = 0;
        foreach (var layer in layerList)
        {
            List<GameObject> gameObjectsToClip = getTilesInLayer(layer, bottomLeftRD, topRightRD);
            if (gameObjectsToClip.Count==0)
            {
                continue;
            }
            foreach (var gameObject in gameObjectsToClip)
            {
                meshClipper.SetGameObject(gameObject);
                for (int submeshID = 0; submeshID < gameObject.GetComponent<MeshFilter>().sharedMesh.subMeshCount; submeshID++)
                {
                    meshClipper.clipSubMesh(boundingbox, submeshID);
                    string layerName = gameObject.GetComponent<MeshRenderer>().materials[submeshID].name.Replace(" (Instance)","");
                    
                    file.AddLayer(meshClipper.clippedVerticesRD, layerName,getColor(gameObject.GetComponent<MeshRenderer>().materials[submeshID]));
                    yield return null;
                }
            }
            loadingScreen.ProgressBar.Percentage(100*layercounter/layerList.Count);
            layercounter++;
        }
        file.Save();
        loadingScreen.Hide();
        Debug.Log("file saved");
    }

    private netDxf.AciColor getColor(Material material)
    {

        return netDxf.AciColor.LightGray;

        Color color;
        try
        {
            color = material.color;
            byte r = (byte)(material.color.r * 256);
            byte g = (byte)(material.color.g * 256);
            byte b = (byte)(material.color.b * 256);
            return new netDxf.AciColor(r, g, b);

        }
        catch (System.Exception)
        {

        


        if (material.GetColor("_BaseColor") !=null)
        {
            byte r = (byte)(material.GetColor("_BaseColor").r * 256);
            byte g = (byte)(material.GetColor("_BaseColor").g * 256);
            byte b = (byte)(material.GetColor("_BaseColor").b * 256);
            return new netDxf.AciColor(r, g, b);
        }
        else
        {
            
        }
        }
    }

    public List<GameObject> getTilesInLayer(Layer layer, Vector3RD bottomLeftRD, Vector3RD topRightRD)
    {
        if (layer == null)
        {
            return new List<GameObject>();
        }
        List<GameObject> output = new List<GameObject>();
        double tilesize = layer.tileSize;
        Debug.Log(tilesize);
        int tileX;
        int tileY;
        foreach (var tile in layer.tiles)
        {
            tileX = tile.Key.x;
            tileY = tile.Key.y;

            if (tileX+tilesize < bottomLeftRD.x || tileX > topRightRD.x)
            {
                continue;
            }
            if (tileY+tilesize<bottomLeftRD.y || tileY > topRightRD.y)
            {
                continue;
            }
            output.Add(tile.Value.gameObject);
        }
        return output;
    }
    
}
