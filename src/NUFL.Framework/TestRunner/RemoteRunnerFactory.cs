using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUFL.Framework.Setting;
using NUFL.Framework.Persistance;
using NUFL.Framework.Analysis;
using NUFL.Service;
namespace NUFL.Framework.TestRunner
{
    public class RemoteRunnerFactory:GlobalInstanceServiceBase,ITestRunnerFactory
    {
        public string Key { set; get; }
        public RemoteRunnerFactory()
        {
        }
        public ITestExectuor CreateExecutor()
        {
            IOption option = null;
            try
            {
                option = (IOption)ServiceManager.Instance.GetService(typeof(IOption), Key);
            }catch(Exception)
            {
                throw new Exception("Cannot find option service with the key " + Key);
            }
            IFLResultPresenter presenter = null;
            try
            {
                presenter = (IFLResultPresenter)ServiceManager.Instance.GetService(typeof(IFLResultPresenter), Key);
            }
            catch (Exception) 
            {
                //no presenter
                System.Diagnostics.Debug.WriteLine("Cannot find presenter service with the key " + Key);
                return new SimpleRunnerFactory().CreateExecutor();
            }
            var fault_locator = new FaultLocator()
            {
                Option = option,
                Presenter = presenter
            };

            var runner = new ProfileTestRunner()
            {
                Option = option,
                ProfilePersistance = fault_locator
            };

            return runner;
          
        }
        public ITestDiscoverer CreateDiscoverer()
        {
            throw new NotImplementedException();
        }
    }
}
