using System;
using System.Collections.Generic;

namespace ChelasInjection
{
    public class TypeConfig
    {
        public Type Tipo;                               // tipo de target
        public bool ConstSemArgs;                       // indica se deve ser utilizado o construtor sem parametros
        public bool ComSingleTon;                       // indica se a instância apenas deve ser criada uma vez na memória
        public bool PerRequest;                         // não utilizado
        public Type[] Params;                           // permite definir que construtor utilizar
        public Func<Object> Values;                     // valores a serem utilizados na criação de uma instância
        public Action<Object> Initialization;           // encapsula um método a efectuar num object

    }


}