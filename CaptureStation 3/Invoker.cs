using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace System
{
    public class Invoker
    {
        private delegate void ControlDelegate();
        private static ControlDelegate Do;
        public static void InvokeThreaded(Action act)
        {
            new Thread(() => act()).Start();    
        }
        public static void InvokeThreaded(Action act, ThreadPriority prior = ThreadPriority.Normal)
        {
            InvokeThreaded(act, prior, "", false);
        }
        public static void InvokeThreaded(Action act , ThreadPriority prior = ThreadPriority.Normal, string name = "", bool bg = false )
        {
            new Thread(() => act()) { Priority = prior, Name = name, IsBackground = bg }.Start();
        }
        public static void Invoke(Control ctrl, Action e)
        {
            try
            {
                if (!ctrl.IsHandleCreated) return;
                Do = new ControlDelegate(e);
                ctrl.Invoke(Do);
            }
            catch { }
        }
        public static bool DoTry(Action doTry, Func<Exception, object> doIfCatch = null, Action doIfFinally = null)
        {
            if (doTry == null) throw new ArgumentNullException();
            else
            {
                try
                {
                    doTry();
                    return true;
                }
                catch (Exception error)
                {
                    if (doIfCatch != null)
                    {
                        doIfCatch(error);
                        return true;
                    }
                    else
                    {
                        if (Debugger.IsAttached) throw;
                    }
                    return true;
                }
            }
        }

    }
}
