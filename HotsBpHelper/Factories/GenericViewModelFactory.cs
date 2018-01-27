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
        private readonly IContainer _container;
        private readonly Type[] _vmList;

        public ViewModelFactory(IContainer container)
        {
            _container = container;
            _vmList = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                          from assemblyType in domainAssembly.GetTypes()
                          where typeof(ViewModelBase).IsAssignableFrom(assemblyType)
                          select assemblyType).ToArray();
        }

        public T CreateViewModel<T>() where T : ViewModelBase
        { 
            
            foreach (var vmType in _vmList.Where(f => !f.Name.StartsWith("Generated")))
            {
                if (vmType == typeof (T))
                {
                    var vm = _container.Get(vmType);
                    return (T)vm;
                }
            }
            
            throw new NotSupportedException($"The type {typeof (T).Name} is not properly registered.");
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