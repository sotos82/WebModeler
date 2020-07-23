using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolActions : MonoBehaviour {

    protected Toggle ExtrudeToggle;

    protected void Awake() {
        ExtrudeToggle = transform.Find(nameof(ExtrudeToggle)).GetComponent<Toggle>();

        ExtrudeToggle.isOn = false;

        ExtrudeToggle.onValueChanged.AddListener(ExtrudeToggleAction);

    }

    public void ExtrudeToggleAction(bool isOn) {        
        if(isOn == true) {
            ModelManager.instance.SetToolState(ModelManager.ToolState.Extrude);
        } else {
            ModelManager.instance.SetToolState(ModelManager.ToolState.None);
        }
    }
}
