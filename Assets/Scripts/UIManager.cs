using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour {

    public InputPanel InputPanel { protected set; get; }
    public ObjectSelectionActions ObjectSelectionActions { protected set; get; }
    public ToolActions ToolActions { protected set; get; }

    protected Transform BarsPanel { set; get; }

    private static UIManager _instance;
    public static UIManager instance {
        get {
            if (_instance == null)
                _instance = FindObjectOfType<UIManager>();
            return _instance;
        }
    }

    private void Awake() {
        _instance = this;

        BarsPanel = transform.Find(nameof(BarsPanel));    
        InputPanel = transform.Find(nameof(InputPanel)).GetComponent<InputPanel>();
        ObjectSelectionActions = BarsPanel.transform.Find("Selections").GetComponentInChildren<ObjectSelectionActions>();
        ToolActions = BarsPanel.transform.Find("Tools").GetComponentInChildren<ToolActions>();

    }
}
