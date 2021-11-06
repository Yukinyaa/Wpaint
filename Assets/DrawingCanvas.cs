using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class DrawingCanvas : MonoBehaviour
{
    Camera cam;
    RenderTexture texture;


    [SerializeField]
    private Vector2 scale = Vector2.one;
    
    GameObject pen;


    public void Initalize(int width, int height)
    {
        if (!cam)
            cam = GetComponent<Camera>();
        texture = new RenderTexture(width, height, 1);
        cam.targetTexture = texture;
        scale = new Vector2(width, height);
    }
    public void ResetCanvas()
    {
        cam.clearFlags = CameraClearFlags.Color;
        cam.backgroundColor = Color.clear;
        cam.Render();
        cam.clearFlags = CameraClearFlags.Nothing;
    }

    internal Texture GetTexture()
    {
        return texture;
    }
}
