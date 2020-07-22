using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectSelectionActions : MonoBehaviour {

    protected Toggle SelectCompoundToggle;
    protected Toggle SelectFaceToggle;
    protected Toggle SelectEdgeToggle;
    protected Toggle SelecVerticeToggle;

    protected void Awake() {
        SelectCompoundToggle = transform.Find(nameof(SelectCompoundToggle)).GetComponent<Toggle>();
        SelectFaceToggle = transform.Find(nameof(SelectFaceToggle)).GetComponent<Toggle>();
        SelectEdgeToggle = transform.Find(nameof(SelectEdgeToggle)).GetComponent<Toggle>();
        SelecVerticeToggle = transform.Find(nameof(SelecVerticeToggle)).GetComponent<Toggle>();

        SelectFaceAction(true);
        SelectFaceToggle.isOn = true;

        SelectCompoundToggle.onValueChanged.AddListener(SelectCompoundAction);
        SelectFaceToggle.onValueChanged.AddListener(SelectFaceAction);
        SelectEdgeToggle.onValueChanged.AddListener(SelectEdgeAction);
        SelecVerticeToggle.onValueChanged.AddListener(SelecVerticeAction);
    }

    public void SelectCompoundAction(bool isOn) {
        ModelManager.Instance.SetSelectionState(ModelManager.SelectionState.Object);
    }

    public void SelectFaceAction(bool isOn) {
        ModelManager.Instance.SetSelectionState(ModelManager.SelectionState.Face);
    }

    public void SelectEdgeAction(bool isOn) {
        ModelManager.Instance.SetSelectionState(ModelManager.SelectionState.Edge);
    }

    public void SelecVerticeAction(bool isOn) {
        ModelManager.Instance.SetSelectionState(ModelManager.SelectionState.Vertice);
    }
}
