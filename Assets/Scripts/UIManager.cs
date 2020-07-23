using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour {

    public InputPanel InputPanel { protected set; get; }

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

        InputPanel = transform.Find(nameof(InputPanel)).GetComponent<InputPanel>();

    }


}
