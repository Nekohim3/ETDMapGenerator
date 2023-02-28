using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGeneratorTest.Utils;

public static class RepeatableCode
{
    public static Exception? Repeat<T>(Func<T> func, int count)
    {
        return null;
    }

    public static (T? result, Exception? exception) RepeatResultWithException<T>(Func<T?> func, int count) where T : class
    {
        Exception? exception = null;
        for (var i = 0; i < count; i++)
        {
            try
            {
                var res = func.Invoke();
                if (res != null)
                    return (res, null);
            }
            catch (Exception e)
            {
                exception = e;
            }
        }

        return (null, exception);
    }
    public static T? RepeatResult<T>(Func<T?> func, int count) where T : class
    {
        for (var i = 0; i < count; i++)
        {
                var res = func.Invoke();
                if (res != null)
                    return res;
        }

        return null;
    }
}
