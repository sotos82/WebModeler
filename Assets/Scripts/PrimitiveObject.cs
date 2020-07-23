using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimitiveObject : MonoBehaviour {

    public static readonly int[] indiceSequence = { 0, 1, 3, 2, 0 };
    public static readonly float eps = 0.0001f;

    public enum QuadOrientation { up, down, left, right, front, back }

    public MeshFilter PrimMeshFilter { protected set; get; }
    public Vector3 initPos;

    public List<Point> PointList { protected set; get; } = new List<Point>();

    private Color initialColor;
    protected void Awake() {

        gameObject.layer = 9;

        initialColor = GetComponent<Renderer>().material.color;
    }

    public void SetPointsActive(bool set) {
        for (int i = 0; i < PointList.Count; i++) {
            PointList[i].gameObject.SetActive(set);
        }
    }

    #region MakeXStripe
    public static PrimitiveObject MakeXStripe(Transform parent, float length, float width, Vector3 pos, bool hasCollider) {

        Vector3[] lineVertices = new Vector3[4];
        lineVertices[0] = new Vector3(-length, 0, -width);
        lineVertices[1] = new Vector3(length, 0, -width);
        lineVertices[2] = new Vector3(-length, 0, width);
        lineVertices[3] = new Vector3(length, 0, width);

        PrimitiveObject po = MakeRectangle(parent, lineVertices, pos, hasCollider);
        po.SetPointsActive(false);
        return po;
    }
    #endregion

    #region MakeZStripe
    public static PrimitiveObject MakeZStripe(Transform parent, float length, float width, Vector3 pos, bool hasCollider) {

        Vector3[] lineVertices = new Vector3[4];
        lineVertices[0] = new Vector3(-width, 0, -length);
        lineVertices[1] = new Vector3(-width, 0, length);
        lineVertices[2] = new Vector3(width, 0, -length);
        lineVertices[3] = new Vector3(width, 0, length);

        PrimitiveObject po = MakeRectangle(parent, lineVertices, pos, hasCollider);
        po.SetPointsActive(false);
        return po;
    }
    #endregion

    #region Normals
    public Vector3 GetNormal {
        get {
            Vector3[] vertices = PrimMeshFilter.mesh.vertices;
            return vertices.Length >= 3 ? Vector3.Cross(vertices[2] - vertices[1], vertices[1] - vertices[0]).normalized : Vector3.zero;
        }
    }

    public Vector3 GetGlobalNormal {
        get {
            Vector3[] vertices = PrimMeshFilter.mesh.vertices;
            for (int i = 0; i < vertices.Length; i++) {
                vertices[i] = GetCompound().transform.rotation * vertices[i];
            }             
            return vertices.Length >= 3 ? Vector3.Cross(vertices[2] - vertices[1], vertices[1] - vertices[0]).normalized : Vector3.zero;
        }
    }
    #endregion

    public void MoveFaceTowardsNormal(float magnitude) {
        float speed = 6f;
        transform.position = Vector3.MoveTowards(transform.position, transform.position + GetNormal * magnitude, speed * Time.deltaTime);
    }

    public float Magnitute {
        //set;
        //get { return Mathf.Abs(vertices[1].x - vertices[0].x)};
        get {
            return Mathf.Abs(PrimMeshFilter.mesh.vertices[1].x - PrimMeshFilter.mesh.vertices[0].x);
        }
    }

    #region UpdateUniformlyPrimitiveObject
    public void UpdateUniformlyPrimitiveObject(float magnitude, bool updatePos) {

        if(updatePos == true) {
            transform.position += (transform.position - GetCompound().transform.position) * Time.deltaTime * magnitude;
        }

        Mesh mesh = PrimMeshFilter.mesh;
        Vector3[] vertices = mesh.vertices;

        //Magnitute = Mathf.Abs(vertices[1].x - vertices[0].x);

        for (int j = 0; j < vertices.Length; j++) {
            Vector3 v = vertices[j];
            vertices[j] += v * Time.deltaTime * magnitude;

            PointList[j].transform.position = vertices[j] + transform.position;
        }

        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        MeshCollider col = GetComponent<MeshCollider>();
        col.sharedMesh = mesh;

    }
    #endregion

    #region MakeTriangle
    public static PrimitiveObject MakeTriangle(Transform parent, Vector3 a, Vector3 b, Vector3 pos, Vector3 c) {

        GameObject go = new GameObject("Triangle");

        go.transform.SetParent(parent);

        MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
        go.GetComponent<Renderer>().material = LoadResources.defaultMaterial;

        //meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));

        MeshFilter meshFilter = go.AddComponent<MeshFilter>();

        Vector3 barycenter = (a + b + c) / 3;

        Mesh mesh = new Mesh();

        //This is marked dynamic for frequent updates
        mesh.MarkDynamic();

        Vector3[] vertices = new Vector3[3] { a - barycenter, b - barycenter, c - barycenter };

        mesh.vertices = vertices;

        int[] tris = new int[3] { 0, 1, 2 };
        mesh.triangles = tris;

        Vector3 normal = Vector3.Cross(vertices[2] - vertices[0], vertices[1] - vertices[0]).normalized;

        Vector3[] normals = new Vector3[4] { normal, normal, normal, normal };
        mesh.normals = normals;

        Vector2[] uv = new Vector2[3] {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
        };
        mesh.uv = uv;
        mesh.RecalculateNormals();
        go.AddComponent<MeshCollider>();

        go.transform.position += pos;

        MeshCollider meshCol = go.GetComponent<MeshCollider>();
        meshCol.sharedMesh = mesh;
        meshFilter.mesh = mesh;

        //Debug.DrawLine(pos, pos + normal * 3, Color.red, 1000);

        go.AddComponent<PrimitiveObject>();
        PrimitiveObject po = go.GetComponent<PrimitiveObject>();
        po.PrimMeshFilter = meshFilter;

        for (int i = 0; i < vertices.Length; i++) {
            po.PointList.Add(Instantiate(LoadResources.point, vertices[i] + po.transform.position, Quaternion.identity, po.transform).GetComponent<Point>());
        }

        return po;
    }
    #endregion


    #region MakeRectangle
    public static PrimitiveObject MakeRectangle(Transform parent, Vector3[] vertices, Vector3 pos, bool hasCollider = true) {

        if (vertices.Length != 4) {
            Debug.LogError("size of vertices must be 4");
        }

        GameObject go = new GameObject("Rectangle");        

        go.transform.SetParent(parent);

        MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
        go.GetComponent<Renderer>().material = LoadResources.defaultMaterial;
        //meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));

        MeshFilter meshFilter = go.AddComponent<MeshFilter>();

        Vector3 barycenter = Vector3.zero;
        for (int i = 0; i < vertices.Length; i++) {
            barycenter += vertices[i];
        }
        barycenter /= vertices.Length;
        for (int i = 0; i < vertices.Length; i++) {
            vertices[i] -= barycenter;
        }

        Mesh mesh = new Mesh { vertices = vertices };
        //front is clock wise
        int[] tris = new int[6] {
             1, 0, 2,
             1, 2, 3
        };
        mesh.triangles = tris;

        Vector3 normal = Vector3.Cross(vertices[2] - vertices[0], vertices[1] - vertices[0]).normalized;
        Vector3[] normals = new Vector3[4] { normal, normal, normal, normal };

        mesh.normals = normals;
        mesh.RecalculateNormals();

        Vector2[] uv = new Vector2[4] {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        mesh.uv = uv;

        go.AddComponent<MeshCollider>();

        if (hasCollider == true) {
            MeshCollider meshCol = go.GetComponent<MeshCollider>();
            meshCol.sharedMesh = mesh;
        }

        meshFilter.mesh = mesh;

        go.transform.position += pos;

        go.AddComponent<PrimitiveObject>();
        PrimitiveObject po = go.GetComponent<PrimitiveObject>();
        po.PrimMeshFilter = meshFilter;

        for (int i = 0; i < vertices.Length; i++) {
            po.PointList.Add(Instantiate(LoadResources.point, vertices[i] + po.transform.position, Quaternion.identity, po.transform).GetComponent<Point>());
        }

        return po;
    }
    #endregion

    #region MakeQuad
    public static PrimitiveObject MakeQuad(Transform parent, float size, Vector3 pos, QuadOrientation qo = QuadOrientation.up, bool hasCollider = true) {

        float halfSize = 0.5f * size;
        Vector3[] vertices = new Vector3[4];
        if (qo == QuadOrientation.up) {
            vertices[0] = new Vector3(-halfSize, 0, -halfSize);
            vertices[1] = new Vector3(halfSize, 0, -halfSize);
            vertices[2] = new Vector3(-halfSize, 0, halfSize);
            vertices[3] = new Vector3(halfSize, 0, halfSize);
        } else if (qo == QuadOrientation.down) {
            vertices[0] = new Vector3(-halfSize, 0, -halfSize);
            vertices[1] = new Vector3(-halfSize, 0, halfSize);
            vertices[2] = new Vector3(halfSize, 0, -halfSize);
            vertices[3] = new Vector3(halfSize, 0, halfSize);
        } else if (qo == QuadOrientation.left) {
            vertices[0] = new Vector3(0, -halfSize, -halfSize);
            vertices[1] = new Vector3(0, halfSize, -halfSize);
            vertices[2] = new Vector3(0, -halfSize, halfSize);
            vertices[3] = new Vector3(0, halfSize, halfSize);
        } else if (qo == QuadOrientation.right) {
            vertices[0] = new Vector3(0, -halfSize, -halfSize);
            vertices[1] = new Vector3(0, -halfSize, halfSize);
            vertices[2] = new Vector3(0, halfSize, -halfSize);
            vertices[3] = new Vector3(0, halfSize, halfSize);
        } else if (qo == QuadOrientation.front) {
            vertices[0] = new Vector3(-halfSize, -halfSize, 0);
            vertices[1] = new Vector3(-halfSize, halfSize, 0);
            vertices[2] = new Vector3(halfSize, -halfSize, 0);
            vertices[3] = new Vector3(halfSize, halfSize, 0);
        } else if (qo == QuadOrientation.back) {
            vertices[0] = new Vector3(-halfSize, -halfSize, 0);
            vertices[1] = new Vector3(halfSize, -halfSize, 0);
            vertices[2] = new Vector3(-halfSize, halfSize, 0);
            vertices[3] = new Vector3(halfSize, halfSize, 0);
        }

        PrimitiveObject po = MakeRectangle(parent, vertices, pos, hasCollider);
        return po;
    }
    #endregion

    private bool isSelected = false;
    public bool IsSelected {
        set {
            if (value == true) {
                GetComponent<Renderer>().material.color = Color.green;
            } else {
                RestoreColor();
            }
            isSelected = value;
        }
        get {
            return isSelected;
        }
    }

    public void SetColor(Color c) {
        GetComponent<Renderer>().material.color = c;
    }

    public void RestoreColor() {
        GetComponent<Renderer>().material.color = initialColor;
    }

    public void SetPosition(Vector3 pos) {
        transform.position = pos;
    }

    public CompoundObject GetCompound() {
        return transform.parent.GetComponent<CompoundObject>();
    }

}
