using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class AnalyticsService : MonoBehaviour, IAnalyticsService
{
    private const string FILE_NAME = "analytics_events.json";

    [SerializeField] private string serverUrl = "url";
    [SerializeField] private float cooldownBeforeSend = 5f;

    private List<Event> _eventsToSend = new();
    private EventsList _eventsList;
    private float _lastSendTime = 0f;

    private void Awake()
    {
        LoadEventsFromFile();
        if(_eventsToSend.Count > 0)
            StartCoroutine(SendEvents());
    }

    private void Update()
    {
        if (Time.time - _lastSendTime >= cooldownBeforeSend && _eventsToSend.Count > 0)        
            StartCoroutine(SendEvents());
    }

    private void OnApplicationQuit() => 
        SaveEventsToFile();


    public void TrackEvent(string type, string data) => 
        _eventsToSend.Add(new Event(type, data));

    private IEnumerator SendEvents()
    {
        WWWForm form = new WWWForm();
        _eventsList = new EventsList(_eventsToSend);
        _eventsToSend.Clear();
        form.AddField("Analytics", JsonUtility.ToJson(_eventsList));
        _lastSendTime = float.MaxValue;
        using (UnityWebRequest request = UnityWebRequest.Post(serverUrl, form))
        {
            yield return request.SendWebRequest();
            yield return new WaitForSeconds(2f);
            if (request.result != UnityWebRequest.Result.Success)
            {
                _eventsToSend.AddRange(_eventsList.events);
                _eventsList = null;
            }

            _lastSendTime = Time.deltaTime;
        }
    }

    private void LoadEventsFromFile()
    {
        if (!File.Exists(GetFilePath()))
            return;

        string json = File.ReadAllText(GetFilePath());
        _eventsToSend = JsonUtility.FromJson<EventsList>(json).events;
        File.Delete(GetFilePath());
    }

    private void SaveEventsToFile()
    {
        if (_eventsList is not null)        
            _eventsToSend.AddRange(_eventsList.events);

        if (_eventsToSend.Count > 0)
        {
            string json = JsonUtility.ToJson(new EventsList(_eventsToSend));
            File.WriteAllText(GetFilePath(), json);
        }
    }

    private string GetFilePath() => 
        Path.Combine(Application.persistentDataPath, FILE_NAME);

    [Serializable]
    private class Event
    {
        public string Type;
        public string Data;

        public Event(string type, string data)
        {
            Type = type;
            Data = data;
        }
    }

    [Serializable]
    private class EventsList
    {
        public List<Event> events;

        public EventsList(List<Event> events) => 
            this.events = new List<Event>(events);
    }
}
