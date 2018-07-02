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
}