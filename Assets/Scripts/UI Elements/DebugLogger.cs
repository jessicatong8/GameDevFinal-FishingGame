using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Global debug logger that tracks method calls and events
/// Usage: DebugLogger.Instance.LogMethodCall("MethodName");
/// </summary>
public class DebugLogger : MonoBehaviour
{
    public static DebugLogger Instance
    {
        get
        {
            if (isQuitting) 
            {
                // Optionally log to console so you still see the message 
                // without creating a new GameObject
                return null; 
            }

            if (instance == null)
            {
                if (!Application.isPlaying) { return null; } // Don't create instance in edit mode

                GameObject go = new GameObject("DebugLogger");
                instance = go.AddComponent<DebugLogger>();

                // Optional: Prevents the logger from being destroyed between scenes
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }
    private static DebugLogger instance;
    private static bool isQuitting = false; // Track the application state
    private Queue<LogEntry> methodLog = new Queue<LogEntry>();
    private const int MAX_LOG_ENTRIES = 3;
    private struct LogEntry
    {
        public string Message;
        public float TimeSeconds;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        isQuitting = false; // Reset if we load a new scene
    }
    
    private void OnApplicationQuit()
    {
        isQuitting = true;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public void LogMethodCall(string methodName, string additionalInfo = "")
    {
        string logEntry = methodName;

        if (!string.IsNullOrEmpty(additionalInfo))
            logEntry += $" | {additionalInfo}";

        AddEntry(logEntry);
        Debug.Log(logEntry);
    }

    public void Log(string message)
    {
        AddEntry(message);
        Debug.Log(message);
    }

    public void LogWarning(string message)
    {
        AddEntry($"WARNING: {message}");
        Debug.LogWarning(message);
    }

    public void LogError(string message)
    {
        AddEntry($"ERROR: {message}");
        Debug.LogError(message);
    }

    private void AddEntry(string logEntry)
    {
        methodLog.Enqueue(new LogEntry
        {
            Message = logEntry,
            TimeSeconds = Time.realtimeSinceStartup
        });

        if (methodLog.Count > MAX_LOG_ENTRIES)
            methodLog.Dequeue();
    }

    public List<string> GetMethodLog(float lastSeconds = -1f)
    {
        List<string> entries = new List<string>();
        float now = Time.realtimeSinceStartup;

        foreach (LogEntry entry in methodLog)
        {
            if (lastSeconds < 0f || now - entry.TimeSeconds <= lastSeconds)
            {
                entries.Add(entry.Message);
            }
        }

        return entries;
    }

    public void ClearLog()
    {
        methodLog.Clear();
    }
}
