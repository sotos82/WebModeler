using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class MathLibrary {
    private static long nextInteger = 0;
    public static long NextInteger { get { return nextInteger++; } }

    public static int FindIndexOfMaxInFloatArray(float[] a) {
        int index = 0;
        float max = 0;
        for (int i = 0; i < a.Length; i++) {
            if (a[i] > max) {
                index = i;
                max = a[i];
            }
        }
        return index;
    }

    public static Vector3 VectorPerpendTo(Vector3 a) {
        Vector3 result = a.z < a.x ? new Vector3(a.y, -a.x, 0) : new Vector3(0, -a.z, a.y);
        return result.normalized;
    }

    public static void GetOrthogonalToVector(Vector3 a, out Vector3 b, out Vector3 c) {
        b = VectorPerpendTo(a);
        c = Vector3.Cross(a, b).normalized;
    }

    public static Vector3 Get3DCirclePoint(Vector3 center, float radius, Vector3 axis, float theta) {
        GetOrthogonalToVector(axis, out Vector3 a, out Vector3 b);
        float rcosth = radius * Mathf.Cos(theta);
        float rsinth = radius * Mathf.Sin(theta);
        return center + rcosth * a + rsinth * b;
    }

    public static float Map(float s, float a1, float a2, float b1, float b2) {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }

    public static Color CombineColors(params Color[] aColors) {
        Color result = new Color(0, 0, 0, 0);
        foreach (Color c in aColors) {
            result += c;
        }
        result /= aColors.Length;
        return result;
    }

    public static Color RandomColor() {
        return new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
    }

    public static float PolygonOrientation(Vector3 a, Vector3 b, Vector3 c) {
        double az = a.z;
        double ax = a.x;
        double bx = b.x;
        double bz = b.z;
        double cx = c.x;
        double cz = c.z;

        return (float)((bx * cz + ax * bz + az * cx) - (az * bx + bz * cx + ax * cz));

        //return (b.x * c.z + a.x * b.z + a.z * c.x) - (a.z * b.x + b.z * c.x + a.x * c.z);
    }

    public static int FindIndexOfMinInFloatArray(float[] a) {
        int index = 0;
        float max = int.MaxValue;
        for (int i = 0; i < a.Length; i++) {
            if (a[i] < max) {
                index = i;
                max = a[i];
            }
        }
        return index;
    }

    public static int FindIndexOfMinInFloatList(List<float> a) {
        int index = 0;
        float max = int.MaxValue;
        for (int i = 0; i < a.Count; i++) {
            if (a[i] < max) {
                index = i;
                max = a[i];
            }
        }
        return index;
    }

    public static float DistanceToLine(Ray ray, Vector3 point) {
        return Vector3.Cross(ray.direction, point - ray.origin).magnitude;
    }

    public static Vector3 LinePointItersectionPoint(Ray ray, Vector3 point) {
        return ray.origin + ray.direction * Vector3.Dot(ray.direction, point - ray.origin);
    }

    public static bool IsInEllipse(float a, float b, float c, Vector3 position) {
        return position.x * position.x / (a * a) + position.y * position.y / (b * b) + position.z * position.z / (c * c) < 1;
    }

    public static bool IsInCylinder(float radius, Vector3 position) {
        return position.x * position.x + position.z * position.z < radius * radius;
    }

    public static Vector3 RandomOnCylinderRim(float radius) {
        float angle = Random.Range(0, 2 * Mathf.PI);
        Vector3 pos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
        return pos;
    }

    public static Vector3 RandomOnUnitCircle(float radius) {
        float angle = Random.Range(0f, 2 * Mathf.PI);
        float x = Mathf.Sin(angle) * radius;
        float z = Mathf.Cos(angle) * radius;
        return new Vector3(x, 0, z);
    }

    public static float Normal(float x, float mu, float sigma) {
        return 1 / (sigma * Mathf.Sqrt(2 * Mathf.PI)) * Mathf.Exp(-(x - mu) * (x - mu) / (2 * sigma * sigma));
    }

    // Determine the signed angle between two vectors, with normal 'n'
    // as the rotation axis.
    public static float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n) {
        float x = Vector3.Dot(n, Vector3.Cross(v1, v2));
        float y = Vector3.Dot(v1, v2);

        return Mathf.Atan2(x, y) * Mathf.Rad2Deg;
    }

    public static float SignedVectorAngle(Vector3 referenceVector, Vector3 otherVector, Vector3 normal) {
        Vector3 perpVector = Vector3.Cross(normal, referenceVector);
        float angle = Vector3.Angle(referenceVector, otherVector);
        angle *= Mathf.Sign(Vector3.Dot(perpVector, otherVector));

        return angle;
    }

    public static float LinePlaneIntersect(Vector3 u, Vector3 P0, Vector3 N, Vector3 D, ref Vector3 point) {
        float s = Vector3.Dot(N, (D - P0)) / Vector3.Dot(N, u);
        point = P0 + s * u;
        return s;
    }

    public static Vector3 ProjectVectorOnPlane(Vector3 planeNormal, Vector3 v) {
        planeNormal.Normalize();
        float vectorPlaneDistance = Vector3.Dot(planeNormal, v);
        return (v - planeNormal * vectorPlaneDistance).normalized;
    }

    public static Vector3 ProjectVectorOnPlaneNOTNORM(Vector3 planeNormal, Vector3 vector) {
        return vector - planeNormal * Vector3.Dot(planeNormal, vector);
    }

    public static Vector3 ClampMagnitude(Vector3 v, float max, float min) {
        double sm = v.sqrMagnitude;
        if (sm > max * (double)max) return v.normalized * max;
        else if (sm < min * min) return v.normalized * min;
        return v;
    }

    public static Vector2 ClampMagnitude(Vector2 v, float max, float min) {
        double sm = v.sqrMagnitude;
        if (sm > max * (double)max) return v.normalized * max;
        else if (sm < min * min) return v.normalized * min;
        return v;
    }

    public static Vector3 CircleLineIntersection(float radius, Vector2 t, Vector2 direction) {
        //double[] extent = { 100000 * direction.x, 100000 * direction.y };
        //double[] p1 = { t.x + extent[0], t.x + extent[1] };
        //double[] p2 = { t.x - extent[0], t.x - extent[1] };
        //double dx = p2[0] - p1[0];
        //double dy = p2[1] - p1[1];
        //double drSq = dx * dx + dy * dy;
        //double IdrSq = 1 / drSq;
        //double det = p1[0] * p2[1] - p2[0] * p1[1];
        //double Sqrt = System.Math.Sqrt(radius * radius * drSq - det * det);
        //double auxX = System.Math.Sign(dy) * dx * Sqrt;
        //double auxY = System.Math.Abs(dy) * Sqrt;
        //double[] point = { (det * dy + auxX) * IdrSq, 0, (-det * dx + auxY) * IdrSq };
        //double dot = point[0] * direction.x + point * direction.y;

        Vector2 extent = 100000 * direction;
        Vector2 p1 = t + extent;
        Vector2 p2 = t - extent;
        float dx = p2.x - p1.x;
        float dy = p2.y - p1.y;
        float drSq = dx * dx + dy * dy;
        float IdrSq = 1 / drSq;
        float det = p1.x * p2.y - p2.x * p1.y;
        float Sqrt = Mathf.Sqrt(radius * radius * drSq - det * det);
        float auxX = Mathf.Sign(dy) * dx * Sqrt;
        float auxY = Mathf.Abs(dy) * Sqrt;
        Vector3 point = new Vector3((det * dy + auxX) * IdrSq, 0, (-det * dx + auxY) * IdrSq);
        float dot = point.x * direction.x + point.z * direction.y;  //Be careful, SHOULD BE point.z because Vector3
        if (dot < 0) {
            point.x = (det * dy - auxX) * IdrSq;
            point.z = (-det * dx - auxY) * IdrSq;
        }
        return point;
    }

    public static bool ApproximatelyVector(Vector3 a, Vector3 b) {
        return Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y) && Mathf.Approximately(a.z, b.z);
    }
};
