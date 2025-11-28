using System.Collections.Generic;
using UnityEngine;


public class DependencyInjector : MonoBehaviour
{
    static Dictionary<System.Type, System.Object> m_Dependencies = new Dictionary<System.Type, System.Object>();
    public static T GetDependency<T>()
    {
        if (!m_Dependencies.ContainsKey(typeof(T)))
        {
            Debug.LogError("Cannot find: " + typeof(T).ToString()+".");
            return default(T);
        }
        return (T)m_Dependencies[typeof(T)];
    }
    public static void AddDependency<T>(System.Object obj)
    {
        if (m_Dependencies.ContainsKey(typeof(T)))
        {
            Debug.Log("There's already an object of type: " + typeof(T).ToString());
            Debug.Log("Object 1: " + m_Dependencies[typeof(T)].GetType().ToString());
            Debug.Log("Object 2: " + obj.GetType().ToString());
            m_Dependencies.Remove(typeof(T));
        }
        m_Dependencies.Add(typeof(T), obj);
    }
}
