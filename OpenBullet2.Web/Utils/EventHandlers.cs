namespace OpenBullet2.Web.Utils;

// https://www.devleader.ca/2023/02/14/async-eventhandlers-a-simple-safety-net-to-the-rescue/
static internal class EventHandlers
{
    public static EventHandler<TArgs> TryAsync<TArgs>(
        Func<object?, TArgs, Task> callback,
        Action<Exception> errorHandler)
        => TryAsync(
            callback,
            ex =>
            {
                errorHandler.Invoke(ex);
                return Task.CompletedTask;
            });

    public static EventHandler<TArgs> TryAsync<TArgs>(
        Func<object?, TArgs, Task> callback,
        Func<Exception, Task> errorHandler) =>
        async (s, e) =>
        {
            try
            {
                await callback.Invoke(s, e);
            }
            catch (Exception ex)
            {
                await errorHandler.Invoke(ex);
            }
        };

    public static EventHandler TryAsync(
        Func<object?, EventArgs, Task> callback,
        Func<Exception, Task> errorHandler) =>
        async (s, e) =>
        {
            try
            {
                await callback.Invoke(s, e);
            }
            catch (Exception ex)
            {
                await errorHandler.Invoke(ex);
            }
        };
}
