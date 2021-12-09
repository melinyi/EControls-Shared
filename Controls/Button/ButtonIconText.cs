using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.ComponentModel;

namespace EControl.Controls
{
    public class ButtonIconText : Button
    {

        public ButtonIconText()
        {
     
            var tempLateStr = @"<ControlTemplate TargetType='Button' xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                    <Border 
                    BorderBrush = '{Binding RelativeSource={RelativeSource Mode=TemplatedParent},Path=BorderBrush}'
                    BorderThickness = '{Binding RelativeSource={RelativeSource Mode=TemplatedParent},Path=BorderThickness}'
                    CornerRadius = '{Binding RelativeSource={RelativeSource Mode=TemplatedParent},Path=BorderRadius}'
                    Background = '{Binding RelativeSource={RelativeSource Mode=TemplatedParent},Path=Background}'
                    Height = '{Binding RelativeSource={RelativeSource Mode=TemplatedParent},Path=Height}'
                    Padding = '{Binding RelativeSource={RelativeSource Mode=TemplatedParent},Path=Padding}'
                    >
                         <Grid VerticalAlignment = 'Center'>
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width = 'auto' /> 
                             <ColumnDefinition Width = '*' />  
                          </Grid.ColumnDefinitions>
  
                          <TextBlock Margin = '{Binding RelativeSource={RelativeSource Mode=TemplatedParent},Path=IconMargin}'
                                     FontSize = '{Binding RelativeSource={RelativeSource Mode=TemplatedParent},Path=IconFontSize}' 
                                     Foreground = '{Binding RelativeSource={RelativeSource Mode=TemplatedParent},Path=IconForeground}'
                                     FontFamily = '{Binding RelativeSource={RelativeSource Mode=TemplatedParent},Path=IconFontFamily}' 
                                     Text = '{Binding Icon, RelativeSource={RelativeSource Mode=TemplatedParent}}' 
                          />
  
                          <TextBlock  Grid.Column = '1'
                                   Margin = '{Binding  ContentMargin, RelativeSource={RelativeSource Mode=TemplatedParent}}' 
                                   Text = '{Binding  Content, RelativeSource={RelativeSource Mode=TemplatedParent}}'
                                   FontSize = '{Binding RelativeSource={RelativeSource Mode=TemplatedParent},Path=FontSize}'
                                   FontFamily = '{Binding RelativeSource={RelativeSource Mode=TemplatedParent},Path=FontFamily}'
                                   VerticalAlignment = 'Center' HorizontalAlignment = 'Left' 
                          />
  
                      </Grid>
                    </Border>
                  </ControlTemplate> ";

            this.Template = (ControlTemplate)System.Windows.Markup.XamlReader.Parse(tempLateStr);

        }

     
        public string Icon
        {
            get { return (string)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(string), typeof(ButtonIconText), new PropertyMetadata(""));



        public Brush IconForeground
        {
            get { return (Brush)GetValue(IconForegroundProperty); }
            set { SetValue(IconForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IconForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconForegroundProperty =
            DependencyProperty.Register("IconForeground", typeof(Brush), typeof(ButtonIconText), new PropertyMetadata(Brushes.Black));


        public double IconFontSize
        {
            get { return (double)GetValue(IconFontSizeProperty); }
            set { SetValue(IconFontSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IconFontSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconFontSizeProperty =
            DependencyProperty.Register("IconFontSize", typeof(double), typeof(ButtonIconText), new PropertyMetadata(20d));

        public Thickness IconMargin
        {
            get { return (Thickness)GetValue(IconMarginProperty); }
            set { SetValue(IconMarginProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IconMargin.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconMarginProperty =
            DependencyProperty.Register("IconMargin", typeof(Thickness), typeof(ButtonIconText), new PropertyMetadata(new Thickness(5)));


        public Thickness ContentMargin
        {
            get { return (Thickness)GetValue(ContentMarginProperty); }
            set { SetValue(ContentMarginProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IconMargin.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentMarginProperty =
            DependencyProperty.Register("ContentMargin", typeof(Thickness), typeof(ButtonIconText), new PropertyMetadata(new Thickness(5)));

        public FontFamily IconFontFamily
        {
            get { return (FontFamily)GetValue(IconFontFamilyProperty); }
            set { SetValue(IconFontFamilyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IconFontFamily.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconFontFamilyProperty =
            DependencyProperty.Register("IconFontFamily", typeof(FontFamily), typeof(ButtonIconText), new PropertyMetadata(null));



        public CornerRadius BorderRadius
        {
            get { return (CornerRadius)GetValue(BorderRadiusProperty); }
            set { SetValue(BorderRadiusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BorderRadius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BorderRadiusProperty =
            DependencyProperty.Register("BorderRadius", typeof(CornerRadius), typeof(ButtonIconText), new PropertyMetadata(new CornerRadius(0d)));



    }

}
