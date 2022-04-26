using EControl.Data;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
#nullable enable
namespace EControl.Controls
{
    /// <summary>
    /// 支持虚拟化的面板的基类。
    /// </summary>
    public abstract class VirtualizingPanelBase : VirtualizingPanel, IScrollInfo
    {
        public static readonly DependencyProperty ScrollLineDeltaProperty = DependencyProperty.Register(nameof(ScrollLineDelta), typeof(double), typeof(VirtualizingPanelBase), new FrameworkPropertyMetadata(16.0));
        public static readonly DependencyProperty MouseWheelDeltaProperty = DependencyProperty.Register(nameof(MouseWheelDelta), typeof(double), typeof(VirtualizingPanelBase), new FrameworkPropertyMetadata(48.0));
        public static readonly DependencyProperty ScrollLineDeltaItemProperty = DependencyProperty.Register(nameof(ScrollLineDeltaItem), typeof(int), typeof(VirtualizingPanelBase), new FrameworkPropertyMetadata(1));
        public static readonly DependencyProperty MouseWheelDeltaItemProperty = DependencyProperty.Register(nameof(MouseWheelDeltaItem), typeof(int), typeof(VirtualizingPanelBase), new FrameworkPropertyMetadata(3));
    
        public System.Windows.Controls.ScrollViewer? ScrollOwner { get; set; }

        public bool CanVerticallyScroll { get; set; }
        public bool CanHorizontallyScroll { get; set; }

        protected override bool CanHierarchicallyScrollAndVirtualizeCore => true;

        /// <summary>
        ///用于基于像素的滚动的滚动线增量。默认值为16 dp。
        /// </summary>
        public double ScrollLineDelta { get => (double)GetValue(ScrollLineDeltaProperty); set => SetValue(ScrollLineDeltaProperty, value); }

        /// <summary>
        ///用于基于像素的滚动的鼠标滚轮增量。默认值为48 dp。
        /// </summary>        
        public double MouseWheelDelta { get => (double)GetValue(MouseWheelDeltaProperty); set => SetValue(MouseWheelDeltaProperty, value); }

        /// <summary>
        /// 滚动行增量用于基于项目的滚动。默认值为1项。
        /// </summary>
        public double ScrollLineDeltaItem { get => (int)GetValue(ScrollLineDeltaItemProperty); set => SetValue(ScrollLineDeltaItemProperty, value); }

        /// <summary>
        /// 用于基于项目的滚动的鼠标滚轮增量。默认值为3项。
        /// </summary> 
        public int MouseWheelDeltaItem { get => (int)GetValue(MouseWheelDeltaItemProperty); set => SetValue(MouseWheelDeltaItemProperty, value); }
    
        protected ScrollUnit ScrollUnit => GetScrollUnit(ItemsControl);

        /// <summary>
        /// 用户转动鼠标滚轮时面板滚动的方向。
        /// </summary>
        protected ScrollDirection MouseWheelScrollDirection { get; set; } = ScrollDirection.Vertical;


        protected bool IsVirtualizing => GetIsVirtualizing(ItemsControl);

        protected VirtualizationMode VirtualizationMode => GetVirtualizationMode(ItemsControl);

        /// <summary>
        /// 如果面板处于虚拟化模式，则返回true。否则返回false。
        /// </summary>
        protected bool IsRecycling => VirtualizationMode == VirtualizationMode.Recycling;

        /// <summary>
        /// 视口前后的缓存长度。
        /// </summary>
        protected VirtualizationCacheLength CacheLength { get; private set; }

        /// <summary>
        ///缓存长度的单位。可以是像素、项目或页面。
        ///当ItemsOwner是组项目时，它只能是像素或项目。
        /// </summary>
        protected VirtualizationCacheLengthUnit CacheLengthUnit { get; private set; }

        /// <summary>
        /// ItemsControl（例如ListView）。
        /// </summary>
        protected ItemsControl ItemsControl => ItemsControl.GetItemsOwner(this);

        /// <summary>
        /// ItemsControl（例如ListView）或ItemsControl是否正在分组GroupItem。
        /// </summary>
        protected DependencyObject ItemsOwner
        {
            get
            {
                if (_itemsOwner is null)
                {
                    /*使用反射来访问内部方法，因为
                     *GetItemsOwner方法总是返回itmes控件
                     *实际项目所有者的名称，例如分组时的组项目*/
                    MethodInfo getItemsOwnerInternalMethod = typeof(ItemsControl).GetMethod(
                        "GetItemsOwnerInternal",
                        BindingFlags.Static | BindingFlags.NonPublic,
                        null,
                        new Type[] { typeof(DependencyObject) },
                        null
                    )!;
                    _itemsOwner = (DependencyObject)getItemsOwnerInternalMethod.Invoke(null, new object[] { this })!;
                }
                return _itemsOwner;
            }
        }
        private DependencyObject? _itemsOwner;

        protected ReadOnlyCollection<object?> Items => ((ItemContainerGenerator)ItemContainerGenerator).Items;

        protected new IRecyclingItemContainerGenerator ItemContainerGenerator
        {
            get
            {
                if (_itemContainerGenerator is null)
                {
                    /* Because of a bug in the framework the ItemContainerGenerator 
                     * is null until InternalChildren accessed at least one time. */
                    var children = InternalChildren;
                    _itemContainerGenerator = (IRecyclingItemContainerGenerator)base.ItemContainerGenerator;
                }
                return _itemContainerGenerator;
            }
        }
        private IRecyclingItemContainerGenerator? _itemContainerGenerator;

        public double ExtentWidth => Extent.Width;
        public double ExtentHeight => Extent.Height;
        protected Size Extent { get; private set; } = new Size(0, 0);

        public double HorizontalOffset => Offset.X;
        public double VerticalOffset => Offset.Y;
        protected Size Viewport { get; private set; } = new Size(0, 0);

        public double ViewportWidth => Viewport.Width;
        public double ViewportHeight => Viewport.Height;
        protected Point Offset { get; private set; } = new Point(0, 0);

        /// <summary>
        /// 在视口或缓存中实现的项目范围。
        /// </summary>
        protected ItemRange ItemRange { get; set; }

        private Visibility previousVerticalScrollBarVisibility = Visibility.Collapsed;
        private Visibility previousHorizontalScrollBarVisibility = Visibility.Collapsed;

        protected virtual void UpdateScrollInfo(Size availableSize, Size extent)
        {
            bool invalidateScrollInfo = false;

            if (extent != Extent)
            {
                Extent = extent;
                invalidateScrollInfo = true;

            }
            if (availableSize != Viewport)
            {
                Viewport = availableSize;
                invalidateScrollInfo = true;
            }

            if (ViewportHeight != 0 && VerticalOffset != 0 && VerticalOffset + ViewportHeight + 1 >= ExtentHeight)
            {
                Offset = new Point(Offset.X, extent.Height - availableSize.Height);
                invalidateScrollInfo = true;
            }
            if (ViewportWidth != 0 && HorizontalOffset != 0 && HorizontalOffset + ViewportWidth + 1 >= ExtentWidth)
            {
                Offset = new Point(extent.Width - availableSize.Width, Offset.Y);
                invalidateScrollInfo = true;
            }

            if (invalidateScrollInfo)
            {
                ScrollOwner?.InvalidateScrollInfo();
            }
        }

        public virtual Rect MakeVisible(Visual visual, Rect rectangle)
        {
            Point pos = visual.TransformToAncestor(this).Transform(Offset);

            double scrollAmountX = 0;
            double scrollAmountY = 0;

            if (pos.X < Offset.X)
            {
                scrollAmountX = -(Offset.X - pos.X);
            }
            else if ((pos.X + rectangle.Width) > (Offset.X + Viewport.Width))
            {
                scrollAmountX = (pos.X + rectangle.Width) - (Offset.X + Viewport.Width);
            }

            if (pos.Y < Offset.Y)
            {
                scrollAmountY = -(Offset.Y - pos.Y);
            }
            else if ((pos.Y + rectangle.Height) > (Offset.Y + Viewport.Height))
            {
                scrollAmountY = (pos.Y + rectangle.Height) - (Offset.Y + Viewport.Height);
            }

            SetHorizontalOffset(Offset.X + scrollAmountX);

            SetVerticalOffset(Offset.Y + scrollAmountY);

            double visibleRectWidth = Math.Min(rectangle.Width, Viewport.Width);
            double visibleRectHeight = Math.Min(rectangle.Height, Viewport.Height);

            return new Rect(scrollAmountX, scrollAmountY, visibleRectWidth, visibleRectHeight);
        }

        protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                    RemoveInternalChildRange(args.Position.Index, args.ItemUICount);
                    break;
                case NotifyCollectionChangedAction.Move:
                    RemoveInternalChildRange(args.OldPosition.Index, args.ItemUICount);
                    break;
            }
        }

        protected int GetItemIndexFromChildIndex(int childIndex)
        {
            var generatorPosition = GetGeneratorPositionFromChildIndex(childIndex);
            return ItemContainerGenerator.IndexFromGeneratorPosition(generatorPosition);
        }

        protected virtual GeneratorPosition GetGeneratorPositionFromChildIndex(int childIndex)
        {
            return new GeneratorPosition(childIndex, 0);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            /*有时滚动时，滚动条会无缘无故隐藏。在这种情况下，“IsMeasureValid”
             *ScrollOwner的属性为false。为防止出现无限循环，将忽略mesasure调用*/
            if (ScrollOwner != null)
            {
                bool verticalScrollBarGotHidden = ScrollOwner.VerticalScrollBarVisibility == ScrollBarVisibility.Auto
                    && ScrollOwner.ComputedVerticalScrollBarVisibility != Visibility.Visible
                    && ScrollOwner.ComputedVerticalScrollBarVisibility != previousVerticalScrollBarVisibility;

                bool horizontalScrollBarGotHidden = ScrollOwner.HorizontalScrollBarVisibility == ScrollBarVisibility.Auto
                   && ScrollOwner.ComputedHorizontalScrollBarVisibility != Visibility.Visible
                   && ScrollOwner.ComputedHorizontalScrollBarVisibility != previousHorizontalScrollBarVisibility;

                previousVerticalScrollBarVisibility = ScrollOwner.ComputedVerticalScrollBarVisibility;
                previousHorizontalScrollBarVisibility = ScrollOwner.ComputedHorizontalScrollBarVisibility;

                if (!ScrollOwner.IsMeasureValid && verticalScrollBarGotHidden || horizontalScrollBarGotHidden)
                {
                    return availableSize;
                }
            }

            var groupItem = ItemsOwner as IHierarchicalVirtualizationAndScrollInfo;

            Size extent;
            Size desiredSize;

            if (groupItem != null)
            {
                /* If the ItemsOwner is a group item the availableSize is ifinity. 
                 * Therfore the vieport size provided by the group item is used. */
                /*如果ItemsOwner是一个组项，则availableSize为iUnity。
                 *因此，使用组项提供的vieport大小*/
                var viewportSize = groupItem.Constraints.Viewport.Size;
                var headerSize = groupItem.HeaderDesiredSizes.PixelSize;
                double availableWidth = Math.Max(viewportSize.Width - 5, 0); // 左边缘5 dp left margin of 5 dp
                double availableHeight = Math.Max(viewportSize.Height - headerSize.Height, 0);
                availableSize = new Size(availableWidth, availableHeight);

                extent = CalculateExtent(availableSize);

                desiredSize = new Size(extent.Width, extent.Height);

                Extent = extent;
                Offset = groupItem.Constraints.Viewport.Location;
                Viewport = groupItem.Constraints.Viewport.Size;
                CacheLength = groupItem.Constraints.CacheLength;
                CacheLengthUnit = groupItem.Constraints.CacheLengthUnit; //可以是项目或像素 can be Item or Pixel
            }
            else
            {
                extent = CalculateExtent(availableSize);
                double desiredWidth = Math.Min(availableSize.Width, extent.Width);
                double desiredHeight = Math.Min(availableSize.Height, extent.Height);
                desiredSize = new Size(desiredWidth, desiredHeight);

                UpdateScrollInfo(desiredSize, extent);
                CacheLength = GetCacheLength(ItemsOwner);
                CacheLengthUnit = GetCacheLengthUnit(ItemsOwner); //可以是页面、项目或像素 can be Page, Item or Pixel
            }

            ItemRange = UpdateItemRange();

            RealizeItems();
            VirtualizeItems();

            return desiredSize;
        }

        private int count = 0;

        /// <summary>
        /// 恢复可见项和缓存项。Realizes visible and cached items.
        /// </summary>
        protected virtual void RealizeItems()
        {
            var startPosition = ItemContainerGenerator.GeneratorPositionFromIndex(ItemRange.StartIndex);

            int childIndex = startPosition.Offset == 0 ? startPosition.Index : startPosition.Index + 1;

            using (ItemContainerGenerator.StartAt(startPosition, GeneratorDirection.Forward, true))
            {
                for (int i = ItemRange.StartIndex; i <= ItemRange.EndIndex; i++, childIndex++)
                {
                    UIElement child = (UIElement)ItemContainerGenerator.GenerateNext(out bool isNewlyRealized);
                    if (isNewlyRealized || /*recycled*/!InternalChildren.Contains(child))
                    {

                        count++;
                        if (count>=1000)
                        {
                            Console.WriteLine("GC~"+DateTime.Now);
                            System.GC.Collect();
                            count= 0;
                        }

                        if (childIndex >= InternalChildren.Count)
                        {
                            AddInternalChild(child);
                        }
                        else
                        {
                            InsertInternalChild(childIndex, child);
                        }
                        ItemContainerGenerator.PrepareItemContainer(child);

                        child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    }

                    if (child is IHierarchicalVirtualizationAndScrollInfo groupItem)
                    {
                        groupItem.Constraints = new HierarchicalVirtualizationConstraints(
                            new VirtualizationCacheLength(0),
                            VirtualizationCacheLengthUnit.Item,
                            new Rect(0, 0, ViewportWidth, ViewportHeight));
                        child.Measure(new Size(ViewportWidth, ViewportHeight));
                    }
                }
            }
        }

        /// <summary>
        /// 虚拟化（清理）不再可见或缓存的项。
        /// </summary>
        protected virtual void VirtualizeItems()
        {
            for (int childIndex = InternalChildren.Count - 1; childIndex >= 0; childIndex--)
            {
                //从ChildIndex获取生成器位置
                var generatorPosition = GetGeneratorPositionFromChildIndex(childIndex);

                int itemIndex = ItemContainerGenerator.IndexFromGeneratorPosition(generatorPosition);

                if (itemIndex != -1 && !ItemRange.Contains(itemIndex))
                {
                    if (VirtualizationMode == VirtualizationMode.Recycling)
                    {
                        ItemContainerGenerator.Recycle(generatorPosition, 1);
                    }
                    else
                    {
                        ItemContainerGenerator.Remove(generatorPosition, 1);
                    }
                    RemoveInternalChildRange(childIndex, 1);
                }
            }
        }

        /// <summary>
        /// Calculates the extent that would be needed to show all items.
        /// </summary>
        protected abstract Size CalculateExtent(Size availableSize);

        /// <summary>
        /// 计算视口中可见或缓存的项目范围。 Calculates the item range that is visible in the viewport or cached.
        /// </summary>
        protected abstract ItemRange UpdateItemRange();

        public void SetVerticalOffset(double offset)
        {
            if (offset < 0 || Viewport.Height >= Extent.Height)
            {
                offset = 0;
            }
            else if (offset + Viewport.Height >= Extent.Height)
            {
                offset = Extent.Height - Viewport.Height;
            }
            Offset = new Point(Offset.X, offset);
            ScrollOwner?.InvalidateScrollInfo();
            InvalidateMeasure();
        }

        public void SetHorizontalOffset(double offset)
        {
            if (offset < 0 || Viewport.Width >= Extent.Width)
            {
                offset = 0;
            }
            else if (offset + Viewport.Width >= Extent.Width)
            {
                offset = Extent.Width - Viewport.Width;
            }
            Offset = new Point(offset, Offset.Y);
            ScrollOwner?.InvalidateScrollInfo();
            InvalidateMeasure();
        }

        protected void ScrollVertical(double amount)
        {
            SetVerticalOffset(VerticalOffset + amount);
        }

        protected void ScrollHorizontal(double amount)
        {
            SetHorizontalOffset(HorizontalOffset + amount);
        }

        public void LineUp() => ScrollVertical(ScrollUnit == ScrollUnit.Pixel ? -ScrollLineDelta : GetLineUpScrollAmount());
        public void LineDown() => ScrollVertical(ScrollUnit == ScrollUnit.Pixel ? ScrollLineDelta : GetLineDownScrollAmount());
        public void LineLeft() => ScrollHorizontal(ScrollUnit == ScrollUnit.Pixel ? -ScrollLineDelta : GetLineLeftScrollAmount());
        public void LineRight() => ScrollHorizontal(ScrollUnit == ScrollUnit.Pixel ? ScrollLineDelta : GetLineRightScrollAmount());

        public void MouseWheelUp()
        {
            if (MouseWheelScrollDirection == ScrollDirection.Vertical)
            {
                ScrollVertical(ScrollUnit == ScrollUnit.Pixel ? -MouseWheelDelta : GetMouseWheelUpScrollAmount());
            }
            else
            {
                MouseWheelLeft();
            }
        }

        public void MouseWheelDown()
        {
            if (MouseWheelScrollDirection == ScrollDirection.Vertical)
            {
                ScrollVertical(ScrollUnit == ScrollUnit.Pixel ? MouseWheelDelta : GetMouseWheelDownScrollAmount());
            }
            else
            {
                MouseWheelRight();
            }
        }

        public void MouseWheelLeft() => ScrollHorizontal(ScrollUnit == ScrollUnit.Pixel ? -MouseWheelDelta : GetMouseWheelLeftScrollAmount());
        public void MouseWheelRight() => ScrollHorizontal(ScrollUnit == ScrollUnit.Pixel ? MouseWheelDelta : GetMouseWheelRightScrollAmount());

        public void PageUp() => ScrollVertical(ScrollUnit == ScrollUnit.Pixel ? -ViewportHeight : GetPageUpScrollAmount());
        public void PageDown() => ScrollVertical(ScrollUnit == ScrollUnit.Pixel ? ViewportHeight : GetPageDownScrollAmount());
        public void PageLeft() => ScrollHorizontal(ScrollUnit == ScrollUnit.Pixel ? -ViewportHeight : GetPageLeftScrollAmount());
        public void PageRight() => ScrollHorizontal(ScrollUnit == ScrollUnit.Pixel ? ViewportHeight : GetPageRightScrollAmount());

        protected abstract double GetLineUpScrollAmount();
        protected abstract double GetLineDownScrollAmount();
        protected abstract double GetLineLeftScrollAmount();
        protected abstract double GetLineRightScrollAmount();

        protected abstract double GetMouseWheelUpScrollAmount();
        protected abstract double GetMouseWheelDownScrollAmount();
        protected abstract double GetMouseWheelLeftScrollAmount();
        protected abstract double GetMouseWheelRightScrollAmount();

        protected abstract double GetPageUpScrollAmount();
        protected abstract double GetPageDownScrollAmount();
        protected abstract double GetPageLeftScrollAmount();
        protected abstract double GetPageRightScrollAmount();

    }
}
