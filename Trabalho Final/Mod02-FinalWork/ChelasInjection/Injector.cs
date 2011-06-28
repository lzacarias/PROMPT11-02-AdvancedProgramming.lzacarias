using System;
using System.Reflection;
using ChelasInjection.Exceptions;
using System.Collections.Generic;
using System.Linq;

namespace ChelasInjection
{
    public class Injector
    {
        

        private Binder _myBinder;                       // contém os elementos do meu binder                              
        Dictionary<Type, Object> _objectos_criados;     // dicionário com as instâncias de objectos já criadas        
        HashSet<Type> _chamados;                        // todos os tipos já criados
        Dictionary<Type, Object> _obj_criados_single;   // dicionário com as instâncias singleton de objectos já criadas

        public Injector(Binder myBinder)
        {
            _myBinder = myBinder;
            _myBinder.Configure();
            _obj_criados_single = new Dictionary<Type, object>();
        }
        
        private Object GetInstance(Type type)
        {
         
            // valida se a instância de tipo já foi criada, caso afirmativo devolve    
            Object retObject;                          
            retObject = TryGetInstanceFromCache(type);
            if (retObject != null)
            {
                return retObject;
            }

            
            // caso um tipo já tenha sido chamado devolve uma excepção de dependência circular; caso contrário adiciona aos objectos chamados
            if (_chamados.Contains(type))
            {
                throw new CircularDependencyException(string.Format("type {0} chamado recursivamente", type.Name));
            }
            _chamados.Add(type);

            // caso o tipo existq no dicionário de binds devolve a classe type config na variável typeConfig
            // se não existir adiciona no dicionário de binds caso o tipo não abstracto (situação em que lança excepção)
            TypeConfig typeConfig;
            if (!_myBinder.Binds.TryGetValue(type, out typeConfig))
            {
                if (type.IsAbstract)
                {
                    throw new UnboundTypeException(string.Format("type {0} nao encontrado no dicionario de binds", type.Name));

                }
                else
                {
                    typeConfig = _myBinder.Bind(type, type);

                }
            }

            //verifica se existe um evento registado para este tipo;
            //se for devolvido objecto escreve a "cache" e retorna
            retObject = _myBinder.Resolver(_myBinder, type);
            if (retObject != null)
            {

                WriteCache(typeConfig, type, retObject);
                
                return retObject;

            }

            //obtém para variável ctors todos os construtores públicos do tipo
            //pressupõe sempre existência de pelo menos um construtor público
            ConstructorInfo[] ctors = typeConfig.Tipo.GetConstructors();

            //Se o tipo tiver definido na sua classe typeConfig que deve ser invocado sem argumentos chama o construtor cujo arreio de parametros tenha o tamanho 0 
            if (typeConfig.ConstSemArgs)
            {
                ConstructorInfo ConstNoArgs = ctors.Single(ci => ci.GetParameters().Length == 0);
                return GetRetObject(ConstNoArgs, typeConfig, type);
            }

            // se mais de um contrutor default definido para o tipo deve lançar excepção
            if (ctors.Where(cd => cd.IsDefined(typeof(DefaultConstructorAttribute), false)).Count() > 1)
            {
                throw new MultipleDefaultConstructorAttributesException(string.Format("Rebenta a bolha, existe mais de um construtor default", type.Name));
            }

            // se existir um construtor default utiliza o mesmo para instanciar 
            ConstructorInfo consdefault =
            ctors.SingleOrDefault(cd => cd.IsDefined(typeof(DefaultConstructorAttribute), false));
            if (consdefault != null)
            {
                return GetRetObject(consdefault, typeConfig, type);
            }

            // se estão definidos parametros deve verificar-se qual o construtor a ser utilizado; guarda na variável ciForcado 
            if (typeConfig.Params != null)
            {

                var ciForcado = typeConfig.Tipo.GetConstructor(typeConfig.Params);
                return GetRetObject(ciForcado, typeConfig, type);
            }

            //obter uma lista de construtores e ordenar a mesma começando pelo construtor que tem mais parametros
            //o primeiro "resolvido" é utilizado para criar a instância
            Array.Sort(ctors, (ci1, ci2) => ci2.GetParameters().Length - ci1.GetParameters().Length);
            for (int indConstruc = 0; indConstruc < ctors.Length; indConstruc++)
            {
                retObject = GetRetObject(ctors[indConstruc], typeConfig, type);

                if (retObject != null)
                    return retObject;
                //for (int i = 0; i < pi.Length; i++)
                //{
                //    //parametros[i] = GetInstance(pi[i].ParameterType, chamados);
                //    if (!_myBinder.binds.ContainsKey(pi[i].ParameterType)) ;
                //    {
                //        constOk = false;
                //        break;
                //    }
                //}


            }

            // se não encontrado nenhum instrutor válido lança excepção
            throw new MissingAppropriateConstructorException("nao foi encontrado um construtor válido para iniciar instância");
        }
        /// <summary>
        /// verifica se a instância a criar ja existe nos objectos criados
        /// </summary>
        /// <param name="type">recebe um tipo</param>
        /// <returns>devolve um objecto já criado para o tipo ou null caso not found/// </returns>
        private object TryGetInstanceFromCache(Type type)
        {
            Object resultObj;
            if (_objectos_criados.TryGetValue(type, out resultObj))
            {
                return resultObj;
            }

            if (_obj_criados_single.TryGetValue(type, out resultObj))
            {
                return resultObj;
            }
            return null;
        }

        /// <summary>
        /// este método é responsável por criar novas instâncias 
        /// </summary>
        /// <param name="ci"> construtor elegido para criar a instância</param>
        /// <param name="tp"> classe typeconfig</param>
        /// <param name="tipo"> tipo </param>
        /// <returns>retorna a instância do objecto criada</returns>
        private object GetRetObject(ConstructorInfo ci, TypeConfig tp, Type tipo)
        {
            
            object retObject = null;

            //quando existem parametros especificados já foi definido que existe um construtor válido
            //são criados dois arreios; pi1 tem as propriedades dos valores passados na classe typeconfig, argumentos1 tem os parametros do construtor
            //por cada parametro de argumentos1 não resolvido pelo bind utiliza-se um valor da classe typeconfig
            //
            if (tp.Params != null)
            {
            
                Object valores = tp.Values();
                PropertyInfo[] pi1 = valores.GetType().GetProperties();
                int ind_pi = 0;
                object[] argumentos1 = new object[tp.Params.Length];

                for (int i = 0; i < tp.Params.Length; i++)
                {
                    if (_myBinder.Binds.ContainsKey(tp.Params[i]))
                    {
                        argumentos1[i] = GetInstance(tp.Params[i]);
                    }
                    else
                    {
                        argumentos1[i] = pi1[ind_pi].GetValue(valores, null);
                        ind_pi++;
                    }
                }

                //cria a instância
                retObject = ci.Invoke(argumentos1);
                
                //escreve para memória o objecto criado
                WriteCache(tp, tipo, retObject);

            }

            //guarda todos os parametros do construtor
            ParameterInfo[] pi = ci.GetParameters();

            //verifica se para todos eles tem referência no dicionários de binds
            if (pi.All(param => _myBinder.Binds.ContainsKey(param.ParameterType)))
            {

                //escreve todos os argumentos num array
                object[] argumentos = pi.Select(param1 => GetInstance(param1.ParameterType)).ToArray();

                //cria instância e escreve cache
                retObject = ci.Invoke(argumentos);
                WriteCache(tp, tipo, retObject);
              
                // 
                if (tp.Initialization != null)
                {
                    tp.Initialization(retObject);

                }
            }
            return retObject;
        }
        /// <summary>
        /// escreve todas as instâncias para memória futura
        /// </summary>
        /// <param name="tp">type config</param>
        /// <param name="tipo">tipo </param>
        /// <param name="retObject">objecto criado</param>
        private void WriteCache(TypeConfig tp, Type tipo, object retObject)
        {
            //se singleton escreve no dicionário de singleton  
            if (tp.ComSingleTon)
            {
                _obj_criados_single.Add(tipo, retObject);
            }// caso contrário escreve nos objectos criados
            else
                _objectos_criados.Add(tipo, retObject);
        }

        public T GetInstance<T>()
        {
            _objectos_criados = new Dictionary<Type, Object>();
            _chamados = new HashSet<Type>();
            return (T)GetInstance(typeof(T));

        }
    }
}