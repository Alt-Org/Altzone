using System;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Model.Dto;
using Altzone.Scripts.Model.Loader;
using UnityEngine;

namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Container for application model objects.
    /// </summary>
    public static class Models
    {
        private static readonly Dictionary<string, AbstractModel> ModelsMap = new();

        public static void Load()
        {
            ModelsMap.Clear();
            ModelLoader.LoadModels();
        }

        public static void Add(AbstractModel model, string modelName)
        {
            var modelType = model.GetType();
            var key = $"{modelType.Name}.{modelName}";
            if (ModelsMap.ContainsKey(key))
            {
                throw new UnityException($"model key already exists: {key}");
            }
            ModelsMap.Add(key, model);
        }

        public static T GetByName<T>(object modelName) where T : AbstractModel
        {
            var modelType = typeof(T);
            var key = $"{modelType.Name}.{modelName}";
            if (!ModelsMap.TryGetValue(key, out var anyModel))
            {
                throw new UnityException($"model not found for key: {key}");
            }
            if (anyModel is not T exactModel)
            {
                throw new UnityException(
                    $"model type {anyModel.GetType().Name} is different than excepted type {modelType.Name}for key: {key}");
            }
            return exactModel;
        }

        public static T FindById<T>(int id) where T : AbstractModel
        {
            return ModelsMap.Values.OfType<T>().FirstOrDefault(x => x.Id == id);
        }

        public static T Find<T>(Predicate<T> selector) where T : AbstractModel
        {
            return ModelsMap.Values.OfType<T>().FirstOrDefault(x => selector(x));
        }

        public static List<T> GetAll<T>() where T : AbstractModel
        {
            return ModelsMap.Values.OfType<T>().ToList();
        }
    }
}