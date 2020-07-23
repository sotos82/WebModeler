using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelManager : MonoBehaviour {

    public enum ModelerState { None, DrawQuad, DrawCube, Extrude }
    public enum SelectionState { Object, Face, Edge, Vertex }
    public enum ToolState { None, Extrude }
    protected enum DrawState { isSettingUp, isPlacing, isMoving, isDrawing }

    protected ModelerState modelerState;
    protected ToolState toolState;
    protected SelectionState selectionState;
    protected DrawState drawState;

    private static ModelManager _instance;
    public static ModelManager instance {
        get {
            if (_instance == null)
                _instance = FindObjectOfType<ModelManager>();
            return _instance;
        }
    }

    private void Awake() {

        _instance = this;

        ModelParent = GameObject.Find(nameof(ModelParent)).transform;
    }

    private void Start() {

        //CompoundObject.MakeCube(ModelParent, 2f);

        Transform gridTr = GameObject.Find("Grid").transform;
        float stripeWidth = 0.01f;
        int numOfStripes = 20;
        float stripeLength = numOfStripes * 0.5f;
        for (int i = 0; i < numOfStripes; i++) {
            PrimitiveObject.MakeXStripe(gridTr, stripeLength, stripeWidth, new Vector3(0, 0, i - 0.5f * numOfStripes), false);
            PrimitiveObject po = PrimitiveObject.MakeXStripe(gridTr, stripeLength, stripeWidth, new Vector3(i - 0.5f * numOfStripes, 0, 0), false);
            po.transform.Rotate(Vector3.up, 90f);
        }
    }

    public void SetToolState(ToolState state) {
        toolState = state;
    }

    public void SetSelectionState(SelectionState state) {
        selectionState = state;
    }

    public void SetModelerState(ModelerState state) {
        modelerState = state;
    }

    public Transform ModelParent;

    Point hitPoint;
    PrimitiveObject hitModel3D;
    PrimitiveObject selectedModel3D;
    Transform hitTr;

    private void Update() {

        int layer_mask = LayerMask.GetMask("model3d");
        Ray rayInfo = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool isHit = Physics.Raycast(rayInfo, out RaycastHit hitInfo, float.MaxValue, layerMask: layer_mask);

        if (selectionState == SelectionState.Vertex) {
            if (hitPoint != null) {
                hitPoint.SetRendererEnabled(false);
            }
        }
        if (selectionState == SelectionState.Face) {
            if (hitModel3D != null && hitModel3D.IsSelected == false) {
                hitModel3D.RestoreColor();
            }
        } else if (selectionState == SelectionState.Object) {
            if (hitModel3D != null && hitModel3D.GetCompound().IsSelected == false) {
                hitModel3D.GetCompound().RestoreColor();
            }
        }

        hitTr = null;
        hitModel3D = null;
        hitPoint = null;

        //print(hitInfo.transform);
        //print(modelerState + " " + toolState + " " + selectionState + " " + isExtruding);

        if (isHit == true) {
            hitTr = hitInfo.transform;
            if (isExtruding == false) {
                if (selectionState == SelectionState.Vertex) {
                    hitPoint = hitTr.GetComponent<Point>();
                    if (hitPoint != null) {
                        hitPoint.SetRendererEnabled(true);
                    }
                }
                if (selectionState == SelectionState.Face) {
                    hitModel3D = hitTr.GetComponent<PrimitiveObject>();
                    if (hitModel3D != null && hitModel3D.IsSelected == false) {
                        hitModel3D.SetColor(Color.red);
                    }
                } else if (selectionState == SelectionState.Object) {
                    hitModel3D = hitTr.GetComponent<PrimitiveObject>();
                    if (hitModel3D != null && hitModel3D.IsSelected == false) {
                        hitModel3D.GetCompound().SetColor(Color.red);
                    }
                }
            }
        }

        switch (modelerState) {
            case ModelerState.None:
                modelerState = SelectionActions(ModelerState.None);
                break;
            case ModelerState.DrawQuad:
                modelerState = DrawActions(ModelerState.DrawQuad);
                break;
            case ModelerState.DrawCube:
                modelerState = DrawActions(ModelerState.DrawCube);
                break;
            default:
                break;
        }

        switch (toolState) {
            case ToolState.None:
                isExtruding = false;
                break;
            case ToolState.Extrude:

                if (Input.GetMouseButton(0)) {
                    if (selectedModel3D != null) {
                        float magnitude = Input.GetAxis("Mouse X") + Input.GetAxis("Mouse Y");
                        selectedModel3D.GetCompound().ExtrudeFace(selectedModel3D, magnitude);
                        isExtruding = true;
                    }
                }
                if (isExtruding == true) {
                    if (Input.GetMouseButtonUp(0)) {
                        if (selectedModel3D != null) {
                            selectedModel3D.GetCompound().FinishExtrude();
                            isExtruding = false;
                        }
                        //print("Extrude finished");
                    }
                }

                break;
            default:
                break;
        }
    }

    bool isExtruding = false;
    #region SelectionActions
    protected ModelerState SelectionActions(ModelerState modelerState) {
        if (Input.GetMouseButtonDown(0)) {
            if (selectedModel3D != null) {
                selectedModel3D.GetCompound().SelectAll(false);
            }
            switch (selectionState) {
                case SelectionState.Object:

                    if (hitModel3D != null) {
                        if (selectedModel3D != null) {
                            selectedModel3D.GetCompound().SelectAll(false);
                        }
                        selectedModel3D = hitTr.GetComponent<PrimitiveObject>();
                        selectedModel3D.GetCompound().SelectAll(true);
                    }
                    break;
                case SelectionState.Face:
                    if (hitModel3D != null) {
                        if (selectedModel3D != null) {
                            selectedModel3D.IsSelected = false;
                        }
                        selectedModel3D = hitTr.GetComponent<PrimitiveObject>();
                        selectedModel3D.IsSelected = true;
                    }
                    break;
                case SelectionState.Edge:
                    break;
                case SelectionState.Vertex:

                    break;
                default:
                    break;
            }
        }
        return modelerState;
    }
    #endregion

    #region DrawActions
    CompoundObject newCo = null;
    protected ModelerState DrawActions(ModelerState modelerState) {

        int layer_mask = LayerMask.GetMask("floor");
        Vector3 point = Vector3.zero;
        Ray rayInfo = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(rayInfo, out RaycastHit hitInfo, float.MaxValue, layerMask: layer_mask) == true) {
            point = hitInfo.point;
            point.y = 0;            
        }

        switch (drawState) {
            case DrawState.isSettingUp:
                drawState = DrawState.isPlacing;
                break;

            case DrawState.isPlacing:

                if (Input.GetMouseButtonUp(0)) {

                    if (modelerState == ModelerState.DrawQuad) {
                        newCo = CompoundObject.MakeQuad(ModelParent, 2f);
                        newCo.transform.position = point;
                    } else if (modelerState == ModelerState.DrawCube) {
                        newCo = CompoundObject.MakeCube(ModelParent, 2f);
                        newCo.transform.position = point;
                    }
                    drawState = DrawState.isMoving;
                }

                break;
            case DrawState.isMoving:
                //point = Vector3.zero;
                newCo.transform.position = point;

                if (Input.GetMouseButtonUp(0)) {
                    drawState = DrawState.isDrawing;
                }

                break;
            case DrawState.isDrawing:

                float magnitude = (Input.GetAxis("Mouse X") + Input.GetAxis("Mouse Y"));

                UIManager.instance.InputPanel.SetMagnitudeFieldActive(true);
                UIManager.instance.InputPanel.SetMagnitudeFieldText(newCo.Magnitute.ToString());

                newCo.UpdateUniformlyCompoundbject(magnitude);

                if (Input.GetMouseButtonUp(0)) {

                    UIManager.instance.InputPanel.SetMagnitudeFieldActive(false);

                    drawState = DrawState.isPlacing;
                    return ModelerState.None;
                }

                return modelerState;

            default:
                break;
        }
        return modelerState;
    }
    #endregion
}