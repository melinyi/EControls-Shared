using EControl.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EControl.Tools.Helper
{
    public static class DragMoveSortHelper
    {
        public static object GetDataContext(object sender)
        {
            if (sender is null || !(sender is FrameworkElement))
            {
                throw new ArgumentNullException("参数不能为空且必须是控件");
            }
            return ((FrameworkElement)sender).DataContext;
        }


        public static object GetDragData(DragEventArgs e)
        {
            return e.Data.GetData(e.Data.GetFormats()?.FirstOrDefault());
        }

        public static void ChangeItemIndex(IList itemSource, object sender, DragEventArgs e, DragDirection dragDirection)
        {
            var from = GetDragData(e);
            var to = GetDataContext(sender);

            var element = (FrameworkElement)sender;
            var point = e.GetPosition(element);

            var pre = 0;
            switch (dragDirection)
            {
                case DragDirection.TopAndBottom:
                    if (point.Y > (element.ActualHeight / 2)) pre = 1;
                    break;
                case DragDirection.LeftAndRight:
                    if (point.X > (element.ActualWidth / 2)) pre = 1;
                    break;
            }

            ChangeItemIndex(itemSource, from, to, pre);
        }

        private static void ChangeItemIndex(IList itemSource, object fromObj, object toObj, int pre)
        {
            //其中一方为空
            if (fromObj == null || toObj == null) return;

            //其中一方不为指定的泛型
            //if (!(fromObj is T) || !(toObj is T)) return;

            //两者相同
            if (fromObj.Equals(toObj)) return;

            var fromItem = fromObj;
            var toItem = toObj;
            itemSource.Remove(fromItem);//移除需要变动的对象
            int indexTo = itemSource.IndexOf(toItem);//找到需要插入的位置索引
            if (indexTo == 0 && pre == 0)
            {
                itemSource.Insert(0, fromItem);
            }
            else if (indexTo == itemSource.Count - 1 && pre == 1)
            {
                itemSource.Add(fromItem);
            }
            else
            {
                itemSource.Insert(indexTo + pre, fromItem);
            }
        }

    }
}
