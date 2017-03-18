using System;
using System.Linq;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Plugins;
using Bjd.Utils;
using System.Collections.Generic;
using Bjd.Threading;

namespace Bjd.Components
{
    public class ListComponent : ListBase<ComponentBase>, IDisposable
    {

        private Kernel kernel;

        public ListComponent(Kernel kernel, ListPlugin listPlugin)
        {
            kernel.Logger.TraceInformation($"ListComponent..ctor {listPlugin.GetType().Name} Start");
            try
            {
                this.kernel = kernel;
                Initialize(listPlugin);
            }
            catch (Exception ex)
            {
                kernel.Logger.TraceError(ex.Message);
                kernel.Logger.TraceError(ex.StackTrace);
            }
            finally
            {
                kernel.Logger.TraceInformation($"ListComponent..ctor {listPlugin.GetType().Name} End");
            }
        }

        //名前によるComponentの検索
        //一覧に存在しない名前で検索を行った場合、設計上の問題として処理される
        public ComponentBase Get(string nameTag)
        {
            foreach (ComponentBase cb in Ar)
            {
                if (cb.NameTag == nameTag)
                {
                    return cb;
                }
            }
            Util.RuntimeException(string.Format("nameTag={0}", nameTag));
            return null;
        }

        //型によるComponentの検索
        //一覧に存在しない名前で検索を行った場合、設計上の問題として処理される
        public T Get<T>() where T : ComponentBase
        {
            foreach (ComponentBase cb in Ar)
            {
                var ccb = cb as T;
                if (ccb != null)
                {
                    return ccb;
                }
            }
            Util.RuntimeException(string.Format("type={0}", typeof(T).FullName));
            return null;
        }

        // 初期化
        private void Initialize(ListPlugin listPlugin)
        {
            Ar.Clear();

            foreach (IPlugin p in listPlugin)
            {
                try
                {
                    ConfigurationBase op = kernel.ListOption.Get(p.Name);

                    //Component生成
                    AddComponent(new Conf(op), p);
                }
                catch (Exception ex)
                {
                    this.kernel.Logger.Exception(ex);
                }
            }
        }

        //create ComponentBase
        private void AddComponent(Conf conf, IPlugin onePlugin)
        {
            var o = onePlugin.CreateComponent(kernel, conf);
            if (o != null)
            {
                Ar.Add(o);
            }
        }

    }
}
