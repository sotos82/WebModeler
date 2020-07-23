using UnityEngine;
using System.Collections;

[AddComponentMenu("Camera-Control/Space RTS Camera Style")]
public enum CameraState {
    isIdling,
    isRotating,
    isZooming,
    isPanning,
    isTargeting
}

public enum CameraMode {
    SpaceRts,
    InspectElement
}

public class CamController : MonoBehaviour {
    public LayerMask lm;
    public CameraMode mode = CameraMode.SpaceRts;
    public CameraState state = CameraState.isIdling;
    public float initDistanceToCenter = 100;
    public float xSpeed = 100.0f;
    public float ySpeed = 100.0f;
    public float yMinLimit = -85;
    public float yMaxLimit = 85;
    public float zoomRate = 20;
    public bool panMode = false;
    public float panSpeed = 0.3f;
    public float panThres = 5;
    public float rotationDampening = 5.0f;
    public float cameraMaxDistance = 100;
    public bool isMovingWithRotation;
    public Transform targetTransform;
    public float maxCameraDistance;

    private float xDeg = 0.0f;
    private float yDeg = 0.0f;
    private Vector3 desiredPosition;
    private Vector3 rayPlanePoint;
    private float lastClickTime = 0;
    private float catchTime = 0.25f;
    private bool isLocked = false;
    private Vector3 off = Vector3.zero;
    private Vector3 offSet = Vector3.zero;
    private Plane perpendToViewplane;
    private float enter;
    private readonly float zoomThres = 0.005f;
    private Transform lockedTransform = null;
    public float distanceTozero;
    private static readonly float xmin = 1;
    private static readonly float xmax = 1000;
    private readonly float xminxmaxInv = 1 / (xmax - xmin);
    private RaycastHit hit;
    private Ray ray;
    private Plane planeZero = new Plane(Vector3.up, Vector3.zero);
    private float limit = 100;

    void Awake() {
        cameraMaxDistance = 100;
        targetTransform = new GameObject("Cam targetRotation").transform;

        ResetCamera();        
    }

    protected void OnEnable() {
    }

    #region ResetCamera
    public void ResetCamera() {
        targetTransform.position = Vector3.zero;
        transform.position = targetTransform.position - initDistanceToCenter * transform.forward;
        Vector3 direction = (targetTransform.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

        perpendToViewplane.SetNormalAndPosition(Vector3.up, Vector3.zero);
        lockedTransform = null;
        state = CameraState.isIdling;
        isLocked = false;
        lockedTransform = null;

        distanceTozero = transform.position.magnitude;
        xDeg = MathLibrary.AngleSigned(Vector3.right, transform.right, Vector3.up);
        yDeg = Vector3.Angle(Vector3.up, transform.up);
    }
    #endregion

    private Vector3 lastMousePos;

    void LateUpdate() {

        float wheel = Input.GetAxis("Mouse ScrollWheel");
        if (mode == CameraMode.SpaceRts) {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        } else {
            ray = new Ray(transform.position, (Vector3.zero - transform.position).normalized);
        }
        bool rayIntersectsAnyCollider = Physics.Raycast(ray, out hit, maxCameraDistance, lm.value);

        //print(state);

        if (isLocked) {
            if (lockedTransform != null) {
                offSet = lockedTransform.position - off;
                off = lockedTransform.position;                                                            //in case the lockedTransform is moving

                if (Input.GetMouseButton(1) == false) {
                    state = CameraState.isIdling;

                    float magnitude = (targetTransform.position - transform.position).magnitude;           //in case the lockedTransform is moving
                    transform.position = targetTransform.position - (transform.rotation * Vector3.forward * magnitude) + offSet;
                    targetTransform.position = lockedTransform.position;
                }
            } else {
                isLocked = false;
            }
        }

        if (state != CameraState.isTargeting) {
            if (panMode == true && Input.GetMouseButton(2) == true) {
                if(state != CameraState.isPanning) {
                    lastMousePos = Input.mousePosition;
                }

                state = CameraState.isPanning;
            }
            if (Input.GetMouseButton(1) == true) {
                state = CameraState.isRotating;
            } else if (Input.GetMouseButtonUp(1)) {
                yDeg = transform.rotation.eulerAngles.x;
                if (yDeg > 180) {
                    yDeg -= 360;
                }

                xDeg = transform.rotation.eulerAngles.y;

                state = CameraState.isIdling;
            } else if (wheel != 0) {
                state = CameraState.isZooming;
            }
        }

        switch (state) {

            #region Idling
            case CameraState.isIdling:
                break;
            #endregion

            #region Rotating
            case CameraState.isRotating:

                float getAxisX = Input.GetAxis("Mouse X");
                float getAxisY = Input.GetAxis("Mouse Y");

                if (Mathf.Abs(getAxisX) > 0.05f || Mathf.Abs(getAxisY) > 0.05f) {
                    isMovingWithRotation = true;
                }

                xDeg += getAxisX * xSpeed;
                yDeg -= getAxisY * ySpeed;
                yDeg = ClampAngle(yDeg, yMinLimit, yMaxLimit, 5);

                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(yDeg, xDeg, 0), Time.deltaTime * rotationDampening / Time.timeScale);
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0);

                float magnitude = (targetTransform.position - transform.position).magnitude;
                transform.position = targetTransform.position - (transform.rotation * Vector3.forward * magnitude) + offSet;                

                targetTransform.position = targetTransform.position + offSet;

                break;
            #endregion

            #region Zooming
            case CameraState.isZooming:

                Vector3 hitPoint = Vector3.zero;
                if (rayIntersectsAnyCollider) {
                    hitPoint = hit.point;
                } else {
                    planeZero.Raycast(ray, out float enter1);
                    hitPoint = ray.GetPoint(enter1);
                }
                perpendToViewplane.SetNormalAndPosition(-transform.forward, hitPoint);

                perpendToViewplane.Raycast(ray, out enter);
                rayPlanePoint = ray.GetPoint(Mathf.Min(enter, 100f));
                ray.direction = transform.forward;

                float par = 0.33f;
                enter = Mathf.Abs(enter);
                targetTransform.position = transform.forward * enter + transform.position;
                if (wheel > 0) {
                    desiredPosition = MathLibrary.ClampMagnitude((rayPlanePoint - transform.position), cameraMaxDistance, 10) * par + transform.position;
                } else if (wheel < 0) {
                    desiredPosition = -MathLibrary.ClampMagnitude((targetTransform.position - transform.position), cameraMaxDistance, 10) * par + transform.position;
                }

                transform.position = Vector3.Lerp(transform.position, desiredPosition, zoomRate * Time.deltaTime / Time.timeScale);

                if (Vector3.SqrMagnitude(transform.position - desiredPosition) < zoomThres) {
                    state = CameraState.isIdling;
                }
                break;
            #endregion

            //#region Zooming
            //case CameraState.isZooming:
            //    Vector3 hitPoint = Vector3.zero;
            //    //if (rayIntersectsAnyCollider) {
            //    //    hitPoint = hit.point;
            //    //} else {
            //    //    planeZero.Raycast(ray, out float enter1);
            //    //    hitPoint = ray.GetPoint(enter1);
            //    //}
            //    perpendToViewplane.SetNormalAndPosition(-transform.forward, hitPoint);
            //    perpendToViewplane.Raycast(ray, out enter);
            //    rayPlanePoint = ray.GetPoint(Mathf.Min(enter, limit));
            //    ray.direction = transform.forward;
            //    //if (lockedTransform != null) {
            //    //    UnlockObject();
            //    //}
            //    float par = 0.05f;
            //    enter = Mathf.Abs(enter);
            //    //targetTransform.position = transform.forward * enter + transform.position;
            //    targetTransform.position = Vector3.zero;
            //    rayPlanePoint = Vector3.zero;
            //    if (wheel > 0) {
            //        desiredPosition = MathLibrary.ClampMagnitude((rayPlanePoint - transform.position), cameraMaxDistance, 50) * par + transform.position;
            //    } else if (wheel < 0) {
            //        desiredPosition = -MathLibrary.ClampMagnitude((targetTransform.position - transform.position), cameraMaxDistance, 50) * par + transform.position;
            //    }
            //    transform.position = Vector3.Lerp(transform.position, desiredPosition, zoomRate * Time.deltaTime / Time.timeScale);
            //    if (Vector3.SqrMagnitude(transform.position - desiredPosition) < zoomThres) {
            //        state = CameraState.isIdling;
            //    }
            //    break;
            //#endregion

            #region Panning
            case CameraState.isPanning:

                Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0) * 0.5f;

                Vector3 centerToMouse = Input.mousePosition - lastMousePos;

                float distFromCenter = centerToMouse.magnitude;

                if (lockedTransform != null) {
                    UnlockObject();
                }

                rayPlanePoint.Set(transform.forward.x, 0, transform.forward.z);
                targetTransform.Translate(centerToMouse * Time.deltaTime * panSpeed);   //here, right is wrt the loc ref because Space.Self by default
                transform.Translate(centerToMouse * Time.deltaTime * panSpeed);

                if(Input.GetMouseButtonUp(2) == true) {
                    state = CameraState.isIdling;
                }

                //float panNorm = transform.position.y;
                //if ((Input.mousePosition.x - Screen.width + panThres) > 0) {
                //    targetTransform.Translate(Vector3.right * -panSpeed * Time.deltaTime * panNorm);   //here, right is wrt the loc ref because Space.Self by default
                //    transform.Translate(Vector3.right * -panSpeed * Time.deltaTime * panNorm);
                //} else if ((Input.mousePosition.x - panThres) < 0) {
                //    targetTransform.Translate(Vector3.right * panSpeed * Time.deltaTime * panNorm);
                //    transform.Translate(Vector3.right * panSpeed * Time.deltaTime * panNorm);
                //}
                //if ((Input.mousePosition.y - Screen.height + panThres) > 0) {
                //    rayPlanePoint.Set(transform.forward.x, 0, transform.forward.z);
                //    targetTransform.Translate(rayPlanePoint.normalized * -panSpeed * Time.deltaTime * panNorm, Space.World);
                //    transform.Translate(rayPlanePoint.normalized * -panSpeed * Time.deltaTime * panNorm, Space.World);
                //}
                //if ((Input.mousePosition.y - panThres) < 0) {
                //    rayPlanePoint.Set(transform.forward.x, 0, transform.forward.z);
                //    targetTransform.Translate(rayPlanePoint.normalized * panSpeed * Time.deltaTime * panNorm, Space.World);
                //    transform.Translate(rayPlanePoint.normalized * panSpeed * Time.deltaTime * panNorm, Space.World);
                //}

                //if (lockedTransform != null)
                //    UnlockObject();

                //float panNorm = transform.position.y;
                //if ((Input.mousePosition.x - Screen.width + panThres) > 0) {
                //    targetTransform.Translate(Vector3.right * -panSpeed * Time.deltaTime * panNorm);   //here, right is wrt the loc ref because Space.Self by default
                //    transform.Translate(Vector3.right * -panSpeed * Time.deltaTime * panNorm);
                //} else if ((Input.mousePosition.x - panThres) < 0) {
                //    targetTransform.Translate(Vector3.right * panSpeed * Time.deltaTime * panNorm);
                //    transform.Translate(Vector3.right * panSpeed * Time.deltaTime * panNorm);
                //}
                //if ((Input.mousePosition.y - Screen.height + panThres) > 0) {
                //    rayPlanePoint.Set(transform.forward.x, 0, transform.forward.z);
                //    targetTransform.Translate(rayPlanePoint.normalized * -panSpeed * Time.deltaTime * panNorm, Space.World);
                //    transform.Translate(rayPlanePoint.normalized * -panSpeed * Time.deltaTime * panNorm, Space.World);
                //}
                //if ((Input.mousePosition.y - panThres) < 0) {
                //    rayPlanePoint.Set(transform.forward.x, 0, transform.forward.z);
                //    targetTransform.Translate(rayPlanePoint.normalized * panSpeed * Time.deltaTime * panNorm, Space.World);
                //    transform.Translate(rayPlanePoint.normalized * panSpeed * Time.deltaTime * panNorm, Space.World);
                //}
                break;
            #endregion

            #region Targeting
            case CameraState.isTargeting:

                off = lockedTransform.position;
                offSet = lockedTransform.position - off;
                targetTransform.position = Vector3.Lerp(targetTransform.position, lockedTransform.position,
                    3 * zoomRate * Time.deltaTime / Time.timeScale);

                desiredPosition = targetTransform.position - limit * transform.forward;

                transform.position = Vector3.Lerp(transform.position, desiredPosition, zoomRate * Time.deltaTime / Time.timeScale);

                if (Vector3.SqrMagnitude(transform.position - desiredPosition) < 50 * 50) {
                    state = CameraState.isIdling;
                }

                break;
            #endregion

            default:
                break;
        }

        targetTransform.position = Vector3.ClampMagnitude(targetTransform.position, cameraMaxDistance);
        distanceTozero = transform.position.magnitude;
        if (distanceTozero > cameraMaxDistance) {
            transform.position = Vector3.ClampMagnitude(transform.position, cameraMaxDistance);
        }
    }

    //#region LockObject
    //protected float inSqDist = 0;
    //public void LockObject(Transform transformToLock) {
    //    if (transformToLock.GetComponent<SolarSystem>()) {
    //        inSqDist = (transform.position - (transformToLock.position - Globals.MapSolarSystemRadius * transform.forward)).sqrMagnitude;
    //        state = CameraState.isTargeting;
    //        lockedTransform = transformToLock;
    //    } else {
    //        state = CameraState.isIdling;
    //        isLocked = true;
    //        lockedTransform = transformToLock;
    //        off = lockedTransform.position;
    //        offSet = lockedTransform.position - off;

    //        targetTransform.position = lockedTransform.position;
    //        //transform.position = targetRotation.position - lockedTransform.localScale.x * transform.forward;
    //        transform.position = targetTransform.position - (lockedTransform.position - transform.position).magnitude * transform.forward;
    //    }
    //}
    //#endregion

    #region UnlockObject
    private void UnlockObject() {
        isLocked = false;
        lockedTransform = null;
        offSet = Vector3.zero;
    }
    #endregion

    #region MouseXBoarder
    private int MouseXBoarder()         //Mouse right left or in the screen
    {
        if ((Input.mousePosition.x - Screen.width + panThres) > 0)
            return 1;
        else if ((Input.mousePosition.x - panThres) < 0)
            return -1;
        else
            return 0;
    }
    #endregion

    #region MouseYBoarder
    private int MouseYBoarder()         //Mouse above below or in the screen
    {
        if ((Input.mousePosition.y - Screen.height + panThres) > 0)
            return 1;
        else if ((Input.mousePosition.y - panThres) < 0)
            return -1;
        else
            return 0;
    }
    #endregion

    #region ClampAngle
    private static float ClampAngle(float angle, float minOuter, float maxOuter, float inner) {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;

        angle = Mathf.Clamp(angle, minOuter, maxOuter);

        if (angle < inner && angle > 0)
            angle -= 2 * inner;
        else if (angle > -inner && angle < 0)
            angle += 2 * inner;

        return angle;
    }
    #endregion

    #region DoubleClick
    private bool DoubleClick(float t) {
        if (Input.GetMouseButtonDown(0)) {
            if ((Time.time - lastClickTime) < catchTime * Time.timeScale) {
                lastClickTime = Time.time;
                return true;
            } else {
                lastClickTime = Time.time;
                return false;
            }
        } else
            return false;
    }
    #endregion
}
