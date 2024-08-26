using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

public static class ThreadHelperClass
{
    public static void GenericInvoke(Control control, Action action)
    {
        if (control.InvokeRequired)
        {
            control.Invoke(action);
        }
        else
        {
            action();
        }
    }

    public static Task GenericInvokeAsync(Control control, Action action)
    {
        var tcs = new TaskCompletionSource<object>();
        GenericInvoke(control, () =>
        {
            try
            {
                action();
                tcs.SetResult(null);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        return tcs.Task;
    }
}
