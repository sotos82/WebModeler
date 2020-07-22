using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum LineType { Continuous, Discrete }
public class VectorLineGL {
    protected bool drawLine = true;
    public float lineWidth = 1.0f;
    public bool isOrtho;

    protected GameObject lineManagerGL;

    public bool ActiveSelf { get { return drawLine; } }
    public Material Material { get; }

    public float SetGetAlpha {
        set { Material.color = new Color(Material.color.r, Material.color.g, Material.color.b, value); }
        get { return Material.color.a; }
    }
    public LineType LineType { get; }

    public string name;

    private readonly bool checkForLineManager = true;

    private List<Vector3> auxlinePoints;
    public List<Vector3> LinePoints { get; private set; }

    public Vector3 this[int number] { get { return LinePoints[number]; } }

    public VectorLineGL(List<Vector3> linePoints, LineType lineType, Color color, bool isOrtho = false, float lineWidth = 1, string name = "", Material material = null) {
        if (checkForLineManager == false) {
            if (LineManagerGL.instance == null) {
                lineManagerGL = new GameObject("LineManagerGL");
                lineManagerGL.AddComponent<LineManagerGL>();
                Debug.Log("LineManager was not found. Creating...");
            }
        }
        this.name = name;
        this.LineType = lineType;
        this.LinePoints = linePoints;
        this.isOrtho = isOrtho;

        if (material == null) {
            //Priority Additive
            Shader shader = Shader.Find("Particles/Priority Additive (Soft)");
            //Shader shader = Shader.Find("Hidden/Internal-Colored");
            Material = new Material(shader);
            // Turn on alpha blending
            Material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            Material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        } else {
            Material = material;
        }

        Material.color = color;
        this.lineWidth = lineWidth;

        if (LinePoints.Count % 2 != 0 && lineType == LineType.Continuous)
            Debug.LogError("Continous lines should have an even number of points.");
    }

    public VectorLineGL(LineType lineType, Color color, bool isOrtho = false, float lineWidth = 1, string name = "", Material material = null) {
        if (checkForLineManager == false) {
            if (LineManagerGL.instance == null) {
                GameObject lineManagerGL = new GameObject("LineManagerGL");
                lineManagerGL.AddComponent<LineManagerGL>();
                Debug.Log("LineManager was not found. Creating...");
            }
        }
        this.name = name;
        LinePoints = new List<Vector3>();
        this.LineType = lineType;

        if (material == null) {
            //Priority Additive
            Shader shader = Shader.Find("Particles/Standard Unlit");
            //Shader shader = Shader.Find("Standard");
            Material = new Material(shader);
            // Turn on alpha blending
            Material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            Material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        } else {
            Material = material;
        }

        this.isOrtho = isOrtho;

        Material.color = color;
        this.lineWidth = lineWidth;
    }

    public VectorLineGL(LineType lineType, int segments, Color color, bool isOrtho = false, float lineWidth = 1, string name = "", Material material = null) {
        if (checkForLineManager == false) {
            if (LineManagerGL.instance == null) {
                GameObject lineManagerGL = new GameObject("LineManagerGL");
                lineManagerGL.AddComponent<LineManagerGL>();
                Debug.Log("LineManager was not found. Creating...");
            }
        }
        this.name = name;
        LinePoints = new List<Vector3>();
        this.LineType = lineType;
        this.isOrtho = isOrtho;

        if (material == null) {
            //Priority Additive
            Shader shader = Shader.Find("Particles/Priority Additive (Soft)");
            //Shader shader = Shader.Find("Hidden/Internal-Colored");
            Material = new Material(shader);
            // Turn on alpha blending
            Material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            Material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        } else {
            Material = material;
        }

        Material.color = color;
        this.lineWidth = lineWidth;

        for (int i = 0; i < segments; i++) {
            LinePoints.Add(Vector3.zero);
        }
    }

    public static void MakeCicle(LineType lineType_, Color color_, bool isOrtho, float lineWidth_, Vector3 center, Vector3 up,
    float radius = 1, int segments = 50, float time = float.MaxValue) {
        var line = new VectorLineGL(lineType_, color_, isOrtho, lineWidth_) {
            auxlinePoints = new List<Vector3>()
        };
        for (int i = 0; i < segments; i++) {
            line.auxlinePoints.Add(new Vector3(Mathf.Cos(2 * Mathf.PI * i / (segments - 1)) + center.x, center.y, Mathf.Sin(2 * Mathf.PI * i / (segments - 1))));
            line.LinePoints.Add(line.auxlinePoints[i] * radius + center);
        }
    }

    public void MakeCicle(Vector3 center, float radius = 1, int segments = 50, bool isOrtho = false) {
        auxlinePoints = new List<Vector3>();

        for (int i = 0; i < segments; i++) {
            auxlinePoints.Add(
                new Vector3(Mathf.Cos(2 * Mathf.PI * i / (segments - 1)),
                0,
                Mathf.Sin(2 * Mathf.PI * i / (segments - 1)))
                );
            LinePoints.Add(auxlinePoints[i] * radius + center);
        }
    }

    public void MakeCicle3D(Vector3 center, Vector3 up, float radius = 1, int segments = 50, bool isOrtho = false) {
        auxlinePoints = new List<Vector3>();

        for (int i = 0; i < segments; i++) {
            auxlinePoints.Add(MathLibrary.Get3DCirclePoint(center, radius, up, 2 * Mathf.PI * i / (segments - 1)));
            LinePoints.Add(auxlinePoints[i]);
        }
    }

    public void UpdateCicle(Color color) {
        Material.color = color;
    }

    public void UpdateCicle(Vector3 center, float radius) {
        for (int i = 0; i < LinePoints.Count; i++) {
            LinePoints[i] = radius * auxlinePoints[i] + center;
        }
    }

    public void UpdateCicle3D(Vector3 axis, Vector3 center, float radius) {
        int seg = LinePoints.Count;
        float c = 2 * Mathf.PI / (seg - 1);
        for (int i = 0; i < LinePoints.Count; i++) {
            LinePoints[i] = MathLibrary.Get3DCirclePoint(center, radius, axis, c * i);
        }
    }

    public void UpdateCicle(Vector3 center, float radius, Color color) {
        for (int i = 0; i < LinePoints.Count; i++) {
            LinePoints[i] = radius * auxlinePoints[i] + center;
        }
        Material.color = color;
    }

    public void UpdateLines(List<Vector3> linePoints_) {
        LinePoints = linePoints_;
    }

    public void UpdatePoint(int i, Vector3 point) {
        LinePoints[i] = point;
    }

    public void AddPoint(Vector3 point) {
        LinePoints.Add(point);
    }

    public void RemovePoint(Vector3 point) {
        LinePoints.Remove(point);
    }

    public void Draw() {
        LineManagerGL.instance?.AddLine(this);
    }

    public void Destroy() {
        LineManagerGL.instance?.RemoveLine(this);
        Object.Destroy(lineManagerGL);
    }

    public void SetActive(bool enabled) {
        drawLine = enabled;
    }

    public static void DrawLine(Vector3 start_, Vector3 end_, Color color_, bool isOrtho = false, float lineWidth_ = 1, float time = -1f) {
        var line = new VectorLineGL(LineType.Continuous, color_, isOrtho, lineWidth_);

        line.AddPoint(start_);
        line.AddPoint(end_);

        if (time > 0.0f)
            LineManagerGL.instance.DestroyLine(line, time);

        line.Draw();
    }

    public static void DrawArrow(Vector3 start, Vector3 end, Color color, bool isOrtho = false, float lineWidth = 1, float time = -1f) {
        var line = new VectorLineGL(LineType.Discrete, color, isOrtho, lineWidth);

        float percentage = 0.05f;
        line.AddPoint(start);
        line.AddPoint(end);
        Vector3 direction = end - start;
        Vector3 up = Vector3.Cross(direction, new Vector3(direction.z, 0, -direction.x)).normalized;
        Vector3 Vl = Quaternion.AngleAxis(+30, up) * direction * percentage;
        Vector3 Vr = Quaternion.AngleAxis(-30, up) * direction * percentage;
        line.AddPoint(end);
        line.AddPoint(end - Vl);
        line.AddPoint(end);
        line.AddPoint(end - Vr);

        if (time > 0.0f) {
            LineManagerGL.instance.DestroyLine(line, time);
        }

        line.Draw();
    }
}