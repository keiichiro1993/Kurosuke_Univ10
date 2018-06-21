using System;
using System.Diagnostics;

namespace Kurosuke_Universal.Utils
{

    public class DelegateCommand<T> : System.Windows.Input.ICommand
    {
        private readonly Action<T> m_Execute;
        private readonly Func<T, bool> m_CanExecute;
        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action<T> execute)
            : this(execute, (x) => true)
        { /* empty */ }

        public DelegateCommand(Action<T> execute, Func<T, bool> canexecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            m_Execute = execute;
            m_CanExecute = canexecute;
        }

        [DebuggerStepThrough]
        public bool CanExecute(object p)
        {
            try
            {
                var _Value = (T)Convert.ChangeType(p, typeof(T));
                return m_CanExecute == null ? true : m_CanExecute(_Value);
            }
            catch
            {
                Debugger.Break();
                return false;
            }
        }

        public void Execute(object p)
        {
            if (CanExecute(p))
                try
                {
                    var _Value = (T)Convert.ChangeType(p, typeof(T));
                    m_Execute(_Value);
                }
                catch { Debugger.Break(); }
        }

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }
    }
}
