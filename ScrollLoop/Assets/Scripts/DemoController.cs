using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoController : MonoBehaviour {
    [SerializeField]
    private ScrollLoopController scroll;

    List<string> list = new List<string>();
    // Use this for initialization
    void Start () {

        for(int i = 0; i <12; i++) {
            list.Add(i.ToString());
        }
        scroll.initWithData(list);

    }

    public void OnClick() {
        // list.Insert(2, "123");
        //   list.Add("123");
        //  list.RemoveAt(1);

        scroll.updateCell(3, "33333333");  //更新单个数据
        //scroll.refresh(false);  //当显示的对象部分或全部没有替换时使用false刷新效率较高，但是对象中的内容不会更新,true会全部更新
    }
}
