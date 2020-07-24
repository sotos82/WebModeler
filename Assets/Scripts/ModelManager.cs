using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ModelManager : MonoBehaviour {

    public enum ModelerState { None, DrawQuad, DrawCube, Extrude }
    public enum SelectionState { Object, Face, Edge, Vertex }
    public enum ToolState { None, Extrude }
    protected enum DrawState { isSettingUp, isPlacing, isMoving, isDrawing }

    protected ModelerState modelerState;
    protected ToolState toolState;
    protected SelectionState selectionState;
    protected DrawState drawState;

    public Transform ModelParent;

    Point hitPoint;
    PrimitiveObject hitPrimitiveObject;
    PrimitiveObject selectedPrimitiveObject;
    Transform hitTr;

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

        if (state == ToolState.None) {
            UIManager.instance.InputPanel.SetAllActive(false);
        } else {
            UIManager.instance.InputPanel.SetAllActive(true);
        }

        toolState = state;
    }

    public void SetSelectionState(SelectionState state) {
        selectionState = state;
    }

    public void SetModelerState(ModelerState state) {

        modelerState = state;
    }

    bool isExtruding = false;

    private void Update() {

        int layer_mask = LayerMask.GetMask("model3d");
        Ray rayInfo = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool isHit = Physics.Raycast(rayInfo, out RaycastHit hitInfo, float.MaxValue, layerMask: layer_mask);

        RestoreSelectionAction();

        SelectAction(isHit, hitInfo);

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

                if (selectionState != SelectionState.Face) {
                    break;
                }

                if (Input.GetMouseButton(0)) {
                    if (EventSystem.current.IsPointerOverGameObject() == false) {
                        if (selectedPrimitiveObject != null) {
                            float magnitude = Input.GetAxis("Mouse X") + Input.GetAxis("Mouse Y");
                            ExtrudeFace(magnitude, true);
                            isExtruding = true;
                        }
                    }
                }
                if (isExtruding == true) {
                    if (Input.GetMouseButtonUp(0)) {
                        if (EventSystem.current.IsPointerOverGameObject() == false) {
                            if (selectedPrimitiveObject != null) {
                                selectedPrimitiveObject.GetCompound().FinishExtrude();
                                isExtruding = false;
                            }
                        }
                    }
                }

                break;
            default:
                break;
        }
    }

    protected void ExtrudeFace(float magnitude, bool isContinous) {
        if (selectedPrimitiveObject != null) {
            selectedPrimitiveObject.GetCompound().ExtrudeFace(selectedPrimitiveObject, magnitude, isContinous);
        }
    }

    public void ExtrudeFaceOnce(float magnitude, bool isContinous) {
        if (selectionState != SelectionState.Face) {
            return;
        }
        if (selectedPrimitiveObject != null) {
            selectedPrimitiveObject.GetCompound().ExtrudeFace(selectedPrimitiveObject, magnitude, isContinous);
            if (selectedPrimitiveObject != null) {
                selectedPrimitiveObject.GetCompound().FinishExtrude();
                isExtruding = false;
            }
        }
    }

    #region SelectionActions
    protected ModelerState SelectionActions(ModelerState modelerState) {
        if (Input.GetMouseButtonDown(0)) {
            if (EventSystem.current.IsPointerOverGameObject() == false) {
                if (selectedPrimitiveObject != null) {
                    selectedPrimitiveObject.GetCompound().SelectAll(false);
                    selectedPrimitiveObject = null;
                }
            }
            switch (selectionState) {
                case SelectionState.Object:

                    if (hitPrimitiveObject != null) {
                        if (selectedPrimitiveObject != null) {
                            selectedPrimitiveObject.GetCompound().SelectAll(false);
                        }
                        selectedPrimitiveObject = hitTr.GetComponent<PrimitiveObject>();
                        selectedPrimitiveObject.GetCompound().SelectAll(true);
                    }
                    break;
                case SelectionState.Face:
                    if (hitPrimitiveObject != null) {
                        if (selectedPrimitiveObject != null) {
                            selectedPrimitiveObject.IsSelected = false;
                        }
                        selectedPrimitiveObject = hitTr.GetComponent<PrimitiveObject>();
                        selectedPrimitiveObject.IsSelected = true;
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

                UIManager.instance.ToolActions.ExtrudeToggleAction(false);

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
                newCo.transform.position = point;

                if (Input.GetMouseButtonUp(0)) {
                    drawState = DrawState.isDrawing;
                }

                break;
            case DrawState.isDrawing:
                if (newCo != null) {
                    float magnitude = (Input.GetAxis("Mouse X") + Input.GetAxis("Mouse Y"));

                    UIManager.instance.InputPanel.SetMagnitudeFieldActive(true);
                    UIManager.instance.InputPanel.SetMagnitudeFieldText(newCo.Magnitute.ToString());

                    newCo.UpdateUniformlyCompoundbject(magnitude);
                }
                if (Input.GetMouseButtonUp(0)) {
                    UIManager.instance.InputPanel.SetAllActive(false);

                    newCo = null;

                    drawState = DrawState.isSettingUp;
                    return ModelerState.None;
                }

                return modelerState;

            default:
                break;
        }
        return modelerState;
    }
    #endregion

    #region SelectAction
    protected void SelectAction(bool isHit, RaycastHit hitInfo) {
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
                    hitPrimitiveObject = hitTr.GetComponent<PrimitiveObject>();
                    if (hitPrimitiveObject != null && hitPrimitiveObject.IsSelected == false) {
                        hitPrimitiveObject.SetColor(Color.red);
                    }
                } else if (selectionState == SelectionState.Object) {
                    hitPrimitiveObject = hitTr.GetComponent<PrimitiveObject>();
                    if (hitPrimitiveObject != null && hitPrimitiveObject.IsSelected == false) {
                        hitPrimitiveObject.GetCompound().SetColor(Color.red);
                    }
                }
            }
        }
    }
    #endregion

    #region RestoreSelectionAction
    protected void RestoreSelectionAction() {
        if (selectionState == SelectionState.Vertex) {
            if (hitPoint != null) {
                hitPoint.SetRendererEnabled(false);
            }
        }
        if (selectionState == SelectionState.Face) {
            if (hitPrimitiveObject != null && hitPrimitiveObject.IsSelected == false) {
                hitPrimitiveObject.RestoreColor();
            }
        } else if (selectionState == SelectionState.Object) {
            if (hitPrimitiveObject != null && hitPrimitiveObject.GetCompound().IsSelected == false) {
                hitPrimitiveObject.GetCompound().RestoreColor();
            }
        }
        hitTr = null;
        hitPrimitiveObject = null;
        hitPoint = null;
    }
    #endregion
}