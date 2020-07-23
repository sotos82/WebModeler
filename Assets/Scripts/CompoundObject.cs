using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompoundObject : MonoBehaviour {

    protected List<PrimitiveObject> primObjectList = new List<PrimitiveObject>();

    protected enum InitialShape { Quad, Cube };
    protected InitialShape initialShape;

    public static CompoundObject MakeCube(Transform parent, float size, string name = "") {

        GameObject go = new GameObject("Cube");
        if (name != "") {
            go.name = name;
        }
        go.transform.SetParent(parent);

        go.AddComponent<CompoundObject>();
        CompoundObject co = go.GetComponent<CompoundObject>();

        co.initialShape = InitialShape.Cube;

        co.primObjectList.Add(PrimitiveObject.MakeQuad(go.transform, size, new Vector3(0, 0.5f * size, 0), PrimitiveObject.QuadOrientation.up));
        co.primObjectList.Add(PrimitiveObject.MakeQuad(go.transform, size, new Vector3(0, -0.5f * size, 0), PrimitiveObject.QuadOrientation.down));
        co.primObjectList.Add(PrimitiveObject.MakeQuad(go.transform, size, new Vector3(-0.5f * size, 0, 0), PrimitiveObject.QuadOrientation.left));
        co.primObjectList.Add(PrimitiveObject.MakeQuad(go.transform, size, new Vector3(0.5f * size, 0, 0), PrimitiveObject.QuadOrientation.right));
        co.primObjectList.Add(PrimitiveObject.MakeQuad(go.transform, size, new Vector3(0, 0, 0.5f * size), PrimitiveObject.QuadOrientation.front));
        co.primObjectList.Add(PrimitiveObject.MakeQuad(go.transform, size, new Vector3(0, 0, -0.5f * size), PrimitiveObject.QuadOrientation.back));

        return co;
    }

    public static CompoundObject MakeQuad(Transform parent, float size, string name = "") {

        GameObject go = new GameObject("Quad");
        if (name != "") {
            go.name = name;
        }
        go.transform.SetParent(parent);

        go.AddComponent<CompoundObject>();
        CompoundObject co = go.GetComponent<CompoundObject>();

        co.initialShape = InitialShape.Quad;

        co.primObjectList.Add(PrimitiveObject.MakeQuad(go.transform, size, new Vector3(0, 0, 0), PrimitiveObject.QuadOrientation.up));
        co.primObjectList.Add(PrimitiveObject.MakeQuad(go.transform, size, new Vector3(0, 0, 0), PrimitiveObject.QuadOrientation.down));

        return co;
    }

    public float Magnitute { get; set; }

    public void UpdateUniformlyCompoundbject(float magnitude) {

        float magnScale = 2f;
        Magnitute = primObjectList[0].Magnitute;

        for (int i = 0; i < primObjectList.Count; i++) {
            PrimitiveObject po = primObjectList[i];
            po.UpdateUniformlyPrimitiveObject(magnScale * magnitude, initialShape == InitialShape.Cube);
        }
    }
    private readonly float eps = 0.00001f;
    protected List<PrimitiveObject> extrFacesList;
    public void ExtrudeFace(PrimitiveObject face, float magnitude) {

        Vector3 normal = face.GetNormal;
        Vector3 normEps = normal * eps;
        Vector3[] verticesFace = face.PrimMeshFilter.mesh.vertices;

        int[] indiceSequence = new int[] { 0, 1, 3, 2, 0 };

        if (extrFacesList == null) {
            extrFacesList = new List<PrimitiveObject>();

            face.initPos = face.transform.position;

            for (int i = 0; i < verticesFace.Length; i++) {
                Vector3[] vs1 = new Vector3[4] {    verticesFace[indiceSequence[i]] - normEps,
                                                    verticesFace[indiceSequence[i+1]] - normEps,
                                                    verticesFace[indiceSequence[i]] + normEps,
                                                    verticesFace[indiceSequence[i+1]] + normEps };

                PrimitiveObject newPo = PrimitiveObject.MakeRectangle(transform, vs1, (face.transform.position + face.initPos) * 0.5f + (verticesFace[indiceSequence[i]] + verticesFace[indiceSequence[i + 1]]) * 0.5f, true);
                primObjectList.Add(newPo);
                extrFacesList.Add(newPo);
            }
        }

        if (extrFacesList != null) {
            face.MoveFaceTowardsNormal(magnitude);
            for (int i = 0; i < extrFacesList.Count; i++) {
                PrimitiveObject extrPo = extrFacesList[i];

                extrPo.transform.position = (face.transform.position + face.initPos) * 0.5f + (verticesFace[indiceSequence[i]] + verticesFace[indiceSequence[i + 1]]) * 0.5f;

                Vector3[] vertices = extrPo.PrimMeshFilter.mesh.vertices;

                vertices[0] = face.initPos + verticesFace[indiceSequence[i]] - extrPo.transform.position - normEps;
                vertices[1] = face.initPos + verticesFace[indiceSequence[i + 1]] - extrPo.transform.position - normEps;
                vertices[2] = face.transform.position + verticesFace[indiceSequence[i]] - extrPo.transform.position + normEps;
                vertices[3] = face.transform.position + verticesFace[indiceSequence[i + 1]] - extrPo.transform.position + normEps;

                extrPo.PrimMeshFilter.mesh.vertices = vertices;
                extrPo.PrimMeshFilter.mesh.RecalculateBounds();

                for (int j = 0; j < vertices.Length; j++) {
                    extrPo.PointList[j].transform.position = vertices[j] + extrPo.transform.position;
                }

                MeshCollider col = extrPo.GetComponent<MeshCollider>();
                col.sharedMesh = extrPo.PrimMeshFilter.mesh;
            }

        }
    }

    public void FinishExtrude() {
        extrFacesList = null;
    }

    //protected bool isSelected;
    public bool IsSelected { get; set; }

    public void RestoreColor() {
        for (int i = 0; i < primObjectList.Count; i++) {
            PrimitiveObject po = primObjectList[i];

            po.RestoreColor();
        }
    }

    public void SetColor(Color c) {
        for (int i = 0; i < primObjectList.Count; i++) {
            PrimitiveObject po = primObjectList[i];

            po.SetColor(c);
        }
    }

    public void SelectAll(bool select) {
        IsSelected = select;

        for (int i = 0; i < primObjectList.Count; i++) {
            PrimitiveObject po = primObjectList[i];

            po.IsSelected = select;
        }
    }
}