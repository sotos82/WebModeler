using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LineManagerGL : MonoBehaviour {
    public void AddLine(VectorLineGL line) {
        lineList.Add(line);
    }

    public void RemoveLine(VectorLineGL line) {
        lineList.Remove(line);
    }

    private Transform mainCameraTransform;

    //List<Vector3[]> vvs = new List<Vector3[]>();

    public List<VectorLineGL> lineList;
    private static LineManagerGL _instance;
    public static LineManagerGL instance {
        get {
            //If _instance hasn't been set yet, we grab it from the scene!
            //This will only happen the first time this reference is used.
            if (_instance == null)
                _instance = FindObjectOfType<LineManagerGL>();
            return _instance;
        }
    }

    void Awake() {
        _instance = this;

        lineList = new List<VectorLineGL>();
    }

    public void DestroyLine(VectorLineGL vectorLine, float time) {
        StartCoroutine(DestroyLineCoroutine(vectorLine, time));
    }

    IEnumerator DestroyLineCoroutine(VectorLineGL vectorLine, float time) {
        yield return new WaitForSeconds(time);

        RemoveLine(vectorLine);
        vectorLine = null;
    }

    //protected void Update() {
    //    print(lineList.Count);
    //}

    protected Vector3 GetVertical(VectorLineGL ll, int i) {
        Vector3 aux1, aux0;
        if (i == 0) {
            aux1 = ll.LinePoints[i + 1];
            aux0 = ll.LinePoints[ll.LinePoints.Count - 1];
        } else if (i == ll.LinePoints.Count - 1) {
            aux1 = ll.LinePoints[0];
            aux0 = ll.LinePoints[i - 1];
        } else {
            aux1 = ll.LinePoints[i + 1];
            aux0 = ll.LinePoints[i - 1];
        }      
        return Vector3.Cross(mainCameraTransform.forward, aux1 - aux0).normalized;
    }

    private Color color;
    void OnRenderObject() {
        if (lineList == null) {
            return;
        }

        mainCameraTransform = Camera.main.transform;

        for (int j = 0; j < lineList.Count; j++) {
            VectorLineGL ll = lineList[j];
            if (ll.ActiveSelf == true) {
                int index = ll.LineType == LineType.Continuous ? 1 : 2;
                ll.Material.SetPass(0);

                color = ll.Material.color;
                if (ll.isOrtho) {
                    GL.LoadOrtho();
                }
                if (ll.lineWidth == 1) {
                    GL.PushMatrix();
                    GL.Begin(GL.LINES);
                    GL.Color(color);
                    for (int i = 0; i < ll.LinePoints.Count - 1; i += index) {
                        GL.Vertex(ll[i]);
                        GL.Vertex(ll[i + 1]);
                    }
                    GL.End();
                    GL.PopMatrix();
                } else {
                    float lineWidth = ll.lineWidth * 0.5f;

                    Vector3[] vs = new Vector3[ll.LinePoints.Count];
                    //float[] orient = new float[ll.LinePoints.Count];
                    Vector3 aux, aux1, aux0;
                    for (int i = 0; i < ll.LinePoints.Count; i++) {
                        aux = ll.LinePoints[i];
                        if (i == 0) {
                            aux1 = ll.LinePoints[i + 1];
                            aux0 = ll.LinePoints[ll.LinePoints.Count - 1];
                        } else if (i == ll.LinePoints.Count - 1) {
                            aux1 = ll.LinePoints[0];
                            aux0 = ll.LinePoints[i - 1];
                        } else {
                            aux1 = ll.LinePoints[i + 1];
                            aux0 = ll.LinePoints[i - 1];
                        }
                        vs[i] = Vector3.Cross((aux - Camera.main.transform.position), (aux1 - aux0)).normalized;

                        //Debug.DrawLine(aux, aux + 100 * (aux - Camera.main.transform.position).normalized, Color.green);
                        //Debug.DrawLine(aux, aux + 100 * (aux1 - aux0).normalized, Color.blue);
                        //Debug.DrawLine(aux, aux + 100 * vs[i], Color.red);
                    }

                    GL.PushMatrix();
                    GL.Begin(GL.QUADS);
                    for (int i = 0; i < ll.LinePoints.Count; i += index) {
                        GL.Color(color);

                        Vector3 v2 = ll[0];
                        Vector3 v1 = ll[i];

                        Vector3 vert1 = vs[i];
                        Vector3 vert2 = vs[0];
                        if (i < ll.LinePoints.Count - 1) {
                            vert2 = vs[i + 1];
                            v2 = ll[i + 1];
                        }

                        float d1 = Vector3.Distance(v1, mainCameraTransform.position) * 0.001f;
                        float d2 = Vector3.Distance(v2, mainCameraTransform.position) * 0.001f;

                        Vector3 displ1 = vert1 * lineWidth * d1;
                        Vector3 displ2 = vert2 * lineWidth * d2;

                        GL.TexCoord(new Vector3(0, 0, 0));
                        GL.Vertex(v1 - displ1);
                        GL.TexCoord(new Vector3(1, 0, 0));
                        GL.Vertex(v2 - displ2);
                        GL.TexCoord(new Vector3(1, 1, 0));
                        GL.Vertex(v2 + displ2);
                        GL.TexCoord(new Vector3(0, 1, 0));
                        GL.Vertex(v1 + displ1);
                    }
                    GL.End();
                    GL.PopMatrix();
                }
            }
        }
    }
}