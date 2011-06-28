using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChelasInjection
{
    class ImplemTypeBinder<T> : ITypeBinder<T>, IActivationBinder<T>, IConstructorBinder<T>
    {
        public TypeConfig Tp;

        public ImplemTypeBinder(TypeConfig tp)
        {
            Tp = tp;
        }

        public ITypeBinder<T> WithNoArgumentsConstructor()
        {
            Tp.ConstSemArgs = true;
            return this;
        }

        //public ITypeBinder<T> WithSingletonActivation()

        //{
        //    Tp.ComSingleTon = true;
        //    return null;
        //}

        public IActivationBinder<T> WithActivation { 
            
            get { return this; }
        }




        public ITypeBinder<T> PerRequest()
        {
            Tp.PerRequest = true;
            return this;
        }

        public ITypeBinder<T> Singleton()
        {
            Tp.ComSingleTon = true;
            return this;
        }


        public IConstructorBinder<T> WithConstructor(params Type[] constructorArguments)

        {

            Tp.ConstSemArgs = false;
            Tp.Params = constructorArguments;
            return this;
        }


        public ITypeBinder<T> WithValues(Func<object> values)
        {
            Tp.Values = values;
            return this;
        }

        
        public ITypeBinder<T> InitializeObjectWith(Action<T> initialization)
        {
            Tp.Initialization = o => initialization((T) o);
            return this;

        }

    }



}