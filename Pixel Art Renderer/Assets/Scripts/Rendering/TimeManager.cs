using System;
using UnityEngine;

public sealed class TimeManager : MonoBehaviour
{
    public struct TimeRecord
    {
        public const int MAX_RAW_VALUE = 86400;
        public const int MIN_RAW_VALUE = 0;

        byte Hours;
        byte Minutes;
        byte Seconds;

        public TimeRecord(byte hours, byte minutes, byte seconds)
        {
            Hours = hours;
            Minutes = minutes;
            Seconds = seconds;
        }

        public static TimeRecord operator +(TimeRecord a, TimeRecord b) =>
             Validate(new((byte)(a.Hours + b.Hours), (byte)(a.Minutes + b.Minutes), (byte)(a.Seconds + b.Seconds)));
        public static TimeRecord operator -(TimeRecord a, TimeRecord b) => 
            Validate(new((byte)(a.Hours - b.Hours), (byte)(a.Minutes - b.Minutes), (byte)(a.Seconds - b.Seconds)));
        public static TimeRecord operator +(TimeRecord a, byte b) =>
            Validate(new(a.Hours, a.Minutes, (byte)(a.Seconds + b)));
        public static TimeRecord operator -(TimeRecord a, byte b) =>
            Validate(new(a.Hours, a.Minutes, (byte)(a.Seconds - b)));

        private static TimeRecord Validate(TimeRecord newRecord)
        {
            if (newRecord.Seconds >= 60) 
            {
                newRecord.Minutes += 1;
                newRecord.Seconds -= 60;
            }
            if (newRecord.Minutes >= 60)
            {
                newRecord.Hours += 1;
                newRecord.Minutes -= 60;
            }
            if (newRecord.Hours >= 24)
                newRecord.Hours = 0;

            return newRecord;
        }
        public static TimeRecord FromByteArray(byte[] array) 
            => new(array[0], array[1], array[2]);

        public int GetRawValue() => Hours * 3600 + Minutes * 60 + Seconds;
        public float GetValue01() => GetRawValue() / (float)MAX_RAW_VALUE;
        public override string ToString() => $"[{Hours}:{Minutes}:{Seconds}]";
    }

    public static TimeManager Instance;

    [SerializeField]
    private byte[] _initialGameTime = new byte[3]{8,0,0};
    [SerializeField]
    private short _gameSecondsPerSecond = 4;
    private float _gameSecondBuffer = 0;

    public delegate void AfterTimePassed();

    public static AfterTimePassed OnTimeUpdated;
    public static AfterTimePassed OnGameSecond;
    public TimeRecord ServerTime { get; private set; }
    public TimeRecord LocalTime { get; private set; }
    public TimeRecord GameTime { get; private set; }
    public TimeRecord SessionTime { get; private set; }

    private void Awake()
    {
        if(Instance is not null)
        {
            Destroy(this);
            return;
        }
        DontDestroyOnLoad(this);
        Instance = this;

        InitializeTimeRecords();
        SyncLocalTime();
        InvokeRepeating(nameof(OnSecondPassed), 1f, 1f);
    }
    private void Update() => OnGameTickPassed();

    private void OnGameTickPassed()
    {
        _gameSecondBuffer += Time.deltaTime * _gameSecondsPerSecond;
        if (_gameSecondBuffer < 1f)
            return;

        byte _elapsedGameSeconds = (byte)Mathf.FloorToInt(_gameSecondBuffer);
        GameTime += _elapsedGameSeconds;
        _gameSecondBuffer -= _elapsedGameSeconds;

        OnGameSecond?.Invoke();
    }

    private void OnSecondPassed()
    {
        ServerTime += 1;
        LocalTime += 1;
        GameTime += 1;
        SessionTime += 1;
        OnTimeUpdated?.Invoke();
    }

    private void InitializeTimeRecords()
    {
        ServerTime = new(0, 0, 0);
        GameTime = TimeRecord.FromByteArray(_initialGameTime);
        SessionTime = new(0, 0, 0);
    }

    private void SyncServerTime(TimeRecord serverTime) => ServerTime = serverTime;
    private void SyncLocalTime() => LocalTime = new
    (
        (byte)DateTime.Now.Hour, 
        (byte)DateTime.Now.Minute, 
        (byte)DateTime.Now.Second
    );
    private void SyncGameTime(TimeRecord gameTime) => GameTime = gameTime;
}
