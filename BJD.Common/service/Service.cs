
namespace Bjd.service
{
    public class Service
    {
        Kernel _kernel;
        static Service instance = new Service();


        public static void ServiceMain()
        {
            Service.instance.OnStart();
        }
        protected void OnStart()
        {
            _kernel = new Kernel();
            _kernel.Start();
            //_kernel.MenuOnClick("StartStop_Start");
        }
        protected void OnPause()
        {
            //_kernel.MenuOnClick("StartStop_Stop");
            _kernel.Stop();
        }
        protected void OnContinue()
        {
            //_kernel.MenuOnClick("StartStop_Start");
            _kernel.Start();
        }

        protected void OnStop()
        {
            //_kernel.MenuOnClick("StartStop_Stop");
            _kernel.Stop();
            _kernel.Dispose();
            _kernel = null;
        }

    }

}
