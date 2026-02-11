using System;
using System.Reflection;
using UnityEngine;

public class CheckNetworkConnector : MonoBehaviour
{
    void Start()
    {
        // Check if NetworkConnector type exists
        Type networkConnectorType = Type.GetType("NetworkConnector, Assembly-CSharp");
        if (networkConnectorType != null)
        {
            Debug.Log("NetworkConnector type found!");
            
            // Get all public methods
            MethodInfo[] methods = networkConnectorType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            Debug.Log($"Found {methods.Length} methods in NetworkConnector:");
            
            foreach (MethodInfo method in methods)
            {
                string access = method.IsPublic ? "public" : (method.IsPrivate ? "private" : "protected");
                string staticModifier = method.IsStatic ? "static" : "";
                string methodInfo = $"{access} {staticModifier} {method.ReturnType.Name} {method.Name}(";
                
                ParameterInfo[] parameters = method.GetParameters();
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (i > 0)
                        methodInfo += ", ";
                    methodInfo += $"{parameters[i].ParameterType.Name} {parameters[i].Name}";
                }
                
                methodInfo += ")";
                Debug.Log(methodInfo);
            }
        }
        else
        {
            Debug.LogError("NetworkConnector type not found!");
        }
        
        // Also check for any classes containing "Network" or "Connector" in their name
        Debug.Log("\nChecking for all types containing 'Network' or 'Connector':");
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    if (type.Name.Contains("Network") || type.Name.Contains("Connector"))
                    {
                        Debug.Log($"{type.Assembly.GetName().Name}: {type.FullName}");
                    }
                }
            }
            catch
            {
                // Skip assemblies that cause errors
            }
        }
    }
}
