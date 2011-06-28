using System;
using System.Collections.Generic;

namespace ChelasInjection
{
    public delegate object ResolverHandler(Binder sender, Type t);

    public abstract class Binder
    {

        internal Dictionary<Type, TypeConfig> Binds = new Dictionary<Type, TypeConfig>();


        public void Configure()
        {
            //throw new NotImplementedException();
            InternalConfigure();
        }

        protected abstract void InternalConfigure();


        public event ResolverHandler CustomResolver;


        public ITypeBinder<Target> Bind<Source, Target>()
        {
            return new ImplemTypeBinder<Target>(Bind(typeof(Source), typeof(Target)));
        }



        public ITypeBinder<Source> Bind<Source>()
        {
            //throw new NotImplementedException();
            return Bind<Source, Source>();

        }

        internal TypeConfig Bind(Type TypeSource, Type TypeTarget)
        {


            return Binds[TypeSource] = new TypeConfig { Tipo = TypeTarget }; // substitui o add;
        }

        internal object Resolver(Binder _mybinder, Type tipo)
        {

            if (CustomResolver == null)
            {
                return null;
            }

            foreach (ResolverHandler rh in CustomResolver.GetInvocationList())
            {
                object ob = rh(_mybinder, tipo);
                if (ob != null)
                    return ob;
                    
            }
            return null;

        }
    }
}