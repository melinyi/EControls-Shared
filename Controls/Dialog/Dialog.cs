using EControl.Tools;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace EControl.Controls
{
    public partial class Dialog : ContentControl
    {
        public static readonly DependencyProperty IsClosedProperty = DependencyProperty.Register(
         "IsClosed", typeof(bool), typeof(Dialog), new PropertyMetadata(false));

        public bool IsClosed
        {
            get => (bool)GetValue(IsClosedProperty);
            internal set => SetValue(IsClosedProperty, value);
        }

        public static Dialog Show(object DialogParent, object DialogContent)
        {
            var dialog = new Dialog
            {
                _ParentElement = (FrameworkElement)DialogParent,
                Content = DialogContent
            };

            //获取指定元素的装饰层
            AdornerDecorator decorator = Tools.Helper.VisualHelper.GetChild<AdornerDecorator>((FrameworkElement)DialogParent);

            if (decorator != null)
            {
                if (decorator.Child != null)
                {
                    decorator.Child.IsEnabled = false;
                }
                var layer = decorator.AdornerLayer;
                if (layer != null)
                {
                    var container = new AdornerContainer(layer)
                    {
                        //遮罩背景 Border
                        Child = new Border()
                        {
                            Background = new SolidColorBrush(Color.FromArgb(60, 0, 0, 0)),
                            Child = dialog//设置内容
                        }
                    };

                    //记录值
                    dialog._DialogContainer = container;
                    dialog.IsClosed = false;

                    layer.Add(container);
                }
            }
            else
            {
                throw new InvalidOperationException($"{DialogParent.GetType()} 这个元素没有 {typeof(AdornerDecorator)}");
            }
            return dialog;
        }

        public static Task<DialogResult<TResult>> ShowGetResult<TResult>(object DialogParent, object DialogContent, IDialogGetResultViewModel<TResult> TViewModel)
        {
            ((FrameworkElement)DialogContent).DataContext = TViewModel;
            var dialog = Show(DialogParent, DialogContent);
            return GetShowGetResultTask(dialog, TViewModel);
        }

        public static Task<DialogResult<TResult>> ShowGetResult<TResult>(object DialogParent, object DialogContentWithDataContext)
        {
            if (((FrameworkElement)DialogContentWithDataContext).DataContext is IDialogGetResultViewModel<TResult> TViewModel)
            {
                return ShowGetResult(DialogParent, DialogContentWithDataContext, TViewModel);
            }
            throw new InvalidOperationException($"{((FrameworkElement)DialogContentWithDataContext).DataContext ?? "未绑定DataContext"}， 需要实现接口 {typeof(IDialogGetResultViewModel<TResult>)}");
        }

        private static Task<DialogResult<TResult>> GetShowGetResultTask<TResult>(Dialog dialog, IDialogGetResultViewModel<TResult> TViewModel)
        {
            var tcs = new TaskCompletionSource<DialogResult<TResult>>();
            try
            {
                if (dialog.IsClosed)
                {
                    SetResult();
                }
                else
                {
                    dialog.Unloaded += OnUnloaded;
                    TViewModel.Dialog = dialog;
                }
            }
            catch (Exception e)
            {
                tcs.TrySetException(e);
            }

            return tcs.Task;

            // ReSharper disable once ImplicitlyCapturedClosure
            void OnUnloaded(object sender, RoutedEventArgs args)
            {
                dialog.Unloaded -= OnUnloaded;
                SetResult();
            }

            void SetResult()
            {
                try
                {
                    tcs.TrySetResult(TViewModel.Result);
                }
                catch (Exception e)
                {
                    tcs.TrySetException(e);
                }
            }
        }
    }

    public partial class Dialog : ContentControl
    {
        /// <summary>
        /// Dialog所在的装饰容器
        /// </summary>
        private AdornerContainer _DialogContainer { get; set; }

        /// <summary>
        /// Dialog 的父窗口
        /// </summary>
        private FrameworkElement _ParentElement { get; set; }


#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public Dialog()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        {
            //绑定默认的关闭命令
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, (s, e) => Close()));
        }

        public void Close()
        {
            //防止异步线程调用关闭导致线程访问异常抛出
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_ParentElement != null)
                {
                    Close(_ParentElement);
                }
            });
        }

        private void Close(DependencyObject element)
        {
            if (element != null && _DialogContainer != null)
            {
                AdornerDecorator decorator = Tools.Helper.VisualHelper.GetChild<AdornerDecorator>(element);
                if (decorator != null)
                {
                    if (decorator.Child != null)
                    {
                        decorator.Child.IsEnabled = true;
                    }
                    var layer = decorator.AdornerLayer;
                    layer?.Remove(_DialogContainer);
                    IsClosed = true;
                }
            }
        }
    }
}

