﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour {

    Button ClearSceneButton;

    protected void Awake() {

        ClearSceneButton = transform.Find(nameof(ClearSceneButton)).GetComponent<Button>();

        ClearSceneButton.onClick.AddListener(ClearSceneAction);

    }

    public void ClearSceneAction() {
        Transform modelPArent = GameObject.Find("ModelParent").transform;

        UIManager.instance.InputPanel.SetAllActive(false);

        UIManager.instance.ToolActions.ExtrudeToggleAction(false);

        foreach (Transform child in modelPArent) {
            Destroy(child.gameObject);
        }
    }

}
