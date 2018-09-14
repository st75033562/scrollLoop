using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DemoCell : ScrollCell {

    public Text text;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public override void configureCellData() {
        text.text = "索引：" + DataIndex + "内容："+ (int)DataObject;
    }
}
