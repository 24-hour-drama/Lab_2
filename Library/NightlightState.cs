namespace Library
{
    /// <summary>
    /// Состояние ночника
    /// </summary>
    public class NightlightState
    {
        public bool isOn = false;
        public int Brightness = 50;
        public string currentColor = "white"; // если буде делать цветным
        public int TimerMinutes = 0;
        public bool isTimerActive = false;
    }
}
