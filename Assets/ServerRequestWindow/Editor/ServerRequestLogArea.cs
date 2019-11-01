using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using System.Linq;
//using System;

class SimpleTreeView : TreeView
{
    ServerRequestWindow _parent;

    public SimpleTreeView(TreeViewState treeViewState, ServerRequestWindow parent)
        : base(treeViewState)
    {
        _parent = parent;
        showAlternatingRowBackgrounds = true;
        showBorder = true;
        useScrollView = true;
        Reload();
    }
        
    protected override TreeViewItem BuildRoot ()
    {
        var root = new TreeViewItem {id = 0, depth = -1, displayName = "Root"};
        var allItems = new List<TreeViewItem>();
        for(int i = 0; i < _parent.LogItems.Count; i++) {
            var item = _parent.LogItems[i];
            string displayName = string.Format("[{0}] url={1} type={2}", item.date.ToLongTimeString(), item.url, item.requestType.ToString());
            allItems.Add(new TreeViewItem{id = i, depth = 0, displayName = displayName});
        }
            
        SetupParentsAndChildrenFromDepths (root, allItems);
            
        return root;
    }

    protected override void SelectionChanged(IList<int> selectedIds) {
        if(selectedIds.Count > 0) {
            _parent.OnClickItem(selectedIds[0]);
        }
    }
}