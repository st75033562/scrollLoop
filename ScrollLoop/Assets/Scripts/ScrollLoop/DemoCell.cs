using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DemoCell : ScrollCell {

    public Text text;

    public override void configureCellData() {
     //   Debug.Log("refresh"+ DataIndex);
        text.text = "索引：" + DataIndex + "内容："+ DataObject;
    }
}
