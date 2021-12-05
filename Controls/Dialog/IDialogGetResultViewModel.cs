using System.Windows.Input;

namespace EControl.Controls
{
    public interface IDialogGetResultViewModel<TResult>
    {
        /// <summary>
        /// 无需进行定义，创建Dialog时将会自动赋值操作对象，清除异常抛出即可。
        /// </summary>
        public Dialog Dialog { get; set; }

        /// <summary>
        /// 返回结果
        /// </summary>
        public DialogResult<TResult> Result { get; set; }

        /// <summary>
        /// 需对 <see cref="Result"/> 赋值后，调用 <see cref="CloseCommand"/> 关闭Dialog
        /// </summary>
        public ICommand SaveCommand { get; }

    }

    public class DialogResult<T>
    {
        /// <summary>
        /// 是否保存了结果
        /// </summary>
        public bool IsSave { get; set; } = false;

        /// <summary>
        /// 结果内容
        /// </summary>
        public T ResultContent { get; set; }
    }
}
