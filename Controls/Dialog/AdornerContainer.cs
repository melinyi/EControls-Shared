
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace EControl.Controls
{
    /// <summary>
    /// 装饰层容器
    /// </summary>
    internal class AdornerContainer : Adorner
    {
        private UIElement _child;

        /// <summary>
        /// 装饰层容器
        /// </summary>
        /// <param name="adornedElement">装饰层内容</param>
        public AdornerContainer(UIElement adornedElement) : base(adornedElement)
        {

        }

        public UIElement Child
        {
            get => _child;
            set
            {
                if (value == null)
                {
                    RemoveVisualChild(_child);
                    // ReSharper disable once ExpressionIsAlwaysNull
                    _child = value;
                    return;
                }
                AddVisualChild(value);
                _child = value;
            }
        }

        protected override int VisualChildrenCount => _child != null ? 1 : 0;

        protected override Size ArrangeOverride(Size finalSize)
        {
            _child?.Arrange(new Rect(finalSize));
            return finalSize;
        }
        //protected override Size MeasureOverride(Size constraint)
        //{
        //    _child.Measure(constraint);
        //    _background.Measure(constraint);
        //    return _child.DesiredSize;
        //}

        protected override Visual GetVisualChild(int index)
        {
            if (index == 0 && _child != null) return _child;
            return base.GetVisualChild(index);
        }
    }
}
