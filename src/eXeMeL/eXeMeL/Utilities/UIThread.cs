using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace eXeMeL.Utilities
{
  public static class UIThread
  {
    private static Dispatcher _dispatcher;
    private static bool _invokeSynchronously;
    public const string THREAD_NAME = "UIThread";

    public static void Initialize()
    {
      Initialize(Dispatcher.CurrentDispatcher, false);
    }

    public static void Initialize(bool invokeSynchronously)
    {
      Initialize(Dispatcher.CurrentDispatcher, invokeSynchronously);
    }

    public static void Initialize(Dispatcher uiThreadDispatcher)
    {
      Initialize(uiThreadDispatcher, false);
    }

    public static void Initialize(Dispatcher uiThreadDispatcher, bool invokeSynchronously)
    {
      _invokeSynchronously = invokeSynchronously;
      _dispatcher = uiThreadDispatcher;

      try
      {
        if (_dispatcher.Thread.Name != THREAD_NAME)
          _dispatcher.Thread.Name = THREAD_NAME;
      }
      catch (InvalidOperationException)
      {
        // Thread has already been renamed, shouldn't error out. Usually happens running unit tests
      }
    }

    public static Dispatcher Dispatcher { get { return UIThread._dispatcher; } }

    public static void Run(Action action, DispatcherPriority priority)
    {
      if (!_invokeSynchronously)
        _dispatcher.BeginInvoke(action, priority);
      else
      {
        _dispatcher.Invoke(action, priority, null);
      }
    }

    public static void Run(Action action)
    {
      if (!_invokeSynchronously)
        _dispatcher.BeginInvoke(action);
      else
      {
        _dispatcher.Invoke(action, null);
      }
    }


    // Queue up an action to run once all current items on the UI thread is complete
    public static void Queue(Action action)
    {
      Task.Factory.StartNew(() => Run(action));
    }
  }
}
