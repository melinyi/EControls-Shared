using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace EControl.Controls
{
    public static class WindowAttach
    {

        // Using a DependencyProperty as the backing store for DialogEnableCloseButton.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DialogEnableHandyCloseButtonProperty =
            DependencyProperty.RegisterAttached("DialogEnableHandyCloseButton", typeof(bool), typeof(WindowAttach), new PropertyMetadata(false, OnDialogEnableHandyCloseButtonPropertyChanged));

        private static void OnDialogEnableHandyCloseButtonPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window)
            {
                if ((bool)e.NewValue)
                {
                    window.ContentRendered += Window_ContentRendered_DialogEnableHandyCloseButtonProperty;
                }
                else
                {
                    if (window.Template?.FindName("ButtonClose", window) is System.Windows.Controls.Button button)
                        button.SetBinding(System.Windows.Controls.Button.IsEnabledProperty, "");
                }            

                void Window_ContentRendered_DialogEnableHandyCloseButtonProperty(object sender, EventArgs e)
                {
                    if (window.Template?.FindName("ButtonClose", window) is System.Windows.Controls.Button button)
                    {
                        if (Tools.Helper.VisualHelper.GetChild<System.Windows.Documents.AdornerDecorator>(window) is System.Windows.Documents.AdornerDecorator ado)
                        {
                            button.SetBinding(System.Windows.Controls.Button.IsEnabledProperty, new Binding() { Source = ado.Child, Path = new PropertyPath("IsEnabled") });
                            window.ContentRendered -= Window_ContentRendered_DialogEnableHandyCloseButtonProperty;
                        }
                    }               
                }             
            }
        }

        public static bool GetDialogEnableCloseButton(DependencyObject obj)
        {
            return (bool)obj.GetValue(DialogEnableHandyCloseButtonProperty);
        }

        public static void SetDialogEnableCloseButton(DependencyObject obj, bool value)
        {
            obj.SetValue(DialogEnableHandyCloseButtonProperty, value);
        }



    }
}
