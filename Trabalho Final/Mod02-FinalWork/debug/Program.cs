using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChelasInjection;
using ChelasInjection.Tests;
using ChelasInjection.SampleTypes;

namespace debug
{
    class Program
    {
        static void Main(string[] args)
        {
            Injector _injector;
            _injector = new Injector(new MyBinder());
            
        //  Console.WriteLine(_injector.GetInstance<ISomeInterface1>());
            Console.WriteLine(_injector.GetInstance<ISomeInterface4>());
        
            //Console.WriteLine(_injector.GetInstance<SomeClass1>());
            //Console.WriteLine(_injector.GetInstance<string>());
        //    SomeClass7 sc7 = 
        //    _injector.GetInstance<SomeClass7>();

        //    Console.WriteLine(sc7);
        //    Console.WriteLine(sc7.Sc6);
        }
    }
}
