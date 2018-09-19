using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class ScrollLoopController : UIBehaviour {
    private enum ChildAlignment {
        Dynamic_Center,
        Fixed_Center
    }

    [SerializeField]
    private Vector2 spacing;
    [SerializeField]
    private ChildAlignment childAlignment;
    [SerializeField]
    private ScrollCell cellPrefab;
    public int numOfColumns = 1; //并排显示个数
    private Vector2 childSize;
    private Vector2 parentSize;
    private int visibleCellsRowCount;
    private int visibleCellsTotalCount;
    private LinkedList<ScrollCell> localCellsPool = new LinkedList<ScrollCell>();
    private LinkedList<ScrollCell> cellsInUse = new LinkedList<ScrollCell>();
    private Vector2 childAddSpacSize;
    private Vector2 cellAlignmentSize;
    private int preFirstVisibleIndex;
    private int firstVisibleIndex;
    private IList allData;
    private ScrollRect scrollRect;
    private bool horizontal {
        get { return scrollRect.horizontal; }
    }

    public object callObj { set; get; }

    public void initWithData(IList cellDataList) {
        if(cellDataList == null)
            return;

        allData = cellDataList;
        scrollRect = gameObject.GetComponent<ScrollRect>();
        var cellRectTransform = cellPrefab.GetComponent<RectTransform>();
        cellRectTransform.anchorMin = new Vector2(0, 1);
        cellRectTransform.anchorMax = new Vector2(0, 1);
        cellRectTransform.pivot = new Vector2(0, 1);
        childSize = cellRectTransform.rect.size;
        childAddSpacSize = cellRectTransform.rect.size + spacing;
        parentSize = scrollRect.viewport.rect.size;
        if(childAlignment == ChildAlignment.Dynamic_Center) {
            computeNumOfCols();
        }
        if(horizontal) {
            visibleCellsRowCount = Mathf.CeilToInt(parentSize.x / childAddSpacSize.x);
            cellAlignmentSize = new Vector2(childAddSpacSize.x, parentSize.y / numOfColumns);
        } else {
            visibleCellsRowCount = Mathf.CeilToInt(parentSize.y / childAddSpacSize.y);
            cellAlignmentSize = new Vector2(parentSize.x / numOfColumns, childAddSpacSize.y);
        }
        visibleCellsTotalCount = (visibleCellsRowCount + 1) * numOfColumns;
        
        initCell();
    }

    void Update() {
        computeFirstVisIndex();
        internalCellsUpdate();
    }

    void computeFirstVisIndex() {
        int totalColumns = Mathf.CeilToInt(allData.Count / (float)numOfColumns);
        float columnsNormal = 1.0f / (totalColumns - visibleCellsRowCount);

        if(horizontal)
            firstVisibleIndex = (int)(scrollRect.horizontalNormalizedPosition / columnsNormal);
        else
            firstVisibleIndex = (int)((1 - scrollRect.verticalNormalizedPosition) / columnsNormal);
        int limit = totalColumns - visibleCellsRowCount;
        if(firstVisibleIndex < 0 || limit <= 0)
            firstVisibleIndex = 0;
        else if(firstVisibleIndex >= limit) {
            firstVisibleIndex = limit - 1;
        }
    }

    void computeNumOfCols() { //动态计算可以显示并排数
        int axis = 0;
        if(horizontal) {
            axis = 1;
        }
        numOfColumns = (int)(parentSize[axis] / childAddSpacSize[axis]);
    }

    void initCell() {
        resCells();
        setContentSize();
        for(int i = 0; i < showCellCount(); i++) {
            showCell(i, true);
        }
    }

    int showCellCount() {
        return visibleCellsTotalCount > allData.Count ? allData.Count : visibleCellsTotalCount; 
    }

    void resCells() {
        while(cellsInUse.Count > 0) {
            localCellsPool.AddLast(cellsInUse.First);  //close cell
            cellsInUse.RemoveFirst();
        }
        for(int i = localCellsPool.Count; i < visibleCellsTotalCount; i++) {
            GameObject go = Instantiate(cellPrefab.gameObject) as GameObject;
            localCellsPool.AddLast(go.GetComponent<ScrollCell>());
            go.transform.SetParent(scrollRect.content.transform, false);
            go.SetActive(false);
        }
    }

    void setContentSize() {
        int cellOneWayCount = (int)Math.Ceiling((float)allData.Count / numOfColumns);
        if(horizontal) {
            scrollRect.content.sizeDelta = new Vector2(cellOneWayCount * childAddSpacSize.x, scrollRect.content.sizeDelta.y);
        } else {
            scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, cellOneWayCount * childAddSpacSize.y);
        }
    }

    void showCell(int cellIndex, bool scrollingPositive) {
        if(cellIndex < allData.Count) {
            ScrollCell tempCell = getCellFromPool(scrollingPositive);
            positionCell(tempCell.gameObject, cellIndex);
            tempCell.init(this, allData[cellIndex], cellIndex);
            tempCell.configureCellData();
        }
    }

    ScrollCell getCellFromPool(bool scrollingPositive) {
        if(localCellsPool.Count == 0)
            return null;
        LinkedListNode<ScrollCell> cell = localCellsPool.First;
        localCellsPool.RemoveFirst();

        if(scrollingPositive)
            cellsInUse.AddLast(cell);
        else
            cellsInUse.AddFirst(cell);
        cell.Value.gameObject.SetActive(true);
        return cell.Value;
    }

    void positionCell(GameObject go, int index) {
        Vector2 v2 = (cellAlignmentSize - childSize) / 2;
        v2.y = -v2.y;
        int rowMod = index % numOfColumns;
        if(horizontal)
            go.GetComponent<RectTransform>().anchoredPosition = new Vector2(cellAlignmentSize.x * (index / numOfColumns), -rowMod * cellAlignmentSize.y) + v2;
        else
            go.GetComponent<RectTransform>().anchoredPosition = new Vector2(cellAlignmentSize.x * rowMod, -(index / numOfColumns) * cellAlignmentSize.y) + v2;
    }  

    void internalCellsUpdate() {
        if(preFirstVisibleIndex != firstVisibleIndex) {
            bool scrollingPositive = preFirstVisibleIndex < firstVisibleIndex;
            int indexDelta = Mathf.Abs(preFirstVisibleIndex - firstVisibleIndex);

            int deltaSign = scrollingPositive ? +1 : -1;

            for(int i = 1; i <= indexDelta; i++)
                updateContent(preFirstVisibleIndex + i * deltaSign, scrollingPositive);

            preFirstVisibleIndex = firstVisibleIndex;
        }
    }

    void updateContent(int cellIndex, bool scrollingPositive) {
        int index = scrollingPositive ? ((cellIndex - 1) * numOfColumns) + (visibleCellsTotalCount) : (cellIndex * numOfColumns);
        
        for(int i = 0; i < numOfColumns; i++) {
            freeCell(scrollingPositive);
            if(scrollingPositive) {
                showCell(index + i, scrollingPositive);
            } else {
                showCell(index + numOfColumns - i - 1, scrollingPositive);
            }
        }
    }

    void freeCell(bool scrollingPositive) {
        LinkedListNode<ScrollCell> cell = null;
        if(scrollingPositive) {
            cell = cellsInUse.First;
            cellsInUse.RemoveFirst();
            localCellsPool.AddLast(cell);
        } else {
            cell = cellsInUse.Last;
            cellsInUse.RemoveLast();
            localCellsPool.AddFirst(cell);
        }
        cell.Value.gameObject.SetActive(false);
    }

    public void refresh(bool force) {  //foere = false 表示如果引用同一个对象，则不对内容进行刷新，对只引用判断无效
        setContentSize();
        computeFirstVisIndex();
        preFirstVisibleIndex = firstVisibleIndex;
        LinkedListNode<ScrollCell> cell = cellsInUse.First;
        int effectCount = 0;
        for(int i = 0; i < showCellCount(); i++) {
            var cellIndex = firstVisibleIndex * numOfColumns + i;
            if(cellIndex < allData.Count) {
                effectCount++;
                ScrollCell scrollCell = null;
                if(cell == null) {
                    scrollCell = getCellFromPool(true);
                    positionCell(scrollCell.gameObject, cellIndex);
                } else {
                    scrollCell = cell.Value;
                    cell = cell.Next;
                }
                if(force || scrollCell.DataObject != allData[cellIndex]) {
                    positionCell(scrollCell.gameObject, cellIndex);
                    scrollCell.init(this, allData[cellIndex], cellIndex);
                    scrollCell.configureCellData();
                }
            }
        }
        while(cellsInUse.Count - effectCount > 0) {
            freeCell(false);
        }
    }

    public void updateCell(int index, object data) {
        if(cellsInUse.Count > 0) {
            LinkedListNode<ScrollCell> cell = cellsInUse.First;
            do 
            {
                if(cell.Value.DataIndex == index) {
                    cell.Value.DataObject = data;
                    break;
                }
                cell = cell.Next;
            } while (cell != null);
            
        }
    }
}
