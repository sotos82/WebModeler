using UnityEngine;
using System.Collections.Generic;

public class LoadResources : MonoBehaviour {

    public static GameObject point;
    public static Shader doubleSideShader;

    public static Material defaultMaterial;

    void Awake() {

        defaultMaterial = Resources.Load("Materials/defaultMaterial") as Material;
        point = Resources.Load("Prefabs/Point") as GameObject;
    }
}
