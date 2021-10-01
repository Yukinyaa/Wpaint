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
    
    //Before any objects are drawn
    void OnPreCull___()
    {
        //Find and store the attached camera if we don't already have it cached
        if (!cam)
            cam = GetComponent<Camera>();

        //These functions create projection matrices for both orthographic and perspective projection. Choose between them based on the camera's settings
        Matrix4x4 proj;
        if (cam.orthographic)
            proj = Matrix4x4.Ortho(-cam.orthographicSize * scale.x, cam.orthographicSize * scale.x, -cam.orthographicSize * scale.y, cam.orthographicSize * scale.y, cam.nearClipPlane, cam.farClipPlane);
        else
            proj = Matrix4x4.Perspective(cam.fieldOfView, scale.x / scale.y, cam.nearClipPlane, cam.farClipPlane);

        //Set the camera's projection matrix
        cam.projectionMatrix = proj;
    }

    internal Texture GetTexture()
    {
        return texture;
    }
}
