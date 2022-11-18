using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;


public static class ShaderHelper
{
    public static RenderTexture CreateTexture(Vector2Int texResolution)
    {
        RenderTexture texture = new RenderTexture(texResolution.x, texResolution.y, 0);
        texture.enableRandomWrite = true;
        texture.Create();
        return texture;
    }
}