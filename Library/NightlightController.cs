using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Library
{
    public class NightlightController
    {
        //private NightlightState State { get; set; }
        //public NightlightController() { }

        /// <summary>
        /// Включение
        /// Pre: ночник выключен
        /// </summary>
        /// <returns>режим (вкл./выкл.)</returns>
        /// <exception cref="InvalidOperationException">возникает, если ночние уже включён</exception>
        public bool TurnOn(NightlightState State)
        {
            if (State.isOn) throw new InvalidOperationException("Ночник уже включён");

            State.isOn = true;

            if (State.Brightness == 0)
            {
                State.Brightness = 50;
            }

            return State.isOn;
        }

        /// <summary>
        /// Выключение
        /// Pre: ночник включён
        /// </summary>
        /// <returns>режим (вкл./выкл.)</returns>
        /// <exception cref="InvalidOperationException">возникает, если ночник уже выключен</exception>
        public bool TurnOff(NightlightState State)
        {
            if (!State.isOn) throw new InvalidOperationException("Ночник уже выключен");

            State.isOn = false;

            return State.isOn;
        }

        /// <summary>
        /// Установление яркости
        ///  Pre: ночник включён и значение яркости находится между 0 и 100 включительно
        /// </summary>
        /// <param name="newBrightness">новая яркость</param>
        /// <exception cref="InvalidOperationException">возникает, если ночник выключен</exception>
        /// <exception cref="ArgumentOutOfRangeException">возникает, если яркость вышла за границы</exception>
        public void SetBrightness(NightlightState State, int newBrightness)
        {
            if (!State.isOn) throw new InvalidOperationException("Сначала включите ночник");

            if (newBrightness < 0 || newBrightness > 100) throw new ArgumentOutOfRangeException("Яркость должна быть от 0 до 100");

            State.Brightness = newBrightness;

            if (State.Brightness == 0)
            {
                State.isOn = false;
            }
        }

        /// <summary>
        /// Утсановление цвета
        /// Pre: ночник включён и цвет - непустое значение
        /// </summary>
        /// <param name="newColor">новый цвет</param>
        /// <exception cref="InvalidOperationException">возникает, если ночник выключен</exception>
        /// <exception cref="ArgumentNullException">возникает, если цвет - пустое значение</exception>
        /// <exception cref="ArgumentException">возникает, если запрашиваемый цвет не поддерживается</exception>
        public void SetColor(NightlightState State, string newColor)
        {
            if (!State.isOn) throw new InvalidOperationException("Сначала включите ночник");

            if (string.IsNullOrWhiteSpace(newColor)) throw new ArgumentNullException("Цвет не может быть пустым");

            string formattedColor = newColor.Trim().ToLower();
            if (formattedColor != "warmwhite" && formattedColor != "coldwhite" && formattedColor != "white") throw new ArgumentException("Цвет не поддерживается");

            State.currentColor = newColor;
        }

        /// <summary>
        /// Установление таймера
        /// Pre: ночник включён и кол-во минут в диапазоне от 1 до 60 включительно
        /// </summary>
        /// <param name="minutes">кол-во минут</param>
        /// <exception cref="InvalidOperationException">возникает, если ночник выключен</exception>
        /// <exception cref="ArgumentOutOfRangeException">возникает, если значение минут не в диапазоне</exception>
        public void SetTimer(NightlightState State, int minutes)
        {
            if (!State.isOn) throw new InvalidOperationException("Сначала включите ночник");

            if (minutes < 0 || minutes > 60) throw new ArgumentOutOfRangeException("Таймер устанавливается от 1 до 60 минут");

            State.TimerMinutes = minutes;
            State.isTimerActive = minutes > 0;
        }

        /// <summary>
        /// Имитация тика (когда будет 0 - сброс)
        /// </summary>
        public void TickSecond(NightlightState State)
        {
            if (!State.isTimerActive) return;

            int allMinutes = State.TimerMinutes * 60;
            allMinutes -= 1;

            if (allMinutes <= 0)
            {
                State.TimerMinutes = 0;
                State.isTimerActive = false;
                State.isOn = false;
            }
        }
    }
}
