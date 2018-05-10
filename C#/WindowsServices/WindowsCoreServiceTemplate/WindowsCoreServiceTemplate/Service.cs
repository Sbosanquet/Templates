using PeterKottas.DotNetCore.WindowsService.Base;
using PeterKottas.DotNetCore.WindowsService.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsCoreServiceTemplate
{
    internal class Service : MicroService, IMicroService
    {
        private static readonly AutoResetEvent _closeRequested = new AutoResetEvent(false);

        private Task _work;

        public void Start()
        {
            StartBase();
            _work = Task.Run(() => DoWorkLoop());
        }

        public void Stop()
        {
            StopBase();
            _closeRequested.Set();
            if (_work != null)
            {
                _work.Wait();
                _work = null;
            }
        }

        public void Pause()
        {

        }

        public void Continue()
        {

        }

        public void DoWorkLoop()
        {
            while (!_closeRequested.WaitOne(1000))
            {
                //TODO: Worker code goes here
                Console.WriteLine("Doing some work");
            }
        }
    }
}
