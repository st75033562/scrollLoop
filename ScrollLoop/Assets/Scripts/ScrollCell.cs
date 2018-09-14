using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollCell : MonoBehaviour {
    private ScrollLoopController controller;
    private System.Object dataObject;
    private int dataIndex;

    public System.Object DataObject {
        get { return dataObject; }
        set {
            dataObject = value;
            configureCellData();
        }
    }

    public int DataIndex {
        get { return dataIndex; }
    }

    public ScrollLoopController Controller {
        get { return controller; }
    }

    public void init(ScrollLoopController controller , System.Object data, int index) {
        this.controller = controller;
        dataObject = data;
        dataIndex = index;
    }

    public virtual void configureCellData() {}


}
