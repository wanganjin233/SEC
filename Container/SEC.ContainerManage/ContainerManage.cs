using Autofac; 

namespace SEC.Container
{
    public static class ContainerManage
    {
        private static IContainer? _container = null;
        private static readonly object Locker = new object();
        /// <summary>
        /// 单实列获取
        /// </summary>
        public static IContainer Container
        {
            get
            {
                if (_container == null)
                {
                    lock (Locker)
                    {
                        if (_container == null)
                        {
                            _container = Initialise();
                        }
                    }
                }
                return _container;
            }
        }
        private static IContainer Initialise()
        {

            var builder = new ContainerBuilder();
            //builder.RegisterType<MC3EEthernet>().As<IDriver>();
            var container = builder.Build();
         
            return container;
        }

    }
}