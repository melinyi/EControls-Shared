using System;

namespace EControl.Controls.TabFrame.Interface
{
    public interface ITabPage
    {
        /// <summary>
        /// 页面名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 跳转页面后的后续操作，参数 object， dynamic亦可
        /// </summary>
        Action<object> NavigateContinueWithInvoke { get; }

    }
}
