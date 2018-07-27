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
//using System.Timers;

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
        private int intVal = 1200000;                   // глобальная величина срабатывания таймер 20 мин
        Timer timer = new Timer();                      // создание таймера Timers.Timer()
        

        public MainWindow()
        {
            InitializeComponent();

            timer.Interval = intVal;
            timer.Elapsed += CheckTime;
        }

        private void CheckTime(object sender, EventArgs e)
        {
            timer.Interval = intVal + rnd.Next(-500000, 500000);
            int timeHour = DateTime.Now.Hour;

            if ((arrayStartTime[0] <= timeHour && timeHour < arrayStopTime[0] && !IsRunng(p)) ||
                (arrayStartTime[1] <= timeHour && timeHour < arrayStopTime[1] && !IsRunng(p)) ||
                (arrayStartTime[2] <= timeHour && timeHour < arrayStopTime[2] && !IsRunng(p)))
            {
                string tm = string.Format("запуск в {0}\r\n", DateTime.Now);
                File.AppendAllText("C:\\TimeTest.txt", tm);

                StartProc();
            }
            else if (((arrayStopTime[2] <= timeHour || timeHour < arrayStartTime[0]) && IsRunng(p)) ||
                     (arrayStopTime[0] <= timeHour && timeHour < arrayStartTime[1] && IsRunng(p)) ||
                     (arrayStopTime[1] <= timeHour && timeHour < arrayStartTime[2] && IsRunng(p)))
            {
                string tm = string.Format("остановка в {0}\r\n", DateTime.Now);
                File.AppendAllText("C:\\TimeTest.txt", tm);

                StopProc();
            }
        }

        #region инициализация (включить фильтр)

        public Process Initializing()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            //            ofd.Filter = "Файл exe|*.exe";

            //проверка выбран ли фаил и изменения видимости кнопок 
            //и цвета подписи с указанием выбранного файла
            if (ofd.ShowDialog() == true)
            {
                p.StartInfo.FileName = ofd.FileName;
                nameFile = ofd.SafeFileName;
                btn_start_stop.IsEnabled = true;
                btn_browser.IsEnabled = false;
                label_status_browser.Content = ofd.FileName;
                label_status_browser.Foreground = Brushes.Green;
            }

            return p;
        }

        #endregion

        public void StartProc()
        {
            p.Start();
        }

        public void StopProc()
        {
            p.CloseMainWindow();
        }

        #region проверка на существование процесса

        public static bool IsRunng(Process process)
        {
            try { Process.GetProcessById(process.Id); }
            catch (InvalidOperationException) { return false; }
            catch (ArgumentException) { return false; }
            return true;
        }

        #endregion

        private void Click_btn_StartStop(object sender, RoutedEventArgs e)
        {
            //Если выбранный фаил не имеет процесса - его запустить, иначе останавить
            if (!IsRunng(p))
            {
                // обработка чисел для старта/остановки процесса
                if (indStart)
                {
                    CompareTime();
                    indStart = false;
                }

                btn_start_stop.Content = "Стоп";

                label_status_timer.Content = "приложение включено";
                label_status_timer.Foreground = Brushes.Green;

                timer.Start();
            }
            else
            {
                if (!indStart)
                {
                    indStart = true;
                }

                StopProc();
                btn_start_stop.Content = "Старт";

                label_status_timer.Content = "приложение выключено";
                label_status_timer.Foreground = Brushes.Red;

                timer.Stop();
            }
        }

        private void Click_btn_Browser(object sender, RoutedEventArgs e)
        {
            p = Initializing();
        }

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
        }

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

    }
}
