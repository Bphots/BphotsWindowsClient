using System;
using System.Collections.Generic;
using System.Linq;
using HotsBpHelper.Pages;
using StyletIoC;

// ReSharper disable SuggestBaseTypeForParameter

namespace HotsBpHelper.Factories
{
    public class ViewModelFactory
    {
        private readonly List<IViewModelFactory> _facotries; // ReSharper disable SuggestBaseTypeForParameter
        
        public ViewModelFactory(IContainer container)
        {
            var vmFactoriesList = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                                   from assemblyType in domainAssembly.GetTypes()
                                   where typeof(IViewModelFactory).IsAssignableFrom(assemblyType)
                                   select assemblyType).ToArray();

            _facotries = new List<IViewModelFactory>();
            foreach (var factoryType in vmFactoriesList.Where(f => !f.Name.StartsWith("Generated")))
            {
                var factory = container.Get(factoryType) as IViewModelFactory;
                _facotries.Add(factory);
            }
        }

        public T CreateViewModel<T>()
        {
            foreach (var factory in _facotries)
            {
                var methodInfo = factory.GetType().GetMethod("CreateViewModel");
                if (methodInfo != null && methodInfo.ReturnType == typeof (T))
                {
                    var vm = methodInfo.Invoke(factory, null);
                    return (T) vm;
                }
            }

            throw new NotSupportedException($"The factory for type {typeof (T).Name} is not properly registered.");
        }
    }

    #region Prefered solution, cannot be used due to stylet bug
    /// <summary>
    ///     Prefered generic factory, cannot be used due to stylet bug
    /// </summary>
    public interface IViewModelBaseFactory<T> where T : ViewModelBase
    {
        T CreateViewModel();
    }

    /// <summary>
    ///     Prefered generic factory, cannot be used due to stylet bug
    /// </summary>
    public class GenericViewModelFactory
    {
        private readonly IContainer _ioc;

        public GenericViewModelFactory(IContainer container)
        {
            _ioc = container;
        }

        public T GetViewModel<T>() where T : ViewModelBase
        {
            var factory = _ioc.Get(typeof (IViewModelBaseFactory<T>)) as IViewModelBaseFactory<T>;
            if (factory == null)
                throw new Exception($"The factory for type {typeof (T).Name} is not registered.");

            return factory.CreateViewModel();
        }
    }
    #endregion 
}