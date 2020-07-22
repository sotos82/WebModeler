using UnityEngine;
using System.Collections.Generic;

public class LoadResources : MonoBehaviour {

    public static Shader doubleSideShader;

    public static string VERSION = "PROTO_v0.1";

    void Awake() {
        doubleSideShader = Shader.Find("Ciconia Studio/Double Sided/Standard/Diffuse Bump");

    }
}
