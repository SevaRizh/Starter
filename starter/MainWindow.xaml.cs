using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using Microsoft.Win32;
using System.Timers;
using System.IO;

namespace starter
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Process p = new Process();              // создание переменной для процесса
        private bool indStart = true;                   // индикатор работы/остановки приложения
        private string nameFile;                        // переменная для отображения пути к файлу в программе
        private int[] arrayStartTime = new int[3];      // массив стартовых временных точек
        private int[] arrayStopTime = new int[3];       // массив остановочных временных точек
        private Random rnd = new Random();              // рандомная составляющая срабатывания таймера
        private const int intVal = 1200000;             // глобальная величина срабатывания таймер 20 мин
        Timer timer = new Timer();                      // создание таймера Timers.Timer()
        private const string path = "LocalLog.log";


        public MainWindow()
        {
            InitializeComponent();

            timer.Interval = 20000;
            timer.Elapsed += CheckTime;
        }

        #region событие происходящее по тику таймера

        private void CheckTime(object sender, EventArgs e)
        {
            timer.Interval = intVal + rnd.Next(intVal / 3 * -1, intVal / 3);
            int timeHour = DateTime.Now.Hour;

            if ((arrayStartTime[0] <= timeHour && timeHour < arrayStopTime[0] && !IsRunng(p)) ||
                (arrayStartTime[1] <= timeHour && timeHour < arrayStopTime[1] && !IsRunng(p)) ||
                (arrayStartTime[2] <= timeHour && timeHour < arrayStopTime[2] && !IsRunng(p)))
            {
                LocalLogger("CheckTime: приложение запущено");

                StartProc();
            }
            else if (((arrayStopTime[2] <= timeHour || timeHour < arrayStartTime[0]) && IsRunng(p)) ||
                     (arrayStopTime[0] <= timeHour && timeHour < arrayStartTime[1] && IsRunng(p)) ||
                     (arrayStopTime[1] <= timeHour && timeHour < arrayStartTime[2] && IsRunng(p)))
            {
                LocalLogger("CheckTime: приложение остановлено");

                StopProc();
            }
        }

        #endregion

        #region инициализация файла (включить фильтр)

        public Process Initializing()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            //            ofd.Filter = "Файл exe|*.exe";

            // проверка выбран ли фаил и изменения видимости кнопок 
            // и цвета подписи с указанием выбранного файла
            if (ofd.ShowDialog() == true)
            {
                p.StartInfo.FileName = ofd.FileName;
                nameFile = ofd.SafeFileName;
                btn_start_stop.IsEnabled = true;
                btn_browser.IsEnabled = false;
                label_status_browser.Content = ofd.FileName;
                label_status_browser.Foreground = Brushes.Green;
            }

            LocalLogger("Initializing: инициализация прошла успешно");

            return p;
        }

        #endregion

        #region старт/стоп процесса

        public void StartProc()
        {
            p.Start();
        }

        public void StopProc()
        {
            if (IsRunng(p))
            {
                p.CloseMainWindow(); 
            }
        }

        #endregion

        #region проверка на существование процесса

        public static bool IsRunng(Process process)
        {
            try { Process.GetProcessById(process.Id); }
            catch (InvalidOperationException) { return false; }
            catch (ArgumentException) { return false; }
            return true;
        }

        #endregion

        #region обработка кнопки "старт/стоп"

        private void Click_btn_StartStop(object sender, RoutedEventArgs e)
        {
            // Если выбранный фаил не имеет процесса И глобальный флаг true
            if (!IsRunng(p) && indStart)
            {
                CompareTime();
                indStart = false;
                StartProc();
                btn_start_stop.Content = "Стоп";
                timer.Start();

                LocalLogger("btn_Start: запуск успешен");
            }
                  // Если выбранный фаил имеет процесс и глобальный флаг false
            else //  if(IsRunng(p) && !indStart)
            {
                indStart = true;
                StopProc();
                btn_start_stop.Content = "Старт";
                timer.Stop();

                LocalLogger("btn_Stop: остановка успешна");
            }
        }

        #endregion

        #region обработка кнопки "выбор"

        private void Click_btn_Browser(object sender, RoutedEventArgs e)
        {
            p = Initializing();
        }

        #endregion

        #region обработка времён

        // обработка чисел для старта/остановки процесса
        private void CompareTime()
        {
            arrayStartTime = new int[] { ParseTime(tb_start_1.Text),
                                         ParseTime(tb_start_2.Text),
                                         ParseTime(tb_start_3.Text) };

            arrayStopTime = new int[] { ParseTime(tb_stop_1.Text),
                                        ParseTime(tb_stop_2.Text),
                                        ParseTime(tb_stop_3.Text) };

            Array.Sort(arrayStartTime);
            Array.Sort(arrayStopTime);

            // проверка пропущенного времени, оба скидываются в 99 (в не рабочее положение)
            for (int i = 0; i < arrayStartTime.Length && i < arrayStopTime.Length; i++)
            {
                if (arrayStartTime[i] == 99 || arrayStopTime[i] == 99)
                {
                    arrayStartTime[i] = 99;
                    arrayStopTime[i] = 99;
                }
            }

            LocalLogger("CompareTime: обработка времён пройдена");
        }

        #endregion

        #region парсинг TextBox'a

        private int ParseTime(string str)
        {
            int x = -1;
            
            bool b = int.TryParse(str, out x);

            int z;

            if (x >= 0 && x <= 23 && b == true)
            {
                z = x;
            }
            else
            {
                z = 99;
            }

            return z;
        }

        #endregion

        private void LocalLogger(string str)
        {
            using (StreamWriter logger = new StreamWriter(path, true))
            {
                logger.WriteLine(DateTime.Now + " " + str);
            }
        }

    }
}
