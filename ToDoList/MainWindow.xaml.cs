using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Shapes;
using System.IO;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Microsoft.Win32;
using System.Configuration;
using System.Windows.Threading;
using System.Windows.Media.Animation;

namespace ToDoList
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window,INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 待办
        /// </summary>
        public int WaitNum
        {
            get
            {
                if (itemsColl == null)
                    return 0;
                return itemsColl.Where(x => !x.IsCompleted).Count();
            }            
        }

        /// <summary>
        /// 已办
        /// </summary>
        public int DoneNum
        {
            get
            {
                if (itemsColl == null)
                    return 0;
                return itemsColl.Where(x => x.IsCompleted).Count();
            }
        }

        /// <summary>
        /// 显示的时间
        /// </summary>
        private string date;
        public string Date
        {
            get
            {
                return date;
            }
            set
            {
                date = value;
                OnPropertyChanged(nameof(Date));
            }
        }

        /// <summary>
        /// 当前选中的时间
        /// </summary>
        private DateTime selectDate=DateTime.Now.Date;
        public DateTime SelectDate
        {
            get
            {
                return selectDate;
            }
            set
            {
                if(selectDate.Date!=value.Date)
                {
                    SaveData(selectDate);
                    selectDate = value;
                    OnPropertyChanged(nameof(SelectDate));
                    OnPropertyChanged(nameof(ShowTime));
                    LoadData(value);
                }
            }
        }
        
        public string ShowTime
        {
            get
            {
                string info = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(SelectDate.DayOfWeek);
                return SelectDate.ToString("dd/MM ")+info;
            }
        }

        /// <summary>
        /// 是否自启
        /// </summary>
        private bool autoRun;
        public bool AutoRun
        {
            get
            {
                return autoRun;
            }
            set
            {
                autoRun = value;
                OnPropertyChanged(nameof(AutoRun));
            }
        }

        /// <summary>
        /// 是否旋转
        /// </summary>
        private bool isSpin;
        public bool IsSpin
        {
            get
            {
                return isSpin;
            }
            set
            {
                isSpin = value;
                Util.SetAppSetting("spin", value.ToString());
                OnPropertyChanged(nameof(IsSpin));
                if (value)
                {
                    var animation = FindResource("animation") as BeginStoryboard;
                    animation.Storyboard.Begin(this, true);
                }
                else
                {
                    var animation = FindResource("animation") as BeginStoryboard;
                    animation.Storyboard.Stop(this);
                }
            }
        }



        /// <summary>
        /// 默认背景
        /// </summary>
        private bool isDefalut;
        public bool IsDefault
        {
            get
            {
                return isDefalut;
            }
            set
            {
                isDefalut = value;
                OnPropertyChanged(nameof(IsDefault));
                if (value)
                {
                    Util.SetAppSetting("switchimage", "0");
                }
            }
        }

        /// <summary>
        /// 单照片模式
        /// </summary>
        private bool isSinglePhotoMode;
        public bool IsSinglePhotoMode
        {
            get
            {
                return isSinglePhotoMode;
            }
            set
            {
                isSinglePhotoMode = value;
                OnPropertyChanged(nameof(IsSinglePhotoMode));
                if(value)
                {
                    Util.SetAppSetting("switchimage","1");
                }
            }
        }

        /// <summary>
        /// 照片集模式
        /// </summary>
        private bool isPhotoListMode;
        public bool IsPhotoListMode
        {
            get
            {
                return isPhotoListMode;
            }
            set
            {
                isPhotoListMode = value;
                OnPropertyChanged(nameof(IsPhotoListMode));
                if(value)
                {
                    Util.SetAppSetting("switchimage", "2");
                }
            }
        }

       

        /// <summary>
        /// 更换周期
        /// </summary>
        private int ticks;
        public int Ticks
        {
            get
            {
                return ticks;
            }
            set
            {
                ticks = value;
                OnPropertyChanged(nameof(Ticks));
                Util.SetAppSetting("switchticks", Ticks.ToString());
            }
        }


        /// <summary>
        /// 项集合
        /// </summary>
        private ObservableCollection<ItemModel> itemsColl=new ObservableCollection<ItemModel>();
        public ObservableCollection<ItemModel> ItemsColl
        {
            get
            {                
                return itemsColl;
            }
            set
            {
                itemsColl = value;
                OnPropertyChanged(nameof(ItemsColl));
                OnPropertyChanged(nameof(WaitNum));
                OnPropertyChanged(nameof(DoneNum));
            }
        }


        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        private const int GWL_EX_STYLE = -20;
        private const int WS_EX_APPWINDOW = 0x00040000, WS_EX_TOOLWINDOW = 0x00000080;


        private DispatcherTimer switchTimer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += (s, e) =>
            {
                var helper = new WindowInteropHelper(this).Handle;
                //Performing some magic to hide the form from Alt+Tab
                SetWindowLong(helper, GWL_EX_STYLE, (GetWindowLong(helper, GWL_EX_STYLE) | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW);

                AutoRun = Util.GetAppSetting("autorun").ToUpper().Equals("TRUE");
                var switchimage = Util.GetAppSetting("switchimage").ToString();
                isDefalut = switchimage.Equals("0");
                isSinglePhotoMode = switchimage.Equals("1");
                isPhotoListMode = switchimage.Equals("2");
                string str = Util.GetAppSetting("switchticks");
                ticks = int.Parse(str);
                if (ticks < 3)
                    ticks = 3;
                var tmp = Util.GetAppSetting("spin").ToString();
                isSpin = tmp.ToUpper().Equals("TRUE");
                this.DataContext = this;

                SelectDate = DateTime.Now.Date;
                LoadData(DateTime.Now);
                if (IsSinglePhotoMode)
                {
                    string fileName = Util.GetAppSetting("imagepath");
                    if (File.Exists(fileName))
                    {
                        this.circle.Background = GetImageBrushFromFile(fileName);
                    }
                    else
                    {
                        return;
                    }
                }
                if (IsPhotoListMode)
                {
                    ImageDir_Click(null, null);
                }
                if(IsSpin)
                {
                    var animation = FindResource("animation") as BeginStoryboard;
                    animation.Storyboard.Begin(this, true);
                }
            };
            this.Closing += (s, e) =>
            {
                SaveData(SelectDate);
            };
            
        }

        /// <summary>
        /// 加载当前日期数据
        /// </summary>
        /// <param name="date"></param>
        private void LoadData(DateTime date)
        {            
            string fileName = System.IO.Path.Combine(Environment.CurrentDirectory, "Data", date.ToString("yyyyMMdd") + ".dat");
            try
            {
                if (!File.Exists(fileName))
                {
                    ItemsColl = null;
                    return;
                }
                string str = File.ReadAllText(fileName,Encoding.UTF8);
                var data = JsonConvert.DeserializeObject<ObservableCollection<ItemModel>>(str);
                ItemsColl = data;
            }
            catch
            {
                ItemsColl = null;
            }
        }

        /// <summary>
        /// 保存当前日期数据
        /// </summary>
        /// <param name="date"></param>
        private void SaveData(DateTime date)
        {
            //判断是不是有内容，没有内容则跳过
            if (ItemsColl == null || ItemsColl.Count == 0)
                return;
            string fileName = System.IO.Path.Combine(Environment.CurrentDirectory, "Data", date.ToString("yyyyMMdd") + ".dat");
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs,Encoding.UTF8))
                {
                    string str = JsonConvert.SerializeObject(itemsColl);
                    sw.WriteLine(str);
                    sw.Flush();
                }
            }
        }

        private void Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //判断内容是否为空
            if(string.IsNullOrEmpty(this.txtedit.Text.Trim()))
            {
                MessageBox.Show("请先填写内容");
                return;
            }
            //增加一条数据
            ItemModel item = new ItemModel();
            item.Time = DateTime.Now.ToString("HH:mm");
            item.Content = this.txtedit.Text.Trim();
            item.IsCompleted = false;
            if (ItemsColl == null)
                ItemsColl = new ObservableCollection<ItemModel>();
            ItemsColl.Add(item);
            OnPropertyChanged(nameof(WaitNum));
            this.editgrid.Visibility = Visibility.Collapsed;
            txtedit.Clear();
            this.AddItem.IsEnabled = true;
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            this.editgrid.Visibility = Visibility.Visible;
            this.AddItem.IsEnabled = false;
        }


        /// <summary>
        /// 接口实现
        /// </summary>
        /// <param name="propertyName"></param>
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// 设置自启动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("ToDoList V1.0\nCreate by Lizhanping\n"+DateTime.Now.Date.ToString("yyyy-MM-dd"));
            return;
        }

        
        private ImageBrush GetImageBrushFromFile(string fileName)
        {
            ImageBrush brush = new ImageBrush();
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(fileName, UriKind.Absolute);
            bi.EndInit();
            brush.ImageSource = bi;
            brush.Stretch = Stretch.UniformToFill;
            return brush;
        }

        /// <summary> 
        /// 开机启动项 
        /// </summary> 
        /// <param name=\"Started\">是否启动</param> 
        /// <param name=\"name\">启动值的名称</param> 
        /// <param name=\"path\">启动程序的路径</param> 
        public static void RunWhenStart(bool Started, string name, string path)
        {
            RegistryKey HKLM = Registry.LocalMachine;
            RegistryKey Run = HKLM.CreateSubKey(@"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\"); 
            if (Started == true)
            {
                try
                {
                    Run.SetValue(name, path);
                    HKLM.Close();
                }
                catch (Exception Err)
                {
                    MessageBox.Show(Err.Message.ToString()); 
                }
            }
            else
            { 
                try
                { 
                  Run.DeleteValue(name); 
                  HKLM.Close(); 
                } 
                catch (Exception) 
                { 
                  // 
                } 
            } 
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            //获取点击删除的index
            var index = ((sender as Button).DataContext as ItemModel);
            ItemsColl.Remove(index);
            OnPropertyChanged(nameof(WaitNum));
            OnPropertyChanged(nameof(DoneNum));
        }

        private void Item_Checked(object sender, RoutedEventArgs e)
        {
            OnPropertyChanged(nameof(WaitNum));
            OnPropertyChanged(nameof(DoneNum));
        }

        private void Item_Unchecked(object sender, RoutedEventArgs e)
        {
            OnPropertyChanged(nameof(WaitNum));
            OnPropertyChanged(nameof(DoneNum));
        }

        private void Auto_Click(object sender, RoutedEventArgs e)
        {
            if (AutoRun)
            {
                AutoRun = false;

            }
            else
            {
                AutoRun = true;
            }
            RunWhenStart(AutoRun, "ToDoList", System.IO.Path.Combine(Environment.CurrentDirectory, "ToDoList.exe"));
            //写配置文件
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            string key = "autorun";
            var value = AutoRun;
            if (config.AppSettings.Settings[key] != null)
                config.AppSettings.Settings[key].Value = value.ToString();
            else
                config.AppSettings.Settings.Add(key, value.ToString());
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        private void Spin_Click(object sender, RoutedEventArgs e)
        {
            IsSpin = !isSpin;
        }

        /// <summary>
        /// 选择了单个照片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SignleImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "请选择照片...";
            ofd.Filter = "(png文件,jpg文件)|*.png;*.jpg";
            ofd.ShowDialog();
            ofd.Multiselect = false;
            if (string.IsNullOrEmpty(ofd.FileName))
            {
                return;
            }
            string file = ofd.FileName;
            this.circle.Background = GetImageBrushFromFile(file);
            //更换成功后，写模式
            IsSinglePhotoMode = true;
            IsPhotoListMode = false;
            IsDefault = false;
            //停掉定时器，如果在运行的话
            switchTimer.Stop();
            switchTimer = null;            
            Util.SetAppSetting("imagepath",file);
            
        }


        private List<string> PhotoList = new List<string>();

        /// <summary>
        /// 选择照片集方式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageDir_Click(object sender, RoutedEventArgs e)
        {
            if (IsPhotoListMode&&sender!=null)
                return;
            IsDefault = false;
            IsSinglePhotoMode = false;
            IsPhotoListMode = true;
            //设置一个定时器
            switchTimer = new DispatcherTimer();
            switchTimer.Interval = new TimeSpan(0, 0, Ticks);
            switchTimer.Tick += SwitchTimer_Tick;
            //启动之前，先加载所有的图片，png或者jpg
            PhotoList.Clear();
            string dir = System.IO.Path.Combine(Environment.CurrentDirectory, "Data", "Image");
            if(Directory.Exists(dir))
            {
                DirectoryInfo di = new DirectoryInfo(dir);
                foreach(var item in di.GetFiles())
                {
                    if(item.Extension.Contains("png")||item.Extension.Contains("jpg"))
                    {
                        PhotoList.Add(item.FullName);
                    }
                }
                switchTimer.Start();
            }
            else
            {
                return;
            }          
        }

        int i = 0;
        private void SwitchTimer_Tick(object sender, EventArgs e)
        {
            int count = PhotoList.Count;

            this.circle.Background = GetImageBrushFromFile(PhotoList[i % count]);
            Console.WriteLine(DateTime.Now.ToString());
            i++;
            if (i == count)
                i = 0;
        }

        private void slide_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            switchTimer.Stop();
            switchTimer.Interval = new TimeSpan(0, 0, Ticks);
            switchTimer.Start();
        }

        /// <summary>
        /// 设置为默认
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Default_Click(object sender, RoutedEventArgs e)
        {
            var obj = FindResource("bck") as RadialGradientBrush;
            this.circle.Background = obj;
            IsDefault = true;
            IsSinglePhotoMode = false;
            IsPhotoListMode = false;
        }

        private void Today_Click(object sender, RoutedEventArgs e)
        {
            SelectDate = DateTime.Now.Date;
        }
    }

    public class ItemModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 时间
        /// </summary>
        private string time;
        public string Time
        {
            get
            {
                return time;
            }
            set
            {
                time = value;
                RaisePropertyChanged(nameof(Time));
            }
        }

        /// <summary>
        /// 内容
        /// </summary>
        private string content;
        public string Content
        {
            get
            {
                return content;
            }
            set
            {
                content = value;
                RaisePropertyChanged(nameof(Content));
            }
        }

        /// <summary>
        /// 是否完成
        /// </summary>
        private bool isCompleted;
        public bool IsCompleted
        {
            get
            {
                return isCompleted;
            }
            set
            {
                isCompleted = value;
                RaisePropertyChanged(nameof(IsCompleted));
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
