using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Global debug logger that tracks method calls and events
/// Usage: DebugLogger.Instance.LogMethodCall("MethodName");
/// </summary>
public class DebugLogger : MonoBehaviour
{
    private struct LogEntry
    {
        public string Message;
        public float TimeSeconds;
    }

    private static DebugLogger instance;
    public static DebugLogger Instance
    {
        get
        {
            if (instance == null)
            {
                if (!Application.isPlaying)
                {
                    return null;
                }

                GameObject go = new GameObject("DebugLogger");
                instance = go.AddComponent<DebugLogger>();
            }
            return instance;
        }
    }

    private Queue<LogEntry> methodLog = new Queue<LogEntry>();
    private const int MAX_LOG_ENTRIES = 100;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
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
