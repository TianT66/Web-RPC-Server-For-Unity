using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections;

namespace TFramework.Web
{
    public interface IHttpContext { HttpContext Context { get; set; } }

    public class WebServiceExecutor
    {
        private readonly Dictionary<string, WebServiceEntry> serviceEntries = new Dictionary<string, WebServiceEntry>();
        private readonly Dictionary<string, Type> serializableTypes = new Dictionary<string, Type>();

        public WebServiceExecutor(IServiceProvider serviceProvider)
        {
            List<Type> services = new List<Type>();
            List<Type> serviceImplementations = new List<Type>();
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies().Where(i => i.IsDynamic == false))
            {
                foreach (Type type in asm.GetTypes())
                {
                    TypeInfo typeInfo = type.GetTypeInfo();
                    if ((typeof(IConvertible).IsAssignableFrom(typeInfo) && type.GetTypeInfo().IsEnum == false) || (type.GetCustomAttributes(false).Length > 0 && typeInfo.GetCustomAttribute<WebRPCParamAttribute>() != null))
                        serializableTypes[type.FullName] = type;
                    if (typeInfo.IsInterface && typeInfo.GetCustomAttribute<WebRPCAttribute>() != null)
                        services.Add(type);
                    else if (typeInfo.IsClass && !typeInfo.IsAbstract && type.Namespace != null && !type.Namespace.StartsWith("System") && !type.Namespace.StartsWith("Microsoft"))
                        serviceImplementations.Add(type);
                }
            }
            foreach (var service in services)
            {
                foreach (var serviceImplementation in serviceImplementations.Where(i => service.GetTypeInfo().IsAssignableFrom(i)))
                {
                    object instance = ActivatorUtilities.CreateInstance(serviceProvider, serviceImplementation);
                    foreach (var methodInfo in service.GetTypeInfo().GetMethods())
                    {
                        MethodInfo implementationMethodInfo = serviceImplementation.GetTypeInfo().GetMethod(methodInfo.Name, methodInfo.GetParameters().Select(p => p.ParameterType).ToArray());
                        Create(instance, methodInfo, implementationMethodInfo);
                    }
                }
            }
        }

        public async Task<object> ExecuteAsync(WebMessage remoteInvokeMessage, HttpContext context)
        {
            WebServiceEntry entry = serviceEntries[remoteInvokeMessage.ServiceId];
            object result = default;
            try
            {
                if (entry.Instance is IHttpContext http) http.Context = context;
                result = await entry.Func(remoteInvokeMessage.Parameters);
                if (result != default && result is Task task)
                {
                    await task;
                    TypeInfo taskType = task.GetType().GetTypeInfo();
                    if (taskType.IsGenericType) result = taskType.GetProperty("Result").GetValue(task);
                }
            }
            catch (Exception exception) { Console.WriteLine(exception); }
            return result;
        }

        private void Create(object instance, MethodInfo method, MethodBase implementationMethod)
        {
            WebServiceEntry entry = new WebServiceEntry
            {
                ServiceId = GenerateServiceId(method),
                Instance = instance,
                Func = parameters => Task.FromResult(implementationMethod.Invoke(instance, DecodeMetadatas(parameters)))
            };
            serviceEntries.Add(entry.ServiceId, entry);
        }

        private string GenerateServiceId(MethodInfo method)
        {
            string id = $"{method.DeclaringType.FullName}.{method.Name}";
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Any())
                id += "_" + string.Join("_", parameters.Select(i => i.Name));
            return id;
        }

        private object[] DecodeMetadatas(List<WebMetadata> metadatas)
        {
            if (metadatas == null) return null;
            object[] res = new object[metadatas.Count];
            for (int i = 0; i < metadatas.Count; i++)
            {
                WebMetadata metadata = metadatas[i];
                if (metadata.type == null || metadata.data == null) return default;
                string typename = metadata.type;
                string data = metadata.data;
                if (typename.EndsWith("[]"))
                {
                    serializableTypes.TryGetValue(typename[..^2], out Type type);
                    Array sourcesArray = (Array)JsonConvert.DeserializeObject(data, type.MakeArrayType());
                    if (sourcesArray != null && sourcesArray.Length > 0)
                    {
                        Array desiredArray = (Array)Activator.CreateInstance(sourcesArray.GetValue(0).GetType().MakeArrayType(), sourcesArray.Length);
                        for (int j = 0; j < sourcesArray.Length; j++)
                            desiredArray.SetValue(sourcesArray.GetValue(j), j);
                        res[i] = desiredArray;
                    }
                    else
                        res[i] = sourcesArray;
                }
                else if (typename.StartsWith("IList"))
                {
                    if (typename.Length > 7 && typename[5] == '<' && typename[^1] == '>')
                    {
                        serializableTypes.TryGetValue(typename[6..^1], out Type type);
                        IList temp = (IList)JsonConvert.DeserializeObject(data, typeof(List<>).MakeGenericType(type));
                        if (temp != null && temp.Count > 0)
                        {
                            IList desiredArray = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(temp[0].GetType()));
                            foreach (var v in temp)
                                desiredArray.Add(v);
                            res[i] = desiredArray;
                        }
                        else
                            res[i] = temp;
                    }
                }
                else
                {
                    serializableTypes.TryGetValue(typename, out Type type);
                    res[i] = JsonConvert.DeserializeObject(data, type);
                }
            }
            return res;
        }
    }
}
