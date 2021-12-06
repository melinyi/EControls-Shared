using EControl.Data;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace EControl.Controls
{
    /// <summary>
    /// VirtualizingWrapPanel，支持虚拟化，可用于水平和垂直方向。
    /// <p class="note">为了正常工作，所有项目必须具有相同的大小。</p>
    /// </summary>
    public class VirtualizingWrapPanel : VirtualizingPanelBase
    {
        #region Deprecated properties

        //[Obsolete("Use SpacingMode")]
        public static readonly DependencyProperty IsSpacingEnabledProperty = DependencyProperty.Register(nameof(IsSpacingEnabled), typeof(bool), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure));

        //[Obsolete("Use IsSpacingEnabled")]
        public bool SpacingEnabled { get => IsSpacingEnabled; set => IsSpacingEnabled = value; }

        ///<summary>
        ///获取或设置一个值，该值指定项目是否均匀分布在宽度上（水平方向）
        ///或高度（垂直方向）。默认值为true。
        ///</summary>
        //[Obsolete("Use SpacingMode")]
        public bool IsSpacingEnabled { get => (bool)GetValue(IsSpacingEnabledProperty); set => SetValue(IsSpacingEnabledProperty, value); }

        //[Obsolete("Use ItemSize")]
        //public Size ChildrenSize { get => ItemSize; set => ItemSize = value; }

        #endregion

        public static readonly DependencyProperty SpacingModeProperty = DependencyProperty.Register(nameof(SpacingMode), typeof(SpacingMode), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(SpacingMode.Uniform, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsMeasure, (obj, args) => ((VirtualizingWrapPanel)obj).Orientation_Changed()));

        public static readonly DependencyProperty ItemSizeProperty = DependencyProperty.Register(nameof(ItemSize), typeof(Size), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(Size.Empty, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty StretchItemsProperty = DependencyProperty.Register(nameof(StretchItems), typeof(bool), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>
        /// 获取或设置排列项目时使用的间距模式。默认值为 <see cref="SpacingMode.Uniform"/>.
        /// </summary>
        public SpacingMode SpacingMode { get => (SpacingMode)GetValue(SpacingModeProperty); set => SetValue(SpacingModeProperty, value); }

        /// <summary>
        /// 获取或设置一个值，该值指定项目的排列方向。默认值为 <see cref="Orientation.Vertical"/>.
        /// </summary>
        public System.Windows.Controls.Orientation Orientation { get => (Orientation)GetValue(OrientationProperty); set => SetValue(OrientationProperty, value); }

        /// <summary>
        /// Gets or sets a value that specifies the size of the items. The default value is <see cref="Size.Empty"/>. 
        /// If the value is <see cref="Size.Empty"/> the size of the items gots measured by the first realized item.
        /// </summary>
        public Size ItemSize { get => (Size)GetValue(ItemSizeProperty); set => SetValue(ItemSizeProperty, value); }

        /// <summary>
        /// 获取或设置一个值，该值指定项目是否拉伸以填充剩余空间。默认值为false。
        /// </summary>
        /// <remarks>
        ///ItemContainerStyle的MaxWidth和MaxHeight属性可用于限制拉伸。
        ///在这种情况下，剩余空间的使用将由SpacingMode属性确定。
        /// </remarks>
        public bool StretchItems { get => (bool)GetValue(StretchItemsProperty); set => SetValue(StretchItemsProperty, value); }

        protected Size childSize;

        protected int rowCount;

        protected int itemsPerRowCount;

        private void Orientation_Changed()
        {
            MouseWheelScrollDirection = Orientation == Orientation.Vertical ? ScrollDirection.Vertical : ScrollDirection.Horizontal;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            UpdateChildSize(availableSize);
            return base.MeasureOverride(availableSize);
        }

        private void UpdateChildSize(Size availableSize)
        {
            if (ItemsOwner is IHierarchicalVirtualizationAndScrollInfo groupItem
                && VirtualizingPanel.GetIsVirtualizingWhenGrouping(ItemsControl))
            {
                if (Orientation == Orientation.Vertical)
                {
                    availableSize.Width = groupItem.Constraints.Viewport.Size.Width;
                    availableSize.Width = Math.Max(availableSize.Width - (Margin.Left + Margin.Right), 0);
                }
                else
                {
                    availableSize.Height = groupItem.Constraints.Viewport.Size.Height;
                    availableSize.Height = Math.Max(availableSize.Height - (Margin.Top + Margin.Bottom), 0);
                }
            }

            if (ItemSize != Size.Empty)
            {
                childSize = ItemSize;
            }
            else if (InternalChildren.Count != 0)
            {
                childSize = InternalChildren[0].DesiredSize;
            }
            else
            {
                childSize = CalculateChildSize(availableSize);
            }

            if (double.IsInfinity(GetWidth(availableSize)))
            {
                itemsPerRowCount = Items.Count;
            }
            else
            {
                itemsPerRowCount = Math.Max(1, (int)Math.Floor(GetWidth(availableSize) / GetWidth(childSize)));
            }

            rowCount = (int)Math.Ceiling((double)Items.Count / itemsPerRowCount);
        }

        private Size CalculateChildSize(Size availableSize)
        {
            if (Items.Count == 0)
            {
                return new Size(0, 0);
            }
            var startPosition = ItemContainerGenerator.GeneratorPositionFromIndex(0);
            using (ItemContainerGenerator.StartAt(startPosition, GeneratorDirection.Forward, true))
            {
                var child = (UIElement)ItemContainerGenerator.GenerateNext();
                AddInternalChild(child);
                ItemContainerGenerator.PrepareItemContainer(child);
                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                return child.DesiredSize;
            }
        }

        protected override Size CalculateExtent(Size availableSize)
        {
            double extentWidth = IsSpacingEnabled && SpacingMode != SpacingMode.None && !double.IsInfinity(GetWidth(availableSize))
                ? GetWidth(availableSize)
                : GetWidth(childSize) * itemsPerRowCount;

            if (ItemsOwner is IHierarchicalVirtualizationAndScrollInfo groupItem)
            {
                if (Orientation == Orientation.Vertical)
                {
                    extentWidth = Math.Max(extentWidth - (Margin.Left + Margin.Right), 0);
                }
                else
                {
                    extentWidth = Math.Max(extentWidth - (Margin.Top + Margin.Bottom), 0);
                }
            }

            double extentHeight = GetHeight(childSize) * rowCount;
            return CreateSize(extentWidth, extentHeight);
        }

        protected void CalculateSpacing(Size finalSize, out double innerSpacing, out double outerSpacing)
        {
            Size childSize = CalculateChildArrangeSize(finalSize);

            double finalWidth = GetWidth(finalSize);

            double totalItemsWidth = Math.Min(GetWidth(childSize) * itemsPerRowCount, finalWidth);
            double unusedWidth = finalWidth - totalItemsWidth;

            SpacingMode spacingMode = IsSpacingEnabled ? SpacingMode : SpacingMode.None;

            switch (spacingMode)
            {
                case SpacingMode.Uniform:
                    innerSpacing = outerSpacing = unusedWidth / (itemsPerRowCount + 1);
                    break;

                case SpacingMode.BetweenItemsOnly:
                    innerSpacing = unusedWidth / Math.Max(itemsPerRowCount - 1, 1);
                    outerSpacing = 0;
                    break;

                case SpacingMode.StartAndEndOnly:
                    innerSpacing = 0;
                    outerSpacing = unusedWidth / 2;
                    break;

                case SpacingMode.None:
                default:
                    innerSpacing = 0;
                    outerSpacing = 0;
                    break;
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double offsetX = GetX(Offset);
            double offsetY = GetY(Offset);

            /* When the items owner is a group item offset is handled by the parent panel. */
            if (ItemsOwner is IHierarchicalVirtualizationAndScrollInfo groupItem)
            {
                offsetY = 0;
            }

            Size childSize = CalculateChildArrangeSize(finalSize);

            CalculateSpacing(finalSize, out double innerSpacing, out double outerSpacing);

            for (int childIndex = 0; childIndex < InternalChildren.Count; childIndex++)
            {
                UIElement child = InternalChildren[childIndex];

                int itemIndex = GetItemIndexFromChildIndex(childIndex);

                int columnIndex = itemIndex % itemsPerRowCount;
                int rowIndex = itemIndex / itemsPerRowCount;

                double x = outerSpacing + columnIndex * (GetWidth(childSize) + innerSpacing);
                double y = rowIndex * GetHeight(childSize);

                if (GetHeight(finalSize) == 0.0)
                {
                    /*当父面板正在分组，而缓存的组项未
                     *在视口中，它没有有效的排列。这意味着                     
                     *高度/宽度为0。因此，项目不应可见，因此                     
                     *他们没有被错误地展示*/
                    child.Arrange(new Rect(0, 0, 0, 0));
                }
                else
                {
                    child.Arrange(CreateRect(x - offsetX, y - offsetY, childSize.Width, childSize.Height));
                }
            }

            return finalSize;
        }

        /// <summary>
        /// 计算子元素排列大小
        /// </summary>
        /// <param name="finalSize"></param>
        /// <returns></returns>
        protected Size CalculateChildArrangeSize(Size finalSize)
        {
            if (StretchItems)
            {
                if (Orientation == Orientation.Vertical)
                {
                    double childMaxWidth = ReadItemContainerStyle(MaxWidthProperty, double.PositiveInfinity);
                    double maxPossibleChildWith = finalSize.Width / itemsPerRowCount;
                    double childWidth = Math.Min(maxPossibleChildWith, childMaxWidth);
                    return new Size(childWidth, childSize.Height);
                }
                else
                {
                    double childMaxHeight = ReadItemContainerStyle(MaxHeightProperty, double.PositiveInfinity);
                    double maxPossibleChildHeight = finalSize.Height / itemsPerRowCount;
                    double childHeight = Math.Min(maxPossibleChildHeight, childMaxHeight);
                    return new Size(childSize.Width, childHeight);
                }
            }
            else
            {
                return childSize;
            }
        }

        private T ReadItemContainerStyle<T>(DependencyProperty property, T fallbackValue) where T : notnull
        {
            var value = ItemsControl.ItemContainerStyle?.Setters.OfType<Setter>()
                .FirstOrDefault(setter => setter.Property == property)?.Value;
            return (T)(value ?? fallbackValue);
        }

        protected override ItemRange UpdateItemRange()
        {
            if (!IsVirtualizing)
            {
                return new ItemRange(0, Items.Count - 1);
            }

            int startIndex;
            int endIndex;

            if (ItemsOwner is IHierarchicalVirtualizationAndScrollInfo groupItem)
            {
                if (!VirtualizingPanel.GetIsVirtualizingWhenGrouping(ItemsControl))
                {
                    return new ItemRange(0, Items.Count - 1);
                }

                var offset = new Point(Offset.X, groupItem.Constraints.Viewport.Location.Y);

                int offsetRowIndex;
                double offsetInPixel;

                int rowCountInViewport;

                if (ScrollUnit == ScrollUnit.Item)
                {
                    offsetRowIndex = GetY(offset) >= 1 ? (int)GetY(offset) - 1 : 0; // ignore header
                    offsetInPixel = offsetRowIndex * GetHeight(childSize);
                }
                else
                {
                    offsetInPixel = Math.Min(Math.Max(GetY(offset) - GetHeight(groupItem.HeaderDesiredSizes.PixelSize), 0), GetHeight(Extent));
                    offsetRowIndex = GetRowIndex(offsetInPixel);
                }

                double viewportHeight = Math.Min(GetHeight(Viewport), Math.Max(GetHeight(Extent) - offsetInPixel, 0));

                rowCountInViewport = (int)Math.Ceiling((offsetInPixel + viewportHeight) / GetHeight(childSize)) - (int)Math.Floor(offsetInPixel / GetHeight(childSize));

                startIndex = offsetRowIndex * itemsPerRowCount;
                endIndex = Math.Min(((offsetRowIndex + rowCountInViewport) * itemsPerRowCount) - 1, Items.Count - 1);

                if (CacheLengthUnit == VirtualizationCacheLengthUnit.Pixel)
                {
                    double cacheBeforeInPixel = Math.Min(CacheLength.CacheBeforeViewport, offsetInPixel);
                    double cacheAfterInPixel = Math.Min(CacheLength.CacheAfterViewport, GetHeight(Extent) - viewportHeight - offsetInPixel);
                    int rowCountInCacheBefore = (int)(cacheBeforeInPixel / GetHeight(childSize));
                    int rowCountInCacheAfter = ((int)Math.Ceiling((offsetInPixel + viewportHeight + cacheAfterInPixel) / GetHeight(childSize))) - (int)Math.Ceiling((offsetInPixel + viewportHeight) / GetHeight(childSize));
                    startIndex = Math.Max(startIndex - rowCountInCacheBefore * itemsPerRowCount, 0);
                    endIndex = Math.Min(endIndex + rowCountInCacheAfter * itemsPerRowCount, Items.Count - 1);
                }
                else if (CacheLengthUnit == VirtualizationCacheLengthUnit.Item)
                {
                    startIndex = Math.Max(startIndex - (int)CacheLength.CacheBeforeViewport, 0);
                    endIndex = Math.Min(endIndex + (int)CacheLength.CacheAfterViewport, Items.Count - 1);
                }
            }
            else
            {
                double viewportSartPos = GetY(Offset);
                double viewportEndPos = GetY(Offset) + GetHeight(Viewport);

                if (CacheLengthUnit == VirtualizationCacheLengthUnit.Pixel)
                {
                    viewportSartPos = Math.Max(viewportSartPos - CacheLength.CacheBeforeViewport, 0);
                    viewportEndPos = Math.Min(viewportEndPos + CacheLength.CacheAfterViewport, GetHeight(Extent));
                }

                int startRowIndex = GetRowIndex(viewportSartPos);
                startIndex = startRowIndex * itemsPerRowCount;

                int endRowIndex = GetRowIndex(viewportEndPos);
                endIndex = Math.Min(endRowIndex * itemsPerRowCount + (itemsPerRowCount - 1), Items.Count - 1);

                if (CacheLengthUnit == VirtualizationCacheLengthUnit.Page)
                {
                    int itemsPerPage = endIndex - startIndex + 1;
                    startIndex = Math.Max(startIndex - (int)CacheLength.CacheBeforeViewport * itemsPerPage, 0);
                    endIndex = Math.Min(endIndex + (int)CacheLength.CacheAfterViewport * itemsPerPage, Items.Count - 1);
                }
                else if (CacheLengthUnit == VirtualizationCacheLengthUnit.Item)
                {
                    startIndex = Math.Max(startIndex - (int)CacheLength.CacheBeforeViewport, 0);
                    endIndex = Math.Min(endIndex + (int)CacheLength.CacheAfterViewport, Items.Count - 1);
                }
            }

            return new ItemRange(startIndex, endIndex);
        }

        private int GetRowIndex(double location)
        {
            int calculatedRowIndex = (int)Math.Floor(location / GetHeight(childSize));
            int maxRowIndex = (int)Math.Ceiling((double)Items.Count / (double)itemsPerRowCount);
            return Math.Max(Math.Min(calculatedRowIndex, maxRowIndex), 0);
        }

        protected override void BringIndexIntoView(int index)
        {
            var offset = (index / itemsPerRowCount) * GetHeight(childSize);
            if (Orientation == Orientation.Horizontal)
            {
                SetHorizontalOffset(offset);
            }
            else
            {
                SetVerticalOffset(offset);
            }
        }

        protected override double GetLineUpScrollAmount()
        {
            return -Math.Min(childSize.Height * ScrollLineDeltaItem, Viewport.Height);
        }

        protected override double GetLineDownScrollAmount()
        {
            return Math.Min(childSize.Height * ScrollLineDeltaItem, Viewport.Height);
        }

        protected override double GetLineLeftScrollAmount()
        {
            return -Math.Min(childSize.Width * ScrollLineDeltaItem, Viewport.Width);
        }

        protected override double GetLineRightScrollAmount()
        {
            return Math.Min(childSize.Width * ScrollLineDeltaItem, Viewport.Width);
        }

        protected override double GetMouseWheelUpScrollAmount()
        {
            return -Math.Min(childSize.Height * MouseWheelDeltaItem, Viewport.Height);
        }

        protected override double GetMouseWheelDownScrollAmount()
        {
            return Math.Min(childSize.Height * MouseWheelDeltaItem, Viewport.Height);
        }

        protected override double GetMouseWheelLeftScrollAmount()
        {
            return -Math.Min(childSize.Width * MouseWheelDeltaItem, Viewport.Width);
        }

        protected override double GetMouseWheelRightScrollAmount()
        {
            return Math.Min(childSize.Width * MouseWheelDeltaItem, Viewport.Width);
        }

        protected override double GetPageUpScrollAmount()
        {
            return -Viewport.Height;
        }

        protected override double GetPageDownScrollAmount()
        {
            return Viewport.Height;
        }

        protected override double GetPageLeftScrollAmount()
        {
            return -Viewport.Width;
        }

        protected override double GetPageRightScrollAmount()
        {
            return Viewport.Width;
        }

        /* orientation aware helper methods */
        /*方向感知辅助方法*/
        protected double GetX(Point point) => Orientation == Orientation.Vertical ? point.X : point.Y;
        protected double GetY(Point point) => Orientation == Orientation.Vertical ? point.Y : point.X;

        protected double GetWidth(Size size) => Orientation == Orientation.Vertical ? size.Width : size.Height;
        protected double GetHeight(Size size) => Orientation == Orientation.Vertical ? size.Height : size.Width;

        protected Size CreateSize(double width, double height) => Orientation == Orientation.Vertical ? new Size(width, height) : new Size(height, width);
        protected Rect CreateRect(double x, double y, double width, double height) => Orientation == Orientation.Vertical ? new Rect(x, y, width, height) : new Rect(y, x, width, height);
    }
}
