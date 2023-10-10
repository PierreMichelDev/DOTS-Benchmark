using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

[Serializable]
public struct PerformanceRecordingEntry
{
    [SerializeField]
    private string m_SamplerName;
    public string SamplerName => m_SamplerName;
}

public class RecordedData
{
    public Recorder EntryRecorder;
    public List<long> RecordedUnits = new();
}

public class PerformanceRecorder : MonoBehaviour
{
    [SerializeField]
    private PerformanceRecordingEntry[] m_RecordingEntries;
    [SerializeField]
    private Text m_ButtonText;
    [SerializeField]
    private float m_TimeUnitLength = 1.0f;

    private bool m_IsRunning = false;
    private float m_RecordingStartTime;
    private float m_TimeUnitEndTime;
    private RecordedData[] m_RecordedData;

    public void ToggleRecording()
    {
        m_IsRunning = !m_IsRunning;
        if (m_IsRunning)
        {
            m_ButtonText.text = "Stop Recording";
            m_RecordingStartTime = Time.time;
            m_TimeUnitEndTime = m_RecordingStartTime + m_TimeUnitLength;

            m_RecordedData = new RecordedData[m_RecordingEntries.Length];
            for (int i = 0; i < m_RecordingEntries.Length; ++i)
            {
                RecordedData newRecorder = new RecordedData();
                newRecorder.EntryRecorder = Recorder.Get(m_RecordingEntries[i].SamplerName);
                newRecorder.EntryRecorder.enabled = true;
                m_RecordedData[i] = newRecorder;
            }
        }
        else
        {
            m_ButtonText.text = "Start Recording";
            DumpRecordedData();
            m_RecordedData = null;
        }
    }

    private void Update()
    {
        if (m_IsRunning && Time.time > m_TimeUnitEndTime)
        {
            foreach (var recordedData in m_RecordedData)
            {
                recordedData.RecordedUnits.Add(recordedData.EntryRecorder.elapsedNanoseconds);
            }
            m_TimeUnitEndTime = Time.time + m_TimeUnitLength;
        }
    }

    private void DumpRecordedData()
    {
        using StreamWriter sw = new StreamWriter(File.OpenWrite("/Users/pierremichel/Documents/profile_results.csv"));

        sw.Write("Time;");
        for (int i = 0; i < m_RecordingEntries.Length; ++i)
        {
            sw.Write(m_RecordingEntries[i].SamplerName);
            sw.Write(";");
        }
        sw.WriteLine();

        int frameCount = m_RecordedData[0].RecordedUnits.Count;
        for (int frameIndex = 0; frameIndex < frameCount; ++frameIndex)
        {
            sw.Write(m_RecordingStartTime + frameIndex * m_TimeUnitLength);
            sw.Write(";");

            foreach (var recordedData in m_RecordedData)
            {
                sw.Write(recordedData.RecordedUnits[frameIndex]);
                sw.Write(";");
            }
            sw.WriteLine();
        }
    }
}
