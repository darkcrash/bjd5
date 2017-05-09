using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Bjd.Threading
{
    public class LazyCancelTimer
    {
        public static readonly LazyCancelTimer Instance = new LazyCancelTimer();

        private const int MaxQueue = 240;
        private const int MaxCancelQueue = 1024;
        private const int TimerPeriod = 250;
        private TimerContext[] queue = new TimerContext[MaxQueue];
        private int queuePoint = 0;
        private System.Threading.TimerCallback _timerCallback;
        private System.Threading.Timer _timer;

        private LazyCancelTimer()
        {
            _timerCallback = TimerCallback;

            for (var i = 0; i < MaxQueue; i++)
            {
                var newValue = new TimerContext();
                Interlocked.Exchange(ref queue[i], newValue);
            }

            _timer = new System.Threading.Timer(_timerCallback, null, 0, TimerPeriod);
        }

        private void TimerCallback(object state)
        {
            var idx = Interlocked.Increment(ref queuePoint);
            if (idx >= MaxQueue)
            {
                var newIdx = idx % MaxQueue;
                Interlocked.CompareExchange(ref queuePoint, newIdx, idx);
                idx = newIdx;
            }
            //var cArray = queue[idx].Cancel;
            //for (var i = 0; i < MaxCancelQueue; i++)
            //{
            //    var c = Interlocked.Exchange(ref cArray[i], null);
            //    if (c == null) continue;
            //    try { if (!c.IsCancellationRequested) c.Cancel(); } catch { }
            //    //try { c.Dispose(); } catch { }
            //}
            var newValue = new TimerContext();
            var q = Interlocked.Exchange(ref queue[idx], newValue);
            q.Cancel.Cancel();

        }

        //public void Add(CancellationTokenSource cancel, int millisecondsTimeout)
        //{
        //    var offset = millisecondsTimeout / TimerPeriod + 1;
        //    var idx = queuePoint + offset;
        //    if (idx >= MaxQueue) { idx = idx % MaxQueue; }
        //    var cArray = queue[idx].Cancel;
        //    for (var i = 0; i < MaxCancelQueue; i++)
        //    {
        //        var result = Interlocked.CompareExchange(ref cArray[i], cancel, null);
        //        if (result == null) return;
        //    }
        //    throw new OverflowException("LazyCancelTimer queue overflow.");
        //}

        public CancellationToken Get(int millisecondsTimeout)
        {
            var offset = millisecondsTimeout / TimerPeriod + 1;
            var idx = queuePoint + offset;
            if (idx >= MaxQueue) { idx = idx % MaxQueue; }
            return queue[idx].Cancel.Token;
        }


        private class TimerContext
        {
            public CancellationTokenSource Cancel = new CancellationTokenSource();
            //public CancellationTokenSource[] Cancel = new CancellationTokenSource[MaxCancelQueue];
        }
    }
}
